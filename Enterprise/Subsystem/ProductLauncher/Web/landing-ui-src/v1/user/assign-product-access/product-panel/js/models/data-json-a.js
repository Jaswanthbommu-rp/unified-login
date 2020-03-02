//  Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";


        var data = {
          "Id": 3,
          "Type": "Page",
          "DisplayName": "Unified Platform Product Access",
          "Controls": [{
            "Id": 1,
            "Type": "TabGroup",
            "DisplayName": "",
            "Controls": [{
                "Id": 9,
                "DisplayName": "Roles",
                "ParentId": null,
                "ActiveTab": true,
                "Sequence": 1,
                "Controls": [{
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
                          "DisplayName": "RoleType",
                          "DataSource": "roletype",
                          "Sequence": 3
                        }
                      ]
                    }]
                  }
                ]
              }
            ]
          }]
        };

    angular
        .module("settings")
        .value("DataModel1", data
        );
})(angular);
