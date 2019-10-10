//  User View Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function UserViewModel() {
            var s = this;
            s.init();
        }

        var p = UserViewModel.prototype;

        p.init = function () {
            var s = this;
            s.saveTextKey = "text.btn.createUser";
        };

        p.setEditMode = function () {
            var s = this;
            s.saveTextKey = "text.btn.updateUser";
            return s;
        };

         p.setCloneMode = function () {
            var s = this;
            s.saveTextKey = "text.btn.createUser";
            return s;
        };

        p.reset = function () {
            var s = this;
            s.saveTextKey = "text.btn.createUser";
        };

        return new UserViewModel();
    }

    angular
        .module("settings")
        .factory("userViewModel", [factory]);
})(angular);
