//  User Profile Model

(function(angular, undefined) {
    "use strict";

    function factory() {
        function ProfileModel() {
            var s = this;
            s.init();
        }

        var p = ProfileModel.prototype;

        p.init = function() {
            var s = this;
            s.data = {};
        };

        // Getters

        // Setters

        p.setData = function(resp) {
            var s = this;
            s.data = resp.data;
            s.userInitials = s.getInitials();
            s.data.telecommunicationNumber.forEach(function(item) {
                item = s.setPhoneNumber(item);
            });

            if (s.data.preferredContactMethodId === 0) {
                s.data.preferredContactMethodId = "";
            }

            return s;
        };

        p.setPhoneNumber = function(telecomData) {
            var s = this,
                phoneNbr,
                hasPhone = telecomData.areaCode.length > 0 && telecomData.phoneNumber.length > 0;

            phoneNbr = telecomData.phoneNumber;

            if (phoneNbr) {
                if (phoneNbr.length > 3) {
                    phoneNbr = phoneNbr.slice(0, 3) + '-' + phoneNbr.slice(3, 7);
                }
            }

            telecomData.hasValidPhoneNumber = hasPhone;
            telecomData.phoneNumber = hasPhone ? telecomData.areaCode + telecomData.phoneNumber : "";
            telecomData.displayNumber = hasPhone ? ("(" + telecomData.areaCode + ") " + phoneNbr) : "";

            return telecomData;
        };

        // Actions

        p.flushData = function() {
            var s = this;
            s.data = {};
            return s;
        };

        p.loginNameIsEmail = function() {
            var s = this,
                validEmail = false,
                data = s.data.userLogin;

            if (data) {
                validEmail = data.loginNameType === "email";
            }
            return validEmail;
        };

        p.hasValidPhoneNumber = function(telecomData) {
            return telecomData.hasValidPhoneNumber;
        };

        p.getInitials = function() {
            var s = this,
                initials = "",
                d = s.data;

            if (d.firstName) {
                initials = d.firstName[0].toUpperCase();
            }

            if (d.lastName) {
                initials += d.lastName[0].toUpperCase();
            }

            return initials;
        };

        // Assertions
        // Destroy

        p.destroyItem = function(item) {
            item.destroy();
        };

        p.reset = function() {
            var s = this;
            s.userInitials = "";
            s.data = {};
        };

        return new ProfileModel();
    }

    angular
        .module("settings")
        .factory("userActivityLogProfileModel", [
            factory
        ]);
})(angular);