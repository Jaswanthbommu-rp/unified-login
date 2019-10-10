//  Profile Tab Form Model

(function (angular, undefined) {
    "use strict";

    function factory(telecomNumber) {
        function ProfileTabModel() {
            var s = this;
            s.init();
        }

        var p = ProfileTabModel.prototype;

        p.init = function () {
            var s = this;
            s.data = {};
            s.telecomNumbers = [];
        };

        // Getters

        p.getData = function () {
            var s = this;
            s.data.telecommunicationNumber = s.getTelecomData();
            return s.data;
        };

        p.getPhoneNumberFieldNames = function () {
            var s = this,
                names = [];
            s.telecomNumbers.forEach(function (item) {
                names = names.concat(item.getFieldNames());
            });
            return names;
        };

        p.getTelecomData = function () {
            var s = this,
                list = [];

            s.telecomNumbers.forEach(function (item) {
                list.push(item.getData());
            });

            return list;
        };

        // Setters

        p.setData = function (resp) {
            var s = this;
            s.data = resp.data;

            s.telecomNumbers = [];

            s.data.telecommunicationNumber.forEach(function (data) {
                var item = telecomNumber(data);
                s.telecomNumbers.push(item);
                item
                    .setChangeCallback(s.onPhoneNumberChange.bind(s))
                    .setPhoneNumberValidator(s.validatePhoneNumber.bind(s));
            });

            if (!s.data.partyRole) {
                s.data.partyRole = {
                    roleTypeId: ""
                };
            }

            if (s.data.preferredContactMethodId === 0) {
                s.data.preferredContactMethodId = "";
            }

            return s;
        };

        p.setPhoneNumberChangeCallback = function (callback) {
            var s = this;
            s.phoneNumberChangeCallback = callback;
            return s;
        };

        // Actions

        p.flushData = function () {
            var s = this;
            s.data = {};
            return s;
        };

        p.onPhoneNumberChange = function () {
            var s = this;
            if (s.phoneNumberChangeCallback) {
                s.phoneNumberChangeCallback();
            }
            return s;
        };

        p.preferredContactMethodIsPhone = function () {
            var s = this;
            return s.data.preferredContactMethodId == 1;
        };

        p.validatePhoneNumber = function () {
            var s = this,
                isPhone = s.preferredContactMethodIsPhone();

            return isPhone ? s.hasValidPhoneNumber() : true;
        };

        // Assertions

        p.hasValidPhoneNumber = function () {
            var s = this,
                validCount = 0;

            s.telecomNumbers.forEach(function (item) {
                if (item.hasValidPhoneNumber()) {
                    validCount++;
                }
            });

            return validCount > 0;
        };

        // Destroy

        p.destroyItem = function (item) {
            item.destroy();
        };

        p.reset = function () {
            var s = this;
            s.data = {};
            s.telecomNumbers.forEach(s.destroyItem);
            s.telecomNumbers = [];
        };

        return new ProfileTabModel();
    }

    angular
        .module("settings")
        .factory("mpProfileTabModel", [
            "telecomNumberModel",
            factory
        ]);
})(angular);
