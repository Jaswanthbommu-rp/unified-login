using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    public class EmployeeProductMapping
    {
        public int EmployeeProductMappingId { get; set; }
        public int PersonaId { get; set; }
        public int ProductId { get; set; }
        public int ADGroupId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
