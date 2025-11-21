using System;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers
{
	/// <summary>
	/// Product Integration related Helpers
	/// DO NOT use else where
	/// </summary>
	public static class ProductActivityLogger
	{
		#region ActivityLog

		/// <summary>
		/// Write Create User activity log
		/// </summary> 
		public static void WriteCreateUserActivityLog(UserDetails editorUserDetails, UserDetails subjectUserDetails, string blueBookProductName, string blueBookProductCode, Guid correlationId)
		{
			WriteActivityLog(editorUserDetails, subjectUserDetails, blueBookProductCode, correlationId,
				$"{subjectUserDetails.FirstName} {subjectUserDetails.LastName} created in product {blueBookProductName} by user {editorUserDetails.FirstName} {editorUserDetails.LastName}.");
		}

		/// <summary>
		/// Write Update User activity log
		/// </summary> 
		public static void WriteUpdateUserActivityLog(UserDetails editorUserDetails, UserDetails subjectUserDetails, string blueBookProductName, string blueBookProductCode, Guid correlationId)
		{
			WriteActivityLog(editorUserDetails, subjectUserDetails, blueBookProductCode, correlationId,
			   $"{subjectUserDetails.FirstName} {subjectUserDetails.LastName} updated in product {blueBookProductName} by user {editorUserDetails.FirstName} {editorUserDetails.LastName}.");
		}

		/// <summary>
		/// Write Update User-Type Activity Log
		/// </summary> 
		public static void WriteUpdateUserTypeActivityLog(UserDetails editorUserDetails, UserDetails subjectUserDetails, string blueBookProductName, string blueBookProductCode, Guid correlationId, BatchProcessType batchProcessType)
		{
			string message = string.Empty;
			if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin)
			{
				message = $"{subjectUserDetails.FirstName} {subjectUserDetails.LastName} user type changed from Regular User to admin in {blueBookProductName} by {editorUserDetails.FirstName} {editorUserDetails.LastName}.";
			}
			else if (batchProcessType == BatchProcessType.UserTypeAdminToRegular)
			{
				message = $"{subjectUserDetails.FirstName} {subjectUserDetails.LastName} user type changed from admin to Regular User in {blueBookProductName} by {editorUserDetails.FirstName} {editorUserDetails.LastName}.";
			}
            else if (batchProcessType == BatchProcessType.UserTypeAdminToExternal)
            {
                message = $"{subjectUserDetails.FirstName} {subjectUserDetails.LastName} user type changed from admin to External User in {blueBookProductName} by {editorUserDetails.FirstName} {editorUserDetails.LastName}.";
            }
            else if (batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
            {
                message = $"{subjectUserDetails.FirstName} {subjectUserDetails.LastName} user type changed from External User to admin user in {blueBookProductName} by {editorUserDetails.FirstName} {editorUserDetails.LastName}.";
            }

            WriteActivityLog(editorUserDetails, subjectUserDetails, blueBookProductCode, correlationId,
				message);
		}

		/// <summary>
		/// Write Unassign User activity log
		/// </summary> 
		public static void WriteUnassignUserActivityLog(UserDetails editorUserDetails, UserDetails subjectUserDetails, string blueBookProductName, string blueBookProductCode, Guid correlationId)
		{
			WriteActivityLog(editorUserDetails, subjectUserDetails, blueBookProductCode, correlationId,
			   $"{subjectUserDetails.FirstName} {subjectUserDetails.LastName} is unassigned in product {blueBookProductName} by user {editorUserDetails.FirstName} {editorUserDetails.LastName}.");
		}

		private static void WriteActivityLog(UserDetails editorUserDetails, UserDetails subjectUserDetails, string blueBookProductCode, Guid correlationId, string message)
		{
			// log product user updated activity
			try
			{
				LogActivity.WriteActivity(new ActivityDetails
				{
					LogActivityTypeName = LogActivityTypeConstants.PRODUCT_ACCESS,
					LogCategoryName = LogActivityCategoryType.ProductAccess.ToString(),
					CorrelationId = correlationId.ToString(),
					BooksMasterOrganizationId = subjectUserDetails.BooksMasterId,
                    OrganizationPartyId = subjectUserDetails.OrganizationPartyId,
					Message = message,

					FromUserLoginName = editorUserDetails.LoginName,
					FromUserLoginId = editorUserDetails.UserId,
					FromUserFirstName = editorUserDetails.FirstName,
					FromUserLastName = editorUserDetails.LastName,
					FromUserRealpageId = editorUserDetails.UserRealPageId.ToString(),

					ToUserLoginId = subjectUserDetails.UserId,
					ToUserLoginName = subjectUserDetails.LoginName,
					ToUserFirstName = subjectUserDetails.FirstName,
					ToUserLastName = subjectUserDetails.LastName,
					ToUserRealpageId = subjectUserDetails.UserRealPageId.ToString(),

					BooksProductCode = blueBookProductCode
				});
			}
			catch (Exception ex)
			{
			}
		}

		#endregion
	}
}

