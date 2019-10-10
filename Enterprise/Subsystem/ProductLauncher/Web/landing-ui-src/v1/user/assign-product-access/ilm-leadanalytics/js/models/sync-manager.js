//  Sync Manager Model

(function (angular, undefined) {
    "use strict";

    function factory(pubsub) {
        function SyncManager() {
            var s = this;
            s.init();
        }

        var p = SyncManager.prototype;

        p.init = function () {
            var s = this;

            s.groupMap = {};
            s.propertyMap = {};

            s.groupList = [];
            s.propertyList = [];
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

        p.setGroupList = function (list) {
            var s = this;
            s.groupList = list;
            s.renderMap();
            return s;
        };

        p.setGroupSelectKey = function (key) {
            var s = this;
            s.groupSelectKey = key;
            return s;
        };

        p.setPropertyList = function (list) {
            var s = this;
            s.propertyList = list;
            s.renderMap();
            return s;
        };

        p.setPropertySelectKey = function (key) {
            var s = this;
            s.propertySelectKey = key;
            return s;
        };

        // Actions

        p.allGroupToPropertySync = function () {
            var s = this;

            angular.forEach(s.groupMap, function (groupData) {
                s.groupToPropertySync(groupData.group);
            });
            pubsub.publish("ilmla.updateGrids");

            return s;
        };

        p.allPropertyToGroupSync = function () {
            var s = this;

            angular.forEach(s.propertyMap, function (propertyData) {
                s.propertyToGroupSync(propertyData.property);
            });
            pubsub.publish("ilmla.updateGrids");

            return s;
        };

        p.groupToPropertySync = function (group) {
            var s = this,
                propertyList,
                selectState = false;

            propertyList = s.groupMap['group' + group.id].properties;

            if (group[s.groupSelectKey]) {
                selectState = true;
            }

            s.updateSelectState(propertyList, s.propertySelectKey, selectState);

            pubsub.publish("ilmla.updateGrids");

            return s;
        };

        p.propertyToGroupSync = function (property) {
            var s = this,
                groupId,
                groupData,
                selected,
                propertyData = s.propertyMap['prop' + property.id];

            if (property[s.propertySelectKey]) {
                return s;
            }

            if (propertyData.groupId ) {
                groupId = propertyData.groupId;
                groupData = s.groupMap['group' + groupId];
                selected = s.allSelected(groupData.properties, s.propertySelectKey);

                groupData.group[s.groupSelectKey] = selected;
            }
            pubsub.publish("ilmla.updateGrids");

            return s;
        };

        p.renderMap = function () {
            var s = this;

            if (s.propertyList.empty() || s.groupList.empty()) {
                return s;
            }

            s.groupList.forEach(function (group) {
                s.groupMap['group' + group.id] = {
                    group: group,
                    properties: []
                };
            });

            s.propertyList.forEach(function (property) {
                if (property.groupId ) {
                    s.propertyMap['prop' + property.id] = {
                        property: property,
                        groupId : property.groupId
                    };

                    var group = s.groupMap['group' + property.groupId ];
                    group.properties.push(property);
                }
            });

            return s;
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
            s.groupMap = {};
            s.propertyMap = {};

            s.groupList = [];
            s.propertyList = [];
        };

        return new SyncManager();
    }

    angular
        .module("settings")
        .factory("syncManager", [
            "pubsub",
            factory
        ]);
})(angular);
