//  revenueManagement Data Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function RevenueManagementData() {
            var s = this;
            s.init();
        }

        var p = RevenueManagementData.prototype;

        p.init = function () {
            var s = this;
            s.changed = false;
            s.active = false;

            s.data = {
                "records": []
            };

            s.padata = {
                productId: 32,
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
            s.selectedProperties = [];
            s.propertyGroup = [];
            s.propertyGroupList = [];
            s.ready = false;
            s.multiCompany = false;
            s.companies = [];
            s.companyRoles = [];
            s._padata = angular.copy(s.padata);
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

        p.isCompanyDataExists = function () {
            var s = this;
            return s.companies.length > 0;
        };

        p.isCompanyPropertyDataExists = function () {
            var s = this;
            return s.properties.length > 0;
        };

        p.setSelectedProperties = function (propertiesData) {
            var s = this;
            s.selectedProperties = propertiesData;
        };

        p.getSelectedProperties = function () {
            var s = this;
            return s.selectedProperties;
        };

        p.getProperties = function () {
            var s = this;
            return s.properties;
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
            s.companies = companiesData;
            s.ready = true;
        };

        p.setCompanyRoles = function (data) {
            var s = this;
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
                hasPropertyGroups = false;
            s.data = angular.copy(s._data);

            if (s.propertyGroup && s.propertyGroup.length) {
                s.propertyGroupList = [];

                s.propertyGroup.forEach(function (propertyGroup) {
                    if (propertyGroup.isAssigned) {
                        s.propertyGroupList.push(propertyGroup.groupId);
                    }
                });

                hasPropertyGroups = s.propertyGroupList.length > 0;
            }

            if (s.companyRoles && s.companyRoles.length) {
                s.companyRoles.forEach(function (company) {
                    s.padata = angular.copy(s._padata);
                    s.padata.inputJson.roleList = [];
                    s.padata.inputJson.propertyList = [];

                    if (company.isAssigned) {
                        company.roles.forEach(function (role) {
                            if (role.isAssigned) {
                                s.padata.inputJson.roleList.push(role.name);
                            }
                        });

                        if (!s.properties.empty()) {
                            s.properties.forEach(function (property) {
                                if (property.companyId === company.companyId) {
                                    property.properties.forEach(function (item) {
                                        if (item.isAssigned) {
                                            s.padata.inputJson.propertyList.push(item.propertyId);
                                        }
                                    });
                                }
                            });
                        }

                        if (s.padata.inputJson.roleList && s.padata.inputJson.roleList.length) {
                            s.padata.inputJson.companyId = company.companyId;
                            s.padata.inputJson.isAssigned = company.isAssigned;
                            s.padata.inputJson.propertyGroupList = s.propertyGroupList;
                            s.data.records.push(s.padata);
                        }
                    }
                });
            }

            hasRoles = s.data.records.length > 0;

            if (hasRoles) {
                return s.data.records;
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
            s.roleList = [];
            s.propertyGroupList = [];
            s.companies = [];
            s.roles = [];
            s.companyRoles = [];

            s.padata = angular.copy(s._padata);
            s.data = angular.copy(s._data);
        };

        return new RevenueManagementData();
    }

    angular
        .module("settings")
        .factory("revenueManagementDataModel", [factory]);
})(angular);
