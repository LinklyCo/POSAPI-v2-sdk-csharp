using Linkly.PosApi.Sdk.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Linkly.PosApi.Sdk.DemoPos.Utils;

internal class SessionData
{
    public ReceiptAutoPrint ReceiptAutoPrint { get; set; } = ReceiptAutoPrint.POS;
    public bool CutReceipt { get; set; }
    public Guid AppGuid { get; set; } = Guid.NewGuid();
    public List<Pad> Pads { get; set; } = new ();
    public List<SurchargeRates> SurchargeRates { get; set; } = new ();
    public List<Lane> Lanes { get; set; } = new ();
}

internal class Lane
{
    public string Username { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;

    public string AuthEndpoint { get; set; } = "https://auth.sandbox.cloud.pceftpos.com";
    public string PosEndpoint { get; set; } = "https://rest.pos.sandbox.cloud.pceftpos.com";
    public bool LastActive { get; set; } = true;
    public IList<TransactionSessions> Transactions { get; set; } = new List<TransactionSessions>();
}

internal class TransactionSessions
{
    public string? Type { get; set; }
    public Guid SessionId { get; set; }
    public string? RequestTimestamp { get; set; }
    public string? ResponseTimestamp { get; set; }
    public object? Request { get; set; } 
    public object? Response { get; set; }
    public ErrorResponse? Error { get; set; }
}

internal class Pad : IDataErrorInfo
{
    public Pad()
    {
    }

    public string Error => string.Concat(this[Tag], " ", this[Value]);

    public string this[string columnName]
    {
        get
        {
            string errorMessage = string.Empty;
            switch (columnName)
            {
                case nameof(Tag):
                    if (string.IsNullOrWhiteSpace(Tag))
                        errorMessage = $"{nameof(Tag)} is required.";
                    break;
                case nameof(Value):
                    if (string.IsNullOrWhiteSpace(Value))
                        errorMessage = $"{nameof(Value)} is required.";
                    break;
            }
            return errorMessage;
        }
    }

    public bool Use { get; set; }
    public string Tag { get; set; } = "tag";
    public string Value { get; set; } = "value";
}

internal class SurchargeRates : IDataErrorInfo
{
    public SurchargeRates()
    {
    }

    [JsonIgnore]
    public string Error => string.Concat(this[Bin], " ", this[Value]);

    public string this[string columnName]
    {
        get
        {
            string errorMessage = string.Empty;
            switch (columnName)
            {
                case nameof(Bin):
                    if (string.IsNullOrWhiteSpace(Bin) || !(int.TryParse(Bin, out int bin) && bin > 0))
                        errorMessage = $"{nameof(Bin)} is required.";
                    break;
                case nameof(Value):
                    if (string.IsNullOrWhiteSpace(Value) || !(decimal.TryParse(Value, out decimal value) && value > 0 && value < 100))
                        errorMessage = $"{nameof(Value)} is required.";
                    break;
            }
            return errorMessage;
        }
    }

    public string Type { get; set; } = Constants.Percentage;

    public string Bin { get; set; } = "0";
    public string Value { get; set; } = "0";
}