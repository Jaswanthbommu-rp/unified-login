using JsonApiSerializer;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.WebHook;
using UnifiedLogin.LandingAPI.Test.Extensions;
using UnifiedLogin.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.ControllerTest
{
    [ExcludeFromCodeCoverage]
    public class WebHookTests : TestBase
    {
        private static Guid _RealPageId = new Guid("C802694D-5553-4527-8616-3C0F434AE62D");
        private static Guid _adminRealPageId = new Guid("C802694D-1111-2222-3333-3C0F434AE62D");
        private static Guid _externalOrganizationRealPageId = DefaultUserClaim.ExternalCompanyRealPageId;
        private static string _CompanyName = "Test Company";
        private static DateTime _CreateDate = DateTime.MaxValue.ToUniversalTime();
        private static int _PartyId = 54321;
        private static int _ExternalPartyId = 71861;
        private static long _BooksMasterId = 12345;
        private static long _BooksCompanyMasterId = 15862;
        private static int _organizationTypeId = 6;
        private static int _vendorOrganizationTypeId = 14;
        private static int _otherOrganizationTypeId = 7;
        private static string _organizationTypeName = "Multifamily";
        private static string _invalidOrganizationTypeName = "InvalidOrgType";
        private static string _externalOrganizationTypeName = "Other";
        private static int _organizationDomainId = 1;
        private static string _organizationDomainName = "Primary";

        private static DefaultUserClaim _userClaim;

        private readonly string _mockJson_books_customercompany_deleted = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customercompany.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t\"link\": \"/customercompany?filter[customerCompanyId]={customerCompanyId}&filter[deletedAt]=not:null\",\r\n\t\t\"payload\": {\r\n    \t\t\"customerCompanyId\": 15862,\r\n    \t\t\"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n    \t\t\"replacementCustomerCompanyId\"  : 9999\r\n\t\t}\r\n\t}\r\n}\r\n";
        private readonly string _mockJson_books_customercompany_deleted_Signature = "2c136a645e98e682babdaba914c3ff2a81ac0d9fd41e60c9cd27e7fb74aef05d";

        private readonly string _mockJson_books_customercompany_deleted_missing_customercompanyid = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customercompany.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t\"link\": \"/customercompany?filter[customerCompanyId]={customerCompanyId}&filter[deletedAt]=not:null\",\r\n\t\t\"payload\": {\r\n    \t\t\"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n    \t\t\"replacementCustomerCompanyId\"  : 9999\r\n\t\t}\r\n\t}\r\n}\r\n";
        private readonly string _mockJson_books_customercompany_deleted_missing_customercompanyid_Signature = "37ba2498b33d96dfdcce3cb6993ad66b48e879d0904b1090a6a7857ef4e0775c";

        private readonly string _mockJson_books_customercompany_deleted_null_customercompanyid = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customercompany.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t\"link\": \"/customercompany?filter[customerCompanyId]={customerCompanyId}&filter[deletedAt]=not:null\",\r\n\t\t\"payload\": {\r\n    \t\t\"customerCompanyId\": null,\r\n    \t\t\"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n    \t\t\"replacementCustomerCompanyId\"  : 9999\r\n\t\t}\r\n\t}\r\n}\r\n";
        private readonly string _mockJson_books_customercompany_deleted_null_customercompanyid_Signature = "ad0f422783095cb66b87111ec0bdbb1bbc6eb34a2c074f0bd47e5a34410c2676";

        private readonly string _mockJson_books_customercompany_deleted_null_replacementcustomercompanyid = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customercompany.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t\"link\": \"/customercompany?filter[customerCompanyId]={customerCompanyId}&filter[deletedAt]=not:null\",\r\n\t\t\"payload\": {\r\n    \t\t\"customerCompanyId\": 15862,\r\n    \t\t\"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n    \t\t\"replacementCustomerCompanyId\"  : null\r\n\t\t}\r\n\t}\r\n}\r\n";
        private readonly string _mockJson_books_customercompany_deleted_null_replacementcustomercompanyid_Signature = "e17c9b4448e1f3b8b2ce2d2277650199a798c58cced15c1ef257346ffbb3921e";

        private readonly string _mockJson_books_customerproperty_deleted = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customerproperty.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t  \"link\": \"/customerproperty?filter[customerPropertyId]={customerPropertyId}&filter[deletedAt]=not:null\",\r\n\t\t  \"payload\": {\r\n\t\t    \"customerPropertyId\": 199685,\r\n\t\t    \"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n\t\t    \"replacementCustomerPropertyId\": 123456789\r\n\t\t  }\r\n\t}\r\n}\r\n";
        private readonly string _mockJson_books_customerproperty_deleted_Signature = "5a29729cf7e401e05905e2146761735e9bc23c80bd0d646c4d1d8674eae51c6c";

        private readonly string _mockJson_books_customerproperty_deleted_missing_customerpropertyid = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customerproperty.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t  \"link\": \"/customerproperty?filter[customerPropertyId]={customerPropertyId}&filter[deletedAt]=not:null\",\r\n\t\t  \"payload\": {\r\n\t\t    \"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n\t\t    \"replacementCustomerPropertyId\": 123456789\r\n\t\t  }\r\n\t}\r\n}\r\n";
        private readonly string _mockJson_books_customerproperty_deleted_missing_customerpropertyid_Signature = "244036174789556c720aef46357fda9741aad802a579496014e61221ed99ea2c";

        private readonly string _mockJson_books_customerproperty_deleted_null_customerpropertyid = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customerproperty.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t  \"link\": \"/customerproperty?filter[customerPropertyId]={customerPropertyId}&filter[deletedAt]=not:null\",\r\n\t\t  \"payload\": {\r\n\t\t    \"customerPropertyId\": null,\r\n\t\t    \"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n\t\t    \"replacementCustomerPropertyId\": 123456789\r\n\t\t  }\r\n\t}\r\n}\r\n";
        private readonly string _mockJson_books_customerproperty_deleted_null_customerpropertyid_Signature = "8c43650ba0b1e722120e4b9b7e264b76f6d3c817259d725faadba845361fc7b4";

        private readonly string _mockJson_books_customerproperty_deleted_null_replacementcustomerpropertyid = "{\r\n\t\"id\": \"601e13a6-7360-ceda-bf0c-41c62fa694c7\",\r\n\t\"topic\": \"books.customerproperty.deleted\",\r\n\t\"createdAt\": \"2020-04-21T08:25:31-05:00\",\r\n\t\"payload\": {\r\n\t\t  \"link\": \"/customerproperty?filter[customerPropertyId]={customerPropertyId}&filter[deletedAt]=not:null\",\r\n\t\t  \"payload\": {\r\n\t\t    \"customerPropertyId\": 199685,\r\n\t\t    \"deletedAt\": \"2020-02-21 11:35:43.000000-0600\",\r\n\t\t    \"replacementCustomerPropertyId\": null\r\n\t\t  }\r\n\t}\r\n}\r\n";
        private readonly string _mockJson_books_customerproperty_deleted_null_replacementcustomerpropertyid_Signature = "704e37bab4ae5534cc7f8f459e6b83b58e75c4d5e95a6b9a37ccf11bbb216fcb";

        private readonly string _mockJson_books_provisioning_upfmorder_create = "{\"id\":\"7ac983c3-bb3f-f5a6-baf1-e41b139d690b\",\"topic\":\"provisioning.upfmorder.create\",\"createdAt\":\"2020-05-19T12:59:54-05:00\",\"payload\":{\"source\":\"UPFM\",\"company\":{\"city\":\"SAN FRANCISCO\",\"state\":\"CA\",\"county\":\"SAN FRANCISCO COUNTY\",\"address\":\"1 BUSH ST STE 900\",\"country\":\"UNITED STATES\",\"postalCode\":\"94104-4425\",\"companyName\":\"VERITAS INVESTMENTS\",\"productCenters\":[],\"customerCompanyId\":1948,\"companyInstanceSourceId\":null},\"properties\":[{\"city\":\"SAN FRANCISCO\",\"state\":\"CA\",\"units\":35,\"county\":\"SAN FRANCISCO COUNTY\",\"address\":\"100 BRODERICK ST\",\"country\":\"UNITED STATES\",\"postalCode\":\"94117-3158\",\"propertyName\":\"100 BRODERICK\",\"productCenters\":[{\"productCenterSourceId\":\"17\"},{\"productCenterSourceId\":\"4\"}],\"customerPropertyId\":391411,\"propertyInstanceSourceId\":null}],\"customerEnvironment\":\"Primary\"}}";
        private readonly string _mockJson_books_provisioning_upfmorder_create_Signature = "13b82334dbf47345b48737af5eb59912870b4722aa1b33da9a02cfd418876acb";

        private readonly string _mockJson_books_provisioning_upfmorder_create_update = "{\"id\":\"7ac983c3-bb3f-f5a6-baf1-e41b139d690b\",\"topic\":\"provisioning.upfmorder.create\",\"createdAt\":\"2020-05-19T12:59:54-05:00\",\"payload\":{\"source\":\"UPFM\",\"company\":{\"city\":\"SAN FRANCISCO\",\"state\":\"CA\",\"county\":\"SAN FRANCISCO COUNTY\",\"address\":\"1 BUSH ST STE 900\",\"country\":\"UNITED STATES\",\"postalCode\":\"94104-4425\",\"companyName\":\"VERITAS INVESTMENTS\",\"productCenters\":[],\"customerCompanyId\":1948,\"companyInstanceSourceId\":\"" + _RealPageId + "\"},\"properties\":[{\"city\":\"SAN FRANCISCO\",\"state\":\"CA\",\"units\":35,\"county\":\"SAN FRANCISCO COUNTY\",\"address\":\"100 BRODERICK ST\",\"country\":\"UNITED STATES\",\"postalCode\":\"94117-3158\",\"propertyName\":\"100 BRODERICK\",\"productCenters\":[{\"productCenterSourceId\":\"17\"},{\"productCenterSourceId\":\"4\"}],\"customerPropertyId\":391411,\"propertyInstanceSourceId\":null}],\"customerEnvironment\":\"Primary\"}}";
        private readonly string _mockJson_books_provisioning_upfmorder_create_update_Signature = "ab5d3ed246ec6a2ed01a33afc7f699e44b3e18ec7bb5b0a4fcd3dd4787370fed";

        private readonly string _mockJson_books_provisioning_upfmorder_create_nulldomain = "{\"id\":\"7ac983c3-bb3f-f5a6-baf1-e41b139d690b\",\"topic\":\"provisioning.upfmorder.create\",\"createdAt\":\"2020-05-19T12:59:54-05:00\",\"payload\":{\"source\":\"UPFM\",\"company\":{\"city\":\"SAN FRANCISCO\",\"state\":\"CA\",\"county\":\"SAN FRANCISCO COUNTY\",\"address\":\"1 BUSH ST STE 900\",\"country\":\"UNITED STATES\",\"postalCode\":\"94104-4425\",\"companyName\":\"VERITAS INVESTMENTS\",\"productCenters\":[],\"customerCompanyId\":1948,\"companyInstanceSourceId\":null},\"properties\":[{\"city\":\"SAN FRANCISCO\",\"state\":\"CA\",\"units\":35,\"county\":\"SAN FRANCISCO COUNTY\",\"address\":\"100 BRODERICK ST\",\"country\":\"UNITED STATES\",\"postalCode\":\"94117-3158\",\"propertyName\":\"100 BRODERICK\",\"productCenters\":[{\"productCenterSourceId\":\"17\"},{\"productCenterSourceId\":\"4\"}],\"customerPropertyId\":391411,\"propertyInstanceSourceId\":null}],\"customerEnvironment\":null}}";
        private readonly string _mockJson_books_provisioning_upfmorder_create_nulldomain_Signature = "5bf97d010439a93a40251271779f511f2514d15cfaf809dcebdb2b377577f510";

        private readonly string _mockJson_books_provisioning_upfmorder_cancel = "{\"id\":\"0ad8351d-c38b-abe2-0619-954d5836683b\",\"topic\":\"provisioning.upfmorder.cancel\",\"createdAt\":\"2020-05-19T12:59:54-05:00\",\"payload\":{\"source\":\"UPFM\",\"company\":{\"productCenters\":[{\"productCenterSourceId\":\"17\"}],\"companyInstanceSourceId\":\"" + _RealPageId + "\"},\r\n\t\t\"properties\":[]}}";
        private readonly string _mockJson_books_provisioning_upfmorder_cancel_Signature = "b64bda1dfd2fd5b77a24d69b780d761902d525ee8ac17cf8a8952a8445cd930c";

        private readonly string _mockJson_books_provisioning_upfmorder_cancel_Invalid_CompanyInstance = "{\"id\":\"0ad8351d-c38b-abe2-0619-954d5836683b\",\"topic\":\"provisioning.upfmorder.cancel\",\"createdAt\":\"2020-05-19T12:59:54-05:00\",\"payload\":{\"source\":\"UPFM\",\"company\":{\"productCenters\":[{\"productCenterSourceId\":\"17\"}],\"companyInstanceSourceId\":\"\"},\r\n\t\t\"properties\":[]}}";
        private readonly string _mockJson_books_provisioning_upfmorder_cancel_Invalid_CompanyInstance_Signature = "7c656ff8db5c511837cf66c34c1bb895f5d82b54081562dba74eb275e0783109";

        private readonly string _mockJson_books_provisioning_upfmorder_cancel_Invalid_ProductInstance = "{\"id\":\"0ad8351d-c38b-abe2-0619-954d5836683b\",\"topic\":\"provisioning.upfmorder.cancel\",\"createdAt\":\"2020-05-19T12:59:54-05:00\",\"payload\":{\"source\":\"UPFM\",\"company\":{\"productCenters\":[{\"productCenterSourceId\":\"\"}],\"companyInstanceSourceId\":\"" + _RealPageId + "\"},\r\n\t\t\"properties\":[]}}";
        private readonly string _mockJson_books_provisioning_upfmorder_cancel_Invalid_ProductInstance_Signature = "83eb77869a79e5e9754eca36024a40a591db928bfa48befff7c210dcb0434819";

        private readonly string _mockJson_books_provisioning_upfmclone_create = "{\r\n    \"id\": \"6b80fcb5-2538-cff9-3807-89df7ed4a166\",\r\n    \"topic\": \"provisioning.upfmclone.create\",\r\n    \"payload\": {\r\n        \"source\": \"UPFM\",\r\n        \"company\": {\r\n            \"city\": \"\",\r\n            \"state\": \"\",\r\n            \"county\": \"\",\r\n            \"address\": \"\",\r\n            \"country\": \"\",\r\n            \"postalCode\": \"\",\r\n            \"companyName\": \"hotsprov634958558\",\r\n            \"productCenters\": [],\r\n            \"customerCompanyId\": 1386886,\r\n            \"customerEnvironment\": \"Primary\",\r\n            \"companyInstanceSourceId\": null,\r\n            \"cloneCompanyInstanceSourceId\": \"3d3865fb-4be4-401f-96ab-c552aee97512\"\r\n        },\r\n        \"properties\": [{\r\n                \"city\": \"Durganborough\",\r\n                \"state\": \"HI\",\r\n                \"units\": 152,\r\n                \"county\": null,\r\n                \"address\": \"9884 Hintz Mount\",\r\n                \"country\": \"USA\",\r\n                \"postalCode\": \"88093-5599\",\r\n                \"propertyName\": \"teshotsapril232021_b649275\",\r\n                \"productCenters\": [{\r\n                        \"productCenterSourceId\": \"63\"\r\n                    }\r\n                ],\r\n                \"customerPropertyId\": 1571329,\r\n                \"customerEnvironment\": \"Primary\",\r\n                \"propertyInstanceSourceId\": null,\r\n                \"clonePropertyInstanceSourceId\": \"73c95fb8-e17d-4a4b-98b9-d22bf9c1b1af\"\r\n            }\r\n        ],\r\n        \"customerEnvironment\": \"Primary\"\r\n    },\r\n    \"createdAt\": \"2021-08-26T14:27:51-05:00\"\r\n}\r\n";
        private readonly string _mockJson_books_provisioning_upfmclone_create_Signature = "446dc41b9c29e1d51b5851b617ecfe9ca857b7030df2550baf8546e56d255b54";

        private readonly string _mockJson_books_provisioning_upfmclone_create_missing_clonecompanyid = "{\r\n    \"id\": \"6b80fcb5-2538-cff9-3807-89df7ed4a166\",\r\n    \"topic\": \"provisioning.upfmclone.create\",\r\n    \"payload\": {\r\n        \"source\": \"UPFM\",\r\n        \"company\": {\r\n            \"city\": \"\",\r\n            \"state\": \"\",\r\n            \"county\": \"\",\r\n            \"address\": \"\",\r\n            \"country\": \"\",\r\n            \"postalCode\": \"\",\r\n            \"companyName\": \"hotsprov634958558\",\r\n            \"productCenters\": [],\r\n            \"customerCompanyId\": 1386886,\r\n            \"customerEnvironment\": \"Primary\",\r\n            \"companyInstanceSourceId\": null\r\n        },\r\n        \"properties\": [{\r\n                \"city\": \"Durganborough\",\r\n                \"state\": \"HI\",\r\n                \"units\": 152,\r\n                \"county\": null,\r\n                \"address\": \"9884 Hintz Mount\",\r\n                \"country\": \"USA\",\r\n                \"postalCode\": \"88093-5599\",\r\n                \"propertyName\": \"teshotsapril232021_b649275\",\r\n                \"productCenters\": [{\r\n                        \"productCenterSourceId\": \"63\"\r\n                    }\r\n                ],\r\n                \"customerPropertyId\": 1571329,\r\n                \"customerEnvironment\": \"Primary\",\r\n                \"propertyInstanceSourceId\": null,\r\n                \"clonePropertyInstanceSourceId\": \"73c95fb8-e17d-4a4b-98b9-d22bf9c1b1af\"\r\n            }\r\n        ],\r\n        \"customerEnvironment\": \"Primary\"\r\n    },\r\n    \"createdAt\": \"2021-08-26T14:27:51-05:00\"\r\n}\r\n";
        private readonly string _mockJson_books_provisioning_upfmclone_create_missing_clonecompanyid_Signature = "46e859bef104f269265861e2432afee9d26ec9c3ead183ced20c2efce401d9b4";

        private readonly string _mockJson_books_provisioning_upfmclone_create_missing_clonepropertyid = "{\r\n    \"id\": \"6b80fcb5-2538-cff9-3807-89df7ed4a166\",\r\n    \"topic\": \"provisioning.upfmclone.create\",\r\n    \"payload\": {\r\n        \"source\": \"UPFM\",\r\n        \"company\": {\r\n            \"city\": \"\",\r\n            \"state\": \"\",\r\n            \"county\": \"\",\r\n            \"address\": \"\",\r\n            \"country\": \"\",\r\n            \"postalCode\": \"\",\r\n            \"companyName\": \"hotsprov634958558\",\r\n            \"productCenters\": [],\r\n            \"customerCompanyId\": 1386886,\r\n            \"customerEnvironment\": \"Primary\",\r\n            \"companyInstanceSourceId\": null,\r\n            \"cloneCompanyInstanceSourceId\": \"3d3865fb-4be4-401f-96ab-c552aee97512\"\r\n        },\r\n        \"properties\": [{\r\n                \"city\": \"Durganborough\",\r\n                \"state\": \"HI\",\r\n                \"units\": 152,\r\n                \"county\": null,\r\n                \"address\": \"9884 Hintz Mount\",\r\n                \"country\": \"USA\",\r\n                \"postalCode\": \"88093-5599\",\r\n                \"propertyName\": \"teshotsapril232021_b649275\",\r\n                \"productCenters\": [{\r\n                        \"productCenterSourceId\": \"63\"\r\n                    }\r\n                ],\r\n                \"customerPropertyId\": 1571329,\r\n                \"customerEnvironment\": \"Primary\",\r\n                \"propertyInstanceSourceId\": null\r\n            }\r\n        ],\r\n        \"customerEnvironment\": \"Primary\"\r\n    },\r\n    \"createdAt\": \"2021-08-26T14:27:51-05:00\"\r\n}\r\n";
        private readonly string _mockJson_books_provisioning_upfmclone_create_missing_clonepropertyid_Signature = "7e7aec2a5b21048df8bc4ef3c50c39cfb2e16a9a1ae8d88a748371796c8e722c";

        private readonly string _mockJson_books_provisioning_upfmclone_create_unknown_clonecompanyid = "{\r\n    \"id\": \"6b80fcb5-2538-cff9-3807-89df7ed4a166\",\r\n    \"topic\": \"provisioning.upfmclone.create\",\r\n    \"payload\": {\r\n        \"source\": \"UPFM\",\r\n        \"company\": {\r\n            \"city\": \"\",\r\n            \"state\": \"\",\r\n            \"county\": \"\",\r\n            \"address\": \"\",\r\n            \"country\": \"\",\r\n            \"postalCode\": \"\",\r\n            \"companyName\": \"hotsprov634958558\",\r\n            \"productCenters\": [],\r\n            \"customerCompanyId\": 1386886,\r\n            \"customerEnvironment\": \"Primary\",\r\n            \"companyInstanceSourceId\": null,\r\n            \"cloneCompanyInstanceSourceId\": \"3d3865fb-4be4-401f-96ab-c552aee97512\"\r\n        },\r\n        \"properties\": [{\r\n                \"city\": \"Durganborough\",\r\n                \"state\": \"HI\",\r\n                \"units\": 152,\r\n                \"county\": null,\r\n                \"address\": \"9884 Hintz Mount\",\r\n                \"country\": \"USA\",\r\n                \"postalCode\": \"88093-5599\",\r\n                \"propertyName\": \"teshotsapril232021_b649275\",\r\n                \"productCenters\": [{\r\n                        \"productCenterSourceId\": \"63\"\r\n                    }\r\n                ],\r\n                \"customerPropertyId\": 1571329,\r\n                \"customerEnvironment\": \"Primary\",\r\n                \"propertyInstanceSourceId\": null,\r\n                \"clonePropertyInstanceSourceId\": \"73c95fb8-e17d-4a4b-98b9-d22bf9c1b1af\"\r\n            }\r\n        ],\r\n        \"customerEnvironment\": \"Primary\"\r\n    },\r\n    \"createdAt\": \"2021-08-26T14:27:51-05:00\"\r\n}\r\n";
        private readonly string _mockJson_books_provisioning_upfmclone_create_unknown_clonecompanyid_Signature = "446dc41b9c29e1d51b5851b617ecfe9ca857b7030df2550baf8546e56d255b54";

        private readonly string _mockJson_books_provisioning_upfmclone_create_invalid_clonecompanyid = "{\r\n    \"id\": \"6b80fcb5-2538-cff9-3807-89df7ed4a166\",\r\n    \"topic\": \"provisioning.upfmclone.create\",\r\n    \"payload\": {\r\n        \"source\": \"UPFM\",\r\n        \"company\": {\r\n            \"city\": \"\",\r\n            \"state\": \"\",\r\n            \"county\": \"\",\r\n            \"address\": \"\",\r\n            \"country\": \"\",\r\n            \"postalCode\": \"\",\r\n            \"companyName\": \"hotsprov634958558\",\r\n            \"productCenters\": [],\r\n            \"customerCompanyId\": 1386886,\r\n            \"customerEnvironment\": \"Primary\",\r\n            \"companyInstanceSourceId\": null,\r\n            \"cloneCompanyInstanceSourceId\": \"3d3865fb-4be4-401f-xxxx-c552aee97512\"\r\n        },\r\n        \"properties\": [{\r\n                \"city\": \"Durganborough\",\r\n                \"state\": \"HI\",\r\n                \"units\": 152,\r\n                \"county\": null,\r\n                \"address\": \"9884 Hintz Mount\",\r\n                \"country\": \"USA\",\r\n                \"postalCode\": \"88093-5599\",\r\n                \"propertyName\": \"teshotsapril232021_b649275\",\r\n                \"productCenters\": [{\r\n                        \"productCenterSourceId\": \"63\"\r\n                    }\r\n                ],\r\n                \"customerPropertyId\": 1571329,\r\n                \"customerEnvironment\": \"Primary\",\r\n                \"propertyInstanceSourceId\": null,\r\n                \"clonePropertyInstanceSourceId\": \"73c95fb8-xxxx-4a4b-98b9-d22bf9c1b1af\"\r\n            }\r\n        ],\r\n        \"customerEnvironment\": \"Primary\"\r\n    },\r\n    \"createdAt\": \"2021-08-26T14:27:51-05:00\"\r\n}\r\n";
        private readonly string _mockJson_books_provisioning_upfmclone_create_invalid_clonecompanyid_Signature = "f3b3148b4180aa9b581c35c5adb86c2928011b6a35e1309d074a62c07d1bd788";

        //private readonly string _mockJsonCompanyList = "[\r\n\t{\r\n\t\t\"PartyId\": \""+_PartyId+"\",\r\n\t\t\"Name\": \""+_CompanyName+"\",\r\n\t\t\"OrganizationRealPageId\": \""+_RealPageId+"\",\r\n\t\t\"BooksMasterId\": \""+_BooksMasterId+"\",\r\n\t\t\"BooksCustomerMasterId\": \""+_BooksCompanyMasterId+"\",\r\n\t\t\"SettingName\": \"RealPageEmployeeAccessID\",\r\n\t\t\"PersonRealPageId\": \"guid\",\r\n\t\t\"LoginName\": \"admin@test.com\",\r\n\t}\r\n]";
        private readonly string _mockJsonCompanyList = "{\r\n\t\t\"PartyId\": \"" + _PartyId + "\",\r\n\t\t\"Name\": \"" + _CompanyName + "\",\r\n\t\t\"OrganizationRealPageId\": \"" + _RealPageId + "\",\r\n\t\t\"BooksMasterId\": \"" + _BooksMasterId + "\",\r\n\t\t\"BooksCustomerMasterId\": \"" + _BooksCompanyMasterId + "\",\r\n\t\t\"SettingName\": \"RealPageEmployeeAccessID\",\r\n\t\t\"PersonRealPageId\": \"guid\",\r\n\t\t\"LoginName\": \"admin@test.com\",\r\n\t}";

        private readonly string _mockJson_books_provisioning_upfmvendor_create = "{\r\n\t\"id\": \"5d85fb43-8fa7-2c7a-b00c-a5761b7f3686\",\r\n\t\"topic\": \"books.upfmvendor.create\",\r\n    \"payload\": {\r\n        \"user\": {\r\n            \"firstName\": \"Liza\",\r\n            \"lastName\": \"Jones\",\r\n            \"email\": \"ljones@test.com\"\r\n        },\r\n        \"customerCompanyId\": 1380567,\r\n        \"companyInstanceSourceId\" : \"2230095\",\r\n        \"source\": \"VMP\"\r\n    },\r\n\t\"createdAt\": \"2021-05-28T11:06:02-05:00\"\r\n}";
        private readonly string _mockJson_books_provisioning_upfmvendor_create_Signature = "f85348195246610f8efc3374718335d7047863d7b922d31a4d481ab5b5556f04";

        private readonly string _mockJson_books_provisioning_upfmvendor_create_invalid_company = "{\r\n\t\"id\": \"5d85fb43-8fa7-2222-b00c-a5761b7f3686\",\r\n\t\"topic\": \"books.upfmvendor.create\",\r\n    \"payload\": {\r\n        \"user\": {\r\n            \"firstName\": \"Bad\",\r\n            \"lastName\": \"\",\r\n            \"email\": \"fail@test.com\"\r\n        },\r\n        \"customerCompanyId\": 55555,\r\n        \"companyInstanceSourceId\" : \"11111\",\r\n        \"source\": \"VMP\"\r\n    },\r\n\t\"createdAt\": \"2021-05-28T11:06:02-05:00\"\r\n}";
        private readonly string _mockJson_books_provisioning_upfmvendor_create_invalid_company_Signature = "10c35aa07288db2471c288b931506a08ea789438f73cefab5793957fdc4ed057";

        private List<OrganizationType> _organizationTypeList;
        private List<OrganizationDomain> _organizationDomains;

        private Organization _organization = null;
        //private List<ProductInternalSetting> _productInternalSettings;
        private static List<GbProductMap> _gbProductMap;

        private static List<ContactMechanismUsageType> _contactMechanismUsageTypes;

        public WebHookTests()
        {
            _gbProductMap = new List<GbProductMap>
            {
                new GbProductMap() { BooksProductCode = "OS", Name = "OneSite", ProductId = 1, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "UPFM", Name = "Unified Platform", ProductId = 3, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "AO", Name = "Asset Optimization", ProductId = 4, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PW", Name = "Propertyware", ProductId = 5, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "L2L", Name = "Lead2Lease", ProductId = 6, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "YS", Name = "YieldStar", ProductId = 7, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ACCT", Name = "Financial Suite", ProductId = 8, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LS", Name = "Marketing Center", ProductId = 9, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LVL1", Name = "Prospect Contact Center", ProductId = 10, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "??", Name = "Social", ProductId = 11, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OPSB", Name = "Ops Bid", ProductId = 12, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OPS", Name = "Spend Management", ProductId = 13, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OMS", Name = "Client Portal", ProductId = 14, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LD", Name = "Renters Insurance", ProductId = 15, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "CD", Name = "Vendor Credentialing", ProductId = 16, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "AB", Name = "Resident Portals", ProductId = 17, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "NWP", Name = "Utility Management", ProductId = 18, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LP", Name = "Product Learning Portal", ProductId = 19, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "DOC", Name = "Document Director", ProductId = 20, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OSC", Name = "L&R Conversion Utility", ProductId = 21, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "OC", Name = "OmniChannel", ProductId = 22, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ONST", Name = "On-Site", ProductId = 23, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "RA", Name = "Unified Data Management", ProductId = 24, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SP", Name = "Self-provisioning portal", ProductId = 25, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "UA", Name = "Unified Amenities", ProductId = 26, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "MT", Name = "Migration Tool Application", ProductId = 27, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PUPDATE", Name = "Product Updates", ProductId = 28, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "BI", Name = "Business Intelligence", ProductId = 29, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "PA", Name = "Performance Analytics", ProductId = 30, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "MA", Name = "Investment Analytics", ProductId = 31, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "PO", Name = "YieldStar", ProductId = 32, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "AX", Name = "Axiometrics", ProductId = 33, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "BM", Name = "Benchmarking", ProductId = 34, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "null", Name = "Support Tool", ProductId = 35, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ELMS", Name = "EasyLMS", ProductId = 36, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PHOTO", Name = "Property Photos", ProductId = 37, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "VMP", Name = "Vendor Marketplace", ProductId = 38, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "IMP", Name = "Integration Marketplace", ProductId = 39, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ILMLM", Name = "ILM Lead Management", ProductId = 40, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ILMLA", Name = "ILM Leasing Analytics", ProductId = 41, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SM", Name = "Settings Management", ProductId = 43, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "RPM", Name = "Portfolio Management", ProductId = 44, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "CIMPL", Name = "CIMPL", ProductId = 45, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SSM", Name = "Site Spend Management Portal", ProductId = 46, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "DIQ", Name = "Deposit Alternative", ProductId = 47, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "CPAY", Name = "ClickPay", ProductId = 48, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "HLP", Name = "Simon Help Center", ProductId = 49, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SLM", Name = "Senior Lead Management", ProductId = 50, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LRO", Name = "LRO", ProductId = 51, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "AA", Name = "Amenity Optimization", ProductId = 52, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "AIRM", Name = "AI Revenue Management", ProductId = 53, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "RC", Name = "Rent Control", ProductId = 54, UDMSourceCode = "AO" },
                new GbProductMap() { BooksProductCode = "RENO", Name = "Renovation Manager", ProductId = 55, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SET", Name = "Unified Settings", ProductId = 56, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "SMS-T", Name = "Intelligent Building", ProductId = 57, UDMSourceCode = "IB" },
                new GbProductMap() { BooksProductCode = "SMS-E", Name = "Intelligent Building Energy", ProductId = 58, UDMSourceCode = "IB" },
                new GbProductMap() { BooksProductCode = "SMS-W", Name = "Intelligent Building Water", ProductId = 59, UDMSourceCode = "IB" },
                new GbProductMap() { BooksProductCode = "PME", Name = "PME Dashboard", ProductId = 62, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "RMA", Name = "Market Analytics", ProductId = 66, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "ST", Name = "Support Tool", ProductId = 35, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "HOTS", Name = "Hands On Training System", ProductId = 63, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "PEQ", Name = "P2 Engagement Queue", ProductId = 64, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "LeaseLabs", Name = "LeaseLabs", ProductId = 68, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "RPT", Name = "Reporting", ProductId = 67, UDMSourceCode = null },
                new GbProductMap() { BooksProductCode = "6247", Name = "Self-Guided Tour", ProductId = 65, UDMSourceCode = null },

            };

            _userClaim = new DefaultUserClaim() { CorrelationId = Guid.NewGuid() };

            _organization = new Organization()
            {
                RealPageId = _RealPageId,
                CreateDate = _CreateDate,
                Name = _CompanyName,
                PartyId = _PartyId,
                BooksMasterId = _BooksMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = _organizationTypeId,
                organizationType = new OrganizationType()
                {
                    OrganizationTypeId = _organizationTypeId
                },
                OrganizationDomain = new OrganizationDomain()
                {
                    OrganizationDomainId = _organizationDomainId
                }
            };

            _organizationTypeList = new List<OrganizationType>()
            {
                new OrganizationType()
                {
                    OrganizationTypeId = 6,
                    Name = "Multifamily",
                    CreateDate = new DateTime()
                },
                new OrganizationType()
                {
                    OrganizationTypeId = 14,
                    Name = "Vendor",
                    CreateDate = new DateTime()
                },
                new OrganizationType()
                {
                    OrganizationTypeId = 7,
                    Name = "Other",
                    CreateDate = new DateTime()
                }
            };

            _organizationDomains = new List<OrganizationDomain>()
            {
                new OrganizationDomain()
                {
                    OrganizationDomainId = 1,
                    Name = "Primary",
                    CreateDate = new DateTime()
                },
                new OrganizationDomain()
                {
                    OrganizationDomainId = 2,
                    Name = "UAT",
                    CreateDate = new DateTime()
                }
            };

            //_productInternalSettings = new List<ProductInternalSetting>()
            //{
            //    new ProductInternalSetting() { Name = "TiboWebHookSigningSecret", Value = _mockTiboWebHookSigningSecret },
            //    new ProductInternalSetting() { Name = "IsCloneUsersProcessEnabledForHOTS", Value = "1" },
            //    new ProductInternalSetting() { Name = "ExcludeProductFromOrgSupportUser", Value = "3,4,8,14,28,36,56" }
            //};
            _contactMechanismUsageTypes = new List<ContactMechanismUsageType>
            {
                new ContactMechanismUsageType() { ContactMechanismUsageTypeId = 123, Name = "Email Notification" },
                new ContactMechanismUsageType() { ContactMechanismUsageTypeId = 345, Name = "Email" },
            };

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);
        }

        [Fact]
        public void Post_Books_NullInput()
        {
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            //Act
            HttpResponseMessage response = webHookController.PostBooks(null);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);

            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"Missing Content.\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted_missing_customercompanyid);
            //Act
            response = webHookController.PostBooks(thinEvent);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);

            message = response.Content.ReadAsStringAsync().Result;
            expectedValue = "\"Missing Signature.\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_InvalidSignature()
        {
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted_missing_customercompanyid);
            webHookController.Request.Headers.Add("signature", "12345");

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted_missing_customercompanyid);

            //Act
            var cacheKey = "productInternalSetting_" + (int)ProductEnum.UnifiedPlatform;
            ObjectCache cache = MemoryCache.Default;
            cache.Remove(cacheKey);

            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);

            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"Invalid Signature.\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_MissingSecretKey()
        {
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                .Returns(new List<ProductInternalSetting>());

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted_missing_customercompanyid);
            webHookController.Request.Headers.Add("signature", "12345");

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted_missing_customercompanyid);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);

            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"Missing Signing Secret.\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_Success()
        {
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DataImportMappingUpdate, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted);
            new RPObjectCache().BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_CompanyNotFound()
        {
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);

        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_Failed()
        {
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            List<dynamic> companyList = new List<dynamic>();
            companyList.Add(JsonConvert.DeserializeObject<dynamic>(_mockJsonCompanyList));

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

            mockRepository
                .Setup(m => m.GetMany<dynamic>(StoredProcNameConstants.SP_ListOrganizations, null))
                .Returns(companyList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DataImportMappingUpdate, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = "SQL Error happened here" });

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);
            
            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);

            //Assert
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);

            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"SQL Error happened here id not updated\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_CompanyIdAttributeMissing()
        {
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted_missing_customercompanyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_missing_customercompanyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted_missing_customercompanyid);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_CompanyIdAttributeNull()
        {
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            new RPObjectCache().BustCache();

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted_null_customercompanyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_null_customercompanyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted_null_customercompanyid);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Update_CustomerCompany_BooksMasterId_ReplacementCompanyIdAttributeNull()
        {
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(() => null);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);
            
            new RPObjectCache().BustCache();

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customercompany_deleted_null_replacementcustomercompanyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customercompany_deleted_null_replacementcustomercompanyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customercompany_deleted_null_replacementcustomercompanyid);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Update_CustomerProperty_BooksMasterId_Success()
        {
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePropertyMappingReMap, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            new RPObjectCache().BustCache();

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customerproperty_deleted);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customerproperty_deleted_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customerproperty_deleted);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Update_CustomerProperty_CustomerPropertyIdAttributeMissing()
        {
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            new RPObjectCache().BustCache();

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customerproperty_deleted_missing_customerpropertyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customerproperty_deleted_missing_customerpropertyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customerproperty_deleted_missing_customerpropertyid);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Update_CustomerProperty_CustomerPropertyIdNull()
        {
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            new RPObjectCache().BustCache();

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customerproperty_deleted_null_customerpropertyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customerproperty_deleted_null_customerpropertyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customerproperty_deleted_null_customerpropertyid);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Update_CustomerProperty_ReplacementCustomerPropertyIdNull()
        {
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };
            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_customerproperty_deleted_null_replacementcustomerpropertyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_customerproperty_deleted_null_replacementcustomerpropertyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_customerproperty_deleted_null_replacementcustomerpropertyid);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        #region UPFMOrder_Create
        [Fact]
        public void Post_Books_Provisioning_UPFMOrder_Create_Success()
        {
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            CustomerCompany customercompany = new CustomerCompany() { CustomerCompanyId = 1948, IsActive = true, CompanyName = "Test Company", MigrationStatus = "migrated", CompanyType = _organizationTypeName }; //Category = "rpup"
            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>() { new CustomerCompanyMap() { CompanyInstanceSourceId = "1234567", Source = "OS" } };

            UserLoginOnly userLoginOnly = new UserLoginOnly()
            {
                UserId = 3,
                PartyId = 1,
                LoginName = $"{customercompany.CustomerCompanyId}admin@realpage.com",
                PasswordHash = ""
            };
            UserLoginOnly userLoginOnlyNull = null;

            Guid propertyGuid = new Guid("5C04F18A-FC9B-4A13-AAAF-E26DA83CE516");

            HttpResponseMessage responseCustomerCompany = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(customercompany, new JsonApiSerializerSettings());
            responseCustomerCompany.Content = new StringContent(jsonToSave);

            HttpResponseMessage responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = JsonConvert.SerializeObject(mapResource, new JsonApiSerializerSettings());
            responseMapResource.Content = new StringContent(jsonToSave);

            HttpResponseMessage responsePropertyDetail = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = "{\n\t\"data\": {\n\t\t\"type\": \"customerproperty\",\n\t\t\"id\": \"391411\",\n\t\t\"attributes\": {\n\t\t\t\"customerPropertyId\": 391411,\n\t\t\t\"customerCompanyId\": 1234,\n\t\t\t\"masterPropertyId\": 12345,\n\t\t\t\"propertyName\": \"Test Property\",\n\t\t\t\"address\": {\n\t\t\t\t\"address\": \"11623 Pleasant Meadow Dr\",\n\t\t\t\t\"city\": \"North Potomac\",\n\t\t\t\t\"state\": \"MD\",\n\t\t\t\t\"country\": \"USA\",\n\t\t\t\t\"county\": \"Montgomery\",\n\t\t\t\t\"postalCode\": \"20878-4258\",\n\t\t\t\t\"latitude\": 39.089137,\n\t\t\t\t\"longitude\": -77.238857\n\t\t\t},\n\t\t\t\"units\": null,\n\t\t\t\"stories\": null,\n\t\t\t\"bedCount\": null,\n\t\t\t\"squareFeet\": null,\n\t\t\t\"yearBuilt\": null,\n\t\t\t\"renovationStartDate\": null,\n\t\t\t\"renovationEndDate\": null,\n\t\t\t\"createdAt\": \"2020-08-07 12:49:59.000000-0500\",\n\t\t\t\"modifiedAt\": \"2020-08-07 12:49:59.000000-0500\",\n\t\t\t\"deletedAt\": null,\n\t\t\t\"certifiedAt\": null,\n\t\t\t\"createdBy\": null,\n\t\t\t\"modifiedBy\": null,\n\t\t\t\"geocoded\": true,\n\t\t\t\"isUat\": false,\n\t\t\t\"apn\": \"\",\n\t\t\t\"fips\": \"\",\n\t\t\t\"propertyType\": \"Company\",\n\t\t\t\"propertySubType\": null,\n\t\t\t\"googleLatitude\": null,\n\t\t\t\"googleLongitude\": null,\n\t\t\t\"constructionStatus\": \"Completed\",\n\t\t\t\"constructionType\": null,\n\t\t\t\"assetClass\": null,\n\t\t\t\"buildings\": null,\n\t\t\t\"modifiedSource\": null,\n\t\t\t\"migrationStatus\": null,\n\t\t\t\"hasMedia\": \"Deprecated Field\",\n\t\t\t\"mediaTypeId\": null,\n\t\t\t\"assetType\": \"Not an asset\",\n\t\t\t\"isActive\": false,\n\t\t\t\"companyRelationship\": null,\n\t\t\t\"startDate\": null,\n\t\t\t\"endDate\": null\n\t\t},\n\t\t\"links\": {\n\t\t\t\"self\": \"/customerproperty/391411\"\n\t\t}\n\t}\n}";
            responsePropertyDetail.Content = new StringContent(jsonToSave);

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_SetupOrganization, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = "", RealPageId = _RealPageId });

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DataImportMappingUpdate, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository
                .SetupSequence(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.IsAny<object>()))
                .Returns(userLoginOnly);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_CreateOrganizationProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePropertyInstance, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 12345, RealPageId = propertyGuid, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany/{customercompany.CustomerCompanyId}", responseCustomerCompany);
            //mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[companyInstance.greenBookCares]=true&filter[customerCompanyId]={customercompany.CustomerCompanyId}&include=companyInstance&include=companyInstance.attributes", responseMapResource);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[customerCompanyId]={customercompany.CustomerCompanyId}&include=companyInstance&include=companyInstance.attributes", responseMapResource);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customerproperty/391411", responsePropertyDetail);
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/companyinstance", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            mockHttpMessageHandler.Setup(HttpMethod.Put, $"http://localhost/propertyinstance/{propertyGuid}/{ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)}", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/productcenterenablement/enable", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmorder_create);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmorder_create_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmorder_create);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMOrder_Unknown_Organization_Type_Success()
        {
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            CustomerCompany customercompany = new CustomerCompany() { CustomerCompanyId = 1948, IsActive = true, CompanyName = "Test Company", MigrationStatus = "migrated", CompanyType = _invalidOrganizationTypeName }; //Category = "rpup"
            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>() { new CustomerCompanyMap() { CompanyInstanceSourceId = "1234567", Source = "OS" } };

            UserLoginOnly userLoginOnly = new UserLoginOnly()
            {
                UserId = 3,
                PartyId = 1,
                LoginName = $"{customercompany.CustomerCompanyId}admin@realpage.com",
                PasswordHash = ""
            };
            UserLoginOnly userLoginOnlyNull = null;

            Guid propertyGuid = new Guid("5C04F18A-FC9B-4A13-AAAF-E26DA83CE516");

            HttpResponseMessage responseCustomerCompany = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(customercompany, new JsonApiSerializerSettings());
            responseCustomerCompany.Content = new StringContent(jsonToSave);

            HttpResponseMessage responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = JsonConvert.SerializeObject(mapResource, new JsonApiSerializerSettings());
            responseMapResource.Content = new StringContent(jsonToSave);

            HttpResponseMessage responsePropertyDetail = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = "{\n\t\"data\": {\n\t\t\"type\": \"customerproperty\",\n\t\t\"id\": \"391411\",\n\t\t\"attributes\": {\n\t\t\t\"customerPropertyId\": 391411,\n\t\t\t\"customerCompanyId\": 1234,\n\t\t\t\"masterPropertyId\": 12345,\n\t\t\t\"propertyName\": \"Test Property\",\n\t\t\t\"address\": {\n\t\t\t\t\"address\": \"11623 Pleasant Meadow Dr\",\n\t\t\t\t\"city\": \"North Potomac\",\n\t\t\t\t\"state\": \"MD\",\n\t\t\t\t\"country\": \"USA\",\n\t\t\t\t\"county\": \"Montgomery\",\n\t\t\t\t\"postalCode\": \"20878-4258\",\n\t\t\t\t\"latitude\": 39.089137,\n\t\t\t\t\"longitude\": -77.238857\n\t\t\t},\n\t\t\t\"units\": null,\n\t\t\t\"stories\": null,\n\t\t\t\"bedCount\": null,\n\t\t\t\"squareFeet\": null,\n\t\t\t\"yearBuilt\": null,\n\t\t\t\"renovationStartDate\": null,\n\t\t\t\"renovationEndDate\": null,\n\t\t\t\"createdAt\": \"2020-08-07 12:49:59.000000-0500\",\n\t\t\t\"modifiedAt\": \"2020-08-07 12:49:59.000000-0500\",\n\t\t\t\"deletedAt\": null,\n\t\t\t\"certifiedAt\": null,\n\t\t\t\"createdBy\": null,\n\t\t\t\"modifiedBy\": null,\n\t\t\t\"geocoded\": true,\n\t\t\t\"isUat\": false,\n\t\t\t\"apn\": \"\",\n\t\t\t\"fips\": \"\",\n\t\t\t\"propertyType\": \"Company\",\n\t\t\t\"propertySubType\": null,\n\t\t\t\"googleLatitude\": null,\n\t\t\t\"googleLongitude\": null,\n\t\t\t\"constructionStatus\": \"Completed\",\n\t\t\t\"constructionType\": null,\n\t\t\t\"assetClass\": null,\n\t\t\t\"buildings\": null,\n\t\t\t\"modifiedSource\": null,\n\t\t\t\"migrationStatus\": null,\n\t\t\t\"hasMedia\": \"Deprecated Field\",\n\t\t\t\"mediaTypeId\": null,\n\t\t\t\"assetType\": \"Not an asset\",\n\t\t\t\"isActive\": false,\n\t\t\t\"companyRelationship\": null,\n\t\t\t\"startDate\": null,\n\t\t\t\"endDate\": null\n\t\t},\n\t\t\"links\": {\n\t\t\t\"self\": \"/customerproperty/391411\"\n\t\t}\n\t}\n}";
            responsePropertyDetail.Content = new StringContent(jsonToSave);

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_SetupOrganization, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = "", RealPageId = _RealPageId });

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DataImportMappingUpdate, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository
                .SetupSequence(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.IsAny<object>()))
                .Returns(userLoginOnly);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_CreateOrganizationProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePropertyInstance, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 12345, RealPageId = propertyGuid, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany/{customercompany.CustomerCompanyId}", responseCustomerCompany);
            //mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[companyInstance.greenBookCares]=true&filter[customerCompanyId]={customercompany.CustomerCompanyId}&include=companyInstance&include=companyInstance.attributes", responseMapResource);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[customerCompanyId]={customercompany.CustomerCompanyId}&include=companyInstance&include=companyInstance.attributes", responseMapResource);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customerproperty/391411", responsePropertyDetail);
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/companyinstance", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            mockHttpMessageHandler.Setup(HttpMethod.Put, $"http://localhost/propertyinstance/{propertyGuid}/{ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)}", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/productcenterenablement/enable", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmorder_create);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmorder_create_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmorder_create);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMOrder_Create_Update_Success()
        {
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            CustomerCompany customercompany = new CustomerCompany() { CustomerCompanyId = 1948, IsActive = true, CompanyName = "Test Company", MigrationStatus = "migrated", CompanyType = _organizationTypeName }; //Category = "rpup"
            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>() { new CustomerCompanyMap() { CompanyInstanceSourceId = "1234567", Source = "OS" } };

            Guid companyRealPageId = new Guid();
            UserLoginOnly userLoginOnly = new UserLoginOnly()
            {
                UserId = 3,
                PartyId = 1,
                LoginName = $"{customercompany.CustomerCompanyId}admin@realpage.com",
                PasswordHash = ""
            };
            UserLoginOnly userLoginOnlyNull = null;

            Organization organization = new Organization()
            {
                Name = customercompany.CompanyName,
                RealPageId = new Guid("5C04F18A-FC9B-1234-AAAF-E26DA83CE516")
            };

            Guid propertyGuid = new Guid("5C04F18A-FC9B-4A13-AAAF-E26DA83CE516");

            HttpResponseMessage responseCustomerCompany = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(customercompany, new JsonApiSerializerSettings());
            responseCustomerCompany.Content = new StringContent(jsonToSave);

            HttpResponseMessage responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = JsonConvert.SerializeObject(mapResource, new JsonApiSerializerSettings());
            responseMapResource.Content = new StringContent(jsonToSave);

            HttpResponseMessage responsePropertyDetail = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = "{\n\t\"data\": {\n\t\t\"type\": \"customerproperty\",\n\t\t\"id\": \"391411\",\n\t\t\"attributes\": {\n\t\t\t\"customerPropertyId\": 391411,\n\t\t\t\"customerCompanyId\": 1234,\n\t\t\t\"masterPropertyId\": 12345,\n\t\t\t\"propertyName\": \"Test Property\",\n\t\t\t\"address\": {\n\t\t\t\t\"address\": \"11623 Pleasant Meadow Dr\",\n\t\t\t\t\"city\": \"North Potomac\",\n\t\t\t\t\"state\": \"MD\",\n\t\t\t\t\"country\": \"USA\",\n\t\t\t\t\"county\": \"Montgomery\",\n\t\t\t\t\"postalCode\": \"20878-4258\",\n\t\t\t\t\"latitude\": 39.089137,\n\t\t\t\t\"longitude\": -77.238857\n\t\t\t},\n\t\t\t\"units\": null,\n\t\t\t\"stories\": null,\n\t\t\t\"bedCount\": null,\n\t\t\t\"squareFeet\": null,\n\t\t\t\"yearBuilt\": null,\n\t\t\t\"renovationStartDate\": null,\n\t\t\t\"renovationEndDate\": null,\n\t\t\t\"createdAt\": \"2020-08-07 12:49:59.000000-0500\",\n\t\t\t\"modifiedAt\": \"2020-08-07 12:49:59.000000-0500\",\n\t\t\t\"deletedAt\": null,\n\t\t\t\"certifiedAt\": null,\n\t\t\t\"createdBy\": null,\n\t\t\t\"modifiedBy\": null,\n\t\t\t\"geocoded\": true,\n\t\t\t\"isUat\": false,\n\t\t\t\"apn\": \"\",\n\t\t\t\"fips\": \"\",\n\t\t\t\"propertyType\": \"Company\",\n\t\t\t\"propertySubType\": null,\n\t\t\t\"googleLatitude\": null,\n\t\t\t\"googleLongitude\": null,\n\t\t\t\"constructionStatus\": \"Completed\",\n\t\t\t\"constructionType\": null,\n\t\t\t\"assetClass\": null,\n\t\t\t\"buildings\": null,\n\t\t\t\"modifiedSource\": null,\n\t\t\t\"migrationStatus\": null,\n\t\t\t\"hasMedia\": \"Deprecated Field\",\n\t\t\t\"mediaTypeId\": null,\n\t\t\t\"assetType\": \"Not an asset\",\n\t\t\t\"isActive\": false,\n\t\t\t\"companyRelationship\": null,\n\t\t\t\"startDate\": null,\n\t\t\t\"endDate\": null\n\t\t},\n\t\t\"links\": {\n\t\t\t\"self\": \"/customerproperty/391411\"\n\t\t}\n\t}\n}";
            responsePropertyDetail.Content = new StringContent(jsonToSave);

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(organization);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_SetupOrganization, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = "", RealPageId = _RealPageId });

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DataImportMappingUpdate, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository
                .SetupSequence(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.IsAny<object>()))
                .Returns(userLoginOnlyNull)
                .Returns(userLoginOnly);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_CreateOrganizationProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePropertyInstance, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 12345, RealPageId = propertyGuid, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany/{customercompany.CustomerCompanyId}", responseCustomerCompany);
            //mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[companyInstance.greenBookCares]=true&filter[customerCompanyId]={customercompany.CustomerCompanyId}&include=companyInstance&include=companyInstance.attributes", responseMapResource);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[customerCompanyId]={customercompany.CustomerCompanyId}&include=companyInstance&include=companyInstance.attributes", responseMapResource);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customerproperty/391411", responsePropertyDetail);
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/companyinstance", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            mockHttpMessageHandler.Setup(HttpMethod.Put, $"http://localhost/propertyinstance/{propertyGuid}/{ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)}", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/productcenterenablement/enable", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmorder_create_update);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmorder_create_update_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmorder_create_update);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMOrder_Create_NullDomain()
        {
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmorder_create_nulldomain);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmorder_create_nulldomain_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmorder_create_nulldomain);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);

            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);

            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"Missing customerEnvironment\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMOrder_Create_MissingBlueBook()
        {
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var responsePropertyDetail = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = "{\n\t\"data\": {\n\t\t\"type\": \"customerproperty\",\n\t\t\"id\": \"391411\",\n\t\t\"attributes\": {\n\t\t\t\"customerPropertyId\": 391411,\n\t\t\t\"customerCompanyId\": 1234,\n\t\t\t\"masterPropertyId\": 12345,\n\t\t\t\"propertyName\": \"Test Property\",\n\t\t\t\"address\": {\n\t\t\t\t\"address\": \"11623 Pleasant Meadow Dr\",\n\t\t\t\t\"city\": \"North Potomac\",\n\t\t\t\t\"state\": \"MD\",\n\t\t\t\t\"country\": \"USA\",\n\t\t\t\t\"county\": \"Montgomery\",\n\t\t\t\t\"postalCode\": \"20878-4258\",\n\t\t\t\t\"latitude\": 39.089137,\n\t\t\t\t\"longitude\": -77.238857\n\t\t\t},\n\t\t\t\"units\": null,\n\t\t\t\"stories\": null,\n\t\t\t\"bedCount\": null,\n\t\t\t\"squareFeet\": null,\n\t\t\t\"yearBuilt\": null,\n\t\t\t\"renovationStartDate\": null,\n\t\t\t\"renovationEndDate\": null,\n\t\t\t\"createdAt\": \"2020-08-07 12:49:59.000000-0500\",\n\t\t\t\"modifiedAt\": \"2020-08-07 12:49:59.000000-0500\",\n\t\t\t\"deletedAt\": null,\n\t\t\t\"certifiedAt\": null,\n\t\t\t\"createdBy\": null,\n\t\t\t\"modifiedBy\": null,\n\t\t\t\"geocoded\": true,\n\t\t\t\"isUat\": false,\n\t\t\t\"apn\": \"\",\n\t\t\t\"fips\": \"\",\n\t\t\t\"propertyType\": \"Company\",\n\t\t\t\"propertySubType\": null,\n\t\t\t\"googleLatitude\": null,\n\t\t\t\"googleLongitude\": null,\n\t\t\t\"constructionStatus\": \"Completed\",\n\t\t\t\"constructionType\": null,\n\t\t\t\"assetClass\": null,\n\t\t\t\"buildings\": null,\n\t\t\t\"modifiedSource\": null,\n\t\t\t\"migrationStatus\": null,\n\t\t\t\"hasMedia\": \"Deprecated Field\",\n\t\t\t\"mediaTypeId\": null,\n\t\t\t\"assetType\": \"Not an asset\",\n\t\t\t\"isActive\": false,\n\t\t\t\"companyRelationship\": null,\n\t\t\t\"startDate\": null,\n\t\t\t\"endDate\": null\n\t\t},\n\t\t\"links\": {\n\t\t\t\"self\": \"/customerproperty/391411\"\n\t\t}\n\t}\n}";
            responsePropertyDetail.Content = new StringContent(jsonToSave);

            var responseCustomerCompanyNotFound = new HttpResponseMessage(HttpStatusCode.NotFound);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customerproperty/391411", responsePropertyDetail);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany/1948", responseCustomerCompanyNotFound);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmorder_create);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmorder_create_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmorder_create);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        #endregion

        #region UPFMOrder_Cancel
        [Fact]
        public void Post_Books_Provisioning_UPFMOrder_Cancel_Success()
        {
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_DeleteOrganizationProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DisableUsersForProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 12345, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/productcenteractivation/cancel", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmorder_cancel);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmorder_cancel_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmorder_cancel);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }
        [Fact]
        public void Post_Books_Provisioning_UPFMOrder_Cancel_BadRequest_For_InvalidCompanyInstance()
        {
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_DeleteOrganizationProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DisableUsersForProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 12345, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/productcenteractivation/cancel", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmorder_cancel_Invalid_CompanyInstance);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmorder_cancel_Invalid_CompanyInstance_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmorder_cancel_Invalid_CompanyInstance);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);

            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"Invalid companyInstanceSourceId\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMOrder_Cancel_BadRequest_For_InvalidProductInstance()
        {
            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_DeleteOrganizationProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DisableUsersForProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 12345, ErrorMessage = "" });

            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/productcenteractivation/cancel", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmorder_cancel_Invalid_ProductInstance);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmorder_cancel_Invalid_ProductInstance_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmorder_cancel_Invalid_ProductInstance);

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);
        }
        #endregion

        #region UPFMClone_Create
        [Fact]
        public void Post_Books_Provisioning_UPFMClone_Create_Success()
        {
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            CustomerCompany customercompany = new CustomerCompany() { CustomerCompanyId = 1386886, IsActive = true, CompanyName = "hotsprov634958558", MigrationStatus = "migrated", CompanyType = _organizationTypeName }; //Category = "rpup"
            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>() { new CustomerCompanyMap() { CompanyInstanceSourceId = "1234567", Source = "OS" } };

            UserLoginOnly userLoginOnly = new UserLoginOnly()
            {
                UserId = 3,
                PartyId = 1,
                LoginName = $"{customercompany.CustomerCompanyId}admin@realpage.com",
                PasswordHash = ""
            };
            UserLoginOnly userLoginOnlyNull = null;

            IList<ProductUI> productUIList = new List<ProductUI>() { new ProductUI() { ProductId = 3 }, new ProductUI() { ProductId = 56 } };

            Guid propertyGuid = new Guid("5C04F18A-FC9B-4A13-AAAF-E26DA83CE516");

            HttpResponseMessage responseCustomerCompany = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(customercompany, new JsonApiSerializerSettings());
            responseCustomerCompany.Content = new StringContent(jsonToSave);

            HttpResponseMessage responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = JsonConvert.SerializeObject(mapResource, new JsonApiSerializerSettings());
            responseMapResource.Content = new StringContent(jsonToSave);

            HttpResponseMessage responsePropertyDetail = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = "{\n\t\"data\": {\n\t\t\"type\": \"customerproperty\",\n\t\t\"id\": \"391411\",\n\t\t\"attributes\": {\n\t\t\t\"customerPropertyId\": 391411,\n\t\t\t\"customerCompanyId\": 1234,\n\t\t\t\"masterPropertyId\": 12345,\n\t\t\t\"propertyName\": \"Test Property\",\n\t\t\t\"address\": {\n\t\t\t\t\"address\": \"11623 Pleasant Meadow Dr\",\n\t\t\t\t\"city\": \"North Potomac\",\n\t\t\t\t\"state\": \"MD\",\n\t\t\t\t\"country\": \"USA\",\n\t\t\t\t\"county\": \"Montgomery\",\n\t\t\t\t\"postalCode\": \"20878-4258\",\n\t\t\t\t\"latitude\": 39.089137,\n\t\t\t\t\"longitude\": -77.238857\n\t\t\t},\n\t\t\t\"units\": null,\n\t\t\t\"stories\": null,\n\t\t\t\"bedCount\": null,\n\t\t\t\"squareFeet\": null,\n\t\t\t\"yearBuilt\": null,\n\t\t\t\"renovationStartDate\": null,\n\t\t\t\"renovationEndDate\": null,\n\t\t\t\"createdAt\": \"2020-08-07 12:49:59.000000-0500\",\n\t\t\t\"modifiedAt\": \"2020-08-07 12:49:59.000000-0500\",\n\t\t\t\"deletedAt\": null,\n\t\t\t\"certifiedAt\": null,\n\t\t\t\"createdBy\": null,\n\t\t\t\"modifiedBy\": null,\n\t\t\t\"geocoded\": true,\n\t\t\t\"isUat\": false,\n\t\t\t\"apn\": \"\",\n\t\t\t\"fips\": \"\",\n\t\t\t\"propertyType\": \"Company\",\n\t\t\t\"propertySubType\": null,\n\t\t\t\"googleLatitude\": null,\n\t\t\t\"googleLongitude\": null,\n\t\t\t\"constructionStatus\": \"Completed\",\n\t\t\t\"constructionType\": null,\n\t\t\t\"assetClass\": null,\n\t\t\t\"buildings\": null,\n\t\t\t\"modifiedSource\": null,\n\t\t\t\"migrationStatus\": null,\n\t\t\t\"hasMedia\": \"Deprecated Field\",\n\t\t\t\"mediaTypeId\": null,\n\t\t\t\"assetType\": \"Not an asset\",\n\t\t\t\"isActive\": false,\n\t\t\t\"companyRelationship\": null,\n\t\t\t\"startDate\": null,\n\t\t\t\"endDate\": null\n\t\t},\n\t\t\"links\": {\n\t\t\t\"self\": \"/customerproperty/391411\"\n\t\t}\n\t}\n}";
            responsePropertyDetail.Content = new StringContent(jsonToSave);

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_SetupOrganization, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = "", RealPageId = _RealPageId });

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization, It.IsAny<object>()))
                .Returns(productUIList);

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DataImportMappingUpdate, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository
                .SetupSequence(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.IsAny<object>()))
                .Returns(userLoginOnly);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_CreateOrganizationProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePropertyInstance, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 12345, RealPageId = propertyGuid, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertHotsCompanyRelationship, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 123, RealPageId = propertyGuid, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertHotsPropertyRelationship, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 123, RealPageId = propertyGuid, ErrorMessage = "" });

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany/{customercompany.CustomerCompanyId}", responseCustomerCompany);
            //mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[companyInstance.greenBookCares]=true&filter[customerCompanyId]={customercompany.CustomerCompanyId}&include=companyInstance&include=companyInstance.attributes", responseMapResource);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[customerCompanyId]={customercompany.CustomerCompanyId}&include=companyInstance&include=companyInstance.attributes", responseMapResource);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customerproperty/391411", responsePropertyDetail);
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/companyinstance", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            mockHttpMessageHandler.Setup(HttpMethod.Put, $"http://localhost/propertyinstance/{propertyGuid}/{ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)}", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/productcenterenablement/enable", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmclone_create);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmclone_create_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmclone_create);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMClone_Create_CloningNotEnabled()
        {
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() { Name = "TiboWebHookSigningSecret", Value = _mockTiboWebHookSigningSecret },
                new ProductInternalSetting() { Name = "IsCloneUsersProcessEnabledForHOTS", Value = "0" }
            };

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, 3))))
                .Returns(productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmclone_create_missing_clonecompanyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmclone_create_missing_clonecompanyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmclone_create_missing_clonecompanyid);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"Environment not enabled for HOTS cloning\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);

            productInternalSettings = new List<ProductInternalSetting>()
            {
                new ProductInternalSetting() { Name = "TiboWebHookSigningSecret", Value = _mockTiboWebHookSigningSecret },
            };
            mockRepository
                .Setup(m => m.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                    It.Is<object>(d => TestIsProductId(d, 3))))
                .Returns(productInternalSettings);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            //Arrange
            webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmclone_create_missing_clonecompanyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmclone_create_missing_clonecompanyid_Signature);

            rPObjectCache.BustCache();

            //Act
            response = webHookController.PostBooks(thinEvent);
            message = response.Content.ReadAsStringAsync().Result;
            expectedValue = "\"Environment not enabled for HOTS cloning\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMClone_Create_MissingCloneCompany()
        {
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmclone_create_missing_clonecompanyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmclone_create_missing_clonecompanyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmclone_create_missing_clonecompanyid);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"Missing cloneCompanyInstanceSourceId\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMClone_Create_MissingCloneProperty()
        {
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(_organization);

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmclone_create_missing_clonepropertyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmclone_create_missing_clonepropertyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmclone_create_missing_clonepropertyid);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);
            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"Missing clonePropertyInstanceSourceId\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMClone_Create_UnknownCloneCompany()
        {
            //Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmclone_create_unknown_clonecompanyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmclone_create_unknown_clonecompanyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmclone_create_unknown_clonecompanyid);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);

            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"HOTS Baseline Company 3d3865fb-4be4-401f-96ab-c552aee97512 not found\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMClone_Create_InvalidCloneCompany()
        {
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            //Arrange
            WebHookController webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmclone_create_invalid_clonecompanyid);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmclone_create_invalid_clonecompanyid_Signature);

            ThinEvent<JToken> thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmclone_create_invalid_clonecompanyid);

            RPObjectCache rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            HttpResponseMessage response = webHookController.PostBooks(thinEvent);

            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"Invalid cloneCompanyInstanceSourceId, not Guid\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
            Assert.True(!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.BadRequest);
        }
        #endregion

        #region UPFMVendor_Create
        [Fact]
        public void Post_Books_Provisioning_UPFMVendor_Create_Success()
        {
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            CustomerCompany customercompany = new CustomerCompany() { CustomerCompanyId = 1380567, IsActive = true, CompanyName = "1 AWESOME SERVICE LLC", MigrationStatus = "migrated", CompanyType = "Vendor" }; //Category = "rpup"
            IList<CustomerCompanyMap> mapResource = new List<CustomerCompanyMap>() { new CustomerCompanyMap() { CompanyInstanceSourceId = "2230095", Source = "VMP" } };

            var emptyCompanyInstances = new List<CustomerCompanyInstance>();
            var vendorCustomerCompanyMap = new CustomerCompanyMap() { Domain = "Primary", Source = "VMP", CompanyInstanceSourceId = "2230095" };
            var companyAdminUserLoginOnly = new UserLoginOnly()
            {
                UserId = 3,
                PartyId = 1,
                LoginName = $"{_PartyId}admin@realpage.com",
                PasswordHash = "",
                RealPageId = Guid.NewGuid()
            };

            var vendorAdminPerson = new Person()
            {
                PartyId = 18,
                FirstName = "Liza",
                LastName = "Jones",
                RealPageId = Guid.NewGuid()
            };

            var customerAdminUserLoginOnly = new UserLoginOnly()
            {
                UserId = 67,
                PartyId = vendorAdminPerson.PartyId,
                LoginName = "ljones@test.com",
                PasswordHash = "",
                RealPageId = vendorAdminPerson.RealPageId
            };

            UserLoginOnly userLoginOnlyNull = null;
            var personPartyId = 12345;

            var productUIList = new List<ProductUI>() { new ProductUI() { ProductId = 38 } };

            var personaEnvironments = new List<PersonaEnvironment>() { new PersonaEnvironment() { Name = "Production", PersonaEnvironmentTypeId = 1 } };
            var organization = new Organization()
            {
                RealPageId = _RealPageId,
                CreateDate = _CreateDate,
                Name = "1 AWESOME SERVICE LLC",
                PartyId = _PartyId,
                BooksMasterId = _BooksMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = _vendorOrganizationTypeId,
                organizationType = new OrganizationType()
                {
                    OrganizationTypeId = _vendorOrganizationTypeId
                },
                OrganizationDomain = new OrganizationDomain()
                {
                    OrganizationDomainId = _organizationDomainId
                }
            };

            var externalOrganization = new Organization()
            {
                RealPageId = _externalOrganizationRealPageId,
                CreateDate = _CreateDate,
                Name = "External Users",
                PartyId = _ExternalPartyId,
                BooksMasterId = _BooksMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = _otherOrganizationTypeId,
                organizationType = new OrganizationType()
                {
                    OrganizationTypeId = _otherOrganizationTypeId
                },
                OrganizationDomain = new OrganizationDomain()
                {
                    OrganizationDomainId = _organizationDomainId
                }
            };

            var userRoleTypeList = new List<RoleType>()
            {
                new RoleType() { Name = "User", PartyRoleTypeId = 401, ParentPartyRoleTypeId = 400 },
                new RoleType() { Name = "SuperUser", PartyRoleTypeId = 402, ParentPartyRoleTypeId = 400 },
                new RoleType() { Name = "RealPage Employee", PartyRoleTypeId = 403, ParentPartyRoleTypeId = 400 },
                new RoleType() { Name = "User (No Email)", PartyRoleTypeId = 404, ParentPartyRoleTypeId = 400 },
                new RoleType() { Name = "External User", PartyRoleTypeId = 405, ParentPartyRoleTypeId = 400 },
            };

            var organizationRoleTypeList = new List<RoleType>()
            {
                new RoleType() { Name = "Parent Corporation", PartyRoleTypeId = 201, ParentPartyRoleTypeId = 200 },
                new RoleType() { Name = "Property Management Company", PartyRoleTypeId = 202, ParentPartyRoleTypeId = 200 },
                new RoleType() { Name = "Employer", PartyRoleTypeId = 203, ParentPartyRoleTypeId = 200 },
                new RoleType() { Name = "Site", PartyRoleTypeId = 204, ParentPartyRoleTypeId = 200 },
                new RoleType() { Name = "User Type", PartyRoleTypeId = 205, ParentPartyRoleTypeId = 200 },
            };

            var commonAddresses = new List<CommonAddress>()
            {
                new CommonAddress() { AddressType = "email", AddressString = "ljones@test.com", ContactMechanismId = 53, ContactMechanismUsageTypeId = 345, PartyContactMechanismId = 321 }
            };
            var identityProviderTypes = new List<IdentityProviderType>() { new IdentityProviderType() { ContactMechanismId = 1000, AuthenticationType = "local" }, new IdentityProviderType() { ContactMechanismId = 1001, AuthenticationType = "aad" } };

            var orgStatusList = new List<OrganizationStatus>()
            {
                new OrganizationStatus()
                {
                    PartyId = personPartyId,
                    IsPending = true,
                    IsActive = true,
                    IsExpired = false,
                    StatusTypeId = (int)UserUiStatusType.Active,
                    Status = UserUiStatusType.Active,
                    FromDate = new DateTime(2019, 1, 1)
                }
            };

            var activityList = new List<Activity>() { new Activity() { ActivityCode = "1", Description = "Test Activity", ActivityTypeId = (int)ActivityType.NewUserRegistration, ActivityTokenExpirationMinutes = 60 } };
            var enterpriseRoleList = new List<EnterpriseRole>()
            {
                new EnterpriseRole() { Role = "Platform Administrator", RoleId = 1 },
                new EnterpriseRole() { Role = "Basic End User", RoleId = 2 }
            };

            var vendorAdminUserDetails = new UserDetails()
            {
                FirstName = vendorAdminPerson.FirstName,
                LastName = vendorAdminPerson.LastName,
                Email = customerAdminUserLoginOnly.LoginName,
                LoginName = customerAdminUserLoginOnly.LoginName,
                PersonPartyId = customerAdminUserLoginOnly.PartyId
            };

            var responseCustomerCompany = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(customercompany, new JsonApiSerializerSettings());
            responseCustomerCompany.Content = new StringContent(jsonToSave);

            var responseCustomerCompanyById = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = JsonConvert.SerializeObject(customercompany, new JsonApiSerializerSettings());
            responseCustomerCompanyById.Content = new StringContent(jsonToSave);

            var responseMapResource = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = JsonConvert.SerializeObject(mapResource, new JsonApiSerializerSettings());
            responseMapResource.Content = new StringContent(jsonToSave);

            var responseEmptyCompanyInstances = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = JsonConvert.SerializeObject(emptyCompanyInstances, new JsonApiSerializerSettings());
            responseEmptyCompanyInstances.Content = new StringContent(jsonToSave);

            var responseVendorCustomerCompanyMap = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = JsonConvert.SerializeObject(vendorCustomerCompanyMap, new JsonApiSerializerSettings());
            responseVendorCustomerCompanyMap.Content = new StringContent(jsonToSave);

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.Is<object>(
                        d => TestIsRealPageId(d, organization.RealPageId))))
                .Returns(organization);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.Is<object>(
                    d => TestIsRealPageId(d, _externalOrganizationRealPageId))))
                .Returns(externalOrganization);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_SetupOrganization, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = "", RealPageId = _RealPageId });

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization, It.IsAny<object>()))
                .Returns(productUIList);

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DataImportMappingUpdate, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.Is<object>(
                        d => TestIsLoginName(d, $"{_PartyId}admin@realpage.com"))))
                .Returns(companyAdminUserLoginOnly);

            mockRepository
                .SetupSequence(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.Is<object>(
                    d => TestIsLoginName(d, "ljones@test.com"))))
                .Returns(userLoginOnlyNull)
                .Returns(customerAdminUserLoginOnly)
                .Returns(customerAdminUserLoginOnly);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_CreateOrganizationProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetMany<PersonaEnvironment>(StoredProcNameConstants.SP_GetPersonaEnvironment, It.IsAny<object>()))
                .Returns(personaEnvironments);

            mockRepository
                .Setup(m => m.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, It.Is<object>(
                    d => TestIsRoleTypeName(d, "User Role"))))
                .Returns(userRoleTypeList);

            mockRepository
                .Setup(m => m.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, It.Is<object>(
                    d => TestIsRoleTypeName(d, "Organization Role"))))
                .Returns(organizationRoleTypeList);

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePerson, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = personPartyId, RealPageId = Guid.NewGuid(), ErrorMessage = "" });

            mockRepository.Setup(m => m.GetMany<ContactMechanismUsageType>(StoredProcNameConstants.SP_ListContactMechanismUsageType, It.IsAny<object>()))
                .Returns(() => _contactMechanismUsageTypes);

            mockRepository.Setup(m => m.GetMany<IdentityProviderType>(StoredProcNameConstants.SP_GetOrganizationIdentityProviderType, It.IsAny<object>()))
                .Returns(identityProviderTypes);

            mockRepository.Setup(m => m.GetOne<UserDetails>(StoredProcNameConstants.SP_GetUserDetails, It.IsAny<object>()))
                .Returns(vendorAdminUserDetails);

            mockRepository.Setup(m => m.GetOne<Person>(StoredProcNameConstants.SP_GetPerson, It.IsAny<object>()))
                .Returns(vendorAdminPerson);

            Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateUserLogin, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = realPageId });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserLogin, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = realPageId });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateContactMechanism, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 53, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 5454, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 243, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateElectronicAddress, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 676, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetMany<CommonAddress>(StoredProcNameConstants.SP_ListContactMechanismsForPerson, It.IsAny<object>()))
                .Returns(commonAddresses);

            mockRepository.Setup(m => m.GetMany<OrganizationStatus>(StoredProcNameConstants.SP_ListOrganizationStatusByUserId, It.IsAny<object>()))
                .Returns(orgStatusList);

            mockRepository.Setup(m => m.GetMany<Activity>(StoredProcNameConstants.SP_ListActivity, It.IsAny<object>()))
                .Returns(activityList);

            mockRepository.Setup(m => m.GetMany<EnterpriseRole>(StoredProcNameConstants.SP_SecurityListRolesByRealPageID, It.IsAny<object>()))
                .Returns(enterpriseRoleList);

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateUserLoginPersona, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 4323, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePersona, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 9872, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonaToRole, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 4455, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_AddUpdatePropertyMapping, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 55555, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateEmployeeId, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 66666, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonToOrganization, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 6735, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkIdentityProviderToUserLogin, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 3489, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateBatchProcessorGroup, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 3267, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePartyRoleByRealPageId, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 3421, ErrorMessage = "", RealPageId = Guid.Empty });

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany/{customercompany.CustomerCompanyId}", responseCustomerCompany);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompanymap?filter[customerCompanyId]={customercompany.CustomerCompanyId}&include=companyInstance&include=companyInstance.attributes", responseMapResource);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany?filter[customerCompanyId]=in:{customercompany.CustomerCompanyId}&include=customerCompanyLocation", responseCustomerCompanyById);
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/companyinstance", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/companyinstance/{vendorCustomerCompanyMap.CompanyInstanceSourceId}/VMP", responseVendorCustomerCompanyMap);
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/productcenterenablement/enable", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/companyinstance?filter[source]=UPFM&filter[customerCompanyMap.customerCompanyId]={customercompany.CustomerCompanyId}&fields[companyinstance]=companyInstanceId,source,companyInstanceSourceId,companyName,companyType,isActive,domain", responseEmptyCompanyInstances);
            mockHttpMessageHandler.Setup(HttpMethod.Put, $"http://localhost/companyinstance/{_RealPageId}/UPFM", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/v2/provisioning/company", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/systemproductcenter", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });

            //Arrange
            var webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmvendor_create);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmvendor_create_Signature);

            var thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmvendor_create);

            var rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            var response = webHookController.PostBooks(thinEvent);
            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMVendor_Company_Not_Found()
        {
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            CustomerCompany invalidCustomercompany = new CustomerCompany() { CustomerCompanyId = 55555, IsActive = true, CompanyName = "Invalid company", MigrationStatus = "migrated", CompanyType = "Vendor" }; //Category = "rpup"
            var responseNotFoundCustomerCompany = new HttpResponseMessage(HttpStatusCode.NotFound);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany/{invalidCustomercompany.CustomerCompanyId}", responseNotFoundCustomerCompany);

            //Arrange
            var webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmvendor_create_invalid_company);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmvendor_create_invalid_company_Signature);

            var thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmvendor_create_invalid_company);

            var rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            var response = webHookController.PostBooks(thinEvent);
            Assert.True(!response.IsSuccessStatusCode);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);

            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"Company not found in books environment\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMVendor_VendorCompany_Not_Found()
        {
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            CustomerCompany customercompany = new CustomerCompany() { CustomerCompanyId = 1380567, IsActive = true, CompanyName = "1 AWESOME SERVICE LLC", MigrationStatus = "migrated", CompanyType = "Vendor" }; //Category = "rpup"

            var emptyCompanyInstances = new List<CustomerCompanyInstance>();
            var vendorCustomerCompanyMap = new CustomerCompanyMap() { Domain = "Primary", Source = "VMP", CompanyInstanceSourceId = "2230095" };
            
            var responseCustomerCompany = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(customercompany, new JsonApiSerializerSettings());
            responseCustomerCompany.Content = new StringContent(jsonToSave);

            var responseCustomerCompanyById = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = JsonConvert.SerializeObject(customercompany, new JsonApiSerializerSettings());
            responseCustomerCompanyById.Content = new StringContent(jsonToSave);

            var responseEmptyCompanyInstances = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = JsonConvert.SerializeObject(emptyCompanyInstances, new JsonApiSerializerSettings());
            responseEmptyCompanyInstances.Content = new StringContent(jsonToSave);

            var responseMissingVendorCustomerCompanyMap = new HttpResponseMessage(HttpStatusCode.NotFound);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany/{customercompany.CustomerCompanyId}", responseCustomerCompany);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/companyinstance/{vendorCustomerCompanyMap.CompanyInstanceSourceId}/VMP", responseMissingVendorCustomerCompanyMap);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany?filter[customerCompanyId]=in:{customercompany.CustomerCompanyId}&include=customerCompanyLocation", responseCustomerCompanyById);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/companyinstance?filter[source]=UPFM&filter[customerCompanyMap.customerCompanyId]={customercompany.CustomerCompanyId}&fields[companyinstance]=companyInstanceId,source,companyInstanceSourceId,companyName,companyType,isActive,domain", responseEmptyCompanyInstances);

            //Arrange
            var webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmvendor_create);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmvendor_create_Signature);

            var thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmvendor_create);

            var rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            var response = webHookController.PostBooks(thinEvent);

            Assert.True(!response.IsSuccessStatusCode);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);

            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"Vendor instance not found in books environment\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMVendor_UPFM_Instance_Already_Exists()
        {
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var customercompany = new CustomerCompany() { CustomerCompanyId = 1380567, IsActive = true, CompanyName = "1 AWESOME SERVICE LLC", MigrationStatus = "migrated", CompanyType = "Vendor" }; //Category = "rpup"
            
            var vendorCustomerCompanyMap = new CustomerCompanyMap() { Domain = "Primary", Source = "VMP", CompanyInstanceSourceId = "2230095" };

            var vendorAdminPerson = new Person()
            {
                PartyId = 18,
                FirstName = "Liza",
                LastName = "Jones",
                RealPageId = Guid.NewGuid()
            };

            var responseCustomerCompany = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(customercompany, new JsonApiSerializerSettings());
            responseCustomerCompany.Content = new StringContent(jsonToSave);

            var responseCustomerCompanyById = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = JsonConvert.SerializeObject(customercompany, new JsonApiSerializerSettings());
            responseCustomerCompanyById.Content = new StringContent(jsonToSave);

            var responseExistingCompanyInstances = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = "{\r\n\t\"data\": [\r\n\t\t{\r\n\t\t\t\"type\": \"customercompanyinstance\",\r\n\t\t\t\"attributes\": {\r\n\t\t\t\t\t\"companyInstanceId\": 1234,\r\n\t\t\t\t\t\"source\": \"UPFM\",\r\n\t\t\t\t\t\"companyInstanceSourceId\": \"cf906086-4676-4e3b-8e87-d912afb1120b\",\r\n\t\t\t\t\t\"domain\": \"Primary\"\r\n\t\t\t}\r\n\t\t}\r\n\t]\r\n}";
            responseExistingCompanyInstances.Content = new StringContent(jsonToSave);

            var responseVendorCustomerCompanyMap = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = JsonConvert.SerializeObject(vendorCustomerCompanyMap, new JsonApiSerializerSettings());
            responseVendorCustomerCompanyMap.Content = new StringContent(jsonToSave);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany/{customercompany.CustomerCompanyId}", responseCustomerCompany);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/companyinstance/{vendorCustomerCompanyMap.CompanyInstanceSourceId}/VMP", responseVendorCustomerCompanyMap);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany?filter[customerCompanyId]=in:{customercompany.CustomerCompanyId}&include=customerCompanyLocation", responseCustomerCompanyById);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/companyinstance?filter[source]=UPFM&filter[customerCompanyMap.customerCompanyId]={customercompany.CustomerCompanyId}&fields[companyinstance]=companyInstanceId,source,companyInstanceSourceId,companyName,companyType,isActive,domain", responseExistingCompanyInstances);

            //Arrange
            var webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmvendor_create);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmvendor_create_Signature);

            var thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmvendor_create);

            var rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            var response = webHookController.PostBooks(thinEvent);

            Assert.True(!response.IsSuccessStatusCode);
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);

            var message = response.Content.ReadAsStringAsync().Result;
            var expectedValue = "\"UPFM instance already exists\"";

            Assert.Equal(expectedValue, message, ignoreCase: true);
        }

        [Fact]
        public void Post_Books_Provisioning_UPFMVendor_Unknown_Organization_Type()
        {
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            UserLoginOnly userLoginOnlyNull = null;

            var customercompany = new CustomerCompany() { CustomerCompanyId = 1380567, IsActive = true, CompanyName = "1 AWESOME SERVICE LLC", MigrationStatus = "migrated", CompanyType = "InvalidType" };
            var existingCompanyInstances = new List<CustomerCompanyInstance>();
            var vendorCustomerCompanyMap = new CustomerCompanyMap() { Domain = "Primary", Source = "VMP", CompanyInstanceSourceId = "2230095" };
            var responseCustomerCompany = new HttpResponseMessage(HttpStatusCode.OK);
            var jsonToSave = JsonConvert.SerializeObject(customercompany, new JsonApiSerializerSettings());
            responseCustomerCompany.Content = new StringContent(jsonToSave);

            var responseCustomerCompanyById = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = JsonConvert.SerializeObject(customercompany, new JsonApiSerializerSettings());
            responseCustomerCompanyById.Content = new StringContent(jsonToSave);

            var responseExistingCompanyInstances = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = JsonConvert.SerializeObject(existingCompanyInstances, new JsonApiSerializerSettings());
            responseExistingCompanyInstances.Content = new StringContent(jsonToSave);

            var responseVendorCustomerCompanyMap = new HttpResponseMessage(HttpStatusCode.OK);
            jsonToSave = JsonConvert.SerializeObject(vendorCustomerCompanyMap, new JsonApiSerializerSettings());
            responseVendorCustomerCompanyMap.Content = new StringContent(jsonToSave);
            var personaEnvironments = new List<PersonaEnvironment>() { new PersonaEnvironment() { Name = "Production", PersonaEnvironmentTypeId = 1 } };
            var personPartyId = 12345;
            var vendorAdminPerson = new Person()
            {
                PartyId = 18,
                FirstName = "Liza",
                LastName = "Jones",
                RealPageId = Guid.NewGuid()
            };
            var customerAdminUserLoginOnly = new UserLoginOnly()
            {
                UserId = 67,
                PartyId = vendorAdminPerson.PartyId,
                LoginName = "ljones@test.com",
                PasswordHash = "",
                RealPageId = vendorAdminPerson.RealPageId
            };
            var companyAdminUserLoginOnly = new UserLoginOnly()
            {
                UserId = 3,
                PartyId = 1,
                LoginName = $"{_PartyId}admin@realpage.com",
                PasswordHash = "",
                RealPageId = Guid.NewGuid()
            };
            var organization = new Organization()
            {
                RealPageId = _RealPageId,
                CreateDate = _CreateDate,
                Name = "1 AWESOME SERVICE LLC",
                PartyId = _PartyId,
                BooksMasterId = _BooksMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = _otherOrganizationTypeId,
                organizationType = new OrganizationType()
                {
                    OrganizationTypeId = _vendorOrganizationTypeId
                },
                OrganizationDomain = new OrganizationDomain()
                {
                    OrganizationDomainId = _organizationDomainId
                }
            };
            var vendorAdminUserDetails = new UserDetails()
            {
                FirstName = vendorAdminPerson.FirstName,
                LastName = vendorAdminPerson.LastName,
                Email = customerAdminUserLoginOnly.LoginName,
                LoginName = customerAdminUserLoginOnly.LoginName,
                PersonPartyId = customerAdminUserLoginOnly.PartyId
            };
            var externalOrganization = new Organization()
            {
                RealPageId = _externalOrganizationRealPageId,
                CreateDate = _CreateDate,
                Name = "External Users",
                PartyId = _ExternalPartyId,
                BooksMasterId = _BooksMasterId,
                BooksCustomerMasterId = _BooksCompanyMasterId,
                OrganizationTypeId = _otherOrganizationTypeId,
                organizationType = new OrganizationType()
                {
                    OrganizationTypeId = _otherOrganizationTypeId
                },
                OrganizationDomain = new OrganizationDomain()
                {
                    OrganizationDomainId = _organizationDomainId
                }
            };
            var commonAddresses = new List<CommonAddress>()
            {
                new CommonAddress() { AddressType = "email", AddressString = "ljones@test.com", ContactMechanismId = 53, ContactMechanismUsageTypeId = 345, PartyContactMechanismId = 321 }
            };
            var orgStatusList = new List<OrganizationStatus>()
            {
                new OrganizationStatus()
                {
                    PartyId = personPartyId,
                    IsPending = true,
                    IsActive = true,
                    IsExpired = false,
                    StatusTypeId = (int)UserUiStatusType.Active,
                    Status = UserUiStatusType.Active,
                    FromDate = new DateTime(2019, 1, 1)
                }
            };
            var identityProviderTypes = new List<IdentityProviderType>() { new IdentityProviderType() { ContactMechanismId = 1000, AuthenticationType = "local" }, new IdentityProviderType() { ContactMechanismId = 1001, AuthenticationType = "aad" } };
            var userRoleTypeList = new List<RoleType>()
            {
                new RoleType() { Name = "User", PartyRoleTypeId = 401, ParentPartyRoleTypeId = 400 },
                new RoleType() { Name = "SuperUser", PartyRoleTypeId = 402, ParentPartyRoleTypeId = 400 },
                new RoleType() { Name = "RealPage Employee", PartyRoleTypeId = 403, ParentPartyRoleTypeId = 400 },
                new RoleType() { Name = "User (No Email)", PartyRoleTypeId = 404, ParentPartyRoleTypeId = 400 },
                new RoleType() { Name = "External User", PartyRoleTypeId = 405, ParentPartyRoleTypeId = 400 },
            };
            var activityList = new List<Activity>() { new Activity() { ActivityCode = "1", Description = "Test Activity", ActivityTypeId = (int)ActivityType.NewUserRegistration, ActivityTokenExpirationMinutes = 60 } };
            var enterpriseRoleList = new List<EnterpriseRole>()
            {
                new EnterpriseRole() { Role = "Platform Administrator", RoleId = 1 },
                new EnterpriseRole() { Role = "Basic End User", RoleId = 2 }
            };
            var organizationRoleTypeList = new List<RoleType>()
            {
                new RoleType() { Name = "Parent Corporation", PartyRoleTypeId = 201, ParentPartyRoleTypeId = 200 },
                new RoleType() { Name = "Property Management Company", PartyRoleTypeId = 202, ParentPartyRoleTypeId = 200 },
                new RoleType() { Name = "Employer", PartyRoleTypeId = 203, ParentPartyRoleTypeId = 200 },
                new RoleType() { Name = "Site", PartyRoleTypeId = 204, ParentPartyRoleTypeId = 200 },
                new RoleType() { Name = "User Type", PartyRoleTypeId = 205, ParentPartyRoleTypeId = 200 },
            };

            mockRepository
                .Setup(m => m.UnitOfWork)
                .Returns(mockUnitOfWork.Object);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.Is<object>(
                        d => TestIsRealPageId(d, organization.RealPageId))))
                .Returns(organization);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.Is<object>(
                    d => TestIsRealPageId(d, _externalOrganizationRealPageId))))
                .Returns(externalOrganization);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_SetupOrganization, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = "", RealPageId = _RealPageId });

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);
            
            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomains);

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, It.IsAny<object>()))
                .Returns(_gbProductMap);

            mockRepository
                .Setup(m => m.Execute<RepositoryResponse>(StoredProcNameConstants.SP_CreateOrganizationProduct, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "" });

            mockRepository.Setup(m => m.GetMany<IdentityProviderType>(StoredProcNameConstants.SP_GetOrganizationIdentityProviderType, It.IsAny<object>()))
                            .Returns(identityProviderTypes);
            mockRepository
                .Setup(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.Is<object>(
                 d => TestIsLoginName(d, $"{_PartyId}admin@realpage.com"))))
                .Returns(companyAdminUserLoginOnly);

            mockRepository
                .SetupSequence(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.Is<object>(
                    d => TestIsLoginName(d, "ljones@test.com"))))
                .Returns(userLoginOnlyNull)
                .Returns(customerAdminUserLoginOnly)
                .Returns(customerAdminUserLoginOnly);

            mockRepository
                .Setup(m => m.GetMany<PersonaEnvironment>(StoredProcNameConstants.SP_GetPersonaEnvironment, It.IsAny<object>()))
                .Returns(personaEnvironments);

            mockRepository.Setup(m => m.GetOne<UserDetails>(StoredProcNameConstants.SP_GetUserDetails, It.IsAny<object>()))
                .Returns(vendorAdminUserDetails);

            mockRepository
                .Setup(m => m.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, It.Is<object>(
                    d => TestIsRoleTypeName(d, "User Role"))))
                .Returns(userRoleTypeList);

            mockRepository
                .Setup(m => m.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, It.Is<object>(
                    d => TestIsRoleTypeName(d, "Organization Role"))))
                .Returns(organizationRoleTypeList);

            mockRepository
                .Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePerson, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = personPartyId, RealPageId = Guid.NewGuid(), ErrorMessage = "" });

            mockRepository
                .Setup(m => m.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct,
                    It.IsAny<object>()))
                .Returns(_gbProductMap);

            mockRepository.Setup(m => m.GetMany<ContactMechanismUsageType>(StoredProcNameConstants.SP_ListContactMechanismUsageType, It.IsAny<object>()))
                .Returns(() => _contactMechanismUsageTypes);

            mockRepository.Setup(m => m.GetMany<IdentityProviderType>(StoredProcNameConstants.SP_GetOrganizationIdentityProviderType, It.IsAny<object>()))
                .Returns(identityProviderTypes);

            mockRepository.Setup(m => m.GetOne<UserDetails>(StoredProcNameConstants.SP_GetUserDetails, It.IsAny<object>()))
                .Returns(vendorAdminUserDetails);

            mockRepository.Setup(m => m.GetOne<Person>(StoredProcNameConstants.SP_GetPerson, It.IsAny<object>()))
                .Returns(vendorAdminPerson);

            Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateUserLogin, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = realPageId });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserLogin, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = realPageId });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateContactMechanism, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 53, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 5454, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 243, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateElectronicAddress, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 676, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetMany<CommonAddress>(StoredProcNameConstants.SP_ListContactMechanismsForPerson, It.IsAny<object>()))
                .Returns(commonAddresses);

            mockRepository.Setup(m => m.GetMany<OrganizationStatus>(StoredProcNameConstants.SP_ListOrganizationStatusByUserId, It.IsAny<object>()))
                .Returns(orgStatusList);

            mockRepository.Setup(m => m.GetMany<Activity>(StoredProcNameConstants.SP_ListActivity, It.IsAny<object>()))
                .Returns(activityList);

            mockRepository.Setup(m => m.GetMany<EnterpriseRole>(StoredProcNameConstants.SP_SecurityListRolesByRealPageID, It.IsAny<object>()))
                .Returns(enterpriseRoleList);

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateUserLoginPersona, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 4323, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePersona, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 9872, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonaToRole, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 4455, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_AddUpdatePropertyMapping, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 55555, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateEmployeeId, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 66666, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonToOrganization, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 6735, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkIdentityProviderToUserLogin, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 3489, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateBatchProcessorGroup, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 3267, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePartyRoleByRealPageId, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 3421, ErrorMessage = "", RealPageId = Guid.Empty });

            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany/{customercompany.CustomerCompanyId}", responseCustomerCompany);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/companyinstance/{vendorCustomerCompanyMap.CompanyInstanceSourceId}/VMP", responseVendorCustomerCompanyMap);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/customercompany?filter[customerCompanyId]=in:{customercompany.CustomerCompanyId}&include=customerCompanyLocation", responseCustomerCompanyById);
            mockHttpMessageHandler.Setup(HttpMethod.Get, $"http://localhost/companyinstance?filter[source]=UPFM&filter[customerCompanyMap.customerCompanyId]={customercompany.CustomerCompanyId}&fields[companyinstance]=companyInstanceId,source,companyInstanceSourceId,companyName,companyType,isActive,domain", responseExistingCompanyInstances);
            mockHttpMessageHandler.Setup(HttpMethod.Put, $"http://localhost/companyinstance/{_RealPageId}/UPFM", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });
            mockHttpMessageHandler.Setup(HttpMethod.Post, $"http://localhost/v2/provisioning/company", new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{ \"result\" : \"success\"}") });

            //Arrange
            var webHookController = new WebHookController(mockRepository.Object, _userClaim, mockHttpMessageHandler.Object)
            {
                Request = new HttpRequestMessage(HttpMethod.Post, "webhook/books"),
                Configuration = new HttpClient()
            };

            webHookController.Request.Properties.Add("TibcoPostData", _mockJson_books_provisioning_upfmvendor_create);
            webHookController.Request.Headers.Add("signature", _mockJson_books_provisioning_upfmvendor_create_Signature);

            var thinEvent = JsonConvert.DeserializeObject<ThinEvent<JToken>>(_mockJson_books_provisioning_upfmvendor_create);

            var rPObjectCache = new RPObjectCache();
            rPObjectCache.BustCache();

            //Act
            var response = webHookController.PostBooks(thinEvent);

            Assert.True(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted);
        }

        #endregion

        private bool TestIsRealPageId(object obj, Guid? realPageId)
        {
            if (obj == null && realPageId == null)
            {
                return true;
            }

            if (obj == null)
            {
                return false;
            }

            return obj.ToString().ToLower().Contains($"realpageid = {realPageId}");
        }

        private bool TestIsRoleTypeName(object obj, string roleTypeName)
        {
            return obj.ToString().Contains($"RoleTypeName = {roleTypeName}");
        }

        private bool TestIsLoginName(object obj, string loginName)
        {
            return obj.ToString().Contains($"EnterpriseUserName = {loginName}");
        }

        private bool TestIsProductId(object obj, int productId)
        {
            return obj.ToString().Contains($"ProductId = {productId}");
        }
    }
}
