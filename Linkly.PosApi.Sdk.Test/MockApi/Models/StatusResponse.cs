namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class StatusResponse : PosApiResponse
{
    public string Merchant { get; set; } = null!;

    public string Aiic { get; set; } = null!;

    public int NII { get; set; }

    public string CatId { get; set; } = null!;

    public string CaId { get; set; } = null!;

    public int Timeout { get; set; }

    public bool LoggedOn { get; set; }

    public string PinPadSerialNumber { get; set; } = null!;

    public string PinPadVersion { get; set; } = null!;

    public string BankCode { get; set; } = null!;

    public string BankDescription { get; set; } = null!;

    public string KVC { get; set; } = null!;

    public int SafCount { get; set; }

    public string NetworkType { get; set; } = null!;

    public string HardwareSerial { get; set; } = null!;

    public string RetailerName { get; set; } = null!;

    public PinPadOptionFlags OptionsFlags { get; set; } = new();

    public int SafCreditLimit { get; set; }

    public int SafDebitLimit { get; set; }

    public int MaxSaf { get; set; }

    public string KeyHandlingScheme { get; set; } = null!;

    public int CashoutLimit { get; set; }

    public int RefundLimit { get; set; }

    public string CpatVersion { get; set; } = null!;

    public string NameTableVersion { get; set; } = null!;

    public string TerminalCommsType { get; set; } = null!;

    public int CardMisreadCount { get; set; }

    public int TotalMemoryInTerminal { get; set; }

    public int FreeMemoryInTerminal { get; set; }

    public string EftTerminalType { get; set; } = null!;

    public int NumAppsInTerminal { get; set; }

    public int NumLinesOnDisplay { get; set; }

    public DateTime HardwareInceptionDate { get; set; }

    public bool Success { get; set; }

    public string ResponseCode { get; set; } = null!;

    public string ResponseText { get; set; } = null!;
}