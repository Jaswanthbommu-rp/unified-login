(function (angular) {
    "use strict";

    function LoginCtrl($scope, $window, $filter, loginModel, svc, loginConfig) {
        var vm = this;

        vm.init = function () {
            vm.model = loginModel;
            vm.loginCtrlWatch = $scope.$on("$destroy", vm.destroy);
            vm.loginConfig = loginConfig;
            loginConfig.setMethodsSrc(vm);
            vm.loginForm = null;
            vm.model.initRememberMeState();
            return vm;
        };

        vm.alertAutoLogout = function () {
            var params = $window.location.search;
            if (params && params.indexOf("expiredUser=1") != -1) {
                vm.alertMessages = [
                    $filter("loginText")("login_alert_autologout_1"),
                    $filter("loginText")("login_alert_autologout_2")
                ];
            }
        };

        vm.validateFailed = function ($event) {
            vm.updateRememberMeCookies();
            vm.loginForm.$setSubmitted();
            if (!vm.loginForm.$valid) {
                $event.preventDefault();
            }
        };

        vm.determineIdentityProvider = function () {
            var username = loginModel.form.username;
            vm.model.form.errorMessage = "";
            if (!username || username.trim().length === 0) {
                return; //don't do anything
            }

            vm.updateRememberMeCookies();
			var model = window.identityServer.getModel();
			var signinpair = model.loginUrl.split('=');
			var signin = signinpair[1];
            svc.cancelRequests(); //cancel any pending requests
            svc.checkIDProvider(username, signin)
                .then(vm.redirectPage);
        };

        vm.redirectPage = function (results) {
            if (results && results.url && results.url.trim().length > 0) {
                $window.location = results.url;
            }
        };

        vm.updateRememberMeCookies = function() {
            vm.model.updateRememberMeCookies();
        };

        vm.getQueryStringParams = function () {
            var pairs = $window.location.search.substring(1).split(/[&?]/);
            var params = {};
            angular.forEach(pairs, function(value, key) {
                var pair = value.split('=');
                if (pair[1]) {
                    params[decodeURIComponent(pair[0])] = decodeURIComponent(pair[1]);
                }
            });
            return params;
        };

        vm.destroy = function () {
            loginModel.reset();
            svc.clearRequests();

            vm.loginCtrlWatch();
            vm.loginConfig = undefined;
            vm.loginForm = undefined;
            vm = undefined;
        };

        vm.init();
    }

    angular
        .module("identity")
        .controller("LoginCtrl", [
            "$scope",
            "$window",
            "$filter",
            "loginModel",
            "idpRedirectSvc",
            "loginConfig",
			LoginCtrl
        ]);
})(angular);
