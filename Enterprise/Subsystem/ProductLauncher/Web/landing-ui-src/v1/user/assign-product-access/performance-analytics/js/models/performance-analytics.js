//  BusinessIntelligence Data Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function PerformanceAnalyticsData() {
            var s = this;
            s.init();
        }

        var p = PerformanceAnalyticsData.prototype;

        p.init = function () {
            var s = this;
            s.changed = false;
            s.active = false;

            s.data = {
                "records": []
            };

            s.padata = {
                productId: 30,
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

            s.dropdownRoledata = {
                //title: "Select All Roles",
                options: []
            };

            s.roles = [];
            s.properties = [];
            s.selectedProperties = [];
            s.propertyGroup = [];
            s.propertyGroupList = [];
            s.ready = false;
            s.benchmarkReady = false;
            s.benchmarkDataReady = false;
            s.multiCompany = false;
            s.companies = [];
            s.companyRoles = [];
            s.companyBMRoles = [];
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

        p.setBenchmarkReady = function (bool) {
            var s = this;
            s.benchmarkReady = bool;
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

        p.isBenchmarkDataReady = function () {
            var s = this;
            return s.benchmarkDataReady;
        };

        p.isBenchmarkReady = function () {
            var s = this;
            return s.benchmarkReady;
        };

        p.isCompanyDataExists = function () {
            var s = this;
            return s.companies.length > 0;
        };

        p.isCompanyPropertyDataExists = function () {
            var s = this;
            return s.properties.length > 0;
        };

        p.isCompanyBMRoleDataExists = function () {
            var s = this;
            return s.companyBMRoles.length > 0;
        };

        p.setProperties = function (propertiesData) {
            var s = this;
            s.properties = propertiesData;
        };

        p.setSelectedProperties = function (propertiesData) {
            var s = this;
            s.selectedProperties = propertiesData;
        };

        p.getSelectedProperties = function () {
            var s = this;
            return s.selectedProperties;
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

        p.setCompanyBMRoles = function (data) {
            var s = this;
            s.companyBMRoles = data;
            s.benchmarkDataReady = true;
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
                hasBMRoles = false,
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

                    //Check Company has BenchMark Data
                    if (s.companyBMRoles && s.companyBMRoles.length) {
                        s.companyBMRoles.forEach(function (bmcompany) {
                            if (bmcompany.companyId == company.companyId) {
                                s.padata = angular.copy(s._padata);
                                s.padata.inputJson.roleList = [];
                                s.padata.inputJson.propertyList = [];

                                if (bmcompany.isAssigned) {
                                    bmcompany.roles.forEach(function (role) {
                                        if (role.isAssigned) {
                                            s.padata.inputJson.roleList.push(role.name);
                                        }
                                    });

                                    if (!s.properties.empty()) {
                                        s.properties.forEach(function (property) {
                                            if (property.companyId === bmcompany.companyId) {
                                                property.properties.forEach(function (item) {
                                                    if (item.isAssigned) {
                                                        s.padata.inputJson.propertyList.push(item.propertyId);
                                                    }
                                                });
                                            }
                                        });
                                    }

                                    if (s.padata.inputJson.roleList && s.padata.inputJson.roleList.length) {
                                        s.padata.inputJson.companyId = bmcompany.companyId;
                                        s.padata.inputJson.isAssigned = bmcompany.isAssigned;
                                        s.padata.inputJson.propertyGroupList = s.propertyGroupList;
                                        s.padata.productId = 34;
                                        s.data.records.push(s.padata);
                                    }
                                }
                            }

                        });
                    }
                    //end
                });
                hasRoles = s.data.records.length > 0;
            }

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
            s.benchmarkReady = false;
            s.benchmarkDataReady = false;
            s.properties = [];
            s.propertyGroup = [];
            s.roleList = [];
            s.propertyGroupList = [];
            s.companies = [];
            s.roles = [];
            s.companyRoles = [];
            s.companyBMRoles = [];
            s.dropdownRoledata = {};
            s.padata = angular.copy(s._padata);
            s.data = angular.copy(s._data);
        };

        return new PerformanceAnalyticsData();
    }

    angular
        .module("settings")
        .factory("performanceAnalyticsDataModel", [factory]);
})(angular);
