using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.BlackBook
{
	public class CompanyPropertyRootObject
	{
		public Data data { get; set; }
	}
	public class GetCompanyPropertyInstance
	{
		public int propertyInstanceId { get; set; }
		public string propertyInstanceSourceId { get; set; }
		public string propertyName { get; set; }
		public string source { get; set; }
		public object phaseParentId { get; set; }
		public object legalSiteName { get; set; }
		public object website { get; set; }
		public string address { get; set; }
		public string city { get; set; }
		public string state { get; set; }
		public string country { get; set; }
		public string county { get; set; }
		public string postalCode { get; set; }
		public string sourceAddress { get; set; }
		public string sourceCity { get; set; }
		public string sourceState { get; set; }
		public string sourceCountry { get; set; }
		public object sourceCounty { get; set; }
		public string sourcePostalCode { get; set; }
		public object sourceLatitude { get; set; }
		public object sourceLongitude { get; set; }
		public string tzoffset { get; set; }
		public string tzdst { get; set; }
		public string tzname { get; set; }
		public string tzdescription { get; set; }
		public bool isActive { get; set; }
		public string yearBuilt { get; set; }
		public object renovationStartDate { get; set; }
		public object renovationEndDate { get; set; }
		public object squareFeet { get; set; }
		public string units { get; set; }
		public object stories { get; set; }
		public object bedCount { get; set; }
		public string createdBy { get; set; }
		public string modifiedBy { get; set; }
		public string createdAt { get; set; }
		public string modifiedAt { get; set; }
		public object deletedAt { get; set; }
		public object deletedReason { get; set; }
		public object propertyType { get; set; }
		public object propertySubType { get; set; }
		public string googleLatitude { get; set; }
		public string googleLongitude { get; set; }
		public object constructionStatus { get; set; }
		public bool isUat { get; set; }
		public string assetClass { get; set; }
		public object apn { get; set; }
		public object fips { get; set; }
		public bool greenBookCares { get; set; }
		public object buildings { get; set; }
		public bool nrr { get; set; }
		public object modifiedSource { get; set; }
		public bool isAcquired { get; set; }
	}

	public class Attributes
	{
		public List<GetCompanyPropertyInstance> getCompanyPropertyInstances { get; set; }
	}

	public class Data
	{
		public string type { get; set; }
		public Attributes attributes { get; set; }
	}
}
