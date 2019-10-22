//  BusinessIntelligence Data Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function InvestmentAnalyticsData() {
            var s = this;
            s.init();
        }

        var p = InvestmentAnalyticsData.prototype;

        p.init = function () {
            var s = this;
            s.changed = false;
            s.active = false;
            s.data = {
                "records": []
            };

            s.iadata = {
                productId: 31,
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
            s.markets = [];
            // s.roleList = [];
            s.properties = [];
            s.propertyGroup = [];
            s.propertyGroupList = [];
            s.ready = false;
            s.multiCompany = false;
            s.companies = [];
            s.companyRoles = [];
            s._iadata = angular.copy(s.iadata);
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

        p.setAllMarkets = function (marketsData,val) {
            var s = this;

            marketsData.forEach(function (item) {
               item["isAssigned"] = val;
            });

            s.markets = marketsData;
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
                    s.iadata = angular.copy(s._iadata);
                    s.iadata.inputJson.roleList = [];

                    if (company.isAssigned) {
                        company.roles.forEach(function (role) {
                            if (role.isAssigned) {

                                s.iadata.inputJson.roleList.push(role.name);
                            }
                        });

                        if (s.iadata.inputJson.roleList && s.iadata.inputJson.roleList.length) {
                            s.iadata.inputJson.companyId = company.companyId;
                            s.iadata.inputJson.isAssigned = company.isAssigned;
                            s.iadata.inputJson.propertyGroupList = s.propertyGroupList;
                            s.data.records.push(s.iadata);
                        }
                    }
                });

                hasRoles = s.data.records.length > 0;
            }

            if (hasRoles && hasPropertyGroups) {
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
            s.markets = [];
            s.companyRoles = [];
            s.iadata = angular.copy(s._IAdata);
            s.data = angular.copy(s._data);
        };

        return new InvestmentAnalyticsData();
    }

    angular
        .module("settings")
        .factory("investmentAnalyticsDataModel", [factory]);
})(angular);
