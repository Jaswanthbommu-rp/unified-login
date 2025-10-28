using System;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Product
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
        public bool IsActive { get; set; } = true;

        public string CustomerPropertyId { get; set; }

        public string Domain { get; set; }
        public List<int> ProductList { get; set; }

        public string PropertyInstancePartner { get; set; } = null;

        public string PropertyInstancePartnerSourceId { get; set; } = null;

        public Guid ClonePropertyInstanceSourceId { get; set; }
        public long CustomerCompanyId { get; set; }
    }
}
