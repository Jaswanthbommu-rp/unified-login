using Dapper;
using RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Models;
using RP.Enterprise.Foundation.Activity.Service.Logging.Shared.Models;
using RP.Enterprise.Foundation.DataAccess.Component.Helper;
using RP.Enterprise.Foundation.DataAccess.Component.Model;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Repository
{
	public class ReaderRepository : BaseRepository
	{
		public SearchMetadata GetSearchMetadata()
		{
			var searchCriteria = new SearchMetadata();

			using (var repository = GetRepository())
			{
				var multiResults = repository.QueryMultiple("Logging.ListLoggingMetadata", null);

				if (multiResults != null)
				{
					searchCriteria.LogCategoryList = multiResults.Read<LogCategory>().ToList();
					searchCriteria.LogTypeList = multiResults.Read<LogType>().ToList();
				}
			}

			return searchCriteria;
		}

		//public IList<ActivityDetailMessage> ListActivityLog(ActivityLogFilterCriteria filterCriteria)
		//{
		//	// Set TVP 
		//	var tvp = new TableValueParmInfo
		//	{
		//		TableVariableName = "SearchCriteriaTPV", // variable in SP for TVP
		//		TableParamTypeName = "dbo.SearchCriteria", // TVP name
		//		OrderedColumnName = new[] { "Name", "Value" }, // TVP columns
		//		StoredProcedureName = "Logging.ListActivity"
		//	};

		//	// Set dynamic param for additional params
		//	DynamicParameters param = new DynamicParameters();
		//	dynamic p;
		//	var rowsPerPage = filterCriteria.RowsPerPage;
		//	var pageNumber = filterCriteria.PageNumber;
		//	if (rowsPerPage == 0 || pageNumber == 0)
		//	{
		//		p = new
		//		{
		//			SortOrder = filterCriteria.SortOrder,
		//			SortOrderColumnName = filterCriteria.SortOrderColumnName,
		//		};
		//	}
		//	else
		//	{
		//		p = new
		//		{
		//			SortOrder = filterCriteria.SortOrder,
		//			SortOrderColumnName = filterCriteria.SortOrderColumnName,
		//			RowsPerPage = filterCriteria.RowsPerPage,
		//			PageNumber = filterCriteria.PageNumber
		//		};
		//	}

		//	param.AddDynamicParams(p);
		//	// Execute SP
		//	using (var repository = GetRepository())
		//	{
		//		return repository.GetManyWithTvp<ActivitySearchCriteria, ActivityDetailMessage>(tvp, filterCriteria.ActivitySearchCriteria, param).ToList();
		//	}
		//}

		public ListResponse<ActivityDetailMessage> ListActivityLogDetails(ActivityLogFilterCriteria filterCriteria)
		{
			// Set TVP 
			var tvp = new TableValueParmInfo
			{
				TableVariableName = "SearchCriteriaTPV", // variable in SP for TVP
				TableParamTypeName = "dbo.SearchCriteria", // TVP name
				OrderedColumnName = new[] { "Name", "Value" }, // TVP columns
				StoredProcedureName = "Logging.ListActivity"
			};

			// Set dynamic param for additional params
			DynamicParameters param = new DynamicParameters();
			dynamic p;
			var rowsPerPage = filterCriteria.RowsPerPage;
			var pageNumber = filterCriteria.PageNumber;
			if (rowsPerPage == 0 || pageNumber == 0)
			{
				p = new
				{
					SortOrder = filterCriteria.SortOrder,
					SortOrderColumnName = filterCriteria.SortOrderColumnName,
				};
			}
			else
			{
				p = new
				{
					SortOrder = filterCriteria.SortOrder,
					SortOrderColumnName = filterCriteria.SortOrderColumnName,
					RowsPerPage = filterCriteria.RowsPerPage,
					PageNumber = filterCriteria.PageNumber
				};
			}

			param.AddDynamicParams(p);

            if (filterCriteria.ActivitySearchCriteria != null)
            {
                var dataTable = filterCriteria.ActivitySearchCriteria.ConvertToTableValuedParameter(
                    tvp.TableParamTypeName,
                    tvp.OrderedColumnName);
                param.Add(tvp.TableVariableName, dataTable);
            }

            //param.Add("TotalRows", dbType: DbType.Int32, direction: ParameterDirection.Output);
			// Execute SP
			using (var repository = GetRepository())
            {
                var result = new ListResponse<ActivityDetailMessage>();
                var multiResults = repository.QueryMultiple("Logging.ListActivity", param);
                if (multiResults != null)
                {
                    result.Records = multiResults.Read<ActivityDetailMessage>().ToList();
                    result.TotalRows = multiResults.ReadFirstOrDefault<int>();
                }

                return result;

            }
		}

		public IList<AdditionalParameters> ListActivityAdditionalParams(long activityId)
		{
			using (var repository = GetRepository())
			{
				return repository.GetMany<AdditionalParameters>("Logging.ListActivityDetails", new { activityId }).ToList();
			} 
		}


        /// <summary>
        /// Used to delete the specified companies activity log records
        /// </summary>
        /// <param name="partyId"></param>
        /// <returns></returns>
        public long DeleteOrganizationActivityLog(long partyId)
        {
            dynamic param = new
            {
                OrganizationPartyId = partyId
            };
            using (var repository = GetRepository())
            {
                return repository.GetOne<long>("[Logging].[DeleteCompanyActivityData]", param, 300);
            }
        }
	}
}