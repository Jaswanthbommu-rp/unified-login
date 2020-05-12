//  Sync Manager Model

(function (angular, undefined) {
    "use strict";

    function factory(pubsub, security, persona, $filter) {
        function PASyncManager() {
            var s = this;
            s.init();
        }

        var p = PASyncManager.prototype;

        p.init = function () {
            var s = this;

            s.groupMap = {};
            s.propertyMap = {};
            s.roleMap = {};
            s.bmRoleMap = {};
            s.companyGroupMap = {};

            s.groupList = [];
            s.propertyList = [];
            s.originalPropertyList = [];
            s.roleList = [];
            s.bmRoleList = [];
            s.companyGroupList = [];
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

        p.getPropertyList = function () {
            var s = this;
            return s.propertyList;
        };

        p.getOriginalPropertyList = function () {
            var s = this;
            return s.originalPropertyList;
        };

        p.getComapnyGroupsData = function (companyId) {
            var s = this;
            return s.companyGroupMap['company' + companyId];
        };

        p.getBMRoleList = function () {
            var s = this;
            return s.bmRoleList;
        };

        p.getRoleList = function () {
            var s = this;
            return s.roleList;
        };
        // Setters

        p.setGroupList = function (list) {
            var s = this;
            s.groupList = list;
            s.renderMap();
            return s;
        };

        p.setCompanyGroupList = function (key, list) {
            var s = this;
            if (s.companyGroupList.empty()) {
                s.companyGroupList = list;
            }
            else {
                s.companyGroupList.concat(list);
            }

            s.renderCompanyGroupMap(key);
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

        p.setOriginalPropertyList = function (list) {
            var s = this;
            s.originalPropertyList = angular.copy(list);
            return s;
        };

        p.setPropertySelectKey = function (key) {
            var s = this;
            s.propertySelectKey = key;
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

        p.setBMRoleList = function (list) {
            var s = this;
            s.bmRoleList = list;
            s.renderMap();
            return s;
        };

        p.setBMRoleSelectKey = function (key) {
            var s = this;
            s.bmRoleSelectKey = key;
            return s;
        };
        // Actions

        p.removeCompanyGroup = function (companyId) {
            var s = this,
                groupdata,
                selectState = false;

            if (!angular.equals({}, s.companyGroupMap)) {
                if (s.companyGroupMap['company' + companyId]) {
                    groupdata = s.companyGroupMap['company' + companyId].groups;

                    if (!groupdata.empty()) {
                        groupdata.forEach(function (item) {
                            var index = s.companyGroupList.indexOf(item.groupId);
                            s.companyGroupList.splice(index, 1);
                        });
                    }

                    pubsub.publish("PACR.CompanyGroupReset", s.companyGroupList);
                }
            }

            return s.companyGroupList;
        };


        p.selectedPropertySync = function (property) {
            var s = this,
                propertyList,
                selectState = false,
                assignedCount = 0,
                totalCount = 0;

            propertyList = s.propertyMap['company' + property.companyId].property;

            propertyList.properties.forEach(function (item) {
                if (item.propertyId === property.propertyId) {
                    item["isAssigned"] = property.isAssigned;
                }
                if (item.isAssigned) {
                    assignedCount++;
                }
                totalCount++;
            });

            propertyList.assignedProperties = assignedCount + " of " + totalCount;
            return s;
        };

        p.allPropertiesSync = function (companyId, selected, filterBy) {
            var s = this,
                propertyList,
                filterList,
                selectState = false,
                assignedCount = 0,
                totalCount = 0;

            propertyList = s.propertyMap['company' + companyId].property;
            filterList = $filter("filter")(propertyList.properties, filterBy);
            filterList.forEach(function (item) {
                item["isAssigned"] = selected;
            });
            propertyList.properties.forEach(function (item) {
                    if (item.isAssigned) {
                        assignedCount++;
                    }
                    totalCount++;
            });
            propertyList.assignedProperties = assignedCount + " of " + totalCount;
            return s;
        };

        p.updatePropertyList = function (property) {
            var s = this;
            s.propertyList.push(property);
            return s;
        };

        p.getCompanyGroupList = function () {
            var s = this;
            return s.companyGroupList;
        };

        p.getTranslatedRoleList = function (key) {
            var s = this,
                roleList,
                data,
                option,
                disabled = false,
                selectState = false;

            if (security.isAllowed("viewUser") || s.isUserHasManageProductAccess()) {
                disabled = true;
            }

            s.data = {
                title: "Select All Roles",
                options: []
            };

            roleList = s.roleMap['company' + key].role;

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

        p.getTranslatedBMRoleList = function (key) {
            var s = this,
                roleList,
                data,
                option,
                disabled = false,
                selectState = false;

            if (security.isAllowed("viewUser") || s.isUserHasManageProductAccess()) {
                disabled = true;
            }

            s.data = {
                title: "Select All Roles",
                options: []
            };

            roleList = s.bmRoleMap['company' + key].role;

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
                groupdata,
                selectedRole,
                selectState = false;

            roleData = s.roleMap['company' + key].role;

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
                    if (item.companyId === key) {
                        item.isAssigned = selectState;
                        selectedRole = item;
                    }
                    pubsub.publish("PACR.CompanySelected", selectedRole);
                });

            }

            return s;
        };

        p.deselectAllcompanyRoles = function (companyId) {
            var s = this,
                roleData,
                groupdata,
                selectedRole,
                selectState = false;

            roleData = s.roleMap['company' + companyId].role;

            roleData.forEach(function (item) {
                item.isAssigned = false;
            });

            s.roleList.forEach(function (item) {
                if (item.companyId === companyId) {
                    item["isAssigned"] = false;
                }
            });

            s.removeCompanyGroup(companyId);

            return s;
        };

        p.selectedBMRoleSync = function (key, data) {
            var s = this,
                roleData,
                selectedRole,
                i = 0,
                selectState = false;

            roleData = s.bmRoleMap['company' + key].role;

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

                s.bmRoleList.forEach(function (item) {
                    if (item.companyId === key) {
                        item["isAssigned"] = selectState;
                        selectedRole = item;
                    }

                });
            }
            else {
                roleData.forEach(function (item) {
                    item.isAssigned = false;
                });

                s.bmRoleList.forEach(function (item) {
                    if (item.companyId === key) {
                        item["isAssigned"] = false;
                    }
                });
            }

            return s;
        };

        p.renderCompanyGroupMap = function (key) {
            var s = this;
            if (!s.companyGroupList.empty()) {
                s.companyGroupMap['company' + key] = {
                    groups: []
                };

                var data = s.companyGroupMap['company' + key];

                s.companyGroupList.forEach(function (group) {
                    data.groups.push(group);
                });
            }
            return s;
        };

        p.renderMap = function () {
            var s = this;

            if (!s.roleList.empty()) {
                s.roleList.forEach(function (role) {
                    s.roleMap['company' + role.companyId] = {
                        role: role.roles,
                        roles: []
                    };
                });
            }

            if (!s.bmRoleList.empty()) {
                s.bmRoleList.forEach(function (role) {
                    s.bmRoleMap['company' + role.companyId] = {
                        role: role.roles,
                        roles: []
                    };
                });
            }

            if (!s.groupList.empty()) {
                s.groupList.forEach(function (group) {
                    s.groupMap['company' + group.companyId] = {
                        group: group,
                        groups: []
                    };
                });
            }

            if (!s.propertyList.empty()) {
                s.propertyList.forEach(function (property) {
                    s.propertyMap['company' + property.companyId] = {
                        property: property,
                        properties: []
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

        p.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageAssetOptimizationProductAccess;
        };

        p.reset = function () {
            var s = this;
            s.groupMap = {};
            s.companyGroupMap = {};
            s.propertyMap = {};
            s.roleMap = {};
            s.bmRoleMap = {};
            s.groupList = [];
            s.propertyList = [];
            s.roleList = [];
            s.originalPropertyList = [];
            s.bmRoleList = [];
            s.companyGroupList = [];
        };

        return new PASyncManager();
    }

    angular
        .module("settings")
        .factory("paSyncManager", [
            "pubsub",
            "routeSecurity",
            "personaDetails",
            "$filter",
            factory
        ]);
})(angular);
