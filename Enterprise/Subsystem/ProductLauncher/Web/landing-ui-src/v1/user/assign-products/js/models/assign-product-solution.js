//  Assign Product Solution Model

(function (angular, undefined) {
    "use strict";

    function factory($filter, selectAllItem, persona) {
        function AssignProductSolutionModel() {
            var s = this;
            s.init();
        }

        var p = AssignProductSolutionModel.prototype;

        p.init = function () {
            var s = this;
            s.data = {};
        };

        // Getters

        p.getData = function () {
            var s = this,
                data = angular.copy(s.data);

            delete data.active;
            delete data.touched;
            delete data.selected;
            delete data.assignedUntouched;
            delete data.disabled;

            return data;
        };

        p.getId = function () {
            var s = this;
            return s.data.solutionId;
        };

        p.getKey = function () {
            var s = this;
            return "soln" + s.getId();
        };

        p.getSelectItem = function () {
            var s = this;
            return s.selectItem;
        };

        p.getTitleText = function () {
            var s = this,
                key = "text.solnTitle." + s.data.solutionId;

            return $filter("assignProductsText")(key);
        };

        p.getProductId = function () {
            var s = this;
            return s.data.productId;
        };

        // Setters

        p.setData = function (data) {
            var s = this;
            s.data = data || {};
            s.data.active = true;
            s.data.touched = false;
            s.data.selected = false;
            s.data.assignedUntouched = data.isAssigned;
            s.data.disabled = data.lockOnProductAccess === true ? true : false;

            s.selectItem = selectAllItem({
                obj: s.data,
                activeKey: "active",
                disabledKey: "disabled",
                selectionKey: "isAssigned"
            });

            return s;
        };

        p.setSelected = function (bool) {
            var s = this;

            if (bool === true) {
                s.setTouched();
            }

            bool = bool === undefined ? true : bool;
            s.data.selected = bool;
            return s;
        };

        p.setTouched = function () {
            var s = this;
            s.data.touched = true;
        };

        // Assertions

        p.assignmentChanged = function () {
            var s = this;
            return s.data.isAssigned !== s.data.assignedUntouched;
        };

        p.hasId = function (id) {
            var s = this;
            return s.data.solutionId == id;
        };

        p.isAssigned = function () {
            var s = this;
            return s.data.isAssigned;
        };

        p.isProductDisabled = function () {
            var s = this;
            //product 3 is unfied platform and it's always disabled
            return s.data.disabled && s.data.productId !== 3;
        };

        p.isSelected = function () {
            var s = this;
            return s.data.selected;
        };

        p.wasAssigned = function () {
            var s = this;
            return s.data.assignedUntouched;
        };

        p.wasTouched = function () {
            var s = this;
            return s.data.touched;
        };

        p.destroy = function () {
            var s = this;
            s.data = undefined;
        };

        return function (data) {
            return (new AssignProductSolutionModel()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("assignProductSolutionModel", [
            "$filter",
            "rpSelectAllItemModel",
            "personaDetails",
            factory
        ]);
})(angular);
