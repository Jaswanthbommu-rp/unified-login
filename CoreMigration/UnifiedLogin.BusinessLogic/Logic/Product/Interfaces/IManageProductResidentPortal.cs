using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;

namespace UnifiedLogin.BusinessLogic.Logic.Product.Interfaces
{
	/// <summary>
	/// Interface for Manage Product Resident Portal
	/// </summary>
	public interface IManageProductResidentPortal
	{
		/// <summary>
		/// Get Notification Settings
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>Notifications object</returns>
		Notifications GetNotificationSettings(long editorPersonaId, long userPersonaId);

		/// <summary>
		/// Get Resident Portal Enterprise User Details
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>ResidentPortal object</returns>
		ResidentPortalUser GetUser(long editorPersonaId, long userPersonaId);

		/// <summary>
		/// Include the MessageGroups and Level objects
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">User PersonaId</param>
		/// <param name="residentPortalUser"></param>
		/// <returns>residentPortalUser object</returns>
		IResidentPortalUser SetLevelAndGroupObjects(long editorPersonaId, long userPersonaId, IResidentPortalUser residentPortalUser);

		/// <summary>
		/// List Level
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>Levels list</returns>
		List<ILevel> ListLevels(long editorPersonaId, long userPersonaId);


		/// <summary>
		/// List Messaging Groups
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>Messaging Groups list</returns>
		List<IMessagingGroups> ListMessageGroups(long editorPersonaId, long userPersonaId);

		/// <summary>
		/// Used to list properties  
		/// </summary>
		/// <param name="editorPersonaId">The persona id of the user making the request</param>
		/// <param name="userPersonaId">The persona id of the user being changed</param>
		/// <param name="datafilter"></param>
		/// <returns>ListResponse object</returns>
		ListResponse ListProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter);

		/// <summary>
		/// List Titles
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>Titles list</returns>
		List<ITitle> ListTitles(long editorPersonaId, long userPersonaId);

		/// <summary>
		/// List Resident Portal Enterprise Users Details
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="listSuperUsers">List true = Superusers, false = managers</param>
		/// <returns>List of ResidentPortalEnterpriseUser object</returns>
		IList<ResidentPortalUser> ListUser(long editorPersonaId, bool listSuperUsers = true);

		/// <summary>
		/// Add or update a Resident Portal
		/// 1. SuperUser (enterprise) who has access to all communities for a PMC 
		/// OR
		/// 2. Manager/Staff
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <param name="residentPortal">Used to grant a user level, set the Messaging groups, and Is the Product assigned or removed for the user.</param>
		/// <param name="batchProcessType">batchProcess Type</param>
		/// <param name="additionalParameters"></param>
		/// <returns>ObjectOuput and Error</returns>
		ObjectOutput<IResidentPortalUser, IErrorData> ManageResidentPortalUser(long editorPersonaId, long userPersonaId, ResidentPortal residentPortal, out List<AdditionalParameters> additionalParameters, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser);

		/// <summary>
		/// Unassign User (enterprise or manager)
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>Error object</returns>
		ObjectOutput<IResidentPortalUser, IErrorData> UnassignResidentPortalUser(long editorPersonaId, long userPersonaId);

		/// <summary>
		/// ValidateUserAccess
		/// </summary>
		/// <param name="editorRealpageId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>true or false</returns>
		bool ValidateUserAccess(Guid editorRealpageId, long userPersonaId);

		/// <summary>
		/// ValidateUserAccess
		/// </summary>
		/// <param name="persona">Logged-in user Persona</param>		
		/// <returns>true or false</returns>
		bool ValidateCreateUserAccess(Persona persona);
        
		/// <summary>
        /// List all users
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        ListResponse GetMigrationUsers(long editorPersonaId, RequestParameter datafilter);

        /// <summary>
        /// Update the users migration status
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="migrateUsers"></param>
        /// <returns></returns>
        MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers);


        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="editorPersonaId">The editor persona identifier.</param>
        /// <param name="productUserId">The product user identifier.</param>
        /// <param name="productUsername">The product username.</param>
        /// <returns></returns>
        bool DeleteUser(long editorPersonaId, long productUserId, string productUsername);
    }
}