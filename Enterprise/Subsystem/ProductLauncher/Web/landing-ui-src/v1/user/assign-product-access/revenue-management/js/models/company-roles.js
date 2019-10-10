//  revenue Management drop down Model

(function (angular, undefined) {
    "use strict";

    function factory(dropdownModel) {
        function RevenueManagementRolesDropdownModel() {
            var s = this;
            s.init();
        }

        var p = RevenueManagementRolesDropdownModel.prototype;

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
                labelText: "Select Role",
                getLabelText: s.getLabelText.bind(s),
                //container: ".rm-roles-grid",
                singleGroupSelect: false,
                singleOptionSelect: false,
                groups: [],
                errorMsgs: [],
                validators: {}
            };

            Object.seal(s.data);

            s.dropdown = dropdownModel(s.data);

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
            return (new RevenueManagementRolesDropdownModel()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("revenueManagementRolesDropdownModel", [
           "rpDropdownModel",
           factory
        ]);
})(angular);
