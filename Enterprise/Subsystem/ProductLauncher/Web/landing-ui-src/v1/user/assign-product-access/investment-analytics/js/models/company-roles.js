//  Sample Model

(function (angular, undefined) {
    "use strict";

    function factory(dropdownModel) {
        function InvestmentAnalyticsRolesDropdownModel() {
            var s = this;
            s.init();
        }

        var p = InvestmentAnalyticsRolesDropdownModel.prototype;

        var baseConfig,
            model = {};

        p.init = function () {
            var s = this,
                onChange = s.onChange.bind(s);

            s.selectedOptions = [];

            s.data = {
                textKey: "name",
                required: false,
                valueKey: "isAssigned",
                menuClassName: "",
                disabledKey: "disabled",
                // container: "body",
                labelText: "Select Role",
                getLabelText: s.getLabelText,
                singleGroupSelect: false,
                singleOptionSelect: false,
                groups: [],
                errorMsgs: [],
                validators: {}
            };

            Object.seal(s.data);

            s.dropdown = dropdownModel(s.data);
            s.selectedOptions = s.dropdown.getOptionsSelected();
        };

        p.onChange = function (data) {
            var s = this;
            s.selectedOptions = data;
            return s;
        };

        p.subscribe = function (callback) {
            var s = this;
            return s.dropdown.subscribe("change", callback);
        };
        // Setters
        p.setData = function (roledata) {
            var s = this;
            s.data.groups.push(roledata);
            s.dropdown.extendData(s.data);

            return s;
        };

        p.extendData = function (data) {
            var s = this;
            angular.extend(s.data, data || {});
            s.dropdown.extendData(s.data);

            return s;
        };

        p.getLabelText = function (count) {
            var s = this;
            if (count === 0) {
                return "Select Roles";
            }
            else {
                return count + " selected";
            }
        };

        p.destroy = function () {
            var s = this;
            s.dropdown.destroy();
            s.selectedOptions = [];
            s.dropdown = undefined;
            s.selectedOptions = undefined;
        };


        return function (data) {
            return (new InvestmentAnalyticsRolesDropdownModel()).setData(data);
        };

    }

    angular
        .module("settings")
        .factory("investmentAnalyticsRolesDropdownModel", [
           "rpDropdownModel",
           factory
        ]);
})(angular);
