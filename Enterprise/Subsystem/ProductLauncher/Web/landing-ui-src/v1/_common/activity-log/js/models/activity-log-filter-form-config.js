// Activity Log Filter Configuration

(function (angular) {
    "use strict";

    function factory($filter, baseFormConfig, inputTextConfig, menuConfig, dateTimePickerConfig) {
        var model = baseFormConfig();


        model.init = function () {

            model.keyword = inputTextConfig({
                fieldName: "keyword",
                iconClass: "rp-icon-search2",
                placeholder: "Filter by keyword",
                modelOptions: {
                    debounce: 400
                },
                onChange: model.getMethod("filterByKeyword")
            });

            model.activities = menuConfig({
                nameKey: "label",
                valueKey: "value",
                onChange: model.getMethod("filterByActivities")
            });

            model.daterange = menuConfig({
                nameKey: "label",
                valueKey: "value",
                onChange: model.getMethod("filterByDates")
            });
            model.startDate = dateTimePickerConfig({
                id: "startDate",
                fieldName: "startDate",
                required: false,
                iconClass: "rp-icon-calendar",
                format: "MM/DD/YYYY",
                maxDate: new Date(),
                onChange: model.methods.get("filterByStartDate")
            });

            model.endDate = dateTimePickerConfig({
                id: "endDate",
                fieldName: "endDate",
                required: false,
                iconClass: "rp-icon-calendar",
                format: "MM/DD/YYYY",
                maxDate: new Date(),
                onChange: model.methods.get("filterByEndDate")
            });

            model.sortby = menuConfig({
                nameKey: "label",
                valueKey: "value",
                onChange: model.getMethod("sortByFilter")
            });
        };

        model.reset = function () {
            model.keyword.destroy();
            model.keyword = undefined;
        };


        model.setOptions = function (fieldName, options) {
            if (model[fieldName]) {
                model[fieldName].setOptions(options);
            }
            else {
                logc("activityLogFormConfig.setOptions(): %s is not a valid field name!", fieldName);
            }
        };

        model.init();
        return model;
    }

    angular
        .module("settings")
        .factory("activityLogFormConfig", [
            "$filter",
            "baseFormConfig",
            "rpFormInputTextConfig",
            "rpFormSelectMenuConfig",
            "rpDatetimepickerConfig",
            factory
        ]);
})(angular);
