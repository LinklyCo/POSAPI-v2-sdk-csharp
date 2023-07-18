using System;

namespace Linkly.PosApi.Sdk.Service
{
    /// <summary><see cref="PosApiService" /> options.</summary>
    public class PosApiServiceOptions
    {
        /// <summary>
        /// How long should an async request wait for a response before timing out. There should not be any
        /// reason to change this value.
        /// </summary>
        public TimeSpan AsyncRequestTimeout { get; set; } = TimeSpan.FromMinutes(10);
    }
}