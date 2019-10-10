// Product Item Properties Access Form Configuration

(function (angular) {
    "use strict";

    function factory($filter, baseFormConfig, inputTextConfig, menuConfig) {
        var model = baseFormConfig();

        model.user = inputTextConfig({
            fieldName: "userSearchText",
            iconClass: "rp-icon-search2",
            placeholder: $filter("userListText")("users_filter_user"),
            modelOptions: {
                debounce: 200
            },
            onChange: model.getMethod("filterByName")
        });

        model.products = menuConfig({ //TODO update when service is available
            nameKey: "productName",
            valueKey: "productId",
            onChange: model.getMethod("filterGrid")
        });

        model.properties = menuConfig({ //TODO update when service is available
            nameKey: "propertyName",
            valueKey: "propertyId",
            onChange: model.getMethod("filterGrid")
        });

        model.userType = menuConfig({
            nameKey: "label",
            valueKey: "value"
        });

        model.accountStatus = menuConfig({
            nameKey: "label",
            valueKey: "value",
            // onChange: model.getMethod("addAcctStateFilter")
        });

        model.lockStatus = menuConfig({
            nameKey: "label",
            valueKey: "value",
            // onChange: model.getMethod("addLockStateFilter")
        });

        model.moreFilters = {
            defaultText: $filter("userListText")("users_more_filters"),
            defaultIconClass: "rp-icon-filter active",
            activeIconClass: "rp-icon-filter"
        };

        model.processProductOptions = function (options) {
            options = options.filter(function (item) {
                return (item.showInUserListFilter);
            });

            return $filter("orderBy")(options, "productName");
        };

        model.setOptions = function (fieldName, options) {
            if (model[fieldName]) {
                if (fieldName == "products") {
                    options = model.processProductOptions(options);
                }

                model[fieldName].setOptions(options);
            }
            else {
                logc("usersListFormConfig.setOptions(): %s is not a valid field name!", fieldName);
            }
        };

        return model;
    }

    angular
        .module("settings")
        .factory("usersListFormConfig", [
            "$filter",
            "baseFormConfig",
            "rpFormInputTextConfig",
            "rpFormSelectMenuConfig",
            factory
        ]);
})(angular);
