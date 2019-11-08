//  BusinessIntelligence Data Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function BusinessIntelligenceData() {
            var s = this;
            s.init();
        }

        var p = BusinessIntelligenceData.prototype;

        p.init = function () {
            var s = this;
            s.changed = false;
            s.active = false;
            s.data = {
                productId: 29,
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                    roleList: [],
                    propertyList: [],
                    propertyGroupList: [],
                    isAssigned: false,
                    companyId: ""
                }
            };

            s.roles = [];
            s.properties = [];
            s.propertyGroup = [];
            s.companyRoles = [];
            s.ready = false;
            s.multiCompany = false;
            s.companies = [];
            s._data = angular.copy(s.data);
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

        p.isReady = function () {
            var s = this;
            return s.ready;
        };

        p.isMultiCompany = function () {
            var s = this;
            return s.companies.length > 1;
        };

        p.isSingleCompany = function () {
            var s = this;
            return s.companies.length == 1;
        };

        p.setProperties = function (propertiesData) {
            var s = this;
            s.properties = propertiesData;
        };

        p.setPropertyGroups = function (propertyGroupData) {
            var s = this;
            s.propertyGroup = propertyGroupData;
        };

        p.setCompanies = function (companiesData) {
            var s = this;
            s.ready = true;
            s.companies = companiesData;
            s.multiCompany = s.companies.length > 1;
        };

        p.setCompanyRoles = function (data) {
            var s = this;
            s.ready = true;
            s.companyRoles = data;
        };

        p.setRoles = function (rolesData) {
            var s = this;
            s.roles = rolesData;
        };

        p.getCompanies = function () {
            var s = this;
            return s.companies;
        };

        p.getData = function () {
            var s = this,
                hasRoles = false,
                hasProperties = false,
                hasPropertyGroups = false,
                hasRegions = false;


            if (s.propertyGroup && s.propertyGroup.length) {
                s.data.inputJson.propertyGroupList = [];

                s.propertyGroup.forEach(function (propertyGroup) {
                    if (propertyGroup.isAssigned) {
                        s.data.inputJson.propertyGroupList.push(propertyGroup.groupId);
                    }
                });

                hasPropertyGroups = s.data.inputJson.propertyGroupList.length > 0;
            }

            var propData = s.properties.properties;
            if (propData && propData.length) {
                s.data.inputJson.propertyList = [];

                propData.forEach(function (prop) {
                    if (prop.isAssigned) {
                        s.data.inputJson.propertyList.push(prop.propertyId);
                    }
                });

                hasProperties = s.data.inputJson.propertyList.length > 0;
            }

            var rolesData = s.companyRoles.roles;
            if (rolesData && rolesData.length) {
                s.data.inputJson.roleList = [];

                rolesData.forEach(function (role) {
                    if (role.isAssigned) {
                        s.data.inputJson.roleList.push(role.name);
                    }
                });

                hasRoles = s.data.inputJson.roleList.length > 0;
            }

            if (hasRoles && (hasProperties || hasPropertyGroups)) {
                s.data.inputJson.isAssigned = true;
                s.data.inputJson.companyId = s.companyRoles.companyId;
                return s.data;
            }

            return null;
        };

        p.reset = function () {
            var s = this;

            s.changed = false;
            s.active = false;
            s.ready = false;
            s.properties = [];
            s.propertyGroup = [];
            s.companies = [];
            s.companyRoles = [];
            s.roles = [];
            s.data = angular.copy(s._data);
        };

        return new BusinessIntelligenceData();
    }

    angular
        .module("settings")
        .factory("businessIntelligenceDataModel", [factory]);
})(angular);
