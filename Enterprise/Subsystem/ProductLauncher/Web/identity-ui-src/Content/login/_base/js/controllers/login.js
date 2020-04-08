(function (angular) {
    "use strict";

    function LoginCtrl($scope, $window, $filter, loginModel, svc, loginConfig) {
        var vm = this;

        vm.init = function () {
            vm.model = loginModel;
            vm.vbrowser = vm.returnBrowserSettings();
            vm.loginCtrlWatch = $scope.$on("$destroy", vm.destroy);
            vm.loginConfig = loginConfig;
            loginConfig.setMethodsSrc(vm);
            vm.loginForm = null;
            vm.model.initRememberMeState();
            return vm;
        };

        vm.checkBrowser = function () {
            var ua = $window.navigator.userAgent, tem,
            M = ua.match( /(opera|chrome|safari|firefox|msie|trident(?=\/))\/?\s*(\d+)/i ) || [];
            if ( /trident/i.test( M[1] ) ) {
                tem = /\brv[ :]+(\d+)/g.exec( ua ) || [];
                return  'IE ' + ( tem[1] || '' );
            }

            if ( M[1] === 'Chrome' ) {
                tem = ua.match( /\b(OPR|Edge)\/(\d+)/ );
                if ( tem != null )
                    {return tem.slice( 1 ).join( ' ' ).replace( 'OPR', 'Opera' );}
            }

            M = M[2] ? [M[1], M[2]] : [navigator.appName, navigator.appVersion, '-?'];

            if ( ( tem = ua.match( /version\/(\d+)/i ) ) != null )
                {M.splice( 1, 1, tem[1] );}
            return M.join( ' ' );
        };

        vm.returnBrowserSettings = function() {
            var str = vm.checkBrowser();
            var browser = str.substring( 0, str.indexOf( " " ) );
            var version = str.substring( str.indexOf( " " ) );
            version = version.trim();
            version = parseInt( version );
            logc( browser );
            logc( version );
            return version;
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
