
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
