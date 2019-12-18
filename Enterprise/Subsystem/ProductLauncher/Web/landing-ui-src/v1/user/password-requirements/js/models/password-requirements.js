//  Password Requirements Model

(function (angular, undefined) {
    "use strict";

    function factory(regexlib) {
        function PassReqModel() {
            var s = this;
            s.init();
        }

        var p = PassReqModel.prototype;

        p.init = function (settings) {
            var s = this;
            s.data = {
                realPageId: "",
                oldPassword: "",
                newPassword: "",
                newPasswordCopy: ""
            };
            
            if (settings) {
            s.count = {
                minimumLength: 8,
                maximumLength: 20,
                uppercaseChars: 1,
                lowercaseChars: 1,
                numChars: 1,
                specialChars: 1
            };
                angular.extend(s.count, settings);
            }

            s._data = angular.copy(s.data);
        };

        // Getters

        p.getData = function () {
            var s = this;
            return s.data;
        };

        p.getMatchCount = function (pattern) {
            var s = this;
            if (s.password && s.password.match(pattern)) {
                return s.password.match(pattern)[0].length;
            }
            else {
                return 0;
            }
        };

        // Actions

        p.update = function (password) {
            var s = this;
            s.password = password;
            return s;
        };

        // Assertions

        p.hasLowercaseChars = function () {
            var s = this,
                pattern = /[a-z]+/,
                count = s.getMatchCount(pattern),
                hasCount = count >= s.count.lowercaseChars;

            return s.password && !!s.password.match(pattern) && hasCount;
        };

        p.hasMinChars = function () {
            var s = this;
            return s.password && s.password.length >= s.count.minimumLength;
        };

        p.hasMaxChars = function () {
            var s = this;
            return s.password && s.password.length <= s.count.maximumLength;
        };

        p.hasNumChars = function () {
            var s = this,
                pattern = /[0-9]+/,
                count = s.getMatchCount(pattern),
                hasCount = count >= s.count.numChars;

            return s.password && !!s.password.match(pattern) && hasCount;
        };

        p.hasUppercaseChars = function () {
            var s = this,
                pattern = /[A-Z]+/,
                count = s.getMatchCount(pattern),
                hasCount = count >= s.count.uppercaseChars;

            return s.password && !!s.password.match(pattern) && hasCount;
        };

        p.hasSpecialChars = function () {
            var s = this,
                pattern = regexlib.password,
                count = s.getMatchCount(pattern),
                hasCount = count >= s.count.specialChars;

            return s.password && !!s.password.match() && hasCount;
        };

        p.isValid = function () {
            var s = this;
            return s.hasLowercaseChars() &&
                s.hasMinChars() &&
                s.hasMaxChars() &&
                s.hasNumChars() &&
                s.hasUppercaseChars() &&
                s.hasSpecialChars();
        };

        // Destroy / Reset

        p.reset = function () {
            var s = this;
            s.password = "";
        };

        return new PassReqModel();
    }

    angular
        .module("settings")
        .factory("passReqModel", ["gbRegex", factory]);
})(angular);
