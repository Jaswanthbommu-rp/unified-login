//  Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";


var data = {
  "Id": 14,
  "Type": "Page",
  "DisplayName": "Client Portal Product Access",
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
      "Controls": [{
        "Id": 8,
        "ParentId": 7,
        "Type": "RadioButton",
        "DisplayName": "Single Property",
        "DataSource": "property",
        "Sequence": 1
      },
      {
        "Id": 9,
        "ParentId": 7,
        "Type": "RadioButton",
        "DisplayName": "All Property",
        "DataSource": "all",
        "Sequence": 2
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
            "Type": "RadioButton",
            "DisplayName": "",
            "DataSource": "isAssigned",
            "Sequence": 1
          },
          {
            "Id": 12,
            "ParentId": 10,
            "Type": "Label",
            "DisplayName": "Name",
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
        .value("DataModel", data
        );
})(angular);
