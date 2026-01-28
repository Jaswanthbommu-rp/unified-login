using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.ResponseObject;

namespace UnifiedLogin.LandingAPIEnterprise.Helpers
{
    /// <summary>
    /// Helper class for building paginated responses
    /// </summary>
    public static class PaginatedResponseHelper
    {
        /// <summary>
        /// Creates a paged response with error
        /// </summary>
        public static PagedResponse CreateErrorResponse<T>(int pageNumber, int rowsPerPage, 
            string errorReason) where T : class
        {
            return new PagedResponse
            {
                Data = new List<object>(),
                Meta = new Meta
                {
                    CurrentPage = pageNumber,
                    TotalRows = 0,
                    RowsPerPage = rowsPerPage
                },
                IsError = true,
                ErrorReason = errorReason
            };
        }

        /// <summary>
        /// Creates a successful paged response
        /// </summary>
        public static PagedResponse CreateSuccessResponse<T>(List<T> data, 
            int pageNumber, int rowsPerPage, int totalRows) where T : class
        {
            return new PagedResponse
            {
                Data = new List<object>(data.ConvertAll(x => (object)x)),
                Meta = new Meta
                {
                    CurrentPage = pageNumber,
                    TotalRows = totalRows,
                    RowsPerPage = rowsPerPage
                },
                IsError = false,
                ErrorReason = null
            };
        }

        /// <summary>
        /// Creates a successful paged response from generic list
        /// </summary>
        public static PagedResponse CreateSuccessResponse(List<object> data,
            int pageNumber, int rowsPerPage, int totalRows)
        {
            return new PagedResponse
            {
                Data = data,
                Meta = new Meta
                {
                    CurrentPage = pageNumber,
                    TotalRows = totalRows,
                    RowsPerPage = rowsPerPage
                },
                IsError = false,
                ErrorReason = null
            };
        }
    }
}
