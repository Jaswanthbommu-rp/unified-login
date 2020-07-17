using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
    /// <summary>
    /// The Unified Login instance of the property
    /// </summary>
    public class UPFMPropertyInstance
    {
        public int PropertyInstanceId { get; set; }
        public string Name { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string County { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public Guid InstanceId { get; set; }

        public bool IsAssigned { get; set; }

    }
}
