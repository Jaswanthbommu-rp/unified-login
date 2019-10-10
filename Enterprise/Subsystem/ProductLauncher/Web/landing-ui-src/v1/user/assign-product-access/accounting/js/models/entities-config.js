//  Accounting Entities Grid Config Model

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig, security, persona) {
        var model = gridConfig();

        model.get = function() {
            return [{
                        key: "isAssigned",
                        type: "select",
                        idKey: "propertyId"
                    }, {
                        key: "propertyId",
                        type: "text",
                    }, {
                        key: "propertyName",
                        type: "text",
                    },  {
                        key: "companyId",
                        type: "text",
                    }, {
                        key: "companyName",
                        type: "text",
                    }];
        };

        model.getHeaders = function() {
            return [
                [{
                    key: "isAssigned",
                    type: "select",
                    enabled: model.isSelectAllEnabled()
                }, {
                    key: "propertyId",
                    text: "Entity ID",
                }, {
                    key: "propertyName",
                    text: "Entity Name",
                },{
                    key: "companyId",
                    text: "Company ID",
                }, {
                    key: "companyName",
                    text: "Company Name",
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
                    key: "propertyId",
                    type: "text",
                    placeholder: "Filter by Entity ID"
                },
                {
                    key: "propertyName",
                    type: "text",
                    placeholder: "Filter by Entity Name"
                },
                {
                    key: "companyId",
                    type: "text",
                    placeholder: "Filter by Company Id"
                }, 
                {
                    key: "companyName",
                    type: "text",
                    placeholder: "Filter by Company Name"
                }
            ];
        };

        model.isSelectAllEnabled = function () {
            var isEnabled = !security.isAllowed("viewUser");
            if (isEnabled && persona.isReady())
            {
                isEnabled = persona.data.hasManageAccountingProductAccess;
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
        .factory("AEntitiesGridConfig", [
            "rpGridConfig",
            "routeSecurity",
            "personaDetails",
            factory
        ]);
})(angular);
