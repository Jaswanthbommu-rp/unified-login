using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.OmniChannel;
using Property = UnifiedLogin.SharedObjects.Product.OmniChannel.Property;
using PropertyRole = UnifiedLogin.SharedObjects.Product.OmniChannel.PropertyRole;
using UL = UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using UserAccessGroup = UnifiedLogin.SharedObjects.Product.OmniChannel.UserAccessGroup;
using UserAssignProductPropertyRole = UnifiedLogin.SharedObjects.Product.OmniChannel.UserAssignProductPropertyRole;
using UserLocation = UnifiedLogin.SharedObjects.Product.OmniChannel.UserLocation;

namespace UnifiedLogin.BusinessLogic.Logic.Product
{

    /// <summary>
    /// Used to update OmniCHannel user information
    /// </summary>
    public class ManageProductOmniChannel : ManageProductBase, IManageProductOmniChannel
    {
        #region Private members
        private DefaultUserClaim _userClaims;

        #endregion

        #region Ctor




        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="editorRealPageId">Real page Id of user who is creating new user</param>
        public ManageProductOmniChannel(DefaultUserClaim userClaims) : base((int)ProductEnum.OmniChannel, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
            _userClaims = userClaims;

            WriteToDiagnosticLog("OmniChannel - ManageProductOmniChannel.Ctor - Getting Product settings.");
            _productId = (int)ProductEnum.OmniChannel;
            _editorRealPageId = userClaims.UserRealPageGuid;
            _blueBook = new ManageBlueBook(userClaims);

        }

        #endregion

        #region Public Methods

        #region Properties and Roles



        /// <summary>
        /// Used to get properties  
        /// </summary>
        /// <param name="editorPersonaId">The persona id of the user making the request</param>
        /// <param name="userPersonaId">The persona id of the user being changed</param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            var result = new ListResponse();
            WriteToDiagnosticLog(
              $"OmniChannel - ManageProductOmniChannel.GetProperties - at begining of method for user with editorPersona id - {editorPersonaId}");

            try
            {
                result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO: need to refactor
                if (result.IsError)
                {
                    WriteToErrorLog(
                        $"OmniChannel - ManageProductOmniChannel.GetProperties.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
                    return result;
                }

                int companyInstanceId = GetProductCompanyInstanceId(BlueBookProductConstants.OmniChannel, useTranslate:false).CompanyInstanceId;
                
                WriteToDiagnosticLog($"OmniChannel - - GetProperties-GetProductCompanyInstanceId - Found blue book company instance id - {companyInstanceId}  for user editorPersona id -{editorPersonaId}");

                IList<PropertyInstance> propertyList = _blueBook.GetPropertyInstance(companyInstanceId);
                if (propertyList == null)
                {
                    WriteToErrorLog(
                        $"OmniChannel - ManageProductOmniChannel.GetProperties - GetPropertyInstance - Error looking for propertylist in bluebook for user with company instance id - {companyInstanceId}.");
                    return new ListResponse { IsError = true, ErrorReason = "Property List not found in BlueBook for OmniChannel." };
                }
                WriteToDiagnosticLog($"OmniChannel - ManageProductOmniChannel.GetProperties - GetPropertyInstance - Found total {propertyList.Count} properties with blue book company instance id {companyInstanceId} for user with editorPersona id - {editorPersonaId}.");

                IList<ProductProperty> blueBookPropertyList = propertyList.FromBlueBookToGBProperties() ?? new List<ProductProperty>();
                WriteToDiagnosticLog($"OmniChannel - ManageProductOmniChannel.GetProperties-FromBlueBookToGBProperties() completed for user with editorPersona id -{editorPersonaId}.");

                // need to do a filter on the result
                if (userPersonaId != 0) // Called during updating Existing User
                {
                    WriteToDiagnosticLog(
                        $"OmniChannel - ManageProductOmniChannel.GetProperties- calling MergeProductPropertiesWithGreenbook....for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
                    result = MergeProductPropertiesWithGreenbook(blueBookPropertyList, userPersonaId);
                    WriteToDiagnosticLog(
                         $"OmniChannel - ManageProductOmniChannel.GetProperties-MergeProductPropertiesWithGreenbook completed for user with editorPersona id -{editorPersonaId}.");
                }
                else
                {
                    result = new ListResponse() // Called during creating a new User
                    {
                        Records = blueBookPropertyList.Cast<object>().ToList(),
                        TotalRows = blueBookPropertyList.Count,
                        RowsPerPage = blueBookPropertyList.Count,
                        TotalPages = 1,
                        ErrorReason = string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                result = new ListResponse
                {
                    IsError = true
                };

                if (ex is BlueBookException)
                {
                    result.ErrorReason = ex.Message;
                }
                else
                {
                    result.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
                }

                WriteToErrorLog(
                    $"OmniChannel - ManageProductOmniChannel.GetProperties - There was a problem getting the properties for user with editorPersona id - {editorPersonaId}.",
                    exception: ex);
            }

            return result;
        }


        /// <summary>
        /// Used to get properties  
        /// </summary>
        /// <param name="editorPersonaId">The persona id of the user making the request</param>
        /// <param name="userPersonaId">The persona id of the user being changed</param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetPropertiesByOrganization(long editorPersonaId, long userPersonaId)
        {
            var result = new ListResponse();
            WriteToDiagnosticLog(
              $"OmniChannel - ManageProductOmniChannel.GetProperties - at begining of method for user with editorPersona id - {editorPersonaId}");

            try
            {
                result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO: need to refactor
                if (result.IsError)
                {
                    WriteToErrorLog(
                        $"OmniChannel - ManageProductOmniChannel.GetProperties.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
                    return result;
                }

                int companyInstanceId = GetProductCompanyInstanceId(BlueBookProductConstants.OmniChannel, useTranslate:false).CompanyInstanceId;
                if (companyInstanceId == 0)
                {
                    WriteToErrorLog(
                        $"OmniChannel - ManageProductOmniChannel.GetProperties-GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}.");
                    return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };
                }
                WriteToDiagnosticLog($"OmniChannel - - GetProperties-GetProductCompanyInstanceId - Found blue book company instance id - {companyInstanceId}  for user editorPersona id -{editorPersonaId}");

                //IList<PropertyInstance> propertyList = _blueBook.GetPropertyInstance(companyInstanceId);
                //WriteToDiagnosticLog($"OmniChannel - ManageProductOmniChannel.GetProperties-GetPropertyInstance - Found total {propertyList.Count} properties with blue book company instance id {companyInstanceId} for user with editorPersona id - {editorPersonaId}.");

                IList<ProductProperty> blueBookPropertyList = null; //propertyList.FromBlueBookToGBProperties() ?? new List<ProductProperty>();
                WriteToDiagnosticLog($"OmniChannel - ManageProductOmniChannel.GetProperties-FromBlueBookToGBProperties() completed for user with editorPersona id -{editorPersonaId}.");

                // need to do a filter on the result
                if (userPersonaId != 0) // Called during updating Existing User
                {
                    WriteToDiagnosticLog(
                        $"OmniChannel - ManageProductOmniChannel.GetProperties- calling MergeProductPropertiesWithGreenbook....for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
                    result = MergeProductPropertiesWithGreenbook(blueBookPropertyList, userPersonaId);
                    WriteToDiagnosticLog(
                         $"OmniChannel - ManageProductOmniChannel.GetProperties-MergeProductPropertiesWithGreenbook completed for user with editorPersona id -{editorPersonaId}.");
                }
                else
                {
                    result = new ListResponse() // Called during creating a new User
                    {
                        Records = blueBookPropertyList.Cast<object>().ToList(),
                        TotalRows = blueBookPropertyList.Count,
                        RowsPerPage = blueBookPropertyList.Count,
                        TotalPages = 1,
                        ErrorReason = string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                result = new ListResponse
                {
                    IsError = true
                };

                if (ex is BlueBookException)
                {
                    result.ErrorReason = ex.Message;
                }
                else
                {
                    result.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
                }
                WriteToErrorLog(
                    $"OmniChannel - ManageProductOmniChannel.GetProperties - There was a problem getting the properties for user with editorPersona id - {editorPersonaId}.",
                    exception: ex);
            }

            return result;
        }


        /// <summary>
        /// Returns Roles (User Access Groups in OmniChannel)
        /// </summary>
        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId)
        {
            WriteToDiagnosticLog(
                $"OmniChannel - ManageProductOmniChannel.GetRoles at beginning of method for user with editorPersona id - {editorPersonaId}");

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO:need to refactor
                if (result.IsError)
                {
                    WriteToErrorLog(
                        $"OmniChannel - ManageProductOmniChannel.GetRoles.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
                    return result;
                }

                // get roles from DB for OmniChannel product
                WriteToDiagnosticLog(
                   $"OmniChannel - Getting all GB roles from GB DB - ocr.ListRolesByParty with party id - {partyId}");
                //OmniChannelRepository ocr = new OmniChannelRepository();
                //var gbAllRoles = ocr.ListRolesByParty(partyId);
                int productId = (int)ProductEnum.OmniChannel;
                var productIds = new List<int>(); // GetProductIdsByOrg();

                ProductRepository pr = new ProductRepository();
                IList<int> productIdList = pr.GetProductIdsByCompany(partyId);

                var gbAllRoles = pr.ListRolesForProductByParty(partyId, productIdList, productId);

                WriteToDiagnosticLog(
                    $"OmniChannel - ManageProductOmniChannel.GetRoles.MapProductAccessGroupsToGB() completed for user with editorPersona id - {editorPersonaId}");

                if (userPersonaId != 0) // Called during updating Existing User
                {
                    WriteToDiagnosticLog(
                         $"OmniChannel - ManageProductOmniChannel.GetRoles-MergeAccessGroupsWithGreenbook calling....for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
                    response = MergeSelRolesWithGreenbook(gbAllRoles, userPersonaId);
                    WriteToDiagnosticLog(
                           $"OmniChannel - ManageProductOmniChannel.GetRoles-MergeAccessGroupsWithGreenbook completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
                }
                else // Called during creating a new User
                {
                    response = new ListResponse()
                    {
                        Records = gbAllRoles.Cast<object>().ToList(),
                        TotalRows = gbAllRoles.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                }

            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                WriteToErrorLog($"OmniChannel - ManageProductOmniChannel.GetRoles Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
            }

            return response;
        }


        /// <summary>
        /// Updated to create/update a user in OmniChannel
        /// </summary>
        public string ManageOmniChannelUser(long editorPersonaId, long userPersonaId, UserAssignProductPropertyRole userAssignProductPropertyRole)
        {
            WriteToDiagnosticLog($"OmniChannel - ManageProductOmniChannel.ManageOmniChannelUser - Begin create/update user for user with editorPersona id - {editorPersonaId}.");

            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog($"OmniChannel - ManageProductOmniChannel.ManageOmniChannelUser Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}");
                    return listResponse.ErrorReason;
                }

                var persona = _managePersona.GetPersona(userPersonaId);
                var realPageId = persona.RealPageId;
                //var person = _managePerson.GetPerson(realPageId);
                var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

                // super user
                if (IsSuperUser(userPersonaId))
                {
                    WriteToDiagnosticLog($"OmniChannel - ManageProductOmniChannel.ManageOmniChannelUser - new user is Super user with editorPersona id - {editorPersonaId}.");

                    userAssignProductPropertyRole = new UserAssignProductPropertyRole
                    {
                        PropertyList = new List<string> { "-1" },
                        RoleList = new List<string>()
                    };

                }

                var productLoginName = string.IsNullOrEmpty(_productUsername) ? userLogin.LoginName : _productUsername;

                WriteToDiagnosticLog(
                   $"OmniChannel - ManageProductOmniChannel.ManageOmniChannelUser - _productUsername for user is {_productUsername}.");

                CustomerCompanyMap company = GetProductCompanyInstanceId(BlueBookProductConstants.OmniChannel);


                // enable after debugging : TODO
                if (string.IsNullOrEmpty(company.CompanyInstanceSourceId))
                {
                    WriteToErrorLog($"ManageProductOmniChannel.ManageOmniChannelUser - Error for user with editorPersona id - {editorPersonaId} Error - Company not found.");
                    return "Company Setup Error: Please Contact Support.";
                }

                // Check for user locations
                List<UserLocation> userLocations = null;
                List<UserAccessGroup> userAccessGroups = null;


                if (userAssignProductPropertyRole != null)
                {
                    // map userAssignProductPropertyRole to ProductPropertyRole
                    var productPropertyRole = MapGbObjectToProduct(userAssignProductPropertyRole);

                    if (productPropertyRole.PropertyList != null &&
                        productPropertyRole.PropertyList.Count > 0)
                    {
                        userLocations = productPropertyRole.PropertyList;
                    }

                    if (productPropertyRole.UserAccessGroups != null &&
                        productPropertyRole.UserAccessGroups.Count > 0)
                    {
                        userAccessGroups = productPropertyRole.UserAccessGroups;
                    }
                }


                List<Property> propList = GetAssignedPropertyForPersona(userPersonaId);


                if (propList == null || propList.Count == 0) // New User
                {
                    // Insert into GB DB
                    InsertOmniChannelProductUserDB(userPersonaId, editorPersonaId, userLocations, userAccessGroups);
                }
                else
                {
                    // To Do
                    UpdateDeleteOmniChannelProductUserDB(userPersonaId, editorPersonaId, userLocations, userAccessGroups);
                    WriteToDiagnosticLog($"OmniChannel - ManageProductOmniChannel.ManageOmniChannelUser - trying to UPDATE user with editorPersona id - {editorPersonaId}.");
                }


                return "";
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"OmniChannel - ManageProductOmniChannel.ManageOmniChannelUser - Error for user with editorPersona id - {editorPersonaId}", exception: ex);
                return $"Error - {ex.Message}";
            }
        }

        /// <summary>
        /// Unassign User
        /// </summary> 
        public string UnassignUser(long editorPersonaId, long userPersonaId)
        {
            var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                WriteToErrorLog(
                 $"ManageProductOmniChannel.UnassignUser - Error for user with userPersonaId:{userPersonaId}. ErrorReason-{listResponse.ErrorReason}");
                return listResponse.ErrorReason;
            }

            WriteToInformationLog("ManageProductOmniChannel-UnassignUser userPersonaId:{personaId}", messageProperties: new object[] { userPersonaId });
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);


            return "";
        }

        private ProductPropertyRole MapGbObjectToProduct(UserAssignProductPropertyRole userProductPropertyRole)
        {
            var result = new ProductPropertyRole();


            if (userProductPropertyRole.PropertyList != null &&
                userProductPropertyRole.PropertyList.Count > 0)
            {
                result.PropertyList = new List<UserLocation>();
                foreach (var propId in userProductPropertyRole.PropertyList)
                {
                    result.PropertyList.Add(new UserLocation { PropertyId = propId });
                }
            }

            if (userProductPropertyRole.RoleList != null && userProductPropertyRole.RoleList.Count > 0)
            {
                result.UserAccessGroups = new List<UserAccessGroup>();
                foreach (var roleId in userProductPropertyRole.RoleList)
                {
                    result.UserAccessGroups.Add(new UserAccessGroup { AccessGroupCode = roleId });
                }
            }

            return result;
        }

        private void InsertOmniChannelProductUserDB(long userPersonaId, long editorPersonaId,
             List<UserLocation> userLocations, List<UserAccessGroup> userAccessGroups)
        {
            OmniChannelRepository ocr = new OmniChannelRepository();
            UserAccessGroup role = new UserAccessGroup();

            if (userAccessGroups != null && userAccessGroups.Count > 0)
            {
                role = userAccessGroups[0];
            }

            int productId = (int)ProductEnum.OmniChannel;
            WriteToDiagnosticLog($"OmniChannel - ManageProductOmniChannel.InsertOmniChannelProductUser - calling DB to insert Property/Role assigned to user userPersonaId - {userPersonaId}, properties - {String.Join(",", userLocations)}, Roles - {String.Join(",", userAccessGroups)}.");

            //Inserting Roles
            try
            {
                RepositoryResponse result = new RepositoryResponse();
                result = InsertAssignedUserRoleData(userPersonaId, int.Parse(role.AccessGroupCode));
                if (result.Id < 0)
                {

                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"OmniChannel - ManageProductOmniChannel.InsertAssignedPropRoleToUser - Error for user with userPersonaId - {userPersonaId}, RoleId - {role.AccessGroupCode}", exception: ex);
            }

            // Inserting properties
            foreach (var prop in userLocations)
            {
                try
                {
                    RepositoryResponse result = new RepositoryResponse();
                    result = InsertAssignedUserData(userPersonaId, ProductEnum.OmniChannel, int.Parse(prop.PropertyId), int.Parse(role.AccessGroupCode));
                    if (result.Id < 0)
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    WriteToErrorLog($"OmniChannel - ManageProductOmniChannel.InsertAssignedPropRoleToUser - Error for user with userPersonaId - {userPersonaId}, PropertyId - {prop.PropertyId}, RoleId - {role.AccessGroupCode}", exception: ex);
                }
            }
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
        }


        private void UpdateDeleteOmniChannelProductUserDB(long userPersonaId, long editorPersonaId,
            List<UserLocation> userLocations, List<UserAccessGroup> userAccessGroups)
        {
            OmniChannelRepository ocr = new OmniChannelRepository();
            UserAccessGroup role = new UserAccessGroup();

            if (userAccessGroups != null && userAccessGroups.Count > 0)
            {
                role = userAccessGroups[0];
            }

            List<PropertyRole> propRoleListDB = null; /*GetAssignedPropertyRoleForPersona(userPersonaId);*/ // Existing Assigned Properties and Role in DB for the User

            var isRoleChanged = false;
            int productId = (int)ProductEnum.OmniChannel;

            RepositoryResponse result = new RepositoryResponse();

            //No data found in DB -  insert newly selected list 
            if (propRoleListDB.Count == 0)
            {
                foreach (var prop in userLocations)
                {
                    result = InsertAssignedUserData(userPersonaId, ProductEnum.OmniChannel, int.Parse(prop.PropertyId), int.Parse(role.AccessGroupCode));
                    if (result.Id < 0)
                    {
                        continue;
                    }
                }
            }

            if (propRoleListDB.Count > 0)
            {
                // Has role changed for the user - its always single role
                if (propRoleListDB.Exists(a => a.RoleID.ToString() == role.AccessGroupCode.ToString()) == false)
                {
                    isRoleChanged = true;
                }

                if (isRoleChanged == true)
                {
                    // Delete ALL assigned properties to user as role is changed
                    foreach (var prop in propRoleListDB)
                    {
                        result = DeleteAssignedUserData(userPersonaId, ProductEnum.OmniChannel, prop.PropID, prop.RoleID);
                        if (result.Id < 0)
                        {
                            continue;
                        }
                    }

                    // Assign 
                    foreach (var prop in userLocations)
                    {
                        result = InsertAssignedUserData(userPersonaId, ProductEnum.OmniChannel, int.Parse(prop.PropertyId), int.Parse(role.AccessGroupCode));
                        if (result.Id < 0)
                        {
                            continue;
                        }
                    }
                }

                WriteToDiagnosticLog($"OmniChannel - ManageProductOmniChannel.UpdateDeleteOmniChannelProductUserDB - calling DB to insert Property/Role assigned to user userPersonaId - {userPersonaId}, properties - {String.Join(",", userLocations)}, Roles - {String.Join(",", userAccessGroups)}.");

                // Going thru the existing Prop/Role list from DB to see if they r already in newly selected list 
                if (isRoleChanged == false)
                {
                    foreach (var prop in propRoleListDB)
                    {
                        try
                        {
                            if (userLocations.Exists(a => a.PropertyId == prop.PropID.ToString()) == false)
                            {
                                // Not existing in newly selected list - so delete it
                                result = DeleteAssignedUserData(userPersonaId, ProductEnum.OmniChannel, prop.PropID, prop.RoleID);
                                if (result.Id < 0)
                                {
                                    continue;
                                }

                            }

                        }
                        catch (Exception ex)
                        {
                            WriteToErrorLog($"OmniChannel - ManageProductOmniChannel.InsertAssignedPropRoleToUser - Error for user with userPersonaId - {userPersonaId}, PropertyId - {prop.PropID}, RoleId - {prop.RoleID}", exception: ex);
                        }
                    }
                }

                // Going thru the newly selected Prop/Role list to see if they r already in DB
                if (isRoleChanged == false)
                {
                    foreach (var prop in userLocations)
                    {
                        try
                        {
                            // newly selected item Not existing in  DB - so insert it
                            if (propRoleListDB.Exists(a => a.PropID == int.Parse(prop.PropertyId)) == false)
                            {
                                result = InsertAssignedUserData(userPersonaId, ProductEnum.OmniChannel, int.Parse(prop.PropertyId), int.Parse(role.AccessGroupCode));
                                if (result.Id < 0)
                                {
                                    continue;
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            WriteToErrorLog($"OmniChannel - ManageProductOmniChannel.InsertAssignedPropRoleToUser - Error for user with userPersonaId - {userPersonaId}, PropertyId - {prop.PropertyId}, RoleId - {role.AccessGroupCode}", exception: ex);
                        }
                    }
                }
            }
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
        }


        /// <summary>
        /// Used to assign a property to the given user
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <param name="propID"></param>
        /// <param name="roleID"></param>
        /// <returns></returns>
        private RepositoryResponse InsertAssignedUserData(long userPersonaId, ProductEnum productId, long propID, long roleID)
        {
            return InsertAssignedUserPropertyData(userPersonaId, ProductEnum.OmniChannel, propID);
            /*
	        PropertyRepository pr = new PropertyRepository();
            RepositoryResponse result = new RepositoryResponse();
            WriteToDiagnosticLog($"OmniChannel - ManageProductOmniChannel.InsertAssignedPropRoleToUser START - calling DB to Delete Property/Role assigned to user userPersonaId - {userPersonaId}, PropertyId - {propID}, RoleId - {roleID}.");
            int del = 0;
            result = pr.InsertRemoveAssignedPropertyToUser(userPersonaId, productId, propID, del);
            if (result.Id < 0)
            {
                WriteToErrorLog($"OmniChannel - ManageProductOmniChannel.InsertAssignedPropRoleToUser - Unable to Insert record for user with userPersonaId - {userPersonaId}, PropertyId - {propID}, RoleId - {roleID}");
                return result;
            }

            WriteToDiagnosticLog($"OmniChannel - ManageProductOmniChannel.InsertAssignedPropRoleToUser END - calling DB to Delete Property/Role assigned to user userPersonaId - {userPersonaId}, PropertyId - {propID}, RoleId - {roleID}, resule - {result.Id}.");
            return result;
			*/

        }


        private RepositoryResponse InsertAssignedUserRoleData(long userPersonaId, long roleID)
        {
            OmniChannelRepository ocr = new OmniChannelRepository();
            RepositoryResponse result = new RepositoryResponse();
            //WriteToDiagnosticLog($"OmniChannel - ManageProductOmniChannel.InsertAssignedPropRoleToUser START - calling DB to Delete Property/Role assigned to user userPersonaId - {userPersonaId}, PropertyId - {propID}, RoleId - {roleID}.");
            //int del = 0;
            //result = ocr.InsertAssignedRoleToUser(userPersonaId,  roleID, del);
            //if (result.Id < 0)
            //{
            //    WriteToErrorLog($"OmniChannel - ManageProductOmniChannel.InsertAssignedPropRoleToUser - Unable to Insert record for user with userPersonaId - {userPersonaId}, PropertyId - {propID}, RoleId - {roleID}");
            //    return result;
            //}

            //WriteToDiagnosticLog($"OmniChannel - ManageProductOmniChannel.InsertAssignedPropRoleToUser END - calling DB to Delete Property/Role assigned to user userPersonaId - {userPersonaId}, PropertyId - {propID}, RoleId - {roleID}, resule - {result.Id}.");
            return result;
        }



        private RepositoryResponse DeleteAssignedUserData(long userPersonaId, ProductEnum productId, long propID, long roleID)
        {
            return DeleteAssignedUserPropertyData(userPersonaId, ProductEnum.OmniChannel, propID);
            /*
			PropertyRepository pr = new PropertyRepository();
			RepositoryResponse result = new RepositoryResponse();
            int del = 1; // setting for Delete in DB
            WriteToDiagnosticLog($"OmniChannel - ManageProductOmniChannel.InsertAssignedPropRoleToUser START - calling DB to Delete Property/Role assigned to user userPersonaId - {userPersonaId}, PropertyId - {propID}, RoleId - {roleID}.");

            result = pr.InsertRemoveAssignedPropertyToUser(userPersonaId, productId, propID, del);
            if (result.Id < 0)
            {
                WriteToErrorLog($"OmniChannel - ManageProductOmniChannel.InsertAssignedPropRoleToUser - Unable to Delete record for user with userPersonaId - {userPersonaId}, PropertyId - {propID}, RoleId - {roleID}");
                return result; 
            }
            WriteToDiagnosticLog($"OmniChannel - ManageProductOmniChannel.InsertAssignedPropRoleToUser END - calling DB to Delete Property/Role assigned to user userPersonaId - {userPersonaId}, PropertyId - {propID}, RoleId - {roleID}, resule - {result.Id}.");
            return result;
			*/
        }

        #endregion

        #region Private Methods  

        private List<UL.Role> GetAssignedRoleForPersona(long userPersonaId)
        {
            int productId = (int)ProductEnum.OmniChannel;
            //OmniChannelRepository ocr = new OmniChannelRepository();
            UserRoleRightRepository urr = new UserRoleRightRepository();
            List<UL.Role> propRole = urr.ListRoleByPersona(productId, userPersonaId, null);
            return propRole;
        }

        private ListResponse MergeSelRolesWithGreenbook(IList<ProductRole> allRoles, long userPersonaId)
        {

            // get roles from DB for OmniChannel product
            WriteToDiagnosticLog(
                   $"OmniChannel - Getting assigned user roles from GB DB - GetAssignedRoleForPersona with persona id - {userPersonaId}");
            List<UL.Role> roleList = GetAssignedRoleForPersona(userPersonaId);

            // if a user record exists

            foreach (var role in roleList)
            {
                if (allRoles.Any(a => a.ID == role.RoleID.ToString()))
                {
                    ProductRole selrole = (from a in allRoles
                                           where a.ID == role.RoleID.ToString()
                                           select a).FirstOrDefault();
                    if (selrole != null)
                    {
                        selrole.IsAssigned = true;
                    }
                }
            }

            return new ListResponse()
            {
                Records = allRoles.Cast<object>().ToList(),
                TotalRows = allRoles.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1
            };
        }

        private List<Property> GetAssignedPropertyForPersona(long userPersonaId)
        {
            int productId = (int)ProductEnum.OmniChannel;
            OmniChannelRepository ocr = new OmniChannelRepository();
            List<Property> prop = ocr.ListPropByPersona(userPersonaId, productId);
            return prop;
        }

        private ListResponse MergeProductPropertiesWithGreenbook(IList<ProductProperty> blueBookPropertyList, long userPersonaId)
        {
            WriteToDiagnosticLog(
                  $"OmniChannel - Getting assigned user properties from GB DB - GetAssignedPropertyForPersona with persona id - {userPersonaId}");
            // merge the given user prop with the list
            List<Property> propRoleList = GetAssignedPropertyForPersona(userPersonaId);

            // if a  record exists - set prop assigned to true

            foreach (var propRole in propRoleList)
            {
                if (blueBookPropertyList.Any(a => a.ID == propRole.PropID.ToString()))
                {
                    ProductProperty pp = (from a in blueBookPropertyList
                                          where a.ID == propRole.PropID.ToString()
                                          select a).FirstOrDefault();
                    if (pp != null)
                    {
                        pp.IsAssigned = true;
                    }
                }
            }


            return new ListResponse()
            {
                Records = blueBookPropertyList.Cast<object>().ToList(),
                TotalRows = blueBookPropertyList.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1
            };
        }

        #endregion
    }




    #endregion
}