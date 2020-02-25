//  Product Access Tabs Data Service

(function(angular, undefined) {
    "use strict";

           
        // var data = {
        //   "PageId": 4,
        //   "PageDisplayName": "Client Portal Product Access",
        //   "Tabs": [
        //     {
        //       "Id": 12,
        //       "Name": "SomeId",
        //       "DisplayName": "Properties",
        //       "Sequence": 1,
        //       "Controls": [
        //         {
        //           "Id": 35,
        //           "Type": "Radio Button",
        //           "Name": "RadioControId",
        //           "DisplayName": "Single Property",
        //           "DataSource": "property",
        //           "Sequence": 1
        //         },
        //         {
        //           "Id": 36,
        //           "Type": "Radio Button",
        //           "Name": "RadioControlId",
        //           "DisplayName": "All Properties",
        //           "DataSource": "all",
        //           "Sequence": 2
        //         },
        //         {
        //           "Id": 37,
        //           "Type": "Select Grid",
        //           "Name": "SelectGridId",
        //           "DisplayName": "",
        //           "DataSource": "",
        //           "Sequence": 3,
        //           "Controls": [
        //           {
        //               "Id": 391,
        //               "Type": "Column",
        //               "Name": "AssignedColumnUIId",
        //               "DisplayName": "IsAssigned",
        //               "DataSource": "",
        //               "Sequence": 2,
        //               "Controls": [
        //                 {
        //                   "Id": 42,
        //                   "Type": "Radio Button",
        //                   "Name": "RadioControId",
        //                   "DisplayName": "",
        //                   "DataSource": "isAssigned",
        //                   "Sequence": 1
        //                 }
        //               ]
        //             },
        //             {
        //               "Id": 38,
        //               "Type": "Column",
        //               "Name": "PropertyNameColumnUIId",
        //               "DisplayName": "Property",
        //               "DataSource": "",
        //               "Sequence": 1,
        //               "Controls": [
        //                 {
        //                   "Id": 40,
        //                   "Type": "Label",
        //                   "Name": "PropertyLabelUIId",
        //                   "DisplayName": "",
        //                   "DataSource": "name",
        //                   "Sequence": 1
        //                 }
        //               ]
        //             },
        //             {
        //               "Id": 39,
        //               "Type": "Column",
        //               "Name": "StateColumnUIId",
        //               "DisplayName": "State",
        //               "DataSource": "",
        //               "Sequence": 2,
        //               "Controls": [
        //                 {
        //                   "Id": 41,
        //                   "Type": "Label",
        //                   "Name": "StateLabelUIId",
        //                   "DisplayName": "",
        //                   "DataSource": "state",
        //                   "Sequence": 1
        //                 }
        //               ]
        //             },
                     
        //           ]
        //         }
        //       ]
        //     },
        //     {
        //       "Id": 11,
        //       "Name": "SomeId",
        //       "DisplayName": "Roles",
        //       "Sequence": 2,
        //       "Controls": [
        //         {
        //           "Id": 42,
        //           "Type": "Select Grid",
        //           "Name": "SelectGridId",
        //           "DisplayName": "",
        //           "DataSource": "",
        //           "Sequence": 1,
        //           "Controls": [
        //           {
        //               "Id": 45,
        //               "Type": "Column",
        //               "Name": "AssignedColumnUIId",
        //               "DisplayName": "IsAssigned",
        //               "DataSource": "",
        //               "Sequence": 2,
        //               "Controls": [
        //                 {
        //                   "Id": 46,
        //                   "Type": "Radio Button",
        //                   "Name": "RadioControId",
        //                   "DisplayName": "",
        //                   "DataSource": "isAssigned",
        //                   "Sequence": 1
        //                 }
        //               ]
        //             },
        //             {
        //               "Id": 43,
        //               "Type": "Column",
        //               "Name": "RoleColumnUIId",
        //               "DisplayName": "Role",
        //               "DataSource": "",
        //               "Sequence": 1,
        //               "Controls": [
        //                 {
        //                   "Id": 44,
        //                   "Type": "Label",
        //                   "Name": "RoleLabelUIId",
        //                   "DisplayName": "",
        //                   "DataSource": "name",
        //                   "Sequence": 1
        //                 }
        //               ]
        //             }
                    
        //           ]
        //         }
        //       ]
        //     }
        //   ]
        // };

        var data = {
  "Id": 0,
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
                }
              ]
            }]
          }
        ]
      },
      {
        "id": 15,
        "DisplayName": "Roles",
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
              }
            ]
          }]
        }]
      }
    ]
  }]
};

    angular
        .module("settings")
        .value("DataModel", data
        );
})(angular);
