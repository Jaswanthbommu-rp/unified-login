using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// User Property Controller
    /// </summary>
    [ApiController]
    [Authorize]
    public class UserPropertyController : ControllerBase
    {
        private readonly IUserClaimsAccessor _userClaimsAccessor;

        #region Constructor
        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">User claims accessor</param>
        public UserPropertyController(IUserClaimsAccessor userClaimsAccessor)
        {
            _userClaimsAccessor = userClaimsAccessor;
        }
        #endregion

        /// <summary>
        /// Get persona properties
        /// </summary>
        /// <param name="productId">ProductId</param>
        /// <returns>ProductProperty List</returns>
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ListResponse), StatusCodes.Status200OK)]
        [Route("user/properties")]
        [HttpGet]
        public async Task<IActionResult> GetUserProperties(long productId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                var result = new ListResponse();
                IList<ProductProperty> propertyList = new List<ProductProperty>();

                if (userClaim.UserRealPageGuid == Guid.Empty)
                {
                    return BadRequest("Invalid parameter realPageId.");
                }

                if (productId == 0)
                    return BadRequest("Invalid parameter productId.");

                IManagePersona managePersona = new ManagePersona();

                // Get Properties for Persona by ProductId
                switch (productId)
                {
                    case (int)ProductEnum.OmniChannel:

                        IManageUserProperty manageUser = new ManageUserProperty();
                        result = manageUser.GetAssignedPropertyForPersona(userClaim.PersonaId, productId);
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
                return Ok(result);
            });
        }
    }
}
