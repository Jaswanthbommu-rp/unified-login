using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Hots;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
    public class HOTSCloneUserRepository : BaseRepository, IHOTSCloneUserRepository
	{
		#region Constructor
		/// <summary>
		/// Base constructor
		/// </summary>
		public HOTSCloneUserRepository() : base(DbConnectionEnum.IdpConfigurationDb)
		{
		}

		/// <summary>
		/// Unit test constructor
		/// </summary>
		/// <param name="repository"></param>
		public HOTSCloneUserRepository(IRepository repository) : base(repository)
		{
		}
		#endregion
		 
		public IList<BaseLineCustomerCompanyUser> ListUsers (long OrganizationId)
		{
			
			var procName = StoredProcNameConstants.SP_ListHotsBaseOrganizationUsers;

			dynamic param = new
			{
				OrganizationId = OrganizationId
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetMany<BaseLineCustomerCompanyUser>(procName, param);
				return result;
			}			
		}

		public List<PersonaProductUserDetails> GetUserProducts(long personaId)
		{
			var procName = StoredProcNameConstants.SP_ListProductsByPersonaId;

			dynamic param = new
			{
				PersonaId = personaId,
				ProductStatusValue = ((Int32)ProductBatchStatusType.Success).ToString()
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetMany<PersonaProductUserDetails>(procName, param);
				return result;
			}

		}

		public Guid GetBaseCompanyUPFMId (Guid cloneUpfmId)
		{
			var procName = StoredProcNameConstants.SP_GetBaseCompanyUPFMId;
			dynamic param = new
			{
				RealPageId = cloneUpfmId
			};

			using (var repository = GetRepository())
			{
				return repository.GetOne<Guid>(procName, param);				
			}
		}

        private UserLoginOnly getUserLoginOnly(string enterpriseUserName)
        {
            UserLoginOnly userLogin = new UserLoginOnly();
            dynamic param = new { EnterpriseUserName = enterpriseUserName };
			
            using (var repository = GetRepository())
            {
                userLogin = repository.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, param);
            }

            return userLogin;
        }
		public HotsUser CreateUser (long partyId,BaseLineCustomerCompanyUser user, IProfileDetail baseUserProfile, List<ProductBatch> productBatch)
		{
			HotsUser hotsUser = new HotsUser();
			string loginName;
			loginName = getLoginName(partyId, baseUserProfile);

            UserLoginOnly userLoginOnly = getUserLoginOnly(loginName);
            if (userLoginOnly != null)
            {
                hotsUser.BaselineUserId = baseUserProfile.userLogin.UserId;
                hotsUser.BaselineUserName = baseUserProfile.userLogin.LoginName;
                hotsUser.CloneUserId = userLoginOnly.UserId;
                hotsUser.CloneUserName = userLoginOnly.LoginName;
                return hotsUser;
            }
            
            dynamic param = new
			{
				FirstName = baseUserProfile.FirstName,
				MiddleName = baseUserProfile.MiddleName,
				LastName = baseUserProfile.LastName,
				UserTypeId = baseUserProfile.UserTypeId,
				ThirdPartyIDP = baseUserProfile.userLogin.Is3rdPartyIDP,
				LoginName = loginName,
				NotificationEmail = string.Empty,
				UserEffectiveDate = DateTime.UtcNow,
				UserExpirationDate = new DateTime(9999, 12, 31),
				Phone = baseUserProfile.TelecommunicationNumber?.Count > 0 ? baseUserProfile.TelecommunicationNumber[0].PhoneNumber : string.Empty,
				PhoneType = PhoneType.Work.ToString(), // default to work
				PreferredContactMethod = string.Empty,
				Title = baseUserProfile.Title,
				Suffix = baseUserProfile.Suffix,
				Pwdhash = string.Empty,
				PwdSalt = string.Empty,
				CreateUserSourceType = "HOTS",
				OrganizationId = partyId,
				EmployeeId = baseUserProfile.EmployeeId
			};

			using (var repository = GetRepository())
			{
				repository.UnitOfWork.BeginTransaction();
				try
				{
					// user creation
					var spResult = repository.GetOne<dynamic>(EnterpriseStoredProcNameConstants.SP_CreateUnityUser, param);

					// get real page id
					var newUserRealPageId = spResult.RealPageId.ToString();

					// get persona id & user id
					var newUserPersonaId = repository.GetOne<long>(StoredProcNameConstants.SP_GetActivePersona, new { RealPageId = newUserRealPageId });
					var userId = repository.GetOne<long>(StoredProcNameConstants.SP_GetUserLoginOnly, new { RealPageId = newUserRealPageId });
					bool superUser = false;
					superUser = baseUserProfile.UserTypeId == 402;
					// Add products
					if (productBatch?.Count > 0) //TODO: remove UL product from list?
						SaveProductBatch(repository, user.PersonaId, newUserPersonaId, user.UserRealPageId, productBatch, superUser);

					//COMMIT THE CHANGE
					repository.UnitOfWork.Commit();

					hotsUser.BaselineUserId = baseUserProfile.userLogin.UserId;
					hotsUser.BaselineUserName = baseUserProfile.userLogin.LoginName;
					hotsUser.CloneUserId = userId;
					hotsUser.CloneUserName = loginName;
				}
				catch (Exception exception)
				{
					repository.UnitOfWork.Rollback();
					
					return hotsUser;
				}
			}
			return hotsUser;
		}

		/// <summary>
		/// Used to link a cloned company to a baseline company when using HOTS
		/// </summary>
		/// <param name="baselineCompanyRealPageId"></param>
		/// <param name="cloneCompanyRealPageId"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
        public RepositoryResponse InsertHotsCompanyRelationship(Guid baselineCompanyRealPageId, Guid cloneCompanyRealPageId, int userId)
        {
            dynamic param = new
            {
                BaseLineCompany = baselineCompanyRealPageId,
                CloneCompany = cloneCompanyRealPageId,
                UserId = userId
			};

            using (var repository = GetRepository())
            {
                return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertHotsCompanyRelationship, param);
            }
		}

		/// <summary>
        /// Used to link a cloned property to a baseline property when using HOTS
        /// </summary>
        /// <param name="baselinePropertyInstanceId"></param>
        /// <param name="clonePropertyInstanceId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public RepositoryResponse InsertHotsPropertyRelationship(Guid baselinePropertyInstanceId, Guid clonePropertyInstanceId, int userId)
		{
            dynamic param = new
            {
                BaseLineProperty = baselinePropertyInstanceId,
                CloneProperty = clonePropertyInstanceId,
                UserId = userId
            };

            using (var repository = GetRepository())
            {
                return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertHotsPropertyRelationship, param);
            }
        }

		private string getLoginName (long partyId, IProfileDetail baseUserProfile)
		{
			return string.Concat(baseUserProfile.FirstName.Substring(0), baseUserProfile.LastName, partyId.ToString(), "@realpage.com");
		}

		private void SaveProductBatch(IRepository repository, long editorUserPersonaId, long subjectUserPersonaId, Guid editorUserRealPageId,
			IList<ProductBatch> userProductList, bool superUser)
		{
			//var productRepo = new ProductRepository();
			var inputJson = new RolePropertyList() { PropertyRoleList = new List<PropertyRoleList>(), PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = true };

			foreach (var prod in userProductList)
			{
				dynamic productBatch = new
				{
					PersonRealPageId = editorUserRealPageId,
					CreateUserPersonaId = editorUserPersonaId,
					AssignUserPersonaId = subjectUserPersonaId,
					ProductId = prod.ProductId,
					StatusTypeId = 5,
					RetryCount = 0,
					InputJson =  superUser == true ? inputJson :  prod.InputJson,
					CorrelationId = Guid.NewGuid().ToString(),
					BatchProcessTypeId = BatchProcessType.CreateUpdateProductUser
				};

				var repositoryResponse = repository.Execute<dynamic>(StoredProcNameConstants.SP_CreateProductBatch, productBatch);

				//In-case of an error creating a product batch record, append the ProductCode to the ErrorReason
				if (repositoryResponse.Id == 0)
				{
					throw new Exception($"Exception while inserting product with code {prod.ProductId} in the product batch.");
				}
			}
		}

		
	}
}
