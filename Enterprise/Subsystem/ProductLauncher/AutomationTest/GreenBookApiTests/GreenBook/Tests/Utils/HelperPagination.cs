using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;

/*
namespace GreenBook.Utils
{
    class HelperPagination : TestController
    {
        /// <summary>
        /// TestPaginationTotalRecords : To Validate the Results per page display functionality 
        /// Provide the response model class along with its subclass
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pageSize"></param>
        /// <returns>boolean of type string</returns>
        public void TestPaginationTotalRecords<T, TR>(string url, int pageSize = 100)
        {
            // Variables
            var startRow = 0;
            string endPointUrl;

            var payLoadFormat = "{{filterBy: {{}},sortBy: {{}}, pages: {{total: 0, startRow: {0},resultsPerPage: {1}}}}}";
            //Set up the api url 
            string payload = string.Format(payLoadFormat, startRow, pageSize);

            if (url.Contains("?"))
                endPointUrl = url + "&dataFilter=" + payload;
            else
                endPointUrl = url + "?dataFilter=" + payload;

            //Act
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
            
            Assert.Equal(HttpStatusCode.OK, ResponseHttpStatusCode);

            // Assert
            var responseData = JsonConvert.DeserializeObject<T>(ResponseString);
            var totalRecordsProp = responseData.GetType().GetProperty("totalRecords");
            var recordsProp = responseData.GetType().GetProperty("records");

            if (pageSize >= (int)totalRecordsProp.GetValue(responseData))
            {
                if ((int)totalRecordsProp.GetValue(responseData) == 1)
                    pageSize = 1;
                else
                    pageSize = (int)totalRecordsProp.GetValue(responseData) / 2;

                payload = string.Format(payLoadFormat, startRow, pageSize);

                if (url.Contains("?"))
                    endPointUrl = url + "&dataFilter=" + payload;
                else
                    endPointUrl = url + "?dataFilter=" + payload;

                //Act
                GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
                
                Assert.Equal(HttpStatusCode.OK, ResponseHttpStatusCode);
                
                // Assert
                responseData = JsonConvert.DeserializeObject<T>(ResponseString);
                //totalRecordsProp = responseData.GetType().GetProperty("totalRecords");
                recordsProp = responseData.GetType().GetProperty("records");
            }

            Assert.NotNull(responseData);
            // TODO : Defect 348684 : Total Records display the entire records count irrespective of the resultsPerPage
            //Assert.IsTrue((int)totalRecordsProp.GetValue(responseData) == 15, "");  

            var count = 0;
            var records = (List<TR>)recordsProp.GetValue(responseData);
            foreach (var item in records)
            {
                // TODO : Defect 348684 : Total Records display the entire records count irrespective of the resultsPerPage
                //var recordTotalProp = item.GetType().GetProperty("totalRecords");
                //Assert.IsTrue((int)recordTotalProp.GetValue(item) == 15, "");
                count++;
            }
            Assert.True(count == pageSize);
        }

/*

        /// <summary>
        /// TestPaginationStartRow : To Validate the response records are displayed in the provided range 
        /// Provide the response model class along with its subclass
        /// </summary>
        /// <param name="url"></param>
        /// <param name="compId"></param>
        /// <returns></returns>
        public void TestPaginationStartRow<T, TR>(string url, string compId)
        {
            var pageSize = 100;
            var startRow = 0;
            string endPointUrl;
            HttpWebResponse response;

            var payLoadFormat = "{{filterBy: {{}},sortBy: {{}}, pages: {{total: 0, startRow: {0},resultsPerPage: {1}}}}}";

            string payload = string.Format(payLoadFormat, startRow, pageSize);
            if (url.Contains("?"))
                endPointUrl = url + "&dataFilter=" + payload;
            else
                endPointUrl = url + "?dataFilter=" + payload;

            //Act
            response = GetHttpWebResponse(endPointUrl, AuthHeader, HttpVerb.GET);
            //Extratct json string value
            var responsevalue = getHttpWebResponseValue(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Replacing .0000
            responsevalue = responsevalue.Replace(".0000", "");
            responsevalue = responsevalue.Replace(".0", "");

            // Assert
            var responseDataBef = JsonConvert.DeserializeObject<T>(responsevalue);
            Assert.IsNotNull(responseDataBef);
            var totalRecordsProp = responseDataBef.GetType().GetProperty("totalRecords");
            if (pageSize >= (int)totalRecordsProp.GetValue(responseDataBef))
            {
                if ((int)totalRecordsProp.GetValue(responseDataBef) == 1)
                    pageSize = 1;
                else
                    pageSize = (int)totalRecordsProp.GetValue(responseDataBef) / 2;

                payload = string.Format(payLoadFormat, startRow, pageSize);

                // Setup the API URL
                if (url.Contains("?"))
                    endPointUrl = url + "&dataFilter=" + payload;
                else
                    endPointUrl = url + "?dataFilter=" + payload;

                //Act
                response = GetHttpWebResponse(endPointUrl, AuthHeader, HttpVerb.GET);
                //Extratct json string value
                responsevalue = getHttpWebResponseValue(response);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                // Replacing .0000
                responsevalue = responsevalue.Replace(".0000", "");
                responsevalue = responsevalue.Replace(".0", "");

                // Assert
                responseDataBef = JsonConvert.DeserializeObject<T>(responsevalue);
                Assert.IsNotNull(responseDataBef);
            }

            // First request last record
            var recordsProp = responseDataBef.GetType().GetProperty("records");
            var responseDataBefRecord = ((List<TR>)recordsProp.GetValue(responseDataBef)).LastOrDefault();

            //Set up the api url 
            payload = string.Format(payLoadFormat, pageSize - 1, pageSize);
            if (url.Contains("?"))
                endPointUrl = url + "&dataFilter=" + payload;
            else
                endPointUrl = url + "?dataFilter=" + payload;

            //Act
            response = GetHttpWebResponse(endPointUrl, AuthHeader, HttpVerb.GET);
            //Extratct json string value
            responsevalue = getHttpWebResponseValue(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Replacing .0000 & .0 with ""
            responsevalue = responsevalue.Replace(".0000", "");
            responsevalue = responsevalue.Replace(".0", "");

            var responseData = JsonConvert.DeserializeObject<T>(responsevalue);
            Assert.IsNotNull(responseData);

            // Second request First record
            recordsProp = responseData.GetType().GetProperty("records");
            var responseDataRecord = ((List<TR>)recordsProp.GetValue(responseData)).FirstOrDefault();

            // Unique entity to compare Last and First Record
            var compProp = responseDataRecord.GetType().GetProperty(compId);
            Assert.AreEqual(compProp.GetValue(responseDataBefRecord), compProp.GetValue(responseDataRecord));
        }

    }
}
*/
