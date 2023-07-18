using Linkly.PosApi.Sdk.Models;
using Linkly.PosApi.Sdk.Models.Transaction;
using Linkly.PosApi.Sdk.UnitTest.Common;

namespace Linkly.PosApi.Sdk.UnitTest.TestData;

internal sealed class PurchaseRequestNegativeTestDataBuilder : TestDataBuilder<PurchaseRequest>
{
    public PurchaseRequestNegativeTestDataBuilder() : base(RequestRepository.GetPurchaseRequest)
    {
        Include<TransactionRequestNegativeTestDataBuilder>();

        Add(req => req.Amount, -1);
        Add(req => req.Amount, 0);
        Add(req => req.Amount, TC.ExceededAmount);
        Add(req => req.AmountCash, -1);
        Add(req => req.AmountCash, TC.ExceededAmount);
    }
}

internal sealed class RefundRequestNegativeTestDataBuilder : TestDataBuilder<RefundRequest>
{
    public RefundRequestNegativeTestDataBuilder() : base(RequestRepository.GetRefundRequest)
    {
        Include<TransactionRequestNegativeTestDataBuilder>();

        Add(req => req.Amount, -1);
        Add(req => req.Amount, 0);
        Add(req => req.Amount, TC.ExceededAmount);
    }
}

internal sealed class CashRequestNegativeTestDataBuilder : TestDataBuilder<CashRequest>
{
    public CashRequestNegativeTestDataBuilder() : base(RequestRepository.GetCashRequest)
    {
        Include<TransactionRequestNegativeTestDataBuilder>();

        Add(req => req.AmountCash, -1);
        Add(req => req.AmountCash, 0);
        Add(req => req.AmountCash, TC.ExceededAmount);
    }
}

internal sealed class DepositRequestNegativeTestDataBuilder : TestDataBuilder<DepositRequest>
{
    public DepositRequestNegativeTestDataBuilder() : base(RequestRepository.GetDepositRequest)
    {
        Include<TransactionRequestNegativeTestDataBuilder>();

        Add(req => req.AmountCash, -1);
        Add(req => req.AmountCash, TC.ExceededAmount);
        Add(req => req.AmountCheque, -1);
        Add(req => req.AmountCheque, TC.ExceededAmount);
        Add(req => req.AmountCheque = 0, req => req.AmountCash, 0);
        Add(req => req.AmountCash = 0, req => req.AmountCheque, 0);
    }
}

internal sealed class VoidRequestNegativeTestDataBuilder : TestDataBuilder<VoidRequest>
{
    public VoidRequestNegativeTestDataBuilder() : base(RequestRepository.GetVoidRequest)
    {
        Include<TransactionRequestNegativeTestDataBuilder>();
    }
}

internal sealed class PreAuthRequestNegativeTestDataBuilder : TestDataBuilder<PreAuthRequest>
{
    public PreAuthRequestNegativeTestDataBuilder() : base(RequestRepository.GetPreAuthRequest)
    {
        Include<TransactionRequestNegativeTestDataBuilder>();

        Add(req => req.Amount, -1);
        Add(req => req.Amount, TC.ExceededAmount);
    }
}

internal sealed class PreAuthCancelRequestNegativeTestDataBuilder : TestDataBuilder<PreAuthCancelRequest>
{
    public PreAuthCancelRequestNegativeTestDataBuilder() : base(RequestRepository.GetPreAuthCancelRequest)
    {
        Include<FollowUpTransactionRequestNegativeTestDataBuilder>();
    }
}

internal sealed class PreAuthCompletionRequestNegativeTestDataBuilder : TestDataBuilder<PreAuthCompletionRequest>
{
    public PreAuthCompletionRequestNegativeTestDataBuilder() : base(RequestRepository.GetPreAuthCompletionRequest)
    {
        Include<PreAuthManipulationRequestNegativeTestDataBuilder>();
    }
}

internal sealed class PreAuthExtendRequestNegativeTestDataBuilder : TestDataBuilder<PreAuthExtendRequest>
{
    public PreAuthExtendRequestNegativeTestDataBuilder() : base(RequestRepository.GetPreAuthExtendRequest)
    {
        Include<FollowUpTransactionRequestNegativeTestDataBuilder>();
    }
}

internal sealed class PreAuthTopUpNegativeTestDataBuilder : TestDataBuilder<PreAuthTopUpRequest>
{
    public PreAuthTopUpNegativeTestDataBuilder() : base(RequestRepository.GetPreAuthTopUpRequest)
    {
        Include<PreAuthManipulationRequestNegativeTestDataBuilder>();
    }
}

internal sealed class PreAuthPartialCancelRequestNegativeTestDataBuilder : TestDataBuilder<PreAuthPartialCancelRequest>
{
    public PreAuthPartialCancelRequestNegativeTestDataBuilder() : base(RequestRepository.GetPreAuthPartialCancelRequest)
    {
        Include<PreAuthManipulationRequestNegativeTestDataBuilder>();
    }
}

internal sealed class PreAuthSummaryRequestNegativeTestDataBuilder : TestDataBuilder<PreAuthSummaryRequest>
{
    public PreAuthSummaryRequestNegativeTestDataBuilder() : base(RequestRepository.GetPreAuthSummaryRequest)
    {
        Include<TransactionRequestNegativeTestDataBuilder>();
    }
}

internal sealed class PreAuthInquiryRequestNegativeTestDataBuilder : TestDataBuilder<PreAuthInquiryRequest>
{
    public PreAuthInquiryRequestNegativeTestDataBuilder() : base(RequestRepository.GetPreAuthInquiryRequest)
    {
        Include<FollowUpTransactionRequestNegativeTestDataBuilder>();
    }
}

internal sealed class TransactionRequestNegativeTestDataBuilder : TestDataBuilder<TransactionRequest>
{
    public TransactionRequestNegativeTestDataBuilder(Func<TransactionRequest> getDefaultTestData) : base(getDefaultTestData)
    {
        Include<PosApiRequestNegativeTestDataBuilder>();

        Add(req => req.CurrencyCode, "AU");
        Add(req => req.CurrencyCode, "AUSD");
        Add(req => req.TxnRef, new string('0', 33));
        Add(req => req.AuthCode, -1);
        Add(req => req.AuthCode, 1000000);
        Add(req => req.PanSource, (PanSource)(-1));
        Add(req => req.PanSource, (PanSource)int.MaxValue);
        Add(req => req.PAN, new string('0', 21));

        // This custom CardExpiryDate validator is already unit tested so need need to go crazy
        Add(req => req.DateExpiry, "0000");
        Add(req => req.DateExpiry, "1300");

        Add(req => req.Track2, new string('0', 41));
        Add(req => req.AccountType, (AccountType)(-1));
        Add(req => req.AccountType, (AccountType)int.MaxValue);
        Add(req => req.PanSource = PanSource.PosKeyed, req => req.PAN, null!);
        Add(req => req.PanSource = PanSource.PosKeyed, req => req.PAN, "");
        Add(req => req.PanSource = PanSource.PosKeyed, req => req.PAN, " ");
        Add(req => req.PanSource = PanSource.PosKeyed, req => req.DateExpiry, null!);
        Add(req => req.PanSource = PanSource.PosKeyed, req => req.DateExpiry, "");
        Add(req => req.PanSource = PanSource.PosKeyed, req => req.DateExpiry, " ");
        Add(req => req.PanSource = PanSource.PosSwiped, req => req.Track2, null!);
        Add(req => req.PanSource = PanSource.PosSwiped, req => req.Track2, "");
        Add(req => req.PanSource = PanSource.PosSwiped, req => req.Track2, " ");
    }
}

internal sealed class FollowUpTransactionRequestNegativeTestDataBuilder : TestDataBuilder<FollowUpTransactionRequest>
{
    public FollowUpTransactionRequestNegativeTestDataBuilder(Func<FollowUpTransactionRequest> getDefaultTestData) : base(getDefaultTestData)
    {
        Include<TransactionRequestNegativeTestDataBuilder>();

        Add(req => req.PurchaseAnalysisData, new Dictionary<string, string> { ["RFN"] = null! });
        Add(req => req.PurchaseAnalysisData, new Dictionary<string, string> { ["RFN"] = "" });
    }
}

internal sealed class PreAuthManipulationRequestNegativeTestDataBuilder : TestDataBuilder<PreAuthManipulationRequest>
{
    public PreAuthManipulationRequestNegativeTestDataBuilder(Func<PreAuthManipulationRequest> getDefaultTestData) : base(getDefaultTestData)
    {
        Include<FollowUpTransactionRequestNegativeTestDataBuilder>();

        Add(req => req.Amount, 0);
        Add(req => req.Amount, TC.ExceededAmount);
    }
}