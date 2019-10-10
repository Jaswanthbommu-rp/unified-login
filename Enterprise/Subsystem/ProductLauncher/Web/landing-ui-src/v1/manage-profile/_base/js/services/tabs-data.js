//  Manage Profile Tabs Data Service

(function (angular, undefined) {
    "use strict";

    function MpTabData($filter, userModel) {
        var data,
            svc = this;

        svc.data = data = {
            profile: {
                id: "01",
                isActive: true,
                incUrl: "manage-profile/profile/templates/index.html"
            },

            securityQuestions: {
                id: "02",
                isActive: false,
                incUrl: "manage-profile/security-questions/templates/index.html"
            },

            resetPassword: {
                id: "03",
                isActive: false,
                incUrl: "manage-profile/reset-password/templates/index.html"
            }
        };

        svc.translate = function () {
            var translate = $filter("manageProfileText");

            angular.forEach(data, function (val, key) {
                val.text = translate(key + "Tab");
            });
        };

        svc.getData = function () {
            return data;
        };

        svc.getTabData = function (tabName) {
            return svc.data[tabName];
        };

        svc.getActiveTab = function () {
            var tab;
            angular.forEach(svc.data, function (item) {
                if (item.isActive) {
                    tab = item;
                }
            });
            return tab;
        };

        svc.getList = function () {
            if (userModel.getAuthenticationType() !== "local") {
                return [data.profile];
            }
            else {
                return [
                     data.profile,
                     data.securityQuestions,
                     data.resetPassword
                ];
            }
        };

        svc.getTabById = function (id) {
            var tab;
            angular.forEach(svc.data, function (item) {
                if (item.id == id) {
                    tab = item;
                }
            });
            return tab;
        };

        svc.getTabData = function (tabName) {
            return svc.data[tabName];
        };
    }

    angular
        .module("settings")
        .service("mpTabsData", [
            "$filter",
            "userSessionModel",
            MpTabData
        ]);
})(angular);
