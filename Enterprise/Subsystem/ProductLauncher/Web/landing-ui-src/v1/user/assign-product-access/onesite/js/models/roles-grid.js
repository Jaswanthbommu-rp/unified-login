//  Rights Grid Model

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
                key: "name"
            }, {
                key: "roletype"
            }, {
                key: "infoIcon",
                type: "custom",
                templateUrl: "user/assign-product-access/onesite/templates/rights-info-icon.html"
            }];
        };

        model.getHeaders = function() {
            return [
                [{
                    key: "isAssigned",
                    type: "select",
                    enabled:false  // model.isSelectAllEnabled()
                }, {
                    key: "name",
                    text: "Role",
                }, {
                    key: "roletype",
                    text: "Role Type",
                }, {
                    key: "infoIcon",
                    text: "",
                }]
            ];
        };

        model.getFilters = function () {
            return [
                {
                    key: "isAssigned",
                    value: "",
                    type: "menu",
                    size: "small",
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
                    placeholder: "Filter by Role Name"
                },
                {
                    key: "roletype",
                    type: "text",
                    placeholder: "Filter by Role Type"
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
                isEnabled = persona.data.hasManageOneSiteProductAccess;
            }
            return isEnabled;
        };

        model.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageOneSiteProductAccess;
        };

         model.personaWatch = angular.noop;
         model.personaWatch();
         model.personaWatch = persona.subscribe(model.isSelectAllEnabled);

        return model;
    }
    angular
        .module("settings")
        .factory("osRolesGridConfig", [
            "rpGridConfig",
            "routeSecurity",
            "personaDetails",
            factory
        ]);
})(angular);
