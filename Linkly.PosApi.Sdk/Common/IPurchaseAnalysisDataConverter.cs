namespace Linkly.PosApi.Sdk.Common
{
    /// <summary>
    /// Interface for classes which need to be able to serialise into a PAD string.
    /// </summary>
    internal interface IPurchaseAnalysisDataConverter
    {
        /// <summary>Convert the model to a purchase analysis data string conforming to the expected format.</summary>
        /// <returns>PAD string.</returns>
        string ToPadString();
    }
}