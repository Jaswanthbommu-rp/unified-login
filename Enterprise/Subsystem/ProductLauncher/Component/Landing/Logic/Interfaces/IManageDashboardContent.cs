using System;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
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