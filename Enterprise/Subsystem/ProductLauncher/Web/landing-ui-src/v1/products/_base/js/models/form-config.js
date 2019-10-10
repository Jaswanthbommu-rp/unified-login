//  Sample Select Menu Form Config

(function (angular) {
    "use strict";

    function factory($filter, baseFormConfig, menuConfig, inputConfig) {
        var getText = $filter("productsText");

        var defaultOptions = {
            prodFamily: [
                {
                    name: getText("products.filters.familyDefault"),
                    value: ""
                }
            ],

            prodSolution: [
                {
                    name: getText("products.filters.solutionDefault"),
                    value: ""
                }
            ]
        };

        var model = baseFormConfig();

        model.prodFamily = menuConfig({
            onChange: model.methods.get("onFamilyFilterChange")
        });

        model.prodSolution = menuConfig({
            onChange: model.methods.get("onSolnFilterChange")
        });

        model.prodSolution.setOptionsFilter(model.methods.get("filterSolutionOptions"));

        model.searchText = inputConfig({
            id: "searchText",
            fieldName: "searchText",
            iconClass: "rp-icon-search",
            onChange: model.methods.get("onSearchFilterChange"),
            placeholder: getText("products.filters.searchTextPlaceholder")
        });

        model.setOptions = function (fieldName, fieldOptions) {
            if (model[fieldName]) {
                var options = defaultOptions[fieldName].concat(fieldOptions);
                model[fieldName].setOptions(options);
            }
            else {
                logc("productsFilterFormConfig.setOptions: " + fieldName + " is not a valid field name!");
            }

            return model;
        };

        return model;
    }

    angular
        .module("settings")
        .factory("productsFilterFormConfig", [
            "$filter",
            "baseFormConfig",
            "rpFormSelectMenuConfig",
            "rpFormInputTextConfig",
            factory
        ]);
})(angular);
