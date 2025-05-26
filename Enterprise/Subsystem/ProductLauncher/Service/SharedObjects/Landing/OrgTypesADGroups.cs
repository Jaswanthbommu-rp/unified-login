namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    public class OrgTypesADGroups
    {
        public int OrganizationTypeId { get; set; }

        public string OrganizationTypeName { get; set; }

        public int? ADGroupId { get; set; } = 0;

        public string ADGroupName { get; set; } = null;
    }
}
