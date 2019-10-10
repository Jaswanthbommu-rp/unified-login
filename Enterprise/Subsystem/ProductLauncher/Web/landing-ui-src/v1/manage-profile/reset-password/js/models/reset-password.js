//  Reset Password Form Model

(function (angular, undefined) {
    "use strict";

    function factory($q, $timeout, $stateParams, regex, user, dataSvc, userProfileModel) {
        function ResetPasswordTabModel() {
            var s = this;
            s.init();
        }

        var p = ResetPasswordTabModel.prototype;

        p.init = function () {
            var s = this;
            s.data = {
                oldPassword: "",
                newPassword: "",
                newPasswordCopy: ""
            };

            s.count = {
                minChars: 8,
                maxChars: 20,
                numChars: 1,
                specialChars: 1,
                upperCaseChars: 1,
                lowerCaseChars: 1
            };

            s._data = angular.copy(s.data);

            if ($stateParams.userId) { //means manage profile is accessed from edit user
                s.setUsername(userProfileModel.getUsername());
            }
            else {
                s.setUsername(user.getUsername());
            }
        };

        // Getters

        p.getData = function () {
            var s = this;
            return s.data;
        };

        p.getReqData = function () {
            var s = this;
            return {
                newPassword: s.data.newPassword,
                oldPassword: s.data.oldPassword
            };
        };

        // Setters

        p.setUsername = function (username) {
            var s = this;
            s.username = username;
            return s;
        };

        p.setErrorMsg = function (errorMsg) {
            var s = this;
            s.data.errorMsg = errorMsg;
            return s;
        };

        // Assertions

        p.hasLowercaseChars = function () {
            var s = this;
            return s.data.newPassword && !!s.data.newPassword.match(/[a-z]+/);
        };

        p.hasMinChars = function () {
            var s = this;
            return s.data.newPassword && s.data.newPassword.length >= 8;
        };

        p.hasNumChars = function () {
            var s = this;
            return s.data.newPassword && !!s.data.newPassword.match(/[0-9]+/);
        };

        p.hasUppercaseChars = function () {
            var s = this;
            return s.data.newPassword && !!s.data.newPassword.match(/[A-Z]+/);
        };

        p.hasSpecialChars = function () {
            var s = this,
                pattern = regex.password;
            return s.data.newPassword && !!s.data.newPassword.match(pattern);
        };

        p.isDirty = function () {
            var s = this;
            return !angular.equals(s.data, s._data);
        };

        p.newPasswordMatchesCurrent = function () {
            var s = this,
                deferred = $q.defer();

            $timeout(function () {
                var bool = s.data.newPassword === s.data.oldPassword;
                deferred[bool ? "reject" : "resolve"]();
            }, 50);

            return deferred.promise;
        };

        p.passwordCopyIsValid = function () {
            var s = this,
                deferred = $q.defer();

            $timeout(function () {
                var bool = s.data.newPassword === s.data.newPasswordCopy;
                deferred[bool ? "resolve" : "reject"]();
            }, 50);

            return deferred.promise;
        };

        p.passwordIsValid = function () {
            var s = this,
                deferred = $q.defer();

            $timeout(function () {
                var bool = s.hasMinChars() &&
                    s.hasUppercaseChars() &&
                    s.hasLowercaseChars() &&
                    s.hasNumChars() &&
                    s.hasSpecialChars();

                deferred[bool ? "resolve" : "reject"]();
            }, 50);

            return deferred.promise;
        };

        // Actions

        p.reset = function () {
            var s = this;
            s.data = angular.copy(s._data);
            return s;
        };

        return new ResetPasswordTabModel();
    }

    angular
        .module("settings")
        .factory("mpResetPasswordTabModel", [
            "$q",
            "$timeout",
            "$stateParams",
            "gbRegex",
            "userSessionModel",
            "mpResetPasswordSvc",
            "userProfileModel",
            factory
        ]);
})(angular);
