using System;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class ExternalUserRelationship
    {
        public long UserLoginPersonaId   { get; set; }

        public int ThirdPartyRelationShipId { get; set; }

        public string ThirdPartyRelationShip { get; set; }

        public string ThirdPartyCompanyName { get; set; }

        public Guid? ThirdPartyCompanyRealPageId { get; set; }

        public string OperatorCode { get; set; }
        
        public string OperatorValue { get; set; }
    }
}
