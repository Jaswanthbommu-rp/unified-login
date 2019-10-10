//  Profile Form Model

(function(angular, undefined) {
    "use strict";

    function factory(telecomNumber, impersonate) {
        function ProfileModel() {
            var s = this;
            s.init();
        }

        var p = ProfileModel.prototype;

        p.init = function() {
            var s = this;
            s.data = {};
            s.telecomNumbers = [];
            s.userInitials = "";
        };

        // Getters

        p.getData = function() {
            var s = this;
            s.data.telecommunicationNumber = s.getTelecomData();
            return s.data;
        };

        p.getPhoneNumberFieldNames = function() {
            var s = this,
                names = [];
            s.telecomNumbers.forEach(function(item) {
                names = names.concat(item.getFieldNames());
            });
            return names;
        };

        p.getTelecomData = function() {
            var s = this,
                list = [];

            s.telecomNumbers.forEach(function(item) {
                list.push(item.getData());
            });

            return list;
        };

        // Setters

        p.setData = function(resp) {
            
            var s = this;
            s.data = resp.data;

            impersonate.setData(s.data.isImpersonated);

            s.telecomNumbers = [];
            s.electronicEmails = [];

            s.userInitials = s.getInitials();

            s.data.telecommunicationNumber.forEach(function(data) {
                var item = telecomNumber(data);
                s.telecomNumbers.push(item);
                item
                    .setChangeCallback(s.onPhoneNumberChange.bind(s))
                    .setPhoneNumberValidator(s.validatePhoneNumber.bind(s));
            });

            var a = 0;
            s.data.emailContacts.forEach(function(data) {
                if (data.contactMechanismUsageType.contactMechanismUsageTypeId !== 302) {
                    // var item = telecomNumber(data);
                    // s.electronicEmails.push(item); 
                    s.data.emailContacts.splice(a, 1);
                }
                a++;
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

        p.setIsImpersonated = function (data) {
            var s = this;
            return s.data.isImpersonated;
        };

        p.getIsImpersonated = function () {
            var s = this;
            return s.data.userLogin.fromDate;
        };

        p.setPhoneNumberChangeCallback = function(callback) {
            var s = this;
            s.phoneNumberChangeCallback = callback;
            return s;
        };

        p.addPhone = function() {
            var s = this;
            if (s.telecomNumbers.length > 0) {
                var i = s.telecomNumbers.length - 1;
                var o = s.telecomNumbers[i];
                if (o.data.areaCode === "" && o.data.phoneNumber === "") {
                    return s;
                }
            }
            var data = {
                "partyContactMechanismId": 0,
                "contactMechanismId": 0,
                "countryCode": "",
                "areaCode": "",
                "phoneNumber": "",
                "isDeleted": false,
                "contactMechanismUsageType": {
                    "contactMechanismUsageTypeId": null,
                    "parentContactMechanismUsageTypeId": 200,
                    "name": ""
                }
            };

            var item = telecomNumber(data);
            s.telecomNumbers.push(item);
            item
                .setChangeCallback(s.onPhoneNumberChange.bind(s))
                .setPhoneNumberValidator(s.validatePhoneNumber.bind(s));

            return s;
        };

        p.delPhone = function(itemDel) {
            var s = this;

            if (itemDel.data.areaCode === "" && itemDel.data.phoneNumber === "") {
                itemDel.data.isDeleted = true;
                var a = 0;
                s.telecomNumbers.forEach(function(item) {
                    if (item.data.isDeleted === true) {
                        s.telecomNumbers.splice(a, 1);
                        return;
                    }
                    a++;
                });
            }

            s.data.telecommunicationNumber.forEach(function(item) {
                if (itemDel.data.areaCode === item.areaCode && itemDel.data.phoneNumber === item.phoneNumber) {
                    itemDel.data.isDeleted = true;
                    item.isDeleted = true;
                    return;
                }

            });

            return s;
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

        p.getInitials = function() {
            var s = this,
                name = "",
                d = s.data;

            if (d.firstName) {
                name = d.firstName[0].toUpperCase();
            }

            if (d.lastName) {
                name += d.lastName[0].toUpperCase();
            }

            return name;
        };

        p.onPhoneNumberChange = function() {
            var s = this;
            if (s.phoneNumberChangeCallback) {
                s.phoneNumberChangeCallback();
            }
            return s;
        };

        p.preferredContactMethodIsPhone = function() {
            var s = this;
            return s.data.preferredContactMethodId == 1;
        };

        p.validatePhoneNumber = function() {
            var s = this,
                isPhone = s.preferredContactMethodIsPhone();

            return isPhone ? s.hasValidPhoneNumber() : true;
        };

        // Assertions

        p.hasValidPhoneNumber = function() {
            var s = this,
                validCount = 0;

            s.telecomNumbers.forEach(function(item) {
                if (item.hasValidPhoneNumber()) {
                    validCount++;
                }
            });

            return validCount > 0;
        };

        p.isSuperUser = function() {
            var s = this;
            return s.data.partyRole.roleTypeId === 402;
        };

        // Destroy

        p.destroyItem = function(item) {
            item.destroy();
        };

        p.reset = function() {
            var s = this;
            s.data = {};
            s.telecomNumbers.forEach(s.destroyItem);
            s.telecomNumbers = [];
            s.electronicEmails.forEach(s.destroyItem);
            s.electronicEmails = [];
        };

        return new ProfileModel();
    }

    angular
        .module("settings")
        .factory("profileModel", [
            "telecomNumberModel",
            "userImpersonated",
            factory
        ]);
})(angular);