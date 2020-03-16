//  Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";

var data = {
  "pageId": 4,
  "pageDisplayName": "Client Portal Product Access",
  "productId": 14,
  "productName": "Client Portal",
  "controls": [
    {
      "id": 66,
      "name": "ClientPortalProductAccessTabGroupUIId",
      "dataSource": null,
      "displayName": null,
      "sequence": 1,
      "type": "Tab Group",
      "dependency": false,
      "attributes": null,
      "controls": [
        {
          "id": 67,
          "name": "clientProtalPropertiesTabUIid",
          "dataSource": null,
          "displayName": "Properties",
          "sequence": 1,
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
              "id": 35,
              "name": "RadioControId",
              "dataSource": "property",
              "displayName": "Single Property",
              "sequence": 1,
              "type": "Radio",
              "dependency": false,
              "attributes": null,
              "controls": null
            },
            {
              "id": 36,
              "name": "RadioControlId",
              "dataSource": "all",
              "displayName": "All Properties",
              "sequence": 2,
              "type": "Radio",
              "dependency": false,
              "attributes": null,
              "controls": null
            },
            {
              "id": 73,
              "name": "PropertiesTabSelectGridUIId",
              "dataSource": null,
              "displayName": null,
              "sequence": 3,
              "type": "Select Grid",
              "dependency": false,
              "attributes": null,
              "controls": [
                {
                  "id": 40,
                  "name": "PropertySelectColumnUIId",
                  "dataSource": "isAssigned",
                  "displayName": null,
                  "sequence": 1,
                  "type": "Radio",
                  "dependency": false,
                  "attributes": null,
                  "controls": null
                },
                {
                  "id": 38,
                  "name": "PropertyNameColumnUIId",
                  "dataSource": "name",
                  "displayName": "Property",
                  "sequence": 2,
                  "type": "Label",
                  "dependency": false,
                  "attributes": null,
                  "controls": null
                },
                {
                  "id": 39,
                  "name": "StateColumnUIId",
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
          "id": 68,
          "name": "ClientPortalRolesTabUIId",
          "dataSource": null,
          "displayName": "Roles",
          "sequence": 2,
          "type": "Tab",
          "dependency": false,
          "attributes": null,
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
                  "id": 80,
                  "name": "RoleNameColumnUIId",
                  "dataSource": "name",
                  "displayName": "Role",
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
};

    angular
        .module("settings")
        .value("DataModel", data
        );
})(angular);
