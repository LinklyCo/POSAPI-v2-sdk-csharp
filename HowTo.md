- [1. Introduction](#1-introduction)
- [2. Build and test](#2-build-and-test)
- [3. Sample POS interface](#3-sample-pos-interface)
- [4. Initialising the SDK](#4-initialising-the-sdk)
  - [4.1. Initialising the `SampleEventListener`](#41-initialising-the-sampleeventlistener)
- [5. Pairing](#5-pairing)
  - [5.1. Loading an existing pair secret](#51-loading-an-existing-pair-secret)
- [6. Sending POS requests and handling responses](#6-sending-pos-requests-and-handling-responses)
  - [6.1. Logon](#61-logon)
  - [6.2. Status](#62-status)
  - [6.3. Settlement](#63-settlement)
  - [6.4. Query card](#64-query-card)
  - [6.5. Configure merchant](#65-configure-merchant)
  - [6.6. Reprint receipt](#66-reprint-receipt)
  - [6.7. Send key](#67-send-key)
  - [6.8. Transaction requests](#68-transaction-requests)
    - [6.8.1. Purchase](#681-purchase)
    - [6.8.2. Refund](#682-refund)
    - [6.8.3. Void](#683-void)
    - [6.8.4. Cash out](#684-cash-out)
    - [6.8.5. Deposit](#685-deposit)
    - [6.8.6. Pre-authorisation](#686-pre-authorisation)
    - [6.8.7. Extend pre-authorisation](#687-extend-pre-authorisation)
    - [6.8.8. Top-up pre-authorisation](#688-top-up-pre-authorisation)
    - [6.8.9. Cancel pre-authorisation](#689-cancel-pre-authorisation)
    - [6.8.10. Partially cancel pre-authorisation](#6810-partially-cancel-pre-authorisation)
    - [6.8.11. Complete pre-authorisation](#6811-complete-pre-authorisation)
    - [6.8.12. Pre-authorisation inquiry](#6812-pre-authorisation-inquiry)
    - [6.8.13. Pre-authorisation summary](#6813-pre-authorisation-summary)
  - [6.9. Handling the transaction response](#69-handling-the-transaction-response)
  - [6.10. Retrieve historical transaction](#610-retrieve-historical-transaction)
  - [6.11. Handling display events](#611-handling-display-events)
  - [6.12. Handling receipt events](#612-handling-receipt-events)
  - [6.13. Handling error events](#613-handling-error-events)
- [7. Session recovery](#7-session-recovery)
- [8. More complex POS-PIN Pad configurations](#8-more-complex-pos-pin-pad-configurations)
  - [8.1. Pool of PIN pads shared by one POS](#81-pool-of-pin-pads-shared-by-one-pos)
  - [8.2. POS pool](#82-pos-pool)

# 1. <a name="Introduction">Introduction</a>
This document provides a quick start guide to using the C# POS API v2 SDK with code samples. The purpose of the sample code is to demonstrate easily comprehensible usage of the SDK given a simplistic POS interface. Some examples use hard-coded values which would not be the case with production code. For a complete SDK reference refer to in the Linkly.PosApi.Sdk\docs and to see a Windows demo POS application which uses the SDK, refer to the Linkly.PosApi.Sdk.DemoPos project.

The following guide assumes you have built the Linkly.PosApi.Sdk library and included either the project or library in your POS solution.

# 2. <a name="Build">Build and test</a>
The Linkly.PosApi.Sdk solution contains the following three projects:
* Linkly.PosApi.Sdk - The C# SDK library which can be used to implement a POS application by leveraging the POS API v2. This library uses .NET Standard 2.1 and supports the .NET implementations shown [here](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-1). If you only want to use the SDK and do not care about running the unit tests or Demo POS you can build this project only.
* Linkly.PosApi.Sdk.UnitTest - The unit test project for Linkly.PosApi.Sdk. This requires .NET 6.
* Linkly.PosApi.Sdk.DemoPos - Demo POS application which uses the Linkly.PosApi.Sdk and demonstrates usage of the majority of functionality provided in the SDK. This can be used as a reference implementation for writing a POS.

# 3. <a name="IPos">Sample POS interface</a>
The `IPos` interface is meant to highlight some of the functionality which may be present in an actual POS. Some implementation details have been omitted as that is out of scope of this guide and is dependent on preferred frameworks and libraries.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

/// <summary>
/// Very simplistic POS interface used throughout the sample code.
/// </summary>
internal interface IPos
{
    // For saving and loading the pairing secret after a POS restart
    void SavePairSecret(string secret);
    bool TryLoadSecret(out string secret);

    // For session recovery
    void BeginSession(Guid sessionId);
    void CompleteSession(Guid sessionId);
    bool TryGetLastIncompleteSession(out Guid sessionId);

    // POS display operations
    void DisplayPrompt(IEnumerable<string> lines);
    void DisplayReceipt(IEnumerable<string> lines);
    void DisplayMessage(params string[] lines);
    void DisplayError(string error);

    // SDK operations
    void Pair();
    bool LoadExistingPairing();
    void PreAuthTopUp();
    void Void();
    void PreAuthInquiry();
    void Refund();
    void Purchase();
    void SettlementRequest();
    void Deposit();
    void Logon();
    void PreAuthExtend();
    void SendKey();
    void QueryCardRequest();
    void ConfigureMerchantRequest();
    void RetrieveTransaction();
    void CashOut();
    void PreAuthSummary();
    void PreAuthCompletion();
    void Status();
    void PreAuth();
    void PreAuthCancel();
    void ReprintReceiptRequest();
    void PreAuthPartialCancel();
    void RecoverSession();
}
```

# 4. <a name="InitialiseSdk">Initialising the SDK</a>
The following example shows initialisation of the SDK. An implementation of `IPosApiEventListener` is required. Since the SDK is asynchronous, responses to API requests are handled by this implementation, which in the case of this example is the
`SampleEventListener` class.

The service endpoints for the Linkly Auth API and POS API endpoints must be defined and a `PosVendorDetails` object which specifies the unique identifiers of the POS and vendor should be created. Lastly the
`PosApiService` can be initialised.

At the end of the `SamplePos` constructor there is a call to `LoadExistingPairing()` to check whether there is an [existing POS and PIN pad pairing](#SetPairSecret) followed by a call to `RecoverSession()` which will attempt to [recover a transaction](#SessionRecovery) that may have been interrupted by a software or hardware failure.

<a name="SamplePos.cs"></a>
```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    // Your application should generate it's own unique vendor ID and this value should remain
    // consistent across all instances of your POS.
    private static readonly Guid PosVendorId = new("f852e0ad-12ed-43c5-9824-4657a439de0c"); // Do not use this

    private readonly IPosApiService _posApiService;

    protected SamplePos(ILogger logger, IConfiguration config)
    {
        // Listener which handles events from the POS API.
        var listener = new SampleEventListener(logger, this);

        // API endpoints. Refer to the API documentation for the sandbox or production URIs (https://www.linkly.com.au/apidoc/REST/#introduction).
        var authApiUri = new Uri(config["AuthApiUri"] ?? throw new ArgumentException("AuthApiUri not found", nameof(config)));
        var posApiUri = new Uri(config["PosApiUri"] ?? throw new ArgumentException("PosApiUri not found", nameof(config)));
        var serviceEndpoints = new ApiServiceEndpoint(authApiUri, posApiUri);

        // Each deployment of the same POS application should use unique ID and that same ID
        // should be used when initialising the PosApiService.
        var posId = new Guid(config["PosId"] ?? throw new ArgumentException("PosId not found", nameof(config)));

        // Create an object which contains the vendor details of the POS software
        var posVendorDetails = new PosVendorDetails("Sample POS", "0.0.1a", posId, PosVendorId);

        // Initialise the service
        _posApiService = new PosApiService(logger, null, listener, serviceEndpoints, posVendorDetails);

        // Check if pairing was done and use existing credentials if so
        if (LoadExistingPairing())
            // Recover a previous session if the POS crashed unexpectedly
            RecoverSession();
    }
}
```

## 4.1. <a name="SampleEventListener">Initialising the `SampleEventListener`</a>
The `SampleEventListener` responds to API events triggered by the SDK's internal logic. Most of the request methods in the `IPosApiService` have respective event handlers in `IPosApiEventListener` which will be invoked when a response to a request is received from the API.

To initialise the `SampleEventListener`, a reference to the  `IPos` interface is required for displaying receipts, prompts and errors in the UI.

```cs
namespace Linkly.PosApi.Sdk.Examples.EventListeners;

internal partial class SampleEventListener : IPosApiEventListener
{
    private readonly ILogger _logger;
    private readonly IPos _posInterface;

    public SampleEventListener(ILogger logger, IPos posInterface)
    {
        _logger = logger;
        _posInterface = posInterface;
    }
}

```

# 5. <a name="Pairing">Pairing</a>
The POS interacts with the PIN pad via the Linkly Cloud Gateway (if using the cloud solution). Pairing is the process of associating the PIN pad with the Cloud Gateway.

When a PIN pad is successfully paired to the Cloud Gateway a secret JWT (JSON Web Token) is returned by the SDK and this should be stored persistently by the POS to avoid the need to re-pair when the POS application is restarted.

To pair a PIN pad use the `IPosApiService.PairingRequest()` method. The request must contain valid `Username`, `PairCode` and `Password` credentials.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void Pair()
    {
        var request = new PairingRequest { Username = "example", PairCode = "123456", Password = "Password!" };

        _posApiService.PairingRequest(request);
    }
}
```

When pairing completes, the `IPosApiEventListener.PairingComplete()` listener will be invoked, and the pairing secret (JWT) in the response can be securely saved. Next time the POS starts it can [load the secret](#SetPairSecret) to bypass pairing. The implementation to load and store the secret is out of scope of this document.

```cs
namespace Linkly.PosApi.Sdk.Examples.EventListeners;

internal partial class SampleEventListener : IPosApiEventListener
{
    public void PairingComplete(PairingRequest request, PairingResponse response)
    {
        // Save the pair secret persistently so it can be loaded next time the POS is started
        _posInterface.SavePairSecret(response.Secret);
    }
}
```

## 5.1. <a name="SetPairSecret">Loading an existing pair secret</a>
When the SDK is initialised it normally needs to be paired with a PIN pad, however if a pairing secret was saved when the PIN pad was last paired, that secret can be loaded into the SDK to bypass pairing. In the following example, the code checks if the pairing secret can be loaded from secure storage. If so `IPosApiService.SetPairSecret()` is called and the POS can immediately issue requests to the SDK without pairing.

The following method is [called during initialisation of the `SamplePos`](#SamplePos.cs)

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public bool LoadExistingPairing()
    {
        if (TryLoadSecret(out var secret))
        {
            _posApiService.SetPairSecret(secret);

            return true;
        }

        return false;
    }
}
```

# 6. <a name="Requests">Sending POS requests and handling responses</a>
The following sections will explain how to send the various POS requests and handle the responses. All request types may invoke the [`IPosApiEventListener.Error()`](#ErrorListener) listener if the request fails. Some requests may invoke the [`IPosApiEventListener.Display()`](#DisplayListener) and [`IPosApiEventListener.Receipt()`](#ReceiptListener) listeners and when this is the case it will be explicitly mentioned in the proceeding sections.

## 6.1. <a name="Logon">Logon</a>
Performs a logon of the PIN pad to the bank. The following code sends a `Standard` logon request.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void Logon()
    {
        var request = new LogonRequest { LogonType = LogonType.Standard };

        _posApiService.LogonRequest(request);
    }
}
```

Display an error message if the request is unsuccessful. [`IPosApiEventListener.Display()`](#DisplayListener) and [`IPosApiEventListener.Receipt()`](#ReceiptListener) listeners can be invoked as part of this request.

```cs
namespace Linkly.PosApi.Sdk.Examples.EventListeners;

internal partial class SampleEventListener : IPosApiEventListener
{
    public void LogonComplete(Guid sessionId, LogonRequest request, LogonResponse response)
    {
        if (!response.Success)
        {
            _posInterface.DisplayError($"Logon failed: {response.ResponseText}");
        }
    }
}
```

## 6.2. <a name="Status">Status</a>
Get the terminal status. The following code sends `Standard` status request.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void Status()
    {
        var request = new StatusRequest { StatusType = StatusType.Standard };

        _posApiService.StatusRequest(request);
    }
}
```

Display fields from the response to the UI, or an error if the request is unsuccessful.

```cs
namespace Linkly.PosApi.Sdk.Examples.EventListeners;

internal partial class SampleEventListener : IPosApiEventListener
{
    public void StatusComplete(Guid sessionId, StatusRequest request, StatusResponse response)
    {
        if (!response.Success)
        {
            _posInterface.DisplayError($"Status failed: {response.ResponseText}");
            return;
        }

        _posInterface.DisplayMessage(
            $"Card Misread Count: {response.CardMisreadCount}",
            $"Cash Out Limit: {response.CashoutLimit}",
            $"Refund Limit: {response.RefundLimit}",
            $"Saf Count: {response.SafCount}",
            $"Saf Credit Limit: {response.SafCreditLimit}",
            $"Saf Debit Limit: {response.SafDebitLimit}");
    }
}
```

## 6.3. <a name="Settlement">Settlement</a>
Get a settlement report. The following example has been hard-coded to send a standard daily settlement request.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void SettlementRequest()
    {
        var request = new SettlementRequest { SettlementType = SettlementType.Settlement };

        _posApiService.SettlementRequest(request);
    }
}
```

Display an error message if the request is unsuccessful. [`IPosApiEventListener.Display()`](#DisplayListener) and [`IPosApiEventListener.Receipt()`](#ReceiptListener) listeners can be invoked as part of this request with the settlement report data within.

```cs
namespace Linkly.PosApi.Sdk.Examples.EventListeners;

internal partial class SampleEventListener : IPosApiEventListener
{
    public void SettlementComplete(Guid sessionId, SettlementRequest request, SettlementResponse response)
    {
        if (!response.Success)
        {
            _posInterface.DisplayError(response.ResponseText);
        }
    }
}
```

## 6.4. <a name="QueryCard">Query card</a>
Get the details of a card. The following request has been hard-coded to send a read card request.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void QueryCardRequest()
    {
        var request = new QueryCardRequest { QueryCardType = QueryCardType.ReadCard };

        _posApiService.QueryCardRequest(request);
    }
}
```

Display fields from the response to the UI, or an error message if the request is unsuccessful. The [`IPosApiEventListener.Display()`](#DisplayListener) listener can be invoked as part of this request prompting for a card swipe for example.

```cs
namespace Linkly.PosApi.Sdk.Examples.EventListeners;

internal partial class SampleEventListener : IPosApiEventListener
{
    public void QueryCardComplete(Guid sessionId, QueryCardRequest request, QueryCardResponse response)
    {
        if (!response.Success)
        {
            _posInterface.DisplayError(response.ResponseText);
            return;
        }

        _posInterface.DisplayMessage(
            $"Account Type: {response.AccountType}",
            $"Card Name: {response.CardName}",
            $"Track1: {response.Track1}");
    }
}
```

## 6.5. <a name="ConfigureMerchant">Configure merchant</a>
Configure the PIN pad's `CatId` and `CaId` settings with the acquiring bank. The following example uses a hard-coded `CaId`.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void ConfigureMerchantRequest()
    {
        var request = new ConfigureMerchantRequest { CaId = "0987654321" };

        var sessionId = _posApiService.ConfigureMerchantRequest(request);
        BeginSession(sessionId);
    }
}
```

Display an error message if the request is unsuccessful.

```cs
namespace Linkly.PosApi.Sdk.Examples.EventListeners;

internal partial class SampleEventListener : IPosApiEventListener
{
    public void ConfigureMerchantComplete(Guid sessionId, ConfigureMerchantRequest request, ConfigureMerchantResponse response)
    {
        if (!response.Success)
        {
            _posInterface.DisplayError(response.ResponseText);
        }
    }
}
```

## 6.6. <a name="ReprintReceipt">Reprint receipt</a>
Retrieve a previous receipt from the PIN pad. In the following example `ReprintType.GetLast` is set, which will retrieve the last receipt without re-printing the receipt on the PIN pad.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void ReprintReceiptRequest()
    {
        var request = new ReprintReceiptRequest { ReprintType = ReprintType.GetLast };

        _posApiService.ReprintReceiptRequest(request);
    }
}
```

Display the receipt in the UI, or an error message if the request is unsuccessful.

```cs
namespace Linkly.PosApi.Sdk.Examples.EventListeners;

internal partial class SampleEventListener : IPosApiEventListener
{
    public void ReprintReceiptComplete(Guid sessionId, ReprintReceiptRequest request, ReprintReceiptResponse response)
    {
        if (!response.Success)
        {
            _posInterface.DisplayError(response.ResponseText);
            return;
        }

        _posInterface.DisplayReceipt(response.ReceiptText);
    }
}
```

## 6.7. <a name="SendKey">Send key</a>
Send a key press to the PIN pad. The following example sends the '0' key to the PIN pad which is equivalent to pressing the `OK` or `CANCEL` button (depending on the context). The `IPosApiService.SendKeyRequest()` does not have an event handler and it can be assumed the request was successful if the `IPosApiEventListener.Error()` event is not invoked.

Some use cases for `IPosApiService.SendKeyRequest()` is cancelling a transaction from the POS, or verifying a card signature.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void SendKey()
    {
        var request = new SendKeyRequest { Key = "0" };

        _posApiService.SendKeyRequest(request);
    }
}
```

 ## 6.8. <a name="TransactionRequests">Transaction requests</a>
The `IPosApiService.TransactionRequest()` method can be used to initiate transactions from the PIN pad. This section will show examples of sending all the supported transaction requests. Note the same [`IPosApiEventListener.TransactionComplete()`](#TransactionComplete) listener will be invoked upon successful completion of the transaction regardless of the transaction type. Therefore a single [example of handling the `IPosApiEventLIstener.TransactionComplete()` listener](#TransactionComplete) has been provided.

For simplicity, all the examples use hard-coded transaction data. In a production POS system the request data would be dynamic and most likely sourced from a UI. The examples are intended to provide simple usage scenarios and cases such as tipping are not demonstrated in this document, however they are covered in the SDK documentation and Demo POS application.

In all the proceeding examples a call to `BeginSession()` is invoked after sending the request. This is called to indicate the transaction has started and can be used to assist with [session recovery](#SessionRecovery), which may be desirable to implement for cases where there is a POS hardware or software failure part way through a transaction. The implementation of `BeginSession()` is out of scope of this document, however a theoretical implementation could save the GUID returned by the transaction request to persistent storage and attempt to load it from storage when the POS initialises.

[`IPosApiEventListener.Display()`](#DisplayListener) and [`IPosApiEventListener.Receipt()`](#ReceiptListener) listeners can be invoked as part of this request. The receipt listener will be invoked when the PIN pad prints a transaction receipt. The display listener can be invoked multiple times throughout the transaction when the PIN pad is displaying prompts to the user such as `SWIPE CARD`, `ENTER PIN`, etc.

### 6.8.1. <a name="Purchase">Purchase</a>
Purchase which also facilitates cash out.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void Purchase()
    {
        var request = new PurchaseRequest
        {
            Amount = 5095, // $50.95 purchase
            TxnRef = "123"
        };

        var sessionId = _posApiService.TransactionRequest(request);
        BeginSession(sessionId);
    }
}
```

### 6.8.2. <a name="Refund">Refund</a>
Refund for a purchase.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void Refund()
    {
        var request = new RefundRequest
        {
            Amount = 1000, // $10.00 refund
            TxnRef = "123"
        };

        var sessionId = _posApiService.TransactionRequest(request);
        BeginSession(sessionId);
    }
}
```

### 6.8.3. <a name="Void">Void</a>
Void a purchase before settlement so no funds leave the cardholder's account. A transaction cannot be voided post settlement and will need to be refunded.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void Void()
    {
        var request = new VoidRequest
        {
            Amount = 2995, // void $29.95 transaction
            TxnRef = "123"
        };

        var sessionId = _posApiService.TransactionRequest(request);
        BeginSession(sessionId);
    }
}
```

### 6.8.4. <a name="CashOut">Cash out</a>
Withdraw cash from a cardholder's account.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void CashOut()
    {
        var request = new CashRequest
        {
            AmountCash = 20000, // $200.00 cash out
            TxnRef = "123"
        };

        var sessionId = _posApiService.TransactionRequest(request);
        BeginSession(sessionId);
    }
}
```

### 6.8.5. <a name="Deposit">Deposit</a>
Deposit funds into a cardholder's account.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void Deposit()
    {
        var request = new DepositRequest
        {
            AmountCash = 5000, // $50.00 cash deposit
            AmountCheque = 250000, // $2500.00 total cheque deposit
            TotalCheques = 1, // 1 cheque deposited
            TxnRef = "123"
        };

        var sessionId = _posApiService.TransactionRequest(request);
        BeginSession(sessionId);
    }
}
```

### 6.8.6. <a name="PreAuth">Pre-authorisation</a>
Place a hold on the cardholder's account for `PreAuthRequest.Amount` cents. The request can later be [completed](#PreAuthComplete) to capture that amount into the merchant's account. It is important to note that for a pre-authorisation, a RFN (reference number) will be returned by the [`IPosApiListener.TransactionComplete()`](#TransactionComplete) listener in the `TransactionResponse.RFN` property. This must be used to make follow up transactions on the pre-authorisation and should be stored if the POS is to support this functionality.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void PreAuth()
    {
        var request = new PreAuthRequest
        {
            Amount = 50000, // $500 pre-auth
            TxnRef = "123"
        };

        var sessionId = _posApiService.TransactionRequest(request);
        BeginSession(sessionId);
    }
}
```

### 6.8.7. <a name="PreAuthExtend">Extend pre-authorisation</a>
Pre-authorisation are only valid for a fixed time period (typically 5 days). This operation allows an active pre-authorisation to be extended using the `RFN` from the original pre-authorisation response.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void PreAuthExtend()
    {
        var request = new PreAuthExtendRequest
        {
            RFN = "456", // Must match the RFN from the original pre-auth response
            TxnRef = "123"
        };

        var sessionId = _posApiService.TransactionRequest(request);
        BeginSession(sessionId);
    }
}
```

### 6.8.8. <a name="PreAuthTopUp">Top-up pre-authorisation</a>
Increase the pre-authorisation amount using the `RFN` from the original pre-authorisation response.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void PreAuthTopUp()
    {
        var request = new PreAuthTopUpRequest
        {
            Amount = 9995, // increase the original pre-auth by $99.95
            RFN = "456", // Must match the RFN from the original pre-auth response
            TxnRef = "123"
        };

        var sessionId = _posApiService.TransactionRequest(request);
        BeginSession(sessionId);
    }
}
```

### 6.8.9. <a name="PreAuthCancel">Cancel pre-authorisation</a>
Fully cancel a pre-authorisation using the `RFN` from the original pre-authorisation response.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void PreAuthCancel()
    {
        var request = new PreAuthCancelRequest
        {
            RFN = "456", // Must match the RFN from the original pre-auth response
            TxnRef = "123"
        };

        var sessionId = _posApiService.TransactionRequest(request);
        BeginSession(sessionId);
    }
}
```

### 6.8.10. <a name="PreAuthPartialCancel">Partially cancel pre-authorisation</a>
Reduce the pre-authorisation amount using the `RFN` from the original pre-authorisation response.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void PreAuthPartialCancel()
    {
        var request = new PreAuthPartialCancelRequest
        {
            Amount = 20000, // reduce the amount of the original pre-auth by $200.00
            RFN = "456", // Must match the RFN from the original pre-auth response
            TxnRef = "123"
        };

        var sessionId = _posApiService.TransactionRequest(request);
        BeginSession(sessionId);
    }
}
```

### 6.8.11. <a name="PreAuthComplete">Complete pre-authorisation</a>
Capture the amount (specified in the request) from the cardholder's account. The amount cannot exceed the pre-authorisation amount (taking into account top-ups and partial cancellations). This request must include the `RFN` from the original pre-authorisation response.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void PreAuthCompletion()
    {
        var request = new PreAuthCompletionRequest()
        {
            Amount = 10000, // capture $100.00 from the pre-auth.
            RFN = "456", // Must match the RFN from the original pre-auth response
            TxnRef = "123"
        };

        var sessionId = _posApiService.TransactionRequest(request);
        BeginSession(sessionId);
    }
}
```

### 6.8.12. <a name="PreAuthInquiry">Pre-authorisation inquiry</a>
Verify a pre-authorisation using the `RFN` from the original pre-authorisation response. To check the result of this request look at the `TransactionResponse.AMT` property in the [`IPosApiEventListener.TransactionComplete()`](#TransactionComplete) listener, which should contain the amount currently pre-authorised.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void PreAuthInquiry()
    {
        var request = new PreAuthInquiryRequest
        {
            RFN = "456", // Must match the RFN from the original pre-auth response
            TxnRef = "123"
        };

        var sessionId = _posApiService.TransactionRequest(request);
        BeginSession(sessionId);
    }
}
```

### 6.8.13. <a name="PreAuthSummary">Pre-authorisation summary</a>
Perform an inquiry on all existing pre-authorisations using the `RFN` from the original pre-authorisation response. To check the result of this request look at the `TransactionResponse.PurchaseAnalysisData["PAS"]` key in the [`IPosApiEventListener.TransactionComplete()`](#TransactionComplete) listener.


```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void PreAuthSummary()
    {
        var request = new PreAuthSummaryRequest
        {
            PreAuthIndex = 1, // summary window index to request
            TxnRef = "123"
        };

        var sessionId = _posApiService.TransactionRequest(request);
        BeginSession(sessionId);
    }
}
```

## 6.9. <a name="TransactionComplete">Handling the transaction response</a>
The `IPosApiEventListener.TransactionComplete()` listener is used to retrieve the response from a transaction request. In the following example, details from the transaction response are displayed on the POS. At the end of the method, `CompleteSession()` is invoked, which indicates to the POS that the session has successfully completed and there is no need to attempt [recovery](#SessionRecovery) if the POS hardware or software encounters a failure.

Some fields in the `TransactionResponse` such as `RFN` may need to be saved to facilitate follow-up transactions on pre-authorisations.

```cs
namespace Linkly.PosApi.Sdk.Examples.EventListeners;

internal partial class SampleEventListener : IPosApiEventListener
{
    public void TransactionComplete(Guid sessionId, TransactionRequest request, TransactionResponse response)
    {
        // Display transaction details to the POS. Note the response.RFN may need to be stored
        _posInterface.DisplayMessage(
            $"Response: {response.ResponseText}",
            $"Account Type: {response.AccountType}",
            $"Sale Amount: {response.Amount}",
            $"Surcharge: {response.AmountSurcharge}",
            $"Tip: {response.AmountTip}",
            $"Cash Out: {response.AmountCash}",
            $"Total Amount (inc tip and surcharge): {response.AmountTotal}",
            $"Cash Out: {response.AmountCash}",
            $"Card Type: {response.CardType}",
            $"Txn Type: {response.TxnType}",
            $"RFN: {response.RFN}",
            $"Receipts: {string.Join(Environment.NewLine, response.Receipts)}");

        // Tell the POS the sessionId completed successfully
        _posInterface.CompleteSession(sessionId);
    }
}
```

## 6.10. <a name="RetrieveTransaction">Retrieve historical transaction</a>
Retrieve the transaction details of past transactions using either the `TransactionRequest.TxnRef` or `TransactionRequest.RRN`.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void RetrieveTransaction()
    {
        var request = new RetrieveTransactionRequest { ReferenceType = ReferenceType.ReferenceNo, Reference = "1234" };

        _posApiService.RetrieveTransactionRequest(request);
    }
}
```

Display the `TransactionResponse.ResponseText` of all transactions which matched the criteria in the request. In a real POS implementation, The listener will likely use more of the available fields in the `TransactionResponse`.

```cs
namespace Linkly.PosApi.Sdk.Examples.EventListeners;

internal partial class SampleEventListener : IPosApiEventListener
{
    public void RetrieveTransactionComplete(RetrieveTransactionRequest request, ICollection<TransactionResponse> responses)
    {
        foreach (var response in responses)
        {
            _posInterface.DisplayMessage($"Response: {response.ResponseText}");
            // Display additional details of last transaction
            // ...
        }
    }
}
```

## 6.11. <a name="DisplayListener">Handling display events</a>
Display events are triggered when the PIN pad displays a prompt, for example `SWIPE CARD` or `ENTER PIN`. Display events may simply be echoed on the POS UI or further logic could be triggered based on the content of the message. In the following example the `DisplayResponse.DisplayText` is echoed in the POS UI.

```cs
namespace Linkly.PosApi.Sdk.Examples.EventListeners;

internal partial class SampleEventListener : IPosApiEventListener
{
    public void Display(Guid sessionId, PosApiRequest request, DisplayResponse response)
    {
        // Display the same message as the PIN pad on the POS
        _posInterface.DisplayPrompt(response.DisplayText);
    }
}
```

## 6.12. <a name="ReceiptListener">Handling receipt events</a>
Receipt events are triggered when the PIN pad prints a receipt. In the following example the `ReceiptResponse.ReceiptText` is displayed in the POS UI.

```cs
namespace Linkly.PosApi.Sdk.Examples.EventListeners;

internal partial class SampleEventListener : IPosApiEventListener
{
    public void Receipt(Guid sessionId, PosApiRequest request, ReceiptResponse response)
    {
        _posInterface.DisplayReceipt(response.ReceiptText);
    }
}
```

## 6.13. <a name="ErrorListener">Handling error events</a>
The error listener is triggered when requests fail at the HTTP or application layer, such as if there is a connectivity issue to the API, or authentication fails due to invalid credentials. Errors should be logged to help debug any issues with the POS or SDK. Some errors may be displayed to the UI, such as failed authentication (which would likely indicate user error).

```cs
using System.Net;

namespace Linkly.PosApi.Sdk.Examples.EventListeners;

internal partial class SampleEventListener : IPosApiEventListener
{
    public void Error(Guid? sessionId, IBaseRequest request, ErrorResponse error)
    {
        if (error.Source == ErrorSource.API)
        {
            // Log the error so there is a record of the issue
            _logger.LogError("HTTP Status: {HttpStatusCode}, Message: {Message}", error.HttpStatusCode, error.Message);
            
            // Errors from the API means we can check the HttpStatusCode.
            if (error.HttpStatusCode == HttpStatusCode.Unauthorized)
            {
                _posInterface.DisplayError("Authorisation failed");
                return;
            }
        }
        else if (error.Source == ErrorSource.Internal)
        {
            // Log the exception so there is a record of the issue
            _logger.LogError(error.Exception, "Exception thrown in API: {Message}", error.Message);
        }
        // Display a generic error message to the user
        _posInterface.DisplayError("Something went wrong. Please try again later.");
    }
}
```

# 7. <a name="SessionRecovery">Session recovery</a>
It may be a requirement of the POS to be able to continue a transaction if the POS hardware or software crashes. For example, imagine a transaction is in flight to the bank and the POS crashes. When it restarts the transaction response would be lost unless a recovery mechanism is implemented.

To implement recovery, the POS has to be able to determine when it has crashed. The implementation details of this have been left out of scope of this document, however the basic idea is `IPos` implements several methods. `BeginSession()`, `CompleteSession()` and `TryGetLastIncompleteSession()` which would probably save and load the GUID from the current transaction request using persistent storage.

An example of `BeginSession()` being invoked is in all of the [transaction requests](#TransactionRequests) where this method is called with the session ID of the current transaction.

In the [transaction completion listener](#TransactionComplete), `CompleteSession()` is invoked with the session ID from the response. This marks the session as completed such that session recovery would not be required if the POS crashed at this point.

The following `RecoverSession()` method is called [when the POS initialises](#InitialiseSdk). `TryGetLastIncompleteSession()` checks whether there is an incomplete session and if there is `IPosApiService.ResultRequest()` is invoked, which retrieves all of the events belonging to the session.

```cs
namespace Linkly.PosApi.Sdk.Examples.Pos;

internal abstract partial class SamplePos : IPos
{
    public void RecoverSession()
    {
        if (TryGetLastIncompleteSession(out var sessionId))
        {
            var request = new ResultRequest(sessionId);

            _posApiService.ResultRequest(request);

            // Mark the session as completed so recovery does not run again in an infinite loop.
            CompleteSession(sessionId);
        }
    }
}
```

The following `IPosApiEventListener.ResultComplete()` listener handles the `IPosApiService.ResultRequest()` response.

All `DisplayResponse` and `ReceiptResponse` events are displayed in the POS UI in sequence. If a `TransactionResponse` is encountered, the `TransactionResponse.ResponseText` will be displayed in the POS UI.

```cs
namespace Linkly.PosApi.Sdk.Examples.EventListeners;

internal partial class SampleEventListener : IPosApiEventListener
{
    public void ResultComplete(ResultRequest request, ICollection<PosApiResponse> responses)
    {
        foreach (var response in responses)
        {
            if (response is DisplayResponse displayResponse)
            {
                // Display last PIN pad message. Could be a SWIPE CARD prompt (for example).
                _posInterface.DisplayPrompt(displayResponse.DisplayText);
            }
            else if (response is ReceiptResponse receiptResponse)
            {
                _posInterface.DisplayReceipt(receiptResponse.ReceiptText);
            }
            else if (response is TransactionResponse transactionResponse)
            {
                _posInterface.DisplayMessage($"Response: {transactionResponse.ResponseText}");
                // Display additional details of last transaction if wanted
                // ...
            }
            // Optionally check and handle other response types.
        }
    }
}
```

# 8. <a name="POSConfigurations">More complex POS-PIN Pad configurations</a>
The examples provided assume a 1:1 mapping between POS and PIN pad, however in reality a POS may need to manage multiple PIN pads, or multiple POS applications may need to share a pool of PIN pads. These configurations can be supported with some caveats which will be mentioned.

## 8.1. <a name="PinPadPool">Pool of PIN pads shared by one POS</a>
If a single POS needs to manage a pool of PIN pads, the POS will need to manage multiple instances of the SDK - one per PIN pad. Each SDK instance would be paired to it's own PIN pad.

## 8.2. <a name="POSPool">POS pool</a>
If a pool of POS' share access to a single PIN pad the POS' need to make sure they do not attempt to send requests to a PIN pad which is currently handling a request from another POS. Synchronisation across isolated POS processes is probably impractical so the most feasible method of dealing with resource contention is error handling.

When a request is sent to a PIN pad which is busy handling another request, the PIN pad will return a `BY` (PINPAD BUSY) `ResponseCode` in the listener's response model. The response code can be checked and an appropriate message can be displayed on the POS.

```cs
if (response.ResponseCode == "BY")
    // show an appropriate error message
```

This check is only required for the following listeners: `ConfigureMerchantComplete`, `QueryCardComplete`, `ReprintReceiptComplete`, `SettlementComplete`, `StatusComplete`, `TransactionComplete`.