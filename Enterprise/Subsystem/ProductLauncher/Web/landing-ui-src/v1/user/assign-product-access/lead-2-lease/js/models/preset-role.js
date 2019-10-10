//  presets Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function PresetRole() {
            var s = this;
            s.init();
        }

        var p = PresetRole.prototype;

        p.init = function () {
            var s = this;
            s.data = {
                "id": "",
                "name": "Select a Preset Role",
                "roleIds": []
            };

            s._data = angular.copy(s.data);
        };

        p.containsId = function (id) {
            var s = this;
            return s.data.roleIds.contains(parseInt(id));
        };

        p.getData = function () {
            var s = this;
            return s.data;
        };

        p.setData = function (role) {
            var s = this;
            s.data = role;
        };

        p.reset = function () {
            var s = this;

            s.data = angular.copy(s._data);
        };

        return new PresetRole();
    }

    angular
        .module("settings")
        .factory("presetRoleModel", [factory]);
})(angular);
