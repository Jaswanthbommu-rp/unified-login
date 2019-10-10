//  Tabs  Config

(function(angular, undefined) {
    "use strict";

    function SpndMgmtAssignTabsData($filter) {

        var data,
            svc = this;

        svc.data = data = {
            rights: {
                id: "01",
                isActive: true,
                text: "Rights",
                incUrl: "roles-and-rights/roles/property-management/spend-management/assign-role/rights/templates/spndmgmt-rights-tab.html"
            },
            workflow: {
                id: "02",
                isActive: false,
                text: "Workflow",
                incUrl: "roles-and-rights/roles/property-management/spend-management/assign-role/rights/templates/spndmgmt-workflow-tab.html"
            },
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
                data.rights,
                data.workflow
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

            return "roles-and-rights/roles/assign-role/base/templates/" + url + "-tab.html";
        };

    }

    angular
        .module("settings")
        .service("spndMgmtAssignTabsData", [
            "$filter",
            SpndMgmtAssignTabsData
        ]);
})(angular);