using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// StatusType Controller
    /// </summary>
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class StatusTypeController : ControllerBase
    {
        /// <summary>
        /// List of StatusTypes
        /// </summary>
        /// <param name="CategoryTypeName">Category TypeName (e.g. Status)</param>
        /// <param name="CategoryName">Category Name (e.g. User Status)</param>
        /// <returns>List of StatusType Details</returns>
        [HttpGet("statustype/categorytype/{CategoryTypeName}/categoryname/{CategoryName}")]
        [ProducesResponseType(typeof(IList<StatusType>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStatusType(string CategoryTypeName, string CategoryName)
        {
            return await Task.Run<IActionResult>(() =>
            {
                if (string.IsNullOrWhiteSpace(CategoryTypeName) || string.IsNullOrWhiteSpace(CategoryName))
                {
                    var apiError = new ApiError()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Status = (short)StatusCodes.Status400BadRequest,
                        Title = "Invalid parameter.",
                        Detail = "Invalid parameter(s):"
                            + (string.IsNullOrWhiteSpace(CategoryTypeName) ? $" Category TypeName: {CategoryTypeName}" : "")
                            + (string.IsNullOrWhiteSpace(CategoryName) ? $" Category Name: {CategoryName}" : ""),
                        Links = string.Empty,
                        Code = "StatusType.GetStatusType.1",
                        Source = new ApiErrorSource()
                        {
                            JsonPointer = string.Empty,
                            Parameter = string.Empty
                        }
                    };
                    return BadRequest(apiError);
                }

                IManageStatusType manageStatusType = new ManageStatusType();
                IList<StatusType> statusTypeList = manageStatusType.GetStatusType(CategoryTypeName, CategoryName);

                if (statusTypeList.Count == 0)
                {
                    var apiError = new ApiError()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Status = (short)StatusCodes.Status404NotFound,
                        Title = "Status Types not found.",
                        Detail = $"Status Types not found for Category TypeName: {CategoryTypeName}, Category name: {CategoryName}",
                        Links = string.Empty,
                        Code = "StatusType.GetStatusType.2",
                        Source = new ApiErrorSource()
                        {
                            JsonPointer = string.Empty,
                            Parameter = string.Empty
                        }
                    };
                    return BadRequest(apiError);
                }

                return Ok(statusTypeList);
            });
        }
    }
}
