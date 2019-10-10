//  Property Group Grid Config Model

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig, security, persona) {
        var model = gridConfig();

        model.get = function() {
            return [{
                key: "isAssigned",
                type: "select",
                idKey: "id"
            }, {
                key: "name",
                type: "text",
            }, {
                key: "infoTooltip",
                type: "custom",
                templateUrl: "user/assign-product-access/marketing-center/templates/property-group-icon.html"
            }];
        };

        model.getHeaders = function() {
            return [
                [{
                    key: "isAssigned",
                    type: "select",
                    enabled: model.isSelectAllEnabled()
                }, {
                    key: "name",
                    text: "Property Group",
                }, {
                    key: "infoTooltip",
                    text: ""
                }]
            ];
        };

        model.getFilters = function () {
            return [
                {
                    key: "isAssigned",
                    type: "menu",
                    value: "",
                    options: [
                        {
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
                    key: "name",
                    type: "text",
                    placeholder: "Filter by Property Group Name"
                },
                { }
            ];
        };

        model.isSelectAllEnabled = function () {
            var isEnabled = !security.isAllowed("viewUser");
            if (isEnabled  && persona.isReady())
            {
                isEnabled = persona.data.hasManageMarketingCenterProductAccess;
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
        .factory("MCPropertyGroupGridConfig", [
            "rpGridConfig",
            "routeSecurity",
            "personaDetails",
            factory
        ]);
})(angular);
