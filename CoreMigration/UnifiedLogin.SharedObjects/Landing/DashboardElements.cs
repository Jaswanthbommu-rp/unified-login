using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Holds all the information found on the dashboard
	/// </summary>	
	public class DashboardElements
	{
		/// <summary>
		/// Profile of the logged in user
		/// </summary>	
		public IProfileDetail ProfileDetail { get; set; }
		
		/// <summary>
		/// Training and achievements of the logged in user
		/// </summary>	
		public IList<TrainingAchievement> TrainingAchievements { get; set; }

		/// <summary>
		/// Resource products that user has access including no access (disabled in UI)
		/// </summary>	
		public IList<PersonaProductUserDetails> Resources { get; set; }
		
	}
}
