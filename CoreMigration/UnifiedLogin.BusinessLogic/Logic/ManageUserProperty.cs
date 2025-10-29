using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.OmniChannel;

namespace UnifiedLogin.BusinessLogic.Logic
{
	public class ManageUserProperty : IManageUserProperty
    {
        /// <summary>
        /// Update User Detail and Products
        /// </summary>
        /// <param name="userPersonaId">User Persona ID</param>		
        /// <param name="productId">Product ID</param>		
        /// <returns>ListResponse object</returns>
        public ListResponse GetAssignedPropertyForPersona(long userPersonaId, long productId)
        {
            List<UserProperty> propertyList = new List<UserProperty>();
            var result = new ListResponse();
            //int productId = (int)ProductEnum.OmniChannel;
            OmniChannelRepository ocr = new OmniChannelRepository();
            try
            {
                switch (productId)
                {
                    case (int)ProductEnum.OmniChannel:

                        List<Property> propList = ocr.ListPropByPersona(userPersonaId, productId);
                        foreach (var item in propList)
                        {
                            propertyList.Add(new UserProperty { PropID = item.PropID, IsAssigned = true });
                        }

                        result = new ListResponse()
                        {
                            IsError = false,
                            Records = propertyList.Cast<object>().ToList(),
                            TotalRows = propertyList.Count,
                            RowsPerPage = propertyList.Count,
                            TotalPages = 1,
                            ErrorReason = ""
                        };
                        break;

                    default:

                        result = new ListResponse()
                        {
                            IsError = false,
                            Records = propertyList.Cast<object>().ToList(),
                            TotalRows = propertyList.Count,
                            RowsPerPage = propertyList.Count,
                            TotalPages = 1,
                            ErrorReason = "No results found for the product requested."
                        };
                        break;
                }


               
            }
            catch (Exception ex)
            {
                result = new ListResponse()
                {
                    IsError = true,
                    Records = propertyList.Cast<object>().ToList(),
                    TotalRows = propertyList.Count,
                    RowsPerPage = propertyList.Count,
                    TotalPages = 1,
                    ErrorReason = "Error occured while processing request." + ex.Message
                };
            }

            return result;
        }
    }
}