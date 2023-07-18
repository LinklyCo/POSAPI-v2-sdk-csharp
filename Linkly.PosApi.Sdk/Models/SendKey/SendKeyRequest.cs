using System;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.Results;

namespace Linkly.PosApi.Sdk.Models.SendKey
{
    /// <summary>Send a key to the PIN pad. This request does not have a response.</summary>
    public class SendKeyRequest : PosApiRequest
    {
        /// <summary>Session ID to send the key to.</summary>
        [JsonIgnore]
        public Guid SessionId { get; set; }

        /// <summary>The key to send to the PIN pad.</summary>
        /// <remarks>
        /// <list type="table">
        /// <listheader><term>Number</term><description>Button</description></listheader>
        /// <item>
        /// <term>0</term><description>CANCEL or OK (only one can be displayed at a time)</description>
        /// </item>
        /// <item><term>1</term><description>YES</description></item>
        /// <item><term>2</term><description>NO</description></item>
        /// <item><term>3</term><description>AUTH</description></item>
        /// </list>
        /// </remarks>
        /// <example>0</example>
        public string Key { get; set; } = null!;

        /// <summary>Entry data collected by POS. Maximum length of 60 characters.</summary>
        public string Data { get; set; } = "";

        public override ValidationResult Validate() => new SendKeyRequestValidator().Validate(this);
    }

    public class SendKeyRequestValidator : AbstractValidator<SendKeyRequest>
    {
        public SendKeyRequestValidator()
        {
            Include(new PosApiRequestValidator());
            RuleFor(req => req.SessionId).NotEmpty();
            RuleFor(req => req.Key).NotEmpty();
            RuleFor(req => req.Data).MaximumLength(60);
        }
    }
}