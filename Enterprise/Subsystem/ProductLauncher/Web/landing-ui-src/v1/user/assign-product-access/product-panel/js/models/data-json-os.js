//  Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";

// var data = {
//   "pageId": 1,
//   "pageDisplayName": "OneSite Product Access",
//   "productId": 1,
//   "productName": "OneSite",
//   "controls": [{
//     "id": 66,
//     "name": "ClientPortalProductAccessTabGroupUIId",
//     "dataSource": null,
//     "displayName": null,
//     "sequence": 1,
//     "type": "Tab Group",
//     "dependency": false,
//     "attributes": null,
//     "controls": [{
//       "id": 67,
//       "name": "clientProtalPropertiesTabUIid",
//       "dataSource": null,
//       "displayName": "Properties",
//       "sequence": 1,
//       "type": "Tab",
//       "dependency": false,
//       "attributes": [{
//         "key": "Default",
//         "value": "True"
//       }],
//       "controls": [{
//         "id": 35,
//         "name": "RadioControId",
//         "dataSource": "all",
//         "displayName": "Allow access to all current and future properties",
//         "sequence": 1,
//         "type": "Switch",
//         "dependency": false,
//         "attributes": null,
//         "controls": null
//       },
//       {
//         "id": 73,
//         "name": "PropertiesTabSelectGridUIId",
//         "dataSource": null,
//         "displayName": null,
//         "sequence": 3,
//         "type": "Multi Select Grid",
//         "dependency": false,
//         "attributes": null,
//         "controls": [{
//           "id": 40,
//           "name": "PropertySelectColumnUIId",
//           "dataSource": "isAssigned",
//           "displayName": null,
//           "sequence": 1,
//           "type": "CheckBox",
//           "dependency": false,
//           "attributes": null,
//           "controls": null
//         },
//         {
//           "id": 38,
//           "name": "PropertyNameColumnUIId",
//           "dataSource": "name",
//           "displayName": "Property",
//           "sequence": 2,
//           "type": "Label",
//           "dependency": false,
//           "attributes": null,
//           "controls": null
//         },
//         {
//           "id": 39,
//           "name": "StateColumnUIId",
//           "dataSource": "state",
//           "displayName": "State",
//           "sequence": 3,
//           "type": "Label",
//           "dependency": false,
//           "attributes": null,
//           "controls": null
//         }]
//       }]
//     },
//     {
//       "id": 68,
//       "name": "ClientPortalRolesTabUIId",
//       "dataSource": null,
//       "displayName": "Roles",
//       "sequence": 2,
//       "type": "Tab",
//       "dependency": false,
//       "attributes": null,
//       "controls": [{
//         "id": 85,
//         "name": "RoleTabSelectGridUIId",
//         "dataSource": null,
//         "displayName": null,
//         "sequence": 2,
//         "type": "Multi Select Grid",
//         "dependency": false,
//         "attributes": null,
//         "controls": [{
//           "id": 41,
//           "name": "RoleSelectColumnUIId",
//           "dataSource": "isAssigned",
//           "displayName": null,
//           "sequence": 1,
//           "type": "CheckBox",
//           "dependency": false,
//           "attributes": null,
//           "controls": null
//         },
//         {
//           "id": 80,
//           "name": "RoleNameColumnUIId",
//           "dataSource": "name",
//           "displayName": "Role",
//           "sequence": 2,
//           "type": "Label",
//           "dependency": false,
//           "attributes": null,
//           "controls": null
//         },
//         {
//           "id": 80,
//           "name": "RoleNameColumnUIId",
//           "dataSource": "roletype",
//           "displayName": "Role Type",
//           "sequence": 2,
//           "type": "Label",
//           "dependency": false,
//           "attributes": null,
//           "controls": null
//         }]
//       }]
//     }]
//   }]
// };
var data = {
  "pageId": 7,
  "pageDisplayName": "OneSite Product Access",
  "productId": 1,
  "productName": "OneSite",
  "controls": [
    {
      "id": 102,
      "name": "OneSiteProductAccessTabGroupUIId",
      "dataSource": null,
      "displayName": null,
      "sequence": 1,
      "type": "Tab Group",
      "dependency": false,
      "attributes": null,
      "controls": [
        {
          "id": 103,
          "name": "OneSiteProductAccessPropertiesTabUIId",
          "dataSource": null,
          "displayName": "Properties",
          "sequence": 1,
          "type": "Tab",
          "dependency": false,
          "attributes": [{"key": "Default","value": "True" }],
          "controls": [
            {
              "id": 104,
              "name": "OneSiteProductAccessAllowaccesstoallcurrentandfuturepropertiesPropertiesSwitchUIId",
              "dataSource": "all",
              "displayName": "Allow access to all current and future properties",
              "sequence": 1,
              "type": "Switch",
              "dependency": false,
              "attributes": null,
              "controls": null
            },
            {
              "id": 105,
              "name": "OneSiteProductAccessPropertiesMultiSelectGridUIId",
              "dataSource": null,
              "displayName": null,
              "sequence": 2,
              "type": "Multi Select Grid",
              "dependency": false,
              "attributes": null,
              "controls": [
                {
                  "id": 106,
                  "name": "OneSiteProductAccessCheckboxUIId",
                  "dataSource": "isAssigned",
                  "displayName": null,
                  "sequence": 1,
                  "type": "Checkbox",
                  "dependency": false,
                  "attributes": null,
                  "controls": null
                },
                {
                  "id": 107,
                  "name": "OneSiteProductAccessPropertyLabelUIId",
                  "dataSource": "name",
                  "displayName": "Property",
                  "sequence": 2,
                  "type": "Label",
                  "dependency": false,
                  "attributes": null,
                  "controls": null
                },
                {
                  "id": 108,
                  "name": "OneSiteProductAccessStateLabelUIId",
                  "dataSource": "state",
                  "displayName": "State",
                  "sequence": 3,
                  "type": "Label",
                  "dependency": false,
                  "attributes": null,
                  "controls": null
                }
              ]
            }
          ]
        },
        {
          "id": 109,
          "name": "OneSiteProductAccessRolesTabUIId",
          "dataSource": null,
          "displayName": "Roles",
          "sequence": 2,
          "type": "Tab",
          "dependency": false,
          "attributes": null,
          "controls": [
            {
              "id": 110,
              "name": "OneSiteProductAccessRolesMultiSelectGridUIId",
              "dataSource": null,
              "displayName": null,
              "sequence": 1,
              "type": "Multi Select Grid",
              "dependency": false,
              "attributes": null,
              "controls": [
                {
                  "id": 111,
                  "name": "OneSiteProductAccessCheckboxUIId",
                  "dataSource": "isAssigned",
                  "displayName": null,
                  "sequence": 1,
                  "type": "Checkbox",
                  "dependency": false,
                  "attributes": null,
                  "controls": null
                },
                {
                  "id": 112,
                  "name": "OneSiteProductAccessRoleLabelUIId",
                  "dataSource": "name",
                  "displayName": "Role",
                  "sequence": 2,
                  "type": "Label",
                  "dependency": false,
                  "attributes": null,
                  "controls": null
                },
                {
                  "id": 113,
                  "name": "OneSiteProductAccessRoleTypeLabelUIId",
                  "dataSource": "roletype",
                  "displayName": "Role Type",
                  "sequence": 3,
                  "type": "Label",
                  "dependency": false,
                  "attributes": null,
                  "controls": null
                },
                {
                  "id": 114,
                  "name": "OneSiteProductAccessIconUIId",
                  "dataSource": "InfoIcon",
                  "displayName": null,
                  "sequence": 4,
                  "type": "Icon",
                  "dependency": false,
                  "attributes": [
                    {
                      "key": "InfoIcon",
                      "value": "Slide"
                    }
                  ],
                  "controls": [
                    {
                      "id": 115,
                      "name": "OneSiteProductAccessRoleDetailsLabelUIId",
                      "dataSource": null,
                      "displayName": "Role Details",
                      "sequence": 1,
                      "type": "Label",
                      "dependency": false,
                      "attributes": null,
                      "controls": null
                    },
                    {
                      "id": 116,
                      "name": "OneSiteProductAccessGridUIId",
                      "dataSource": null,
                      "displayName": "Role Details",
                      "sequence": 2,
                      "type": "Grid",
                      "dependency": false,
                      "attributes": null,
                      "controls": [
                        {
                          "id": 117,
                          "name": "OneSiteProductAccessProductCenterLabelUIId",
                          "dataSource": "centerName",
                          "displayName": "Product Center",
                          "sequence": 1,
                          "type": "Label",
                          "dependency": false,
                          "attributes": null,
                          "controls": null
                        },
                        {
                          "id": 118,
                          "name": "OneSiteProductAccessRightLabelUIId",
                          "dataSource": "description",
                          "displayName": "Right",
                          "sequence": 2,
                          "type": "Label",
                          "dependency": false,
                          "attributes": null,
                          "controls": null
                        }
                      ]
                    }
                  ]
                }
              ]
            }
          ]
        }
      ]
    }
  ]
};



    angular
        .module("settings")
        .value("DataModelOneSite", data
        );
})(angular);
