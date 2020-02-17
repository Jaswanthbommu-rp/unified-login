//  Sync Manager Model

(function (angular, undefined) {
    "use strict";
    function factory(pubsub, security) {
         function PMSyncManager() {
            var s = this;
            s.init();
        }

        var p = PMSyncManager.prototype;

        p.init = function () {
            var s = this;

            s.entityMap = {};
            s.roleMap = {};

            s.entityList = [];
            s.roleList = [];
        };

        // Getters

        p.getSelectedCount = function (list, selectKey) {
            var s = this,
                count = 0;

            list.forEach(function (item) {
                if (item[selectKey]) {
                    count++;
                }
            });

            return count;
        };

         // Setters

        p.setEntityList = function (list) {
            var s = this;
            s.entityList = list;
            s.renderMap();
            return s;
        };

         p.setEntitySelectKey = function (key) {
            var s = this;
            s.entitySelectKey = key;
            return s;
        };

        p.setRoleList = function (list) {
            var s = this;
            s.roleList = list;
            s.renderMap();
            return s;
        };

        p.setRoleSelectKey = function (key) {
            var s = this;
            s.roleSelectKey = key;
            return s;
        };

        p.getTranslatedRoleList = function (key) {
            var s = this,
                roleList,
                data,
                option,
                disabled = false,
                selectState = false;

            if (security.isAllowed("viewUser")) {
                disabled = true;
            }

            s.data = {
                title: "Select All Roles",
                options: []
            };

            roleList = s.roleMap['entity' + key].role;

            roleList.forEach(function (data) {
                option = {
                    name: data.name,
                    isAssigned: data.isAssigned,
                    disabled: disabled
                };

                s.data.options.push(option);
            });

            return s.data;
        };

        p.selectedRoleSync = function (key, data) {
            var s = this,
                roleData,
                selectedRole,
                i = 0,
                selectState = false;

            roleData = s.roleMap['entity' + key].role;

            if (data && data.length > 0) {
                roleData.forEach(function (item) {
                    item.isAssigned = false;
                    data.forEach(function (role) {
                        if (item.name === role.name) {
                            item.isAssigned = role.isAssigned;
                        }
                    });
                });

                roleData.forEach(function (item) {
                    if (item["isAssigned"]) {
                        selectState = true;
                    }
                });

                s.roleList.forEach(function (item) {
                    if (item.id === key) {
                        item["isAssigned"] = selectState;
                    }

                });

            }

            return s;
        };

        p.deselectAllEntityRoles = function (key) {
            var s = this,
                roleData,
                groupdata,
                selectedRole,
                selectState = false;

            roleData = s.roleMap['entity' + key].role;

            roleData.forEach(function (item) {
                item.isAssigned = false;
            });

            s.roleList.forEach(function (item) {
                if (item.id === key) {
                    item["isAssigned"] = false;
                }
            });

            return s;
        };

        p.renderMap = function () {
            var s = this;

            if (!s.roleList.empty()) {
                s.roleList.forEach(function (role) {

                     s.roleMap['entity' + role.id] = {
                         propertiesList: role.propertiesList,
                         role: role,
                         assignedProperties:"abc of 787"
                     };
                });
            }

            if (!s.entityList.empty()) {
                s.entityList.forEach(function (entity) {
                    s.entityMap['entity' + entity.id] = {
                        entity: entity,
                        entities: []
                    };
                });
            }

        };

        p.updateSelectState = function (list, selectKey, bool) {
            var s = this;

            list.forEach(function (item) {
                item[selectKey] = bool;
            });

            return s;
        };

        // Assertions

        p.allSelected = function (list, selectKey) {
            var s = this;
            return s.getSelectedCount(list) === list.length;
        };

        p.reset = function () {
            var s = this;
            s.entityMap = {};
            s.roleMap = {};
            s.entityList = [];
            s.roleList = [];
        };

        return new PMSyncManager();

    }
    angular
        .module("settings")
        .factory("pmSyncManager", [
            "pubsub",
            "routeSecurity",
            factory
        ]);
})(angular);
