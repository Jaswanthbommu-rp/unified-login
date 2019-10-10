//  BusinessIntelligence Property Group Grid Config Model

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig, security, persona) {
        var model = gridConfig();

        model.get = function() {
            return [{
                key: "isAssigned",
                type: "select",
                idKey: "groupId"
            }, {
                key: "groupName",
                type: "text",
            }, {
                key: "infoIcon",
                type: "custom",
                templateUrl: "user/assign-product-access/business-intelligence/templates/property-icon.html"
            }];

        };

        model.getHeaders = function() {
            return [
                [{
                    key: "isAssigned",
                    type: "select",
                    enabled: model.isSelectAllEnabled()
                }, {
                    key: "groupName",
                    text: "Group",
                }, {
                    key: "infoIcon",
                    text: "",
                }]
            ];
        };



        model.getFilters = function() {
            return [{
                    key: "isAssigned",
                    type: "menu",
                    value: "",
                    options: [{
                            value: "",
                            name: "All"
                        },
                        {
                            value: true,
                            name: "Selected"
                        },
                        {
                            value: false,
                            name: "Not Selected"
                        }
                    ]
                },
                {
                    key: "groupName",
                    type: "text",
                    placeholder: "Filter by Group Name"
                },
                {
                    key: "infoIcon",
                    type: "",
                    placeholder: ""
                }
            ];
        };

        model.isSelectAllEnabled = function () {
            var isEnabled = !security.isAllowed("viewUser");
            if (isEnabled  && persona.isReady())
            {
                isEnabled = persona.data.hasManageAssetOptimizationProductAccess;
            }
            return isEnabled;
        };

        model.personaWatch = angular.noop;
         model.personaWatch();
         model.personaWatch = persona.subscribe(model.isSelectAllEnabled);
         
        return model;
    }
    angular
        .module("settings")
        .factory("businessIntelligencePropertyGroupsGridConfig", [
            "rpGridConfig",
            "routeSecurity",
            "personaDetails",
            factory
        ]);
})(angular);