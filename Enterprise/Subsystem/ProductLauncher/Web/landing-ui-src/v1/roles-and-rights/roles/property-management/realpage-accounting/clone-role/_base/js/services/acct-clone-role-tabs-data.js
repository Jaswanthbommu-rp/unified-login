//  Tabs  Config

(function(angular, undefined) {
    "use strict";

    function AcctCloneTabsData($filter) {

        var data,
            svc = this;

        svc.data = data = {
            rights: {
                id: "01",
                isActive: true,
                text: "Rights",
                incUrl: "roles-and-rights/roles/property-management/realpage-accounting/clone-role/rights/templates/acct-rights-tab.html"
            }

            // users: {
            //     id: "02",
            //     isActive: false,
            //     text: "Users",
            //     incUrl: "roles-and-rights/roles/new-role/users/templates/users-tab.html"
            // },

            // enterpriseroles: {
            //     id: "03",
            //     isActive: false,
            //     text: "Enterprise Roles",
            //     incUrl: "roles-and-rights/roles/new-role/ent-roles/templates/enterpriseroles-tab.html"
            // }
        };


        svc.getData = function() {
            return data;
        };

        svc.getTabData = function(tabName) {
            return svc.data[tabName];
        };

        svc.getActiveTab = function() {
            var tab;
            angular.forEach(svc.data, function(item) {
                if (item.isActive) {
                    tab = item;
                }
            });
            return tab;
        };

        svc.getList = function() {
            return [
                data.rights

            ];
        };

        svc.getTabById = function(id) {
            var tab;
            angular.forEach(svc.data, function(item) {
                if (item.id == id) {
                    tab = item;
                }
            });
            return tab;
        };


        svc.getActiveUrl = function() {
            var url;
            svc.data.forEach(function(tab) {
                if (tab.isActive) {
                    url = tab.url;
                }
            });

            return "roles-and-rights/roles/new-role/base/templates/" + url + "-tab.html";
        };

    }

    angular
        .module("settings")
        .service("acctCloneTabsData", [
            "$filter",
            AcctCloneTabsData
        ]);
})(angular);