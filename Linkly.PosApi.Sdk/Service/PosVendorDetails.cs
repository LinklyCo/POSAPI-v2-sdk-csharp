using System;

namespace Linkly.PosApi.Sdk.Service
{
    /// <summary>
    /// Used to identify the POS vendor with Linkly systems.
    /// </summary>
    public class PosVendorDetails
    {
        public readonly Guid PosId;
        public readonly string PosName;
        public readonly Guid PosVendorId;
        public readonly string PosVersion;

        /// <summary>
        /// Initialise a new instance.
        /// </summary>
        /// <param name="posName">Name of the POS vendor.</param>
        /// <param name="posVersion">POS software version.</param>
        /// <param name="posId">Persistent and unique POS instance identifier. Must differ across separate
        /// deployments of the same POS application.</param>
        /// <param name="posVendorId">Persistent and unique POS product identifier. All instances of the
        /// POS application should provide the same UUID.</param>
        /// <exception cref="ArgumentException"></exception>
        public PosVendorDetails(string posName, string posVersion, Guid posId, Guid posVendorId)
        {
            if (string.IsNullOrWhiteSpace(posName))
                throw new ArgumentException("Required", nameof(posName));

            if (string.IsNullOrWhiteSpace(posVersion))
                throw new ArgumentException("Required", nameof(posVersion));

            PosName = posName;
            PosVersion = posVersion;
            PosId = posId;
            PosVendorId = posVendorId;
        }
    }
}