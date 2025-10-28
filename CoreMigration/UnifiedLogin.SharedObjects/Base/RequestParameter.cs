using Microsoft.AspNetCore.Mvc;

namespace UnifiedLogin.SharedObjects.Base
{
	/// <summary>
	/// Used to filter, sort and limit the number of records being returned by the request.
	/// </summary>
	/// <remarks>The object can be bound to a webapi parameter if it is used in a GET statement and decorated with the [FromUri] attribute. i.e. [FromUri] RequestParameter DataFilter. If it is used in a POST request then it should be decorated with the [FromBody] attribute so it will bind correctly.</remarks>
	[ModelBinder(typeof(RequestParameterModelBinder))]
	public class RequestParameter : IRequestParameter
	{
		#region Private Variables
		private PageRequest _pages;
		private Dictionary<string, string> _filterBy;
		private Dictionary<string, string> _sortBy;
		private IList<string> _filterByOrderedColumnNames = new List<string>()
		{
			"ColumnName",
			"SearchValue"
		};
		private IList<string> _sortByOrderedColumnNames = new List<string>()
		{
			"ColumnName",
			"SortDirection"
		};
		#endregion

		#region Public Properties
		/// <summary>
		/// A list of key/value pairs to be used to filter the data in json format i.e. {"name":"john doe", "active": 0}
		/// </summary>
		public Dictionary<string, string> FilterBy
		{
			get { return _filterBy; }
			set { _filterBy = value; }
		}
		/// <summary>
		/// A list of key/value pairs to be used to sort the data  in json format i.e. {"firstname":"asc", "inactive":"desc"}
		/// </summary>
		public Dictionary<string, string> SortBy
		{
			get { return _sortBy; }
			set { _sortBy = value; }
		}
		/// <summary>
		/// Contains the details about the number of records, start record and total rows being returned by the request
		/// </summary>
		public PageRequest Pages
		{
			get { return _pages; }
			set { _pages = value; }
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public RequestParameter()
		{
			_filterBy = new Dictionary<string, string>();
			_sortBy = new Dictionary<string, string>();
			_pages = new PageRequest() { ResultsPerPage = 100, StartRow = 0 };
		}

		public RequestParameter(Dictionary<string, string> FilterBy, Dictionary<string, string> SortBy, PageRequest Page)
		{
			_filterBy = FilterBy;
			_sortBy = SortBy;
			_pages = Page;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Page"></param>
		public RequestParameter(PageRequest Page)
		{
			_pages = Page;
		}

		/// <summary>
		/// More than one filter by column in a Table Value Parameter
		/// </summary>
		public IList<string> FilterByOrderedColumnNames
		{
			get { return _filterByOrderedColumnNames; }
		}

		/// <summary>
		/// More than one sort by column in a Table Value Parameter
		/// </summary>
		public IList<string> SortByOrderedColumnNames
		{
			get { return _sortByOrderedColumnNames; }
		}
		#endregion
	}

	/// <summary>
	/// Used to limit the number of records being returned by the request
	/// </summary>
	public class PageRequest
	{
		private int startRow = 0;
		private int resultsPerPage = 100;

		/// <summary>
		/// The starting row to retrieve for the given data
		/// </summary>
		public int StartRow
		{
			get { return startRow; }
			set { startRow = value; }
		}

		/// <summary>
		/// The number of records to return to the UI
		/// </summary>
		public int ResultsPerPage
		{
			get { return resultsPerPage; }
			set { resultsPerPage = value; }
		}
	}
}


