//  Product Access Tabs Data Service

// (function(angular, undefined) {
//     "use strict";


//         var data = {
//           "Id": 3,
//           "Type": "Page",
//           "DisplayName": "Unified Platform Product Access",
//           "Controls": [{
//             "Id": 1,
//             "Type": "TabGroup",
//             "DisplayName": "",
//             "Controls": [{
//                 "Id": 9,
//                 "DisplayName": "Roles",
//                 "ParentId": null,
//                 "ActiveTab": true,
//                 "Sequence": 1,
//                 "Controls": [{
//                     "Id": 7,
//                     "ParentId": null,
//                     "Type": "MultiSelectGrid",
//                     "DisplayName": null,
//                     "Sequence": 1,
//                     "Controls": [{
//                       "Id": 10,
//                       "ParentId": 7,
//                       "Type": "GridColums",
//                       "DisplayName": "",
//                       "DataSource": "",
//                       "Sequence": 3,
//                       "Controls": [{
//                           "Id": 11,
//                           "ParentId": 10,
//                           "Type": "RadioButton",
//                           "DisplayName": "",
//                           "DataSource": "isAssigned",
//                           "Sequence": 1
//                         },
//                         {
//                           "Id": 12,
//                           "ParentId": 10,
//                           "Type": "Label",
//                           "DisplayName": "Name",
//                           "DataSource": "name",
//                           "Sequence": 2
//                         },
//                         {
//                           "Id": 13,
//                           "ParentId": 10,
//                           "Type": "Label",
//                           "DisplayName": "RoleType",
//                           "DataSource": "roletype",
//                           "Sequence": 3
//                         },
//                         {
//                           "Id": 14,
//                           "ParentId": 10,
//                           "Type": "Custom",
//                           "DisplayName": "",
//                           "DataSource": "infoIcon",
//                           "Sequence": 4
//                         }
//                       ]
//                     }]
//                   }
//                 ]
//               }
//             ]
//           }]
//         };

//     angular
//         .module("settings")
//         .value("DataModel1", data
//         );
// })(angular);



//  Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";

var data = {
  "pageId": 4,
  "pageDisplayName": "Unified Platform Product Access",
  "productId": 3,
  "productName": "Unified Login",
  "controls": [
    {
      "id": 66,
      "name": "UnifiedPlatformProductAccessTabGroupUIId",
      "dataSource": null,
      "displayName": null,
      "sequence": 1,
      "type": "Tab Group",
      "dependency": false,
      "attributes": null,
      "controls": [
        {
          "id": 68,
          "name": "UnifiedPlatformRolesTabUIId",
          "dataSource": null,
          "displayName": "Roles",
          "sequence": 2,
          "type": "Tab",
          "dependency": false,
          "attributes": [
            {
              "key": "Default",
              "value": "True"
            }
          ],
          "controls": [
            {
              "id": 85,
              "name": "RoleTabSelectGridUIId",
              "dataSource": null,
              "displayName": null,
              "sequence": 2,
              "type": "Select Grid",
              "dependency": false,
              "attributes": null,
              "controls": [
                {
                  "id": 41,
                  "name": "RoleSelectColumnUIId",
                  "dataSource": "isAssigned",
                  "displayName": null,
                  "sequence": 1,
                  "type": "Radio",
                  "dependency": false,
                  "attributes": null,
                  "controls": null
                },
                {
                  "id": 42,
                  "name": "RoleNameColumnUIId",
                  "dataSource": "name",
                  "displayName": "Role",
                  "sequence": 2,
                  "type": "Label",
                  "dependency": false,
                  "attributes": null,
                  "controls": null
                },
                {
                  "id": 43,
                  "name": "RoleTypeColumnUIId",
                  "dataSource": "roletype",
                  "displayName": "RoleType",
                  "sequence": 2,
                  "type": "Label",
                  "dependency": false,
                  "attributes": null,
                  "controls": null
                },
                {
                  "id": 44,
                  "name": "RoleIconColumnUIId",
                  "dataSource": "infoIcon",
                  "displayName": "",
                  "sequence": 2,
                  "type": "Custom",
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
};

    angular
        .module("settings")
        .value("DataModel1", data
        );
})(angular);
