using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.Results;
using Linkly.PosApi.Sdk.Common;
using Linkly.PosApi.Sdk.Service;

namespace Linkly.PosApi.Sdk.Models
{
    /// <summary>Common model for POS API requests.</summary>
    public abstract class PosApiRequest : IBaseRequest, IValidatable
    {
        /// <summary>Two digit merchant code. Defaults to "00" (EFTPOS).</summary>
        /// <example>00</example>
        public string Merchant { get; set; } = "00";

        /// <summary>Indicates where the request is to be sent to. Defaults to "00" (EFTPOS).</summary>
        /// <example>00</example>
        public string Application { get; set; } = "00";

        /// <summary>Specify how the receipt should be printed.</summary>
        public ReceiptAutoPrint ReceiptAutoPrint { get; set; } = ReceiptAutoPrint.POS;

        /// <summary>Indicates whether EFTPOS should cut receipts.</summary>
        [JsonConverter(typeof(JsonBoolToBitStringConverter))]
        public bool CutReceipt { get; set; }

        /// <summary>Additional data to be sent or received directly from the PIN pad.</summary>
        public IDictionary<string, string> PurchaseAnalysisData { get; set; } = new Dictionary<string, string>();

        /// <summary>POS name. Set by the <see cref="IPosApiService" />.</summary>
        public string PosName { get; internal set; } = null!;

        /// <summary>POS version. Set by the <see cref="IPosApiService" />.</summary>
        public string PosVersion { get; internal set; } = null!;

        public Guid PosId { get; internal set; }

        /// <summary>Wrap the request data in a "Request" json key.</summary>
        public object ToDto() => new { Request = (object)this };

        /// <summary>Validate the model using the fluent validator.</summary>
        /// <returns>Validation result containing model errors (if any)</returns>
        public abstract ValidationResult Validate();
    }

    internal class PosApiRequestValidator : AbstractValidator<PosApiRequest>
    {
        public PosApiRequestValidator()
        {
            RuleFor(req => req.Merchant).NotNull().Length(2);
            RuleFor(req => req.Application).NotNull().Length(2);
            RuleFor(req => req.ReceiptAutoPrint).IsInEnum();
        }
    }
}