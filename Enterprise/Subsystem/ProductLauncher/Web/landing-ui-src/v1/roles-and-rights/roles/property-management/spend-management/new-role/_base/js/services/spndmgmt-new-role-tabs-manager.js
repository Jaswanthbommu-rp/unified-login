//  new role Tab Manager Service

(function(angular, undefined) {
    "use strict";

    function SpndMgmtNewRoleTabsManager($rootScope, timeout, pubsub, tabsData, tabsMenu, aside, newRoleModel, newRoleSvc, user, persona) {
        var svc = this,
            triggerID = "SpndMgmtNewRoleTabsManager";

        svc.tabs = {};
        svc.activeTabs = {};

        svc.init = function() {
            svc.initActiveTab();
            svc.reqCount = 0;
            svc.reqErrorCount = 0;
            svc.reqSuccessCount = 0;

            svc.updateWatch = pubsub.subscribe("newRoleSpndMgmt.update", svc.onUpdate);
            svc.cancelWatch = pubsub.subscribe("newRoleSpndMgmt.cancel", svc.onCancel);
            svc.tabWatch = tabsMenu.subscribe("change", svc.initActiveTab);
            svc.appWatch = $rootScope.$on("rpAppStateChange", svc.onAppStateChange);
        };

        // Getters

        svc.getTabState = function(id) {
            return tabsData.getTabById(id);
        };

        // Assertions

        svc.allTabsAreValid = function() {
            var count = 0;

            angular.forEach(svc.tabs, function(item, id) {
                if (item.ctrl.isDirty() && !item.ctrl.isValid()) {
                    count++;
                }
            });

            return count === 0;
        };

        // Actions

        svc.focusErrorTab = function() {
            var found = false;

            angular.forEach(svc.tabs, function(item, id) {
                if (!found && item.ctrl.hasSaveError()) {
                    found = true;
                    svc.focusTab(item, id);
                }
            });

            return svc;
        };

        svc.focusInvalidTab = function() {
            var found = false;

            angular.forEach(svc.tabs, function(item, id) {
                if (!found && !item.ctrl.isValid() && item.ctrl.isDirty()) {
                    found = true;
                    svc.focusTab(item, id);
                    timeout(item.ctrl.focusInvalidField, 50);
                }
            });
        };

        svc.focusTab = function(item, id) {
            var tab = tabsData.getTabById(id);
            tabsMenu.activate(tab);
            item.ctrl.setSubmitted();
        };

        svc.initActiveTab = function() {

            var ctrl,
                id = tabsData.getActiveTab().id;

            if (!svc.activeTabs[id]) {
                svc.activeTabs[id] = 1;

                if (svc.tabs[id].ctrl.onTabActive) {
                    svc.tabs[id].ctrl.onTabActive();
                }
            } 
            // else {
            //     if (svc.tabs[id].ctrl.onTabActive) {
            //         svc.tabs[id].ctrl.onTabActive();
            //     }
            // }
        };

        svc.onAppStateChange = function(ev, evData) {
            if (!ev.defaultPrevented && evData.triggerID == triggerID) {
                evData.onContinue();
            }
        };

        svc.onCancel = function() {
            $rootScope.$emit("rpAppStateChange", {
                triggerID: triggerID,
                onContinue: svc.onCancelContinue
            });
        };

        svc.onCancelContinue = function() {
            aside.hide();
        };

        svc.checkSelected = function() {
            var ret = false;
            angular.forEach(svc.tabs, function(item, id) {
                ret = item.ctrl.checkIsSelected();
            });
            return ret;
        };

        svc.onUpdate = function() {

            if (svc.reqCount !== 0) {
                return;
            } else if (svc.allTabsAreValid()) {

                // validation not req for spend mgmt
                //if (svc.checkSelected() === true) {
                    
                    svc.updateAllTabs(newRoleModel.data);
                // }

            } else {
                svc.focusInvalidTab();
            }
        };

        svc.newRole = function() {
            
            var data = {
                "editorPersonaId": persona.getId(),
                "roleName": newRoleModel.data.roleName
            };
            //newRoleSvc.save(data, svc.onCreateRoleSuccess, svc.onCreateRoleError);
        };

        svc.onCreateRoleSuccess = function(resp) {

            if (!angular.isUndefined(resp.errorReason) && resp.errorReason.trim().length === 0 && resp.isError === false) {

                var newRole = {
                    role: resp.records[0]
                };
                svc.updateAllTabs(newRole);
            } else {
                pubsub.publish("smSettings.newRoleError", resp);
            }
        };

        svc.onCreateRoleError = function(resp) {
            pubsub.publish("smSettings.newRoleError", resp.data);
        };

        svc.onUpdateError = function() {
            svc.reqErrorCount++;

            if (svc.reqCount == svc.reqSuccessCount + svc.reqErrorCount) {
                svc.focusErrorTab().resetCounts();
            }
        };

        svc.onUpdateSuccess = function() {
            svc.reqSuccessCount++;

            if (svc.reqCount === svc.reqSuccessCount) {
                aside.hide();
                svc.resetCounts();
                pubsub.publish("spndMgmtSettings.newRole");
            }
        };

        svc.registerTab = function(tab) {

            svc.tabs[tab.id] = {
                ctrl: tab.ctrl
            };
        };

        svc.resetCounts = function() {
            svc.reqCount = 0;
            svc.reqErrorCount = 0;
            svc.reqSuccessCount = 0;
        };

        svc.reset = function() {
            svc.tabWatch();
            svc.appWatch();
            svc.updateWatch();
            svc.cancelWatch();

            svc.tabs = {};
            svc.reqCount = 0;
            svc.activeTabs = {};
        };


        svc.updateAllTabs = function(role) {

            angular.forEach(svc.tabs, function(item, id) {
                // if (item.ctrl.isDirty()) {
                if (item.ctrl.state.id === "01") {
                    svc.reqCount++;
                    item.ctrl.onUpdate(role)
                        .then(svc.onUpdateSuccess, svc.onUpdateError);
                }
                
            });

            if (svc.reqCount === 0) {
                aside.hide();
            }
        };
    }

    angular
        .module("settings")
        .service("spndMgmtNewRoleTabsManager", [
            "$rootScope",
            "timeout",
            "pubsub",
            "spndMgmtNewRoleTabsData",
            "spndmgmtNewRoleTabsMenu",
            "spndmgmtNewRoleAside",
            "spndmgmtNewRoleModel",
            "spndMgmtNewRoleSaveSvc",
            "userSessionModel",
            "personaDetails",
            SpndMgmtNewRoleTabsManager
        ]);
})(angular);