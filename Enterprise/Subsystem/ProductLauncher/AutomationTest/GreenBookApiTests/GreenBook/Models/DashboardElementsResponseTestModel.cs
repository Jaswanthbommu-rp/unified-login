using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Collections.Generic;

namespace GreenBook.Models
{
	/// <summary>
	/// DashboardElementsResponseTestModel class to be user
	/// as Model class for GET Dashboard API Testing.
	/// </summary>
	public class DashboardElementsResponseTestModel : DashboardElementResponse
	{
		public DashboardElementsTestModel dashboardElements { get; set; }
	}
	public class DashboardElementsTestModel : DashboardElements
	{
		public ProfileDetailTestModel profileDetail { get; set; }
		public List<TrainingAchievement> trainingAchievements { get; set; }
		public List<PersonaProductUserDetails> resources { get; set; }
	}

	public class ProfileDetailTestModel : ProfileDetail
	{
		public UserLogin userLogin { get; set; }
		public List<OrganizationTestModel> organization { get; set; }
		public List<CommonAddress> contactMechanism { get; set; }
		public List<PersonaProductUserDetails> assignedProducts { get; set; }
		public List<TelecommunicationNumber> telecommunicationNumber { get; set; }
	}

	public class OrganizationTestModel : Organization
	{
		public PartyRelationshipTestModel partyRelationship { get; set; }
	}

	public class PartyRelationshipTestModel : PartyRelationship
	{
		public RoleType roleTypeFrom { get; set; }
		public RoleType roleTypeTo { get; set; }
		public RelationshipType partyRelationshipType { get; set; }
	}
}
