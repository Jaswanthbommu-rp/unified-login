// Document Management Data Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function DMData() {
            var s = this;
            s.init();
        }

        var p = DMData.prototype;

        p.init = function () {
            var s = this;
            s.roleId = {};
            s.changed = false;
            s.active = false;
            s.tabsReady = false;
            s.data = {
                productId: 20, //TODO: Enum api for products instead of hard coded
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                    roleList: [],
                    departmentList: [],
                    propertyList: [],
                }
            };

            s.roles = [];
            s.departments = [];
            s.properties = [];
            s._data = angular.copy(s.data);
        };

        p.setRoleID = function (roleType, roleId) {
            var s = this;
            s.roleId[roleType] = roleId;
        };

        p.getRoleID = function (roleType) {
            var s = this;
            return s.roleId[roleType];
        };

        p.setChanged = function () {
            var s = this;
            s.changed = true;
            return s;
        };

        p.hasChanged = function () {
            var s = this;
            return s.changed;
        };

        p.setActive = function (bool) {
            var s = this;
            s.active = bool;
            return s;
        };

        p.isActive = function () {
            var s = this;
            return s.active;
        };

        p.setProperties = function (propertiesData) {
            var s = this;
            s.properties = propertiesData;
        };

        p.setDepartments = function (departmentsData) {
            var s = this;
            s.departments = departmentsData;
        };

        p.setRoles = function (rolesData) {
            var s = this;
            s.roles = rolesData;
        };

        p.setDepartments = function (departmentsData) {
            var s = this;
            s.departments = departmentsData;
        };

        p.getTabsReady = function () {
            var s = this;
            return s.tabsReady;
        };

        p.setTabsReady = function (val) {
            var s = this;
            s.tabsReady = val;
        };

        p.getData = function () {
            var s = this,
                hasRoles = false,
                needsProperties = false,
                hasProperties = false,
                propertiesOK = true,
                needsDepartments = false,
                hasDepartments = false,
                departmentsOK = true;

            s.data.inputJson = {};

            if (s.roles && s.roles.length) {
                s.data.inputJson.roleList = [];

                s.roles.forEach(function (role) {
                    if (role.isAssigned()) {
                        s.data.inputJson.roleList.push(role.getId());
                        if (role.roleType == "Site Name") {
                            needsProperties = true;
                        }
                        else if (role.roleType == "Department") {
                            needsDepartments = true;
                        }
                    }
                });

                hasRoles = s.data.inputJson.roleList.length > 0;
            }

            if (needsProperties && s.properties && s.properties.length) {
                s.data.inputJson.propertyList = [];

                s.properties.forEach(function (prop) {
                    if (prop.isAssigned) {
                        s.data.inputJson.propertyList.push(prop.id);
                    }
                });

                hasProperties = s.data.inputJson.propertyList.length > 0;
                propertiesOK = hasProperties;
            }

            if (needsDepartments && s.departments && s.departments.length) {
                s.data.inputJson.departmentList = [];

                s.departments.forEach(function (dept) {
                    if (dept.isAssigned) {
                        s.data.inputJson.departmentList.push(dept.id);
                    }
                });

                hasDepartments = s.data.inputJson.departmentList.length > 0;
                departmentsOK = hasDepartments;
            }

            if (hasRoles && propertiesOK && departmentsOK) {
                return s.data;
            }
            return null;
        };

        p.reset = function () {
            var s = this;
            s.roles = [];
            s.active = false;
            s.changed = false;
            s.tabsReady = false;
            s.properties = [];
            s.departments = [];
            s.data = angular.copy(s._data);
        };

        return new DMData();
    }

    angular
        .module("settings")
        .factory("documentManagementDataModel", [factory]);
})(angular);
