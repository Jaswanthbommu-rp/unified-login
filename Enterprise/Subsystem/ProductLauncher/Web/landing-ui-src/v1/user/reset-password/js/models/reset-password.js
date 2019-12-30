//  Reset Password Form Model

(function (angular, undefined) {
    "use strict";

    function factory($q, $timeout, $params, regex, session, userDetModel) {
        function ResetPasswordModel() {
            var s = this;
            s.init();
        }

        var p = ResetPasswordModel.prototype;

        p.init = function (settings) {
            var s = this;
            s.data = {
                realPageId: "",
                oldPassword: "",
                newPassword: "",
                newPasswordCopy: ""
            };

            s.count = {
                minimumLength: 8,
                maximumLength: 20,
                numChars: 1,
                specialChars: 1,
                upperCaseChars: 1,
                lowerCaseChars: 1
            };

            if (settings) {
                angular.extend(s.count, settings);
            }

            s._data = angular.copy(s.data);
            

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

        p.getIsImpersonated = function () {
            var s = this;
            if( (userDetModel.isImpersonated() != undefined && userDetModel.isImpersonated() === true) && (session.getRealPageId() === $params.realPageId) ){                                        
                return true;
            }
            return false;
        };        


        // Setters
        p.setActive = function () {
            var s = this;
            return s;
        };

        p.setErrorMsg = function (errorMsg) {
            var s = this;
            s.data.errorMsg = errorMsg;
            return s;
        };

        p.hasLowercaseChars = function () {
            var s = this;
            return s.data.newPassword && !!s.data.newPassword.match(/[a-z]+/);
        };

        p.hasMinChars = function () {
            var s = this;
            return s.data.newPassword && s.data.newPassword.length >= s.count.minimumLength;
        };

        p.hasMaxChars = function () {
            var s = this;
            return s.data.newPassword && s.data.newPassword.length <= s.count.maximumLength;
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


        p.editingSelf = function () {
            var s = this;
            return session.getRealPageId() === $params.realPageId;
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
                    s.hasMaxChars() &&
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

        return new ResetPasswordModel();
    }

    angular
        .module("settings")
        .factory("ResetPasswordModel", [
            "$q",
            "$timeout",
            "$stateParams",
            "gbRegex",
            "userSessionModel",
            "userDetailsModel",
            factory
        ]);
})(angular);
