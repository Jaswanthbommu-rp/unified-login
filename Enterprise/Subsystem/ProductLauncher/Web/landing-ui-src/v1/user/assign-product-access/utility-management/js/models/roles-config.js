//  Roles Grid Config Model

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig, security, persona) {
        var model = gridConfig();

        model.get = function() {
            return [{
                key: "isAssigned",
                type: "select",
                idKey: "roleName"
            }, {
                key: "roleName",
                type: "text",
            }, {
                key: "roleDescription",
                type: "text"
            }];
        };

        model.getHeaders = function() {
            return [
                [{
                    key: "isAssigned",
                    type: "select",
                    enabled: model.isSelectAllEnabled()
                }, {
                    key: "roleName",
                    text: "Right",
                }, {
                    key: "roleDescription",
                    text: "Description"
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
                    key: "roleName",
                    type: "text",
                    placeholder: "Filter by Right Name"
                },
                {
                    key: "roleDescription",
                    type: "text",
                    placeholder: "Filter by Description"
                }
            ];
        };

        model.isSelectAllEnabled = function () {
            var isEnabled = !security.isAllowed("viewUser");
            if (isEnabled  && persona.isReady())
            {
                isEnabled = persona.data.hasManageVendorComplianceProductAccess;
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
        .factory("UtilityManagementRolesGridConfig", [
            "rpGridConfig",
            "routeSecurity",
            "personaDetails",
            factory
        ]);
})(angular);
