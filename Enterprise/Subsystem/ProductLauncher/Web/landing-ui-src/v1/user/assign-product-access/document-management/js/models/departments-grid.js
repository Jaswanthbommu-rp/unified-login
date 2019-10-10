//  Rights Grid Model

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig, security, persona) {
        var model = gridConfig();

        model.get = function () {
            return [{
                key: "isAssigned",
                type: "select",
                idKey: "id"
            }, {
                key: "name"
            }];
        };

        model.getHeaders = function () {
            return [
                [{
                    key: "isAssigned",
                    type: "select",
                    enabled: model.isSelectAllEnabled()
                }, {
                    key: "name",
                    text: "Department",
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
                    placeholder: "Filter by Department Name"
                },
            ];
        };

        model.isSelectAllEnabled = function () {
            var isEnabled = !security.isAllowed("viewUser");
            if (isEnabled  && persona.isReady())
            {
                isEnabled = persona.data.hasManageDocumentManagementProductAccess;
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
        .factory("docMgmtDepartmentsGridConfig", [
            "rpGridConfig",
            "routeSecurity",
            "personaDetails",
            factory
        ]);
})(angular);
