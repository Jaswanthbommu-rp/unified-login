//  Sync Manager Model

(function (angular, undefined) {
    "use strict";

    function factory(pubsub) {
        function OSSyncManager() {
            var s = this;
            s.init();
        }

        var p = OSSyncManager.prototype;

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
            pubsub.publish("onsite.updateGrids");

            return s;
        };

        p.allPropertyToGroupSync = function () {
            var s = this;

            angular.forEach(s.propertyMap, function (propertyData) {
                s.propertyToGroupSync(propertyData.property);
            });
            pubsub.publish("onsite.updateGrids");

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
            pubsub.publish("onsite.updateGrids");

            return s;
        };

        p.propertyToGroupSync = function (property) {
            var s = this,
                region_id,
                groupData,
                selected,
                propertyData = s.propertyMap['prop' + property.id];

            if (property[s.propertySelectKey]) {
                return s;
            }

            if (propertyData.region_id) {
                region_id = propertyData.region_id;
                groupData = s.groupMap['group' + region_id];
                selected = s.allSelected(groupData.properties, s.propertySelectKey);

                groupData.group[s.groupSelectKey] = selected;
            }
            pubsub.publish("onsite.updateGrids");

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
                if (property.region_id) {
                    s.propertyMap['prop' + property.id] = {
                        property: property,
                        region_id: property.region_id
                    };

                    var group = s.groupMap['group' + property.region_id];
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

        return new OSSyncManager();
    }

    angular
        .module("settings")
        .factory("osSyncManager", [
            "pubsub",
            factory
        ]);
})(angular);
