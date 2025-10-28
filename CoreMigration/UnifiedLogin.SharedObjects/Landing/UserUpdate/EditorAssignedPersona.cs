using System;

namespace UnifiedLogin.SharedObjects.Landing.UserUpdate
{
    public class EditorAssignedPersona
    {
        public long AssignedPersonaId { get; set; }

        public int AssignedUserTypeId { get; set; }

        public long EditorPersonaId { get; set; }

        public Guid EditorPersonaRealPageId { get; set; }

        public Guid OrganizationRealPageId { get; set; }
    }
}
