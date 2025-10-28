using System;

namespace UnifiedLogin.SharedObjects.Landing
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
