﻿using System;

namespace Linkly.PosApi.Sdk.Models.Authentication
{
    /// <summary>
    /// Obtain a <see cref="Secret" /> token which needs to be used by subsequent API requests. Used
    /// internally for automatic token generation.
    /// </summary>
    internal class TokenRequest : IBaseRequest
    {
        /// <summary>Secret generated by sending a <see cref="PairingRequest" />.</summary>
        public string Secret { get; set; } = null!;

        /// <summary>The name of the POS requesting the token.</summary>
        /// <returns></returns>
        public string PosName { get; set; } = null!;

        /// <summary>The version of the POS requesting the token.</summary>
        public string PosVersion { get; set; } = null!;

        /// <summary>
        /// A unique UUID v4 which identifies the POS instance. This value is generated by the POS as a part of
        /// the POS deployment settings. e.g. Two registers at the same merchant should supply two different
        /// PosId values.
        /// </summary>
        public Guid PosId { get; set; }

        /// <summary>
        /// A unique UUID v4 which identifies the POS product. This value can be hard coded into the build of
        /// the POS. e.g. All merchants using the same POS product should supply the same value.
        /// </summary>
        public Guid PosVendorId { get; set; }
    }
}