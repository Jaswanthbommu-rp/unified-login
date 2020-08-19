//  User Details Form Model

(function (angular, undefined) {
    "use strict";

    function factory(regex, moment, productAccess, session, profiletelecomNumber,eventStream) {
        function UserDetailsModel() {
            var s = this;
            s.init();
        }

        var p = UserDetailsModel.prototype;

        p.init = function () {
            var s = this;
             s.events = {
                update: eventStream()
            };
            
            s.data = {
                firstName: "",
                lastName: "",
                employyeId: "",
                middleName: "",
                notificationEmail: "",
                password: "",
                passwordCopy: "",
                persona: [],
                createUserSourceType:"UnifiedPlatform",
                telecommunicationNumber: [{
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
                }],
                userCustomField: [],
                partyRole: {
                    roleTypeId: ""
                },
                realPageId: "",
                userTypeId: 401,
                PreferredContactMethodId: "",
                industryjobTitle: "",
                title: "",
                phoneNumber: "",
                phoneType: "",
                userLogin: {
                    thruDate: "",
                    loginName: "",
                    isActive: true,
                    isPendig: false,
                    isExpired: false,
                    fromDate: "",
                    is3rdPartyIDP: false,
                    timeZoneOffset: "",
                }
            };


            s.CustomFields = [];
            s.customFieldList = [];
            s.clonedUser = false;
            s.existingUser = false;
            s.show3rdPartyIDPSetting = true;
            s.cleanData = angular.copy(s.data);
            s.adminResetFlag = false;
            s.orgUserTypeId = 401;
            s.ready = false;
        };

        // Getters

        p.getData = function () {
            var s = this,
                data = angular.copy(s.data),
                thruDate = data.userLogin.thruDate,
                prodData = productAccess.getData();

            if (thruDate) {
                thruDate = moment(thruDate).startOf('day').add(1, 'day').startOf('day').subtract(1, 'millisecond').format();
                data.userLogin.thruDate = thruDate;
            }
            else {
                data.userLogin.thruDate = "";
            }

            delete data.passwordCopy;

            if (s.clonedUser) {
                data.clonedUser = true;
            }

            data.productBatch = prodData.productBatch;
            data.customFields = s.CustomFields;

            if (!s.existingUser) {
                data.partyRole.roleTypeId = data.industryjobTitle;

            }

            if (s.existingUser && !s.clonedUser) {
                data.password = null;
                data.userLogin.thruDate = data.userLogin.thruDate || null;
            }

            return data;
        };


        p.addPhone = function () {
            var s = this;
            if (s.data.telecommunicationNumber.length > 0) {
                var emptyFlag = false;
                s.data.telecommunicationNumber.forEach(function (item) {
                    if (item.phoneNumber === "") {
                        emptyFlag = true;
                    }
                });
                if (emptyFlag === true) {
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

            s.data.telecommunicationNumber.push(data);

            return s;
        };

        p.delPhone = function (itemDel) {
            var s = this;

            itemDel.isDeleted = true;
            var a = 0;
            s.data.telecommunicationNumber.forEach(function (item) {
                if (item.isDeleted === true) {
                    s.data.telecommunicationNumber.splice(a, 1);
                    return;
                }
                a++;
            });

            return s;
        };

        p.getParsedPhoneNumbers = function (phones) {
            phones.forEach(function (ph) {
                var phone = ph.phoneNumber;
                if (phone !== "") {
                    ph.countryCode = "";
                    ph.areaCode = phone.slice(0, -7);
                    ph.phoneNumber = phone.slice(3);
                }
            });
            return phones;
        };

        p.getInitials = function () {
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


        p.getFromDate = function () {
            var s = this;
            return s.data.userLogin.fromDate;
        };

        p.setUserTypeDefConfig = function (typeId) {
            var s = this;
            s.data.userTypeId = typeId;
            return s;
        };

        p.setUserCustomField = function (data) {
            var s = this;
            s.CustomFields = data;
            return s;
        };

        p.getUserCustomFields = function () {
            var s = this;
            return s.customFieldList;
        };

        p.getFromDateMinLimit = function () {
            var s = this;
            return s.data.userLogin.fromDate.clone().startOf("day");
        };

        p.getOrgUserTypeId = function () {
            var s = this;
            return s.orgUserTypeId;
        };

        p.getPersonaId = function () {
            var id = 0,
                s = this;

            if (s.data.persona.empty()) {
                return 0;
            }

            if (s.adminResetFlag) {
                return 0;
            }

            var persona = s.data.persona[0];

            return persona.personaId;
        };

        p.getThruDateMinLimit = function () {
            var s = this,
                tomorrow = moment().startOf("day").add(1, "day"),
                limit = moment().startOf("day").add(1, "day");

            if (s.data.userLogin) {
                limit = s.data.userLogin.fromDate.clone().startOf("day").add(1, "day");
            }

            if (limit.isBefore(tomorrow)) {
                limit = tomorrow;
            }

            return limit;
        };


        // Setters
        p.setUserCustomFieldData = function (data) {
            var s = this;
            s.customFieldList = data;
            return s;
        };

        p.setExistingUserLink = function (id) {
            var s = this;
            s.link = "#/user/" + id + "/UserList" + "/edit";
            return s;
        };

        p.getExistingUserLink = function () {
            var s = this;           
            return s.link;            
        };

        p.setShowExistingUserLink = function (bool) {
            var s = this;
            s.linkShow = bool;
            return s;
        };

        p.getShowExistingUserLink = function () {
            var s = this;           
            return s.linkShow;            
        };

        

        p.setData = function (data) {
            
            var s = this,
                //timeZoneOffset = data.userLogin.timeZoneOffset,
                fromDate = data.userLogin.fromDate,
                thruDate = data.userLogin.thruDate;

            s.data = data;
            s.existingUser = true;

            if (fromDate) {
                fromDate = moment(fromDate);
            }
            else {
                fromDate = moment();
            }

            if (thruDate) {
                thruDate = moment(thruDate);
            }

            s.data.userLogin.fromDate = fromDate;
            s.data.userLogin.thruDate = thruDate;

            s.orgUserTypeId = s.data.userTypeId;

            s.setReady(true);
            s.events.update.publish(data);
            return s;
        };

        p.setClonedUser = function (bool) {
            var s = this;
            bool = bool === undefined ? true : bool;
            s.clonedUser = bool;
            return s;
        };

        p.setReady = function (bool) {
            var s = this;
            s.ready = bool === undefined ? true : bool;
            return s;
        };

        p.setFromDate = function (fromDate) {
            var s = this;
            if (!fromDate) {
                s.data.userLogin.fromDate = moment();
            }
            else {
                s.data.userLogin.fromDate = moment(fromDate);
            }

            return s;
        };

        p.setThruDate = function (thruDate) {
            var s = this;

            s.data.userLogin.thruDate = moment(thruDate);
            return s;
        };

        p.set3rdPartyIDP = function (bool) {
            var s = this;
            s.data.userLogin.is3rdPartyIDP = bool;
            return s;
        };

        p.set3rdPartyIDPVisible = function (bool) {
            var s = this;
            s.show3rdPartyIDPSetting = !bool;
            return s;
        };

        p.setAdminResetFlag = function (bool) {
            var s = this;
            s.adminResetFlag = bool;
        };

        p.setExternalUserData = function (data) {
            var s = this;
            s.externalUserData = data;
        };

        p.getExternalUserData = function () {
            var s = this;
            return s.externalUserData;
        };

        p.setForm = function (form) {
            var s = this;
            s.form = form;
        };

        p.getForm = function () {
            var s = this;
            return s.form;
        };

        p.setAOUserRPId = function (id) {
            var s = this;
            s.aoUserRPId = id;
        };

        p.getAOUserRPId = function () {
            var s = this;
            return s.aoUserRPId;
        };
        // Assertions

        p.is3rdPartyIDP = function () {
            var s = this;
            return s.data.userLogin.is3rdPartyIDP;
        };

        p.isClonedUser = function () {
            var s = this;
            return s.clonedUser;
        };

        p.isDisabled = function () {
            var s = this;
            return !(s.data.userLogin.isActive);
        };

        p.isReady = function () {
            var s = this;
            return s.ready;
        };

        p.isCustomFieldsExists = function () {
            var s = this;
            return s.customFieldList && s.customFieldList.length > 0;
        };

        p.isSuperUser = function () {
            var s = this;
            return s.data.userTypeId === 402;
        };

        p.isExternalUser = function () {
            var s = this;
            return s.data.userTypeId === 405;
        };

        p.emailIsReqd = function () {
            var s = this;
            return s.data.userTypeId === 404;
        };

        p.fromDateIsSet = function () {
            var s = this;
            return !!s.data.userLogin.fromDate;
        };

        p.loginNameIsValidEmail = function () {
            var s = this;
            var emailRegex = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
            return emailRegex.test(s.data.userLogin.loginName);
            // return s.data.userLogin.loginName.match(regex.email);
        };

        p.getLoginName = function () {
            var s = this;            
            return s.data.userLogin.loginName;            
        };

        p.clearLoginName = function () {
            var s = this;           
            s.data.userLogin.loginName = "";            
        };

        p.passwordCopyIsEmpty = function () {
            var s = this;
            return !s.data.passwordCopy;
        };

        p.passwordIsEmpty = function () {
            var s = this;
            return !s.data.password;
        };

        p.passwordIsReqd = function () {
            var s = this;
            return (s.data.userTypeId === 404 && !s.is3rdPartyIDP());
        };

        p.passwordsMatch = function () {
            var s = this;
            return s.data.password == s.data.passwordCopy;
        };

        p.thruDateIsBefore = function (data) {
            var s = this,
                thruDate = s.data.userLogin.thruDate,
                isSet = thruDate && thruDate.isBefore;
            return isSet ? thruDate.isBefore(data) : false;
        };

        p.thruDateIsSet = function () {
            var s = this;
            return !!s.data.userLogin.thruDate;
        };

        p.userExists = function () {
            var s = this;
            return s.existingUser;
        };

        p.subscribe = function (callback) {
             var s = this;
            return s.events.update.subscribe(callback);
        };

        // Reset

        p.reset = function () {
            var s = this,
                fromDate = "";

            s.CustomFields = [];
            s.customFieldList = [];
            s.existingUser = false;
            s.clonedUser = false;
            s.data = angular.copy(s.cleanData);
            s.data.userLogin.fromDate = fromDate;
            s.adminResetFlag = false;
        };

        return new UserDetailsModel();
    }

    angular
        .module("settings")
        .factory("userDetailsModel", [
            "regex",
            "moment",
            "productAccessModel",
            "userSessionModel",
            "telecomNumberModel",
            "eventStream",
            factory
        ]);
})(angular);
