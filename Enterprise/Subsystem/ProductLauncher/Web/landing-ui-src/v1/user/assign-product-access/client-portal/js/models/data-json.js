//  Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";

           
        var data = {
          "PageId": 4,
          "PageDisplayName": "Client Portal Product Access",
          "Tabs": [
            {
              "Id": 12,
              "Name": "SomeId",
              "DisplayName": "Properties",
              "Sequence": 1,
              "Controls": [
                {
                  "Id": 35,
                  "Type": "Radio Button",
                  "Name": "RadioControId",
                  "DisplayName": "Single Property",
                  "DataSource": "property",
                  "Sequence": 1
                },
                {
                  "Id": 36,
                  "Type": "Radio Button",
                  "Name": "RadioControlId",
                  "DisplayName": "All Properties",
                  "DataSource": "all",
                  "Sequence": 2
                },
                {
                  "Id": 37,
                  "Type": "Select Grid",
                  "Name": "SelectGridId",
                  "DisplayName": "",
                  "DataSource": "",
                  "Sequence": 3,
                  "Controls": [
                  {
                      "Id": 391,
                      "Type": "Column",
                      "Name": "AssignedColumnUIId",
                      "DisplayName": "IsAssigned",
                      "DataSource": "",
                      "Sequence": 2,
                      "Controls": [
                        {
                          "Id": 42,
                          "Type": "Radio Button",
                          "Name": "RadioControId",
                          "DisplayName": "",
                          "DataSource": "isAssigned",
                          "Sequence": 1
                        }
                      ]
                    },
                    {
                      "Id": 38,
                      "Type": "Column",
                      "Name": "PropertyNameColumnUIId",
                      "DisplayName": "Property",
                      "DataSource": "",
                      "Sequence": 1,
                      "Controls": [
                        {
                          "Id": 40,
                          "Type": "Label",
                          "Name": "PropertyLabelUIId",
                          "DisplayName": "",
                          "DataSource": "name",
                          "Sequence": 1
                        }
                      ]
                    },
                    {
                      "Id": 39,
                      "Type": "Column",
                      "Name": "StateColumnUIId",
                      "DisplayName": "State",
                      "DataSource": "",
                      "Sequence": 2,
                      "Controls": [
                        {
                          "Id": 41,
                          "Type": "Label",
                          "Name": "StateLabelUIId",
                          "DisplayName": "",
                          "DataSource": "state",
                          "Sequence": 1
                        }
                      ]
                    },
                     
                  ]
                }
              ]
            },
            {
              "Id": 11,
              "Name": "SomeId",
              "DisplayName": "Roles",
              "Sequence": 2,
              "Controls": [
                {
                  "Id": 42,
                  "Type": "Select Grid",
                  "Name": "SelectGridId",
                  "DisplayName": "",
                  "DataSource": "",
                  "Sequence": 1,
                  "Controls": [
                  {
                      "Id": 45,
                      "Type": "Column",
                      "Name": "AssignedColumnUIId",
                      "DisplayName": "IsAssigned",
                      "DataSource": "",
                      "Sequence": 2,
                      "Controls": [
                        {
                          "Id": 46,
                          "Type": "Radio Button",
                          "Name": "RadioControId",
                          "DisplayName": "",
                          "DataSource": "isAssigned",
                          "Sequence": 1
                        }
                      ]
                    },
                    {
                      "Id": 43,
                      "Type": "Column",
                      "Name": "RoleColumnUIId",
                      "DisplayName": "Role",
                      "DataSource": "",
                      "Sequence": 1,
                      "Controls": [
                        {
                          "Id": 44,
                          "Type": "Label",
                          "Name": "RoleLabelUIId",
                          "DisplayName": "",
                          "DataSource": "name",
                          "Sequence": 1
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
