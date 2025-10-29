using System;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	/// <summary>
	/// 
	/// </summary>	
	public interface IManageDashboardContent
	{
        /// <summary>
        /// 
        /// </summary>	
        DashboardElementResponse GetDashboardElementResponse(Guid realPageId, Persona persona);

    }
}