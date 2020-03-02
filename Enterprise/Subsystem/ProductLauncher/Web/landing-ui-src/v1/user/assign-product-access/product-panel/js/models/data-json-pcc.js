//  Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";


var data = {
  "Id": 10,
  "Type": "Page",
  "DisplayName": "Prospect Contact Center Product Access",
  "Controls": [{
    "Id": 1,
    "Type": "TabGroup",
    "DisplayName": "",
    "Controls": [{
      "Id": 9,
      "DisplayName": "Properties",
      "ParentId": null,
      "Sequence": 1,
      "Controls": [
      {
        "Id": 8,
        "ParentId": 7,
        "Type": "RadioButton",
        "DisplayName": "Active Properties",
        "DataSource": "property",
        "Sequence": 1
        },
        {
        "Id": 9,
        "ParentId": 7,
        "Type": "RadioButton",
        "DisplayName": "All Properties",
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
    }]
  }]
};
    angular
        .module("settings")
        .value("DataModelpcc", data
        );
})(angular);
