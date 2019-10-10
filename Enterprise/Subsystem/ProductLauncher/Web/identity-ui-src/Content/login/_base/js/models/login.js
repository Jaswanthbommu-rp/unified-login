(function (angular) {
    "use strict";

    function loginModel ($filter, identitySvc, userLookupModel, cookie) {
    	var model = {},
            formDefault,
            changedPasswordMessage,
            identityMsgData = [],
            loginStatusMsgs = {};

        formDefault = {
            username: "",
            password: "",
            rememberMe: false
        };

        changedPasswordMessage = {
            state: false
        };

        loginStatusMsgs = {
            200: {
                msg: $filter("loginText")("login_profile_update_success"),
                class: "text-success"
            },
            201: {
                msg: $filter("loginText")("login_change_pwd_success"),
                class: "text-success"
            }
        };

    	model.init = function () {
    		model.form = angular.copy(formDefault);
            model.form.errorMessage = identitySvc.data.errorMessage;
            model.changedPasswordMessage = angular.extend({}, changedPasswordMessage);
            model.identityMsgData = model.setIdentityCustomData(identitySvc.data.custom, "msgId:");
            model.loginStatusMsgs = loginStatusMsgs;

    		return model;
    	};

        model.initRememberMeState = function() {
            model.form.rememberMe = model.isRememberMeEnabled();
            if (model.form.rememberMe) {
                model.form.username = model.getUsernameCookie();
            }
        };

        model.isRememberMeEnabled = function() {
            return (model.getUsernameCookie() ? true : false);
        };

        model.getUsernameCookie = function() {
            return cookie.read("username");
        };

        model.updateRememberMeCookies = function() {
            if (model.form.rememberMe) {
                cookie.create("username", model.form.username, 365);
            }
            else {
                cookie.erase("username");
            }
        };

        model.setIdentityCustomData = function (identityCustomData, needle) {
            var customData = [];

            identityCustomData.forEach(function (item) {
                if (item.indexOf(needle) === 0) {
                    customData.push(item.slice(needle.length));
                }
            });

            return customData;
        };

        model.clearIdentityMsg = function () {
            model.identityMsgData = identityMsgData;
        };

        model.displayPasswordChangedMessage = function() {
            model.changedPasswordMessage.state = true;

            return model;
        };

        model.clearPasswordChangedMessage = function () {
            model.changedPasswordMessage = angular.extend({}, changedPasswordMessage);

            return model;
        };

    	model.reset = function () {
            //username should flow to forgot pswd page
            userLookupModel.setFormUsername(model.form.username);

            model.form = angular.copy(formDefault);
            model.clearPasswordChangedMessage();
            model.clearIdentityMsg();

            return model;
    	};

    	return model.init();
    }

    angular
    	.module("identity")
    	.factory("loginModel", [
            "$filter",
            "identitySvc",
            "userLookupModel",
            "rpCookie",
            loginModel
        ]);
})(angular);
