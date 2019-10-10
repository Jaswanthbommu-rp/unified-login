using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System.Data;

namespace GreenBook.Tests
{
    public class PostalAddress : TestController
    {
        public PostalAddress(ITestOutputHelper _xUnitTestOutput)
        {
            reusable = new TestUtilities(this);
            this.XunitTestOutPut = _xUnitTestOutput;

            dbManager = new DatabaseController(DbConnString);

            postalAddressUsername = CurrentlyLoggedInUser;
        }

        private string payload = "";
        JsonController jsonManager = new JsonController();
        TestUtilities reusable;
        private readonly ITestOutputHelper XunitTestOutPut;
        private string realPageId = "", postalAddressUsername;
        DatabaseController dbManager;

        // PostalAddress =/api/persons/{realPageId}/PostalAddress

        [Fact, Trait("", "Happy Path")]
        public void GetPostalAddress()
        {
            // Set up the API URL
            realPageId = reusable.GetRealPageId(CurrentlyLoggedInUser);
            EndPointUrl = HostUrl + Properties["PostalAddress"].Replace("{realPageId}", realPageId);

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            // Extract API's JSON Response

            ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.PostalAddress, IErrorData> postalAddress
                = JsonConvert.DeserializeObject<ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.PostalAddress, IErrorData>>(ResponseString);

            if (postalAddress.list.Count <= 0)
            {
                // Set up Payload
                payload = reusable.DoPostPutPostalAddressPayload(postalAddressUsername, HttpVerb.Post);
                XunitTestOutPut.WriteLine("Payload:\n" + payload);
                ObjectListOutput<PostalAddress, IErrorData> expectedLinkPostalAddress
                = JsonConvert.DeserializeObject<ObjectListOutput<PostalAddress, IErrorData>>(payload);

                //Execute POST API
                EndPointUrl = HostUrl + Properties["PostalAddress"].Replace("{realPageId}", realPageId);
                XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
                GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

                // Reexecute GET API
                XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
                GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

                // Reextract API's JSON Response
                postalAddress
                = JsonConvert.DeserializeObject<ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.PostalAddress, IErrorData>>(ResponseString);
            }

            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
            foreach (var postalAdd in postalAddress.list)
            {
                Assert.True(postalAdd.PartyContactMechanismId > 0);
                Assert.True(postalAdd.ContactMechanismId > 0);
                Assert.True(postalAdd.AddressString != null);
                Assert.True(postalAdd.AddressType != null);
                Assert.True(postalAdd.contactMechanismUsageType.ContactMechanismUsageTypeId > 0);
                Assert.True(postalAdd.contactMechanismUsageType.ParentContactMechanismUsageTypeId > 0);
                Assert.True(postalAdd.contactMechanismUsageType.Name != null);
            }
      

        /*
                    // Extract Expected JSON Response
                    DataTable expectedLinkPostalAddress = dbManager.executeQuery("SELECT unpvt.PartyContactMechanismId, unpvt.PartyId, unpvt.ContactMechanismID, "
                            + "unpvt.[FromDate], unpvt.[ThruDate],unpvt.AddressString,"
                            + "unpvt.AddressType, unpvt.ContactMechanismUsageTypeID,unpvt.[ParentContactMechanismUsageTypeID],unpvt.[Name] "
                            + ",'' as [ContactMechanismBoundaryId], '' as [ContactMechanismBoundaryFromDate]"
                            + ", '' as [ContactMechanismBoundaryThruDate], '' as [GeographicBoundaryId]	, '' as [GeographicBoundaryName]"
                            + ", '' as [GeographicBoundaryCode], '' as [Abbreviation], '' as [GeographicBoundaryTypeId], '' as [GeographicBoundaryTypeName] "
                            + "FROM(SELECT  pcm.PartyContactMechanismId, p.PartyId, cm.ContactMechanismID,"
                            + "pcm.[FromDate], pcm.[ThruDate],pa.StreetAddress1, pa.StreetAddress2,"
                            + "pa.StreetAddress3, cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeID, cmut.[ParentContactMechanismUsageTypeID], cmut.[Name] "
                            + "FROM [" + Properties["identityDatabase"] + "].Enterprise.Party p \n"
                            + "JOIN [" + Properties["identityDatabase"] + "].Enterprise.PartyContactMechanism pcm ON pcm.PartyId = p.PartyId "
                            + "AND p.RealPageId = (select RealPageId from [" + Properties["identityDatabase"] + "].Enterprise.Party "
                            + "WHERE partyid = (select partyid from [" + Properties["identityDatabase"] + "].ident.userLogin "
                            + "WHERE loginName = '" + postalAddressUsername + "')) "
                            + "JOIN [" + Properties["identityDatabase"] + "].Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId "
                            + "JOIN [" + Properties["identityDatabase"] + "].Enterprise.[StreetAddress] pa ON pa.ContactMechanismID = cm.ContactMechanismID "
                            + "JOIN [" + Properties["identityDatabase"] + "].Enterprise.ContactMechanismUsage cmu ON cmu.PartyContactMechanismID = pcm.PartyContactMechanismId "
                            + "JOIN [" + Properties["identityDatabase"] + "].Enterprise.ContactMechanismUsageType cmut ON cmu.ContactMechanismUsageTypeID = cmut.ContactMechanismUsageTypeID "
                            + "WHERE (pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE())) AS pvt UNPIVOT "
                            + "(AddressString FOR AddressType IN (StreetAddress1, StreetAddress2, StreetAddress3)) AS unpvt UNION ALL\n"
                            + "SELECT pcm.PartyContactMechanismId, p.PartyId, cm.ContactMechanismID, "
                            + "pcm.[FromDate], pcm.[ThruDate], gb.Name AS AddressString, gbt.Name AS AddressType,"
                            + "cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeID,cmut.[ParentContactMechanismUsageTypeID], cmut.[Name] "
                            + ",cmb.[ContactMechanismBoundaryId], cmb.[FromDate] as [ContactMechanismBoundaryFromDate]"
                            + ", cmb.[ThruDate] as [ContactMechanismBoundaryThruDate], gb.[GeographicBoundaryId], gb.[Name] as [GeographicBoundaryName]"
                            + ", gb.[GeographicBoundaryCode], gb.[Abbreviation], gbt.[GeographicBoundaryTypeId], gbt.[Name] as [GeographicBoundaryTypeName] "
                            + "FROM [" + Properties["identityDatabase"] + "].Enterprise.ContactMechanismUsageType cmut "
                            + "JOIN [" + Properties["identityDatabase"] + "].Enterprise.ContactMechanismUsage cmu ON cmu.ContactMechanismUsageTypeID = cmut.ContactMechanismUsageTypeID "
                            + "JOIN [" + Properties["identityDatabase"] + "].Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID "
                            + "JOIN [" + Properties["identityDatabase"] + "].Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId "
                            + "JOIN [" + Properties["identityDatabase"] + "].Enterprise.ContactMechanismBoundary cmb ON cmb.ContactMechanismId = cm.ContactMechanismID "
                            + "JOIN [" + Properties["identityDatabase"] + "].Enterprise.GeographicBoundary gb ON gb.GeographicBoundaryId = cmb.GeographicBoundaryId "
                            + "JOIN [" + Properties["identityDatabase"] + "].Enterprise.GeographicBoundaryType gbt ON gbt.GeographicBoundaryTypeId = gb.GeographicBoundaryTypeId "
                            + "JOIN [" + Properties["identityDatabase"] + "].Enterprise.Party p ON p.PartyId = pcm.PartyId "
                            + "WHERE p.RealPageId = (select RealPageId from [" + Properties["identityDatabase"] + "].Enterprise.Party "
                            + "WHERE partyid = (select partyid from [" + Properties["identityDatabase"] + "].ident.userLogin "
                            + "WHERE loginName = '" + postalAddressUsername + "')) "
                            + "AND(pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE())");

                    // Assert
                    Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

                    for (int countPostalAddress = 0; countPostalAddress < postalAddress.list.Count; countPostalAddress++)
                    {
                        Assert.NotNull(postalAddress.list[countPostalAddress].PartyContactMechanismId);
                        Assert.True(postalAddress.list[countPostalAddress].PartyContactMechanismId 
                            == int.Parse(expectedLinkPostalAddress.Rows[countPostalAddress]["PartyContactMechanismId"].ToString())
                            , "postalAddress.list[countPostalAddress].PartyContactMechanismId == int.Parse(expectedLinkPostalAddress.Rows[countPostalAddress][\"PartyContactMechanismId\"].ToString())");
                        Assert.NotNull(postalAddress.list[countPostalAddress].ContactMechanismId);
                        Assert.True(postalAddress.list[countPostalAddress].ContactMechanismId 
                            == int.Parse(expectedLinkPostalAddress.Rows[countPostalAddress]["ContactMechanismId"].ToString())
                            , "postalAddress.list[countPostalAddress].ContactMechanismId == int.Parse(expectedLinkPostalAddress.Rows[countPostalAddress][\"ContactMechanismId\"].ToString())");
                        Assert.NotNull(postalAddress.list[countPostalAddress].AddressString);
                        Assert.True(postalAddress.list[countPostalAddress].AddressString == expectedLinkPostalAddress.Rows[countPostalAddress]["AddressString"].ToString()
                            , "postalAddress.list[countPostalAddress].AddressString == expectedLinkPostalAddress.Rows[countPostalAddress][\"AddressString\"].ToString()");
                        Assert.NotNull(postalAddress.list[countPostalAddress].AddressType);
                        Assert.True(postalAddress.list[countPostalAddress].AddressType == expectedLinkPostalAddress.Rows[countPostalAddress]["AddressType"].ToString()
                            , "postalAddress.list[countPostalAddress].AddressType == expectedLinkPostalAddress.Rows[countPostalAddress][\"AddressType\"].ToString()");
                        Assert.NotNull(postalAddress.list[countPostalAddress].contactMechanismUsageType.ContactMechanismUsageTypeId);
                        Assert.True(postalAddress.list[countPostalAddress].contactMechanismUsageType.ContactMechanismUsageTypeId 
                            == int.Parse(expectedLinkPostalAddress.Rows[countPostalAddress]["ContactMechanismUsageTypeId"].ToString())
                            , "postalAddress.list[countPostalAddress].contactMechanismUsageType.ContactMechanismUsageTypeId == int.Parse(expectedLinkPostalAddress.Rows[countPostalAddress][\"ContactMechanismUsageTypeId\"].ToString())");
                        Assert.NotNull(postalAddress.list[countPostalAddress].contactMechanismUsageType.ParentContactMechanismUsageTypeId);
                        Assert.True(postalAddress.list[countPostalAddress].contactMechanismUsageType.ParentContactMechanismUsageTypeId == int.Parse(expectedLinkPostalAddress.Rows[countPostalAddress]["ParentContactMechanismUsageTypeId"].ToString())
                            , "postalAddress.list[countPostalAddress].contactMechanismUsageType.ParentContactMechanismUsageTypeId == int.Parse(expectedLinkPostalAddress.Rows[countPostalAddress][\"ContactMechanismUsageTypeId\"].ToString())");
                        Assert.NotNull(postalAddress.list[countPostalAddress].contactMechanismUsageType.Name);
                        Assert.True(postalAddress.list[countPostalAddress].contactMechanismUsageType.Name == expectedLinkPostalAddress.Rows[countPostalAddress]["Name"].ToString()
                            , "postalAddress.list[countPostalAddress].contactMechanismUsageType.Name == expectedLinkPostalAddress.Rows[countPostalAddress][\"Name\"].ToString()");
                    }
        */
    }

		//[Fact, Trait("", "Data-Driven")]
		public void GetPostalAddressWithContactMechanismUsageTypeName()
		{
			// Set up the API URL
			realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(CurrentlyLoggedInUser)).RealPageId.ToString();
			EndPointUrl = HostUrl + Properties["PostalAddress"].Replace("{realPageId}", realPageId);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.PostalAddress, IErrorData> postalAddress
				= JsonConvert.DeserializeObject<ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.PostalAddress, IErrorData>>(ResponseString);

			if (postalAddress.list.Count <= 0)
			{
				// Set up Payload
				payload = reusable.DoPostPutPostalAddressPayload(postalAddressUsername, HttpVerb.Post);
				XunitTestOutPut.WriteLine("Payload:\n" + payload);

				//Execute POST API
				XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
				GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

				// Reexecute GET API
				XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
				GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

				// Reextract API's JSON Response
				
				postalAddress
				= JsonConvert.DeserializeObject<ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.PostalAddress, IErrorData>>(ResponseString);
			}
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ContactMechanismUsageTypes"];
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
			ObjectListOutput<ContactMechanismUsageType, IErrorData> contactMechanismUsageTypes
				= JsonConvert.DeserializeObject<ObjectListOutput<ContactMechanismUsageType, IErrorData>>(ResponseString);
					
			EndPointUrl = HostUrl + Properties["PostalAddress"].Replace("{realPageId}", realPageId)
				+ "?ContactMechanismUsageTypeName=" + WebUtility.UrlDecode(contactMechanismUsageTypes.list[0].Name);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			postalAddress
				= JsonConvert.DeserializeObject<ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.PostalAddress, IErrorData>>(ResponseString);

			// Extract Expected JSON Response
			DataTable expectedLinkPostalAddress = dbManager.executeQuery("SELECT unpvt.PartyContactMechanismId, unpvt.PartyId, unpvt.ContactMechanismID, "
					+ "unpvt.[FromDate], unpvt.[ThruDate],unpvt.AddressString,"
					+ "unpvt.AddressType, unpvt.ContactMechanismUsageTypeID,unpvt.[ParentContactMechanismUsageTypeID],unpvt.[Name] "
					+ ",'' as [ContactMechanismBoundaryId], '' as [ContactMechanismBoundaryFromDate]"
					+ ", '' as [ContactMechanismBoundaryThruDate], '' as [GeographicBoundaryId]	, '' as [GeographicBoundaryName]"
					+ ", '' as [GeographicBoundaryCode], '' as [Abbreviation], '' as [GeographicBoundaryTypeId], '' as [GeographicBoundaryTypeName] "
					+ "FROM(SELECT  pcm.PartyContactMechanismId, p.PartyId, cm.ContactMechanismID,"
					+ "pcm.[FromDate], pcm.[ThruDate],pa.StreetAddress1, pa.StreetAddress2,"
					+ "pa.StreetAddress3, cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeID, cmut.[ParentContactMechanismUsageTypeID], cmut.[Name] "
					+ "FROM [" + Properties["identityDatabase"] + "].Enterprise.Party p \n"
					+ "JOIN [" + Properties["identityDatabase"] + "].Enterprise.PartyContactMechanism pcm ON pcm.PartyId = p.PartyId "
					+ "AND p.RealPageId = (select RealPageId from [" + Properties["identityDatabase"] + "].Enterprise.Party "
					+ "WHERE partyid = (select partyid from [" + Properties["identityDatabase"] + "].ident.userLogin "
					+ "WHERE loginName = '" + postalAddressUsername + "')) "
					+ "JOIN [" + Properties["identityDatabase"] + "].Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId "
					+ "JOIN [" + Properties["identityDatabase"] + "].Enterprise.[StreetAddress] pa ON pa.ContactMechanismID = cm.ContactMechanismID "
					+ "JOIN [" + Properties["identityDatabase"] + "].Enterprise.ContactMechanismUsage cmu ON cmu.PartyContactMechanismID = pcm.PartyContactMechanismId "
					+ "JOIN [" + Properties["identityDatabase"] + "].Enterprise.ContactMechanismUsageType cmut ON cmu.ContactMechanismUsageTypeID = cmut.ContactMechanismUsageTypeID "
					+ "WHERE (pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE()) AND cmut.[ParentContactMechanismUsageTypeID] = "
					+ contactMechanismUsageTypes.list[0].ContactMechanismUsageTypeId + ") AS pvt UNPIVOT "
					+ "(AddressString FOR AddressType IN (StreetAddress1, StreetAddress2, StreetAddress3)) AS unpvt UNION ALL\n"
					+ "SELECT pcm.PartyContactMechanismId, p.PartyId, cm.ContactMechanismID, "
					+ "pcm.[FromDate], pcm.[ThruDate], gb.Name AS AddressString, gbt.Name AS AddressType,"
					+ "cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeID,cmut.[ParentContactMechanismUsageTypeID], cmut.[Name] "
					+ ",cmb.[ContactMechanismBoundaryId], cmb.[FromDate] as [ContactMechanismBoundaryFromDate]"
					+ ", cmb.[ThruDate] as [ContactMechanismBoundaryThruDate], gb.[GeographicBoundaryId], gb.[Name] as [GeographicBoundaryName]"
					+ ", gb.[GeographicBoundaryCode], gb.[Abbreviation], gbt.[GeographicBoundaryTypeId], gbt.[Name] as [GeographicBoundaryTypeName] "
					+ "FROM [" + Properties["identityDatabase"] + "].Enterprise.ContactMechanismUsageType cmut "
					+ "JOIN [" + Properties["identityDatabase"] + "].Enterprise.ContactMechanismUsage cmu ON cmu.ContactMechanismUsageTypeID = cmut.ContactMechanismUsageTypeID "
					+ "JOIN [" + Properties["identityDatabase"] + "].Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID "
					+ "JOIN [" + Properties["identityDatabase"] + "].Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId "
					+ "JOIN [" + Properties["identityDatabase"] + "].Enterprise.ContactMechanismBoundary cmb ON cmb.ContactMechanismId = cm.ContactMechanismID "
					+ "JOIN [" + Properties["identityDatabase"] + "].Enterprise.GeographicBoundary gb ON gb.GeographicBoundaryId = cmb.GeographicBoundaryId "
					+ "JOIN [" + Properties["identityDatabase"] + "].Enterprise.GeographicBoundaryType gbt ON gbt.GeographicBoundaryTypeId = gb.GeographicBoundaryTypeId "
					+ "JOIN [" + Properties["identityDatabase"] + "].Enterprise.Party p ON p.PartyId = pcm.PartyId "
					+ "WHERE p.RealPageId = (select RealPageId from [" + Properties["identityDatabase"] + "].Enterprise.Party "
					+ "WHERE partyid = (select partyid from [" + Properties["identityDatabase"] + "].ident.userLogin "
					+ "WHERE loginName = '" + postalAddressUsername + "')) "
					+ "AND(pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE())"
					+ "AND cmut.[ParentContactMechanismUsageTypeID] = "	+ contactMechanismUsageTypes.list[0].ContactMechanismUsageTypeId);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

			for (int countPostalAddress = 0; countPostalAddress < postalAddress.list.Count; countPostalAddress++)
			{
				Assert.NotNull(postalAddress.list[countPostalAddress].PartyContactMechanismId);
				Assert.True(postalAddress.list[countPostalAddress].PartyContactMechanismId
					== int.Parse(expectedLinkPostalAddress.Rows[countPostalAddress]["PartyContactMechanismId"].ToString())
					, "postalAddress.list[countPostalAddress].PartyContactMechanismId == int.Parse(expectedLinkPostalAddress.Rows[countPostalAddress][\"PartyContactMechanismId\"].ToString())");
				Assert.NotNull(postalAddress.list[countPostalAddress].ContactMechanismId);
				Assert.True(postalAddress.list[countPostalAddress].ContactMechanismId
					== int.Parse(expectedLinkPostalAddress.Rows[countPostalAddress]["ContactMechanismId"].ToString())
					, "postalAddress.list[countPostalAddress].ContactMechanismId == int.Parse(expectedLinkPostalAddress.Rows[countPostalAddress][\"ContactMechanismId\"].ToString())");
				Assert.NotNull(postalAddress.list[countPostalAddress].AddressString);
				Assert.True(postalAddress.list[countPostalAddress].AddressString == expectedLinkPostalAddress.Rows[countPostalAddress]["AddressString"].ToString()
					, "postalAddress.list[countPostalAddress].AddressString == expectedLinkPostalAddress.Rows[countPostalAddress][\"AddressString\"].ToString()");
				Assert.NotNull(postalAddress.list[countPostalAddress].AddressType);
				Assert.True(postalAddress.list[countPostalAddress].AddressType == expectedLinkPostalAddress.Rows[countPostalAddress]["AddressType"].ToString()
					, "postalAddress.list[countPostalAddress].AddressType == expectedLinkPostalAddress.Rows[countPostalAddress][\"AddressType\"].ToString()");
				Assert.NotNull(postalAddress.list[countPostalAddress].contactMechanismUsageType.ContactMechanismUsageTypeId);
				Assert.True(postalAddress.list[countPostalAddress].contactMechanismUsageType.ContactMechanismUsageTypeId
					== int.Parse(expectedLinkPostalAddress.Rows[countPostalAddress]["ContactMechanismUsageTypeId"].ToString())
					, "postalAddress.list[countPostalAddress].contactMechanismUsageType.ContactMechanismUsageTypeId == int.Parse(expectedLinkPostalAddress.Rows[countPostalAddress][\"ContactMechanismUsageTypeId\"].ToString())");
				Assert.NotNull(postalAddress.list[countPostalAddress].contactMechanismUsageType.ParentContactMechanismUsageTypeId);
				Assert.True(postalAddress.list[countPostalAddress].contactMechanismUsageType.ParentContactMechanismUsageTypeId == int.Parse(expectedLinkPostalAddress.Rows[countPostalAddress]["ParentContactMechanismUsageTypeId"].ToString())
					, "postalAddress.list[countPostalAddress].contactMechanismUsageType.ParentContactMechanismUsageTypeId == int.Parse(expectedLinkPostalAddress.Rows[countPostalAddress][\"ContactMechanismUsageTypeId\"].ToString())");
				Assert.NotNull(postalAddress.list[countPostalAddress].contactMechanismUsageType.Name);
				Assert.True(postalAddress.list[countPostalAddress].contactMechanismUsageType.Name == expectedLinkPostalAddress.Rows[countPostalAddress]["Name"].ToString()
					, "postalAddress.list[countPostalAddress].contactMechanismUsageType.Name == expectedLinkPostalAddress.Rows[countPostalAddress][\"Name\"].ToString()");
			}
		}

		//[Fact, Trait("", "Negative Case")]
		public void GetPostalAddressInvalidRealPageId()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["PostalAddress"].Replace("{realPageId}", "invalidRealPageId");

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains("The request is invalid."), "ResponseString.Contains(\"The request is invalid.\")");
		}

		[Fact, Trait("", "Happy Path")]
		public void PostPostalAddress()
		{
            // Set up Payload
            payload = reusable.DoPostPutPostalAddressPayload(postalAddressUsername, HttpVerb.Post);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			//realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(postalAddressUsername)).RealPageId.ToString();
            realPageId = reusable.GetRealPageId(CurrentlyLoggedInUser);
            EndPointUrl = HostUrl + Properties["PostalAddress"].Replace("{realPageId}", realPageId);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.PostalAddress.PostalAddressOutputResult 
				postalAddressOutput = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.PostalAddress.PostalAddressOutputResult>(ResponseString);
			
			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(postalAddressOutput.ContactMechanismId);
			Assert.True(postalAddressOutput.ContactMechanismId > 1, "postalAddressOutput.ContactMechanismId > 1");			
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostPostalAddressInvalidRealPageId()
		{
			// Set up Payload
			payload = reusable.DoPostPutPostalAddressPayload(postalAddressUsername, HttpVerb.Post);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(CurrentlyLoggedInUser)).RealPageId.ToString();
			EndPointUrl = HostUrl + Properties["PostalAddress"].Replace("{realPageId}", "invalidRealPageId");

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains("The request is invalid."), "ResponseString.Contains(\"The request is invalid.\")");
		}

		//[Fact, Trait("", "Happy Path")]
		public void PutPostalAddress()
		{
			// Set up Payload
			payload = reusable.DoPostPutPostalAddressPayload(postalAddressUsername);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

            // Set up the API URL
            realPageId = reusable.GetRealPageId(CurrentlyLoggedInUser);
            EndPointUrl = HostUrl + Properties["PostalAddress"].Replace("{realPageId}", realPageId);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.PostalAddress.PostalAddressOutputResult
				postalAddressOutput = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.PostalAddress.PostalAddressOutputResult>(ResponseString);
			
			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(postalAddressOutput.ContactMechanismId);
			Assert.True(postalAddressOutput.ContactMechanismId 
				> JsonConvert.DeserializeObject<LinkPostalAddress>(payload).PartyContactMechanism.ContactMechanismId
				, "postalAddressOutput.ContactMechanismId > JsonConvert.DeserializeObject<LinkPostalAddress>(payload).PartyContactMechanism.ContactMechanismId");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PutPostalAddressInvalidRealPageId()
		{
			// Set up Payload
			payload = reusable.DoPostPutPostalAddressPayload(postalAddressUsername);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(CurrentlyLoggedInUser)).RealPageId.ToString();
			EndPointUrl = HostUrl + Properties["PostalAddress"].Replace("{realPageId}", "invalidRealPageId");

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains("The request is invalid."), "ResponseString.Contains(\"The request is invalid.\")");
		}
	}
}
