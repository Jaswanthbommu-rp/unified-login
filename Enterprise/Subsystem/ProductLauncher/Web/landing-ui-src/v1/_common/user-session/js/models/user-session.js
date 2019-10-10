//  User Session Model

(function (angular, undefined) {
    "use strict";

    function factory(eventStream, svc) {
        function UserSessionModel() {
            var s = this;
            s.init();
        }

        var p = UserSessionModel.prototype;

        p.init = function () {
            var s = this;

            s.state = {
                busy: false,
                ready: false
            };

            s.data = {};
            s.update = eventStream();
        };

        // Getters

        p.getData = function () {
            var s = this;
            return s.data;
        };

        p.get = function (key) {
            var s = this;
            return s.data[key];
        };

        p.getAccountExpiry = function () {
            var s = this;
            return s.accountExpiryDays;
        };

        p.getAuthenticationType = function () {
            var s = this;
            return s.data.authenticationType;
        };

        p.getFullName = function () {
            var s = this,
                name = "",
                d = s.data;

            if (d.firstName) {
                name = d.firstName;
            }

            if (d.middleName) {
                name += " " + d.middleName[0].toUpperCase();
            }

            if (d.lastName) {
                name += " " + d.lastName;
            }

            return name;
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

        p.getPartyId = function () {
            var s = this;
            return s.data.partyId;
        };

        p.getPersonaId = function () {
            var s = this;
            return s.data.partyId;
        };

        p.getRealPageId = function () {
            var s = this;
            return s.data.realPageId;
        };

        p.getUsername = function () {
            var s = this;
            return s.data.userLogin.loginName;
        };

        p.getVerificationToken = function () {
            var s = this;
            return s.data.verificationActivityToken;
        };

        p.getOrganizationTimeZone = function () {
            var s = this,
                settings = s.data.organizationSetting,
                defaultTimezoneValue = "";

            settings.forEach(function (setting) {
                if (setting.name === "TimeZone"){
                    defaultTimezoneValue = setting.value;
                }
            });

            return defaultTimezoneValue;
        };

        // Setters

        p.setAccountExpiry = function (accountExpiryDays) {
            var s = this;
            s.accountExpiryDays = accountExpiryDays;
            return s;
        };

        p.setData = function (data) {
            var s = this;
            s.data = data;
            return s;
        };

        // Actions

        p.load = function () {
            var s = this,
                error = s.onLoadError.bind(s),
                success = s.onLoadSuccess.bind(s);

            if (!s.state.ready && !s.state.busy) {
                s.state.busy = true;
                svc.get(success, error);
            }

            return s;
        };

        p.onLoadError = function () {
            var s = this;
            s.state.busy = false;
            logw("UserSession load failed!");
            return s;
        };

        p.onLoadSuccess = function (resp) {
            var s = this;
            s.state.ready = true;
            s.state.busy = false;
            s.setData(resp.data);
            s.update.publish(resp.data);
            return s;
        };

        p.reload = function () {
            var s = this;
            s.reset().load();
            return s;
        };

        p.resetPasswordRequired = function () {
            var s = this;
            return s.data.userLogin.isForceReSetPassword;
        };

        p.isPasswordExpired = function () {
            var s = this;
            return s.data.passwordExpirationDetail.isPasswordExpired;
        };

        p.subscribe = function (callback) {
            var s = this;
            return s.update.subscribe.apply(s.update, arguments);
        };

        // Assertions

        p.isReady = function () {
            var s = this;
            return s.state.ready;
        };

        p.reset = function () {
            var s = this;
            s.state.ready = false;
            return s;
        };

        return new UserSessionModel();
    }

    angular
        .module("settings")
        .factory("userSessionModel", [
            "eventStream",
            "userSessionSvc",
            factory
        ]);
})(angular);
