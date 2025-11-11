using Newtonsoft.Json;
using UnifiedLogin.DataAccess;
using UnifiedLogin.DataAccess.Helper;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using Dapper;
using UnifiedLogin.SharedObjects.Batch;
using System.Data;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository.Enterprise
{
	/// <summary>
	/// Enterprise User Repository
	/// </summary>
	public class EntUserRepository : BaseRepository, IEntUserRepository
	{
		private DefaultUserClaim _userClaim;

		#region Ctor
		/// <summary>
		/// Ctor
		/// </summary>
		public EntUserRepository(DefaultUserClaim userClaim) : base(DbConnectionEnum.IdpConfigurationDb)
		{
			_userClaim = userClaim;
		}
		#endregion

		/// <summary>
		/// Create Enterprise User
		/// </summary>
		/// <param name="userProductDetails">User Product Details</param>
		/// <returns></returns>
		public string CreateEnterpriseUser(UserProductDetails userProductDetails)
		{

			dynamic param = new
			{
				FirstName = userProductDetails.UserProfileDetails.FirstName,
				MiddleName = userProductDetails.UserProfileDetails.MiddleName,
				LastName = userProductDetails.UserProfileDetails.LastName,
				UserTypeId = userProductDetails.UserProfileDetails.UserType,
				ThirdPartyIDP = userProductDetails.UserProfileDetails.IsExternalIdp,
				LoginName = userProductDetails.UserProfileDetails.LoginName,
				NotificationEmail = userProductDetails.UserProfileDetails.Email,
				UserEffectiveDate = userProductDetails.UserProfileDetails.UserEffectiveDate.Value,
				UserExpirationDate = userProductDetails.UserProfileDetails.UserExpirationDate.Value,
				Phone = userProductDetails.UserProfileDetails.Phone,
				PhoneType = PhoneType.Work.ToString(), // default to work
				PreferredContactMethod = string.Empty,
				Title = userProductDetails.UserProfileDetails.Title,
				Suffix = userProductDetails.UserProfileDetails.Suffix,
				Pwdhash = userProductDetails.UserProfileDetails.PasswordHash,
				PwdSalt = userProductDetails.UserProfileDetails.PasswordSalt,
				CreateUserSourceType = "RPX",
				OrganizationId = userProductDetails.UserProfileDetails.OrganizationPartyId,
				EmployeeId = userProductDetails.UserProfileDetails.EmployeeId
			};

			using (var repository = GetRepository())
			{
				repository.UnitOfWork.BeginTransaction();

				// user creation
				var spResult = repository.GetOne<dynamic>(EnterpriseStoredProcNameConstants.SP_CreateUnityUser, param);

				// get real page id
				var newUserRealPageId = spResult.RealPageId.ToString();

				// get persona id & user id
				var newUserPersonaId = repository.GetOne<long>(StoredProcNameConstants.SP_GetActivePersona, new { RealPageId = newUserRealPageId });
				var userId = repository.GetOne<long>(StoredProcNameConstants.SP_GetUserLoginOnly, new { RealPageId = newUserRealPageId });

                IUserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
                if (_userClaim.ImpersonatedBy != Guid.Empty)
                {
                    impersonatorUserLoginOnly = repository.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, new { RealPageId = _userClaim.ImpersonatedBy });
                }
                // Add products
                if (userProductDetails.ProductList.Any()) //TODO: remove UL product from list?
					SaveProductBatch(repository, _userClaim.PersonaId, newUserPersonaId, _userClaim.UserRealPageGuid, userProductDetails.ProductList, impersonatorUserLoginOnly.UserId);
				
				repository.UnitOfWork.Commit();

				return newUserRealPageId;
			} // Transaction Rollbacks during dispose in case any error
		}

		/// <summary>
		/// Get/List Users
		/// </summary>
		/// <param name="organizationPartyId">Company PartyId</param>
		/// <param name="productIdList">List of product ids</param>
		/// <param name="statusTypeId">Status Type Id</param>
		/// <param name="realPageId">Optional User EnterpriseId</param>
		/// <param name="name">Optional filter by FirstName, LastName, or UserName</param>
		/// <param name="rowsPerPage">Optional Rows Per page to return</param>
		/// <param name="pageNumber">Optional PageNumber</param>
		/// <returns>List of Users (List of 1 if getting a user)</returns>
		public IList<UsersData> ListUsers(long organizationPartyId, IList<int> productIdList, int statusTypeId, Guid? realPageId = null, string name = null, int rowsPerPage = 0, int pageNumber = 1)
		{
			IList<UsersData> usersDataList = new List<UsersData>();

			dynamic param = new
			{
				OrganizationId = organizationPartyId,
				ProductIds = TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype"),
				RealPageId = realPageId,
				StatusTypeId = statusTypeId,
				Name = name,
				RowsPerPage = rowsPerPage,
				PageNumber = pageNumber
			};

			try
			{
				using (var repository = GetRepository())
				{
					usersDataList = repository.GetMany<UsersData>(EnterpriseStoredProcNameConstants.SP_ListUserInformation, param, 0);					
				}
			}
			catch (Exception exception)
			{
			}

			return usersDataList;
		}

		/// <summary>
		/// Get/List Users Product Details Login
		/// </summary>
		/// <param name="PersonaId"></param>
		/// <returns>List of UserProductDetailAttribute</returns>
		public IList<UserProductDetailAttribute> ListUserProductDetailsLoginByPersonaId(long PersonaId)
		{
			try
			{
				dynamic param = new
				{
					PersonaId,
				};

				using (var repository = GetRepository())
				{
					return repository.GetMany<UserProductDetailAttribute>(EnterpriseStoredProcNameConstants.SP_ListUsersProductsDetailsLoginByPersonaId, param);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}


		/// <summary>
		/// Get a list of product login for each company
		/// </summary>
		/// <param name="loginName"></param>
		/// <returns>List of UserProductDetailAttribute</returns>
		public IList<UserProductDetailAttribute> ListUserProductDetailsLoginByLoginName(string loginName)
		{
			try
			{
				dynamic param = new
				{
					loginName,
				};

				using (var repository = GetRepository())
				{
					return repository.GetMany<UserProductDetailAttribute>(EnterpriseStoredProcNameConstants.SP_ListUsersProductsDetailsLoginByLoginName, param);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		private void SaveProductBatch(IRepository repository, long editorUserPersonaId, long subjectUserPersonaId, Guid editorUserRealPageId,
			IList<ProductDetail> userProductList, long impersonatorUserId)
		{
			var productRepo = new ProductRepository();
            var batchGroup = CreateBatchProcessGroup(repository);

			foreach (var prod in userProductList)
			{
				dynamic productBatch = new
				{
					PersonRealPageId = editorUserRealPageId,
					CreateUserPersonaId = editorUserPersonaId,
					AssignUserPersonaId = subjectUserPersonaId,
					ProductId = productRepo.GetBooksMasterProductDetail(prod.ProductCode.ToUpper()).ProductId,
					StatusTypeId = 5,
					RetryCount = 0,
                    BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                    ImpersonatorUserId = impersonatorUserId,
                    InputJson = JsonConvert.SerializeObject(new RolePropertyList()
					{
						PropertyList = prod.PropertiesAssigned,
						RoleList = prod.RolesAssigned,
						RegionList = prod.RegionsAssigned,
						IsAssigned = true
					}),
					//CorrelationId = _userClaim.CorrelationId - token has empty guid so commented
				};

				var repositoryResponse = repository.Execute<dynamic>(StoredProcNameConstants.SP_CreateProductBatch, productBatch);

				//In-case of an error creating a product batch record, append the ProductCode to the ErrorReason
				if (repositoryResponse.Id == 0)
				{
					throw new Exception($"Exception while inserting product with code {prod.ProductCode} in the product batch.");
				}
			}
		}

        private BatchProcessorGroup CreateBatchProcessGroup(IRepository repo)
        {
            {
                DynamicParameters param = new DynamicParameters();
                int groupID = 0;
                param.Add("@BatchProcessorGroupID", groupID, dbType: DbType.Int32, direction: ParameterDirection.Output);

                try
                {
                    var a = repo.Execute(StoredProcNameConstants.SP_CreateBatchProcessorGroup, param);
                    groupID = param.Get<int>("@BatchProcessorGroupID");
                }
                catch (Exception ex)
                {
                }

                return new BatchProcessorGroup()
                {
                    BatchProcessorGroupId = groupID,
                    BatchProcessorGroupActivityLogged = false
                };
            }
        }
    }

	public class RepoObject
	{
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string CompanyJobTitle { get; set; }
		public string Title { get; set; }
		public string Phone { get; set; }
		public DateTime UserExpirationDate { get; set; }
		public DateTime UserEffectiveDate { get; set; }
		public string LastName { get; set; }
		public string GBUserType { get; set; }
		public bool ThirdPartyIDP { get; set; }
		public string LoginName { get; set; }
		public string NotificationEmail { get; set; }
		public string TemporaryPassword { get; set; }
		public string PhoneType { get; set; }
		public string PreferredContactMethod { get; set; }
	}
}
