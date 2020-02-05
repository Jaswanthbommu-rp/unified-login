//  App Controller

(function (angular) {
    "use strict";

    function AppCtrl(
        $scope,
        $timeout,
        $window,
        $location,
        $rootScope,
        ENV,
        layout,
        nav,
        headerActions,
        sessionModel,
        userDetailsSvc,
        userAcctNotifs,
        cookie,
        moment) {

        var vm = this,
            url = {
                changePasswordPage: "people/users/:userId/change-password"
            };

        var pageContext = {
            home: {
                pg: "home",
            },
            "people.users": {
                pg: "userlist",
            },
            "people.activity-log": {
                pg: "activity",
            },
            "user.add": {
                pg: "addUser",
                context: {
                    productAccess: {
                        pg: "productAccess"
                    }
                }
            },
            "user.edit": {
                pg: "editUser",
                context: {
                    productAccess: {
                        pg: "productAccess"
                    },
                    securityQuestions: {
                        pg: "securityQuestions"
                    },
                    password: {
                        pg: "password"
                    },
                    activityLog: {
                        pg: "activity"
                    }
                }
            },
            "roles-and-rights.roles": {
                pg: "roles",
            },
            "roles-and-rights.rights": {
                pg: "rights",
            }
        };

        vm.init = function () {
            vm.saveTimezone();
            vm.orgName = "";
            $scope.appLayout = layout.getData();
            sessionModel.subscribe(vm.onSessionReady);
            vm.readyStateTimer = $timeout(vm.setReady, 100);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);

            window.addEventListener('viewNotifications', function() {
                window.location.href = '/home/notifications';
            });
        };

        vm.getState = function () {
            return angular.extend({}, layout.getData(), nav.getState());
        };

        vm.onSessionReady = function () {
            var username = sessionModel.getUsername(),
                token = sessionModel.getVerificationToken(),
                identityToken = cookie.read("access_token");
                
                var org = sessionModel.getOrganization();
                if(org){
                    if(org.length > 0){
                        vm.orgName = org[0].name;
                    }
                }
                
            var omnibar = document.querySelector('omnibar-shell');
            omnibar.environment = ENV.currentEnv;
            omnibar.servers = {
              unity: ENV.landingAPI,
              docs: ENV.helpUrl
            };

            omnibar.auth = identityToken;

            var helpWidget = document.querySelector('omnibar-unified-help');
            $rootScope.$on("$stateChangeSuccess", function (_, toState) {
                helpWidget.helpQuery = "";                
                if (pageContext[toState.name]) {
                    helpWidget.helpQuery = 'pg=ul-' + pageContext[toState.name].pg + '&vr=40&scrver=350';
                }
                omnibar.pageId = (window.location.hash === "#/employee-access") ? "" : window.location.hash;
            });

            if (token && username) {
                $window.location.href = "new-user/#/validate-token/" + token + "/" + username;
            }
            else {
                vm.checkPasswordState();
                vm.checkAccountState();
            }
        };

        vm.checkPasswordState = function () {
            userDetailsSvc.getPasswordState()
                .then(vm.notifyPasswordState);
        };

        vm.checkAccountState = function () {
            var realPageId = sessionModel.getRealPageId();

            userDetailsSvc.getAccountState(realPageId)
                .then(vm.initAccountTimer);
        };

        vm.setReady = function () {
            vm.ready = true;
        };

        vm.initAccountTimer = function (accountState) {
            var logOutDays = accountState.daysToExpire,
                logoutMs = accountState.logOutSetInterval;

            //set number of days before expiration
            sessionModel.setAccountExpiry(logOutDays);

            //start timer
            if (logoutMs !== null && logoutMs !== undefined && logoutMs !== -1) {
                vm.accountExpirationTimer = $timeout(vm.signoutExpiredAccount, logoutMs);
            }
        };

        vm.signoutExpiredAccount = function () {
            logc("Account has expired. Logging out...");
            headerActions.signout({
                expiredUser: 1
            });
        };

        vm.notifyPasswordState = function (passwordState) {
            var severityLevel = passwordState.severityLevel.toLowerCase(),
                daysBeforeExpiring = passwordState.daysToExpire;

            if (passwordState.isError === true) {
                logc("Password Notification Error: %s", passwordState.errorReason);
                return;
            }
            else if (severityLevel === "none") {
                // password status is ok.
                return;
            }
            else if (daysBeforeExpiring > 0) {
                //password is about to expire
                userAcctNotifs.notifyPwdExpiration(daysBeforeExpiring, passwordState.severityLevel, vm.redirectToChangePassword);
            }
            else {
                //password is expired!
                //userAcctNotifs.setPwdExpired(true);
            }
        };

        vm.redirectToChangePassword = function (asCallback) {
            var redirectLink = url.changePasswordPage.replace(":userId", sessionModel.getRealPageId());
            $location.path(redirectLink);

            if (asCallback) {
                $scope.$apply();
            }
            userAcctNotifs.closePwdNotification();
        };

        vm.saveTimezone = function () {
            var offsetTz = -(new Date().getTimezoneOffset() / 60);
            cookie.create("timezone", offsetTz);
        };

        vm.destroy = function () {
            vm.destWatch();

            $timeout.cancel(vm.readyStateTimer);
            vm.readyStateTimer = undefined;

            $timeout.cancel(vm.accountExpirationTimer);
            vm.accountExpirationTimer = undefined;

            vm = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller('AppCtrl', [
            "$scope",
            "$timeout",
            "$window",
            "$location",
            "$rootScope",
            "ENV",
            "appLayoutModel",
            "rpGlobalNavModel",
            "rpGlobalHeaderActions",
            "userSessionModel",
            "userDetailsSvc",
            "userAccountNotificationSvc",
            "rpCookie",
            "moment",
            AppCtrl
        ]);
})(angular);