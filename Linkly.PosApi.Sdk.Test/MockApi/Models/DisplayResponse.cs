namespace Linkly.PosApi.Sdk.UnitTest.MockApi.Models;

internal class DisplayResponse : PosApiResponse
{
    public int NumberOfLines { get; set; }

    public int LineLength { get; set; }

    public List<string> DisplayText { get; set; } = new();

    public bool CancelKeyFlag { get; set; }

    public bool AcceptYesKeyFlag { get; set; }

    public bool DeclineNoKeyFlag { get; set; }

    public bool AuthoriseKeyFlag { get; set; }

    public bool OkKeyFlag { get; set; }

    public string InputType { get; set; } = null!;

    public string GraphicCode { get; set; } = null!;

    public Dictionary<string, string> PurchaseAnalysisData { get; set; } = new();
}