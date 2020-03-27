//  Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";


var data = {
  "pageId": 8,
  "pageDisplayName": "Marketing Center Product Access",
  "productId": 9,
  "productName": "Marketing Center",
  "controls": [
    {
      "id": 119,
      "name": "MarketingCenterProductAccessTabGroupUIId",
      "dataSource": null,
      "displayName": null,
      "sequence": 1,
      "type": "Tab Group",
      "dependency": false,
      "attributes": null,
      "controls": [
        {
          "id": 120,
          "name": "MarketingCenterProductAccessPropertyGroupTabUIId",
          "dataSource": null,
          "displayName": "Property Group",
          "sequence": 1,
          "type": "Tab",
          "dependency": false,
          "attributes": null,
          "controls": [
            {
              "id": 121,
              "name": "MarketingCenterProductAccessPropertyGroupMultiSelectGridUIId",
              "dataSource": null,
              "displayName": null,
              "sequence": 1,
              "type": "Multi Select Grid",
              "dependency": false,
              "attributes": null,
              "controls": [
                {
                  "id": 122,
                  "name": "MarketingCenterProductAccessCheckboxUIId",
                  "dataSource": "isAssigned",
                  "displayName": null,
                  "sequence": 1,
                  "type": "Checkbox",
                  "dependency": false,
                  "attributes": null,
                  "controls": null
                },
                {
                  "id": 123,
                  "name": "MarketingCenterProductAccessPropertyGroupLabelUIId",
                  "dataSource": "name",
                  "displayName": "Property Group",
                  "sequence": 2,
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
          "id": 124,
          "name": "MarketingCenterProductAccessPropertiesTabUIId",
          "dataSource": null,
          "displayName": "Properties",
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
              "id": 125,
              "name": "MarketingCenterProductAccessAssignnewpropertiesautomaticallyPropertiesSwitchUIId",
              "dataSource": null,
              "displayName": "Assign new properties automatically",
              "sequence": 1,
              "type": "Switch",
              "dependency": false,
              "attributes": null,
              "controls": null
            },
            {
              "id": 126,
              "name": "MarketingCenterProductAccessPropertiesMultiSelectGridUIId",
              "dataSource": null,
              "displayName": null,
              "sequence": 2,
              "type": "Multi Select Grid",
              "dependency": false,
              "attributes": null,
              "controls": [
                {
                  "id": 127,
                  "name": "MarketingCenterProductAccessCheckboxUIId",
                  "dataSource": "isAssigned",
                  "displayName": null,
                  "sequence": 1,
                  "type": "Checkbox",
                  "dependency": false,
                  "attributes": null,
                  "controls": null
                },
                {
                  "id": 128,
                  "name": "MarketingCenterProductAccessPropertyLabelUIId",
                  "dataSource": "name",
                  "displayName": "Property",
                  "sequence": 2,
                  "type": "Label",
                  "dependency": false,
                  "attributes": null,
                  "controls": null
                },
                {
                  "id": 129,
                  "name": "MarketingCenterProductAccessStateLabelUIId",
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
          "id": 130,
          "name": "MarketingCenterProductAccessRolesTabUIId",
          "dataSource": null,
          "displayName": "Roles",
          "sequence": 3,
          "type": "Tab",
          "dependency": false,
          "attributes": null,
          "controls": [
            {
              "id": 131,
              "name": "MarketingCenterProductAccessRolesSelectGridUIId",
              "dataSource": null,
              "displayName": null,
              "sequence": 1,
              "type": "Select Grid",
              "dependency": false,
              "attributes": null,
              "controls": [
                {
                  "id": 132,
                  "name": "MarketingCenterProductAccessRadioUIId",
                  "dataSource": "isAssigned",
                  "displayName": null,
                  "sequence": 1,
                  "type": "Radio",
                  "dependency": false,
                  "attributes": null,
                  "controls": null
                },
                {
                  "id": 133,
                  "name": "MarketingCenterProductAccessRoleLabelUIId",
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
        .value("DataModelMc", data
        );
})(angular);
