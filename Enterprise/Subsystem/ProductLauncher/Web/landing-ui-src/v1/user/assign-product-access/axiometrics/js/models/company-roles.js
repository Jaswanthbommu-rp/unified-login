//  axiometrics company role Model

(function (angular, undefined) {
    "use strict";

    function factory(dropdownModel) {
        function AXMRolesDropdownModel() {
            var s = this;
            s.init();
        }

        var p = AXMRolesDropdownModel.prototype;

        var baseConfig,
            model = {};

        p.init = function () {
            var s = this,
                onChange = s.onChange.bind(s);

            s.selectedOptions = [];
            //model.groups = [];
            s.data = {
                textKey: "name",
                required: false,
                valueKey: "isAssigned",
                menuClassName: "",
                disabledKey: "disabled",
                // container: "body",
                labelText: "Select Role",
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

        p.destroy = function () {
            var s = this;
            s.dropdown.destroy();
            s.selectedOptions = [];
            s.dropdown = undefined;
            s.selectedOptions = undefined;
        };

        return function (data) {
            return (new AXMRolesDropdownModel()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("axmRolesDropdownModel", [
           "rpDropdownModel",
           factory
        ]);
})(angular);
