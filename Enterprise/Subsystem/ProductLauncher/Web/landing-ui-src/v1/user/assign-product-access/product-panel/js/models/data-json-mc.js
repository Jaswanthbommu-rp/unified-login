//  Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";


var data = {
  "Id": 9,
  "Type": "Page",
  "DisplayName": "Marketing Center Product Access",
  "Controls": [{
    "Id": 1,
    "Type": "TabGroup",
    "DisplayName": "",
    "Controls": [{
      "Id": 9,
      "DisplayName": "Properties",
      "ParentId": null,
      "ActiveTab": true,
      "Sequence": 1,
      "Controls": [      
      {
        "Id": 19,
        "ParentId": 7,
        "Type": "Switch",
        "DisplayName": "Assign new property by default",
        "DataSource": "assignedP",
        "Sequence": 3
      },      
      {
        "Id": 7,
        "ParentId": null,
        "Type": "MultiSelectGrid",
        "DisplayName": null,
        "Sequence": 1,
        "Controls": [{
          "Id": 10,
          "ParentId": 7,
          "Type": "GridColums",
          "DisplayName": "",
          "DataSource": "",
          "Sequence": 3,
          "Controls": [{
            "Id": 11,
            "ParentId": 10,
            "Type": "CheckBox",
            "DisplayName": "",
            "DataSource": "isAssigned",
            "Sequence": 1
          },
          {
            "Id": 12,
            "ParentId": 10,
            "Type": "Label",
            "DisplayName": "Property",
            "DataSource": "name",
            "Sequence": 2
          },
          {
            "Id": 13,
            "ParentId": 10,
            "Type": "Label",
            "DisplayName": "State",
            "DataSource": "state",
            "Sequence": 3
          }]
        }]
      }]
    },
    {
      "id": 15,
      "DisplayName": "Roles",
      "ActiveTab": false,
      "Sequence": 2,
      "Controls": [{
        "Id": 16,
        "ParentId": null,
        "Type": "MultiSelectGrid",
        "Sequence": 1,
        "Controls": [{
          "Id": 17,
          "ParentId": 16,
          "Type": "GridColums",
          "DisplayName": "Role",
          "Sequence": 1,
          "Controls": [{
            "Id": 18,
            "ParentId": 16,
            "Type": "RadioButton",
            "DisplayName": "",
            "DataSource": "isAssigned",
            "Sequence": 1
          },
          {
            "Id": 19,
            "ParentId": 16,
            "Type": "Label",
            "DisplayName": "Role",
            "DataSource": "name",
            "Sequence": 2
          }]
        }]
      }]
    }]
  }]
};

    angular
        .module("settings")
        .value("DataModelMc", data
        );
})(angular);
