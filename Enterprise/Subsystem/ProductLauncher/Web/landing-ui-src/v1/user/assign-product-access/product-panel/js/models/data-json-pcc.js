//  Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";

var data = {
  "pageId": 3,
  "pageDisplayName": "Prospect Contact Center Product Access",
  "productId": 10,
  "productName": "Prospect Contact Center",
  "controls":
  [
    {
      "id": 66,
      "name": "ClientPortalProductAccessTabGroupUIId",
      "dataSource": null,
      "displayName": null,
      "sequence": 1,
      "type": "Tab Group",
      "dependency": false,
      "attributes": null,
      "controls":
      [
          {
            "id": 67,
            "name": "clientProtalPropertiesTabUIid",
            "dataSource": null,
            "displayName": "Properties",
            "sequence": 1,
            "type": "Tab",
            "dependency": false,
            "attributes": [{"key": "Default","value": "True" }],
            "controls":
            [
              {
                "id": 25,
                "name": "RadioButtonId",
                "dataSource": "active",
                "displayName": "Active Properties",
                "sequence": 1,
                "type": "Radio",
                "dependency": false,
                "attributes": null,
                "controls": null
              },
              {
                "id": 26,
                "name": "RadioButtonId",
                "dataSource": "inactive",
                "displayName": "Inactive Properties",
                "sequence": 2,
                "type": "Radio",
                "dependency": false,
                "attributes": null,
                "controls": null
              },
              {
                "id": 27,
                "name": "RadioButtonId",
                "dataSource": "all",
                "displayName": "All Properties",
                "sequence": 3,
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
                "controls":
                [
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
          }
      ]
    }
  ]
};
    angular
        .module("settings")
        .value("DataModelpcc", data
        );
})(angular);
