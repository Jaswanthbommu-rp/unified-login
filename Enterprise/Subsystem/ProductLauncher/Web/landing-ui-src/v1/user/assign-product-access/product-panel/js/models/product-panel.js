//Client Portal Data model

(function (angular, undefined) {
    "use strict";

    function factory(dataSyncManager) {
        function ProductPanelData() {
            var s = this;
            s.init();
        }

        var p = ProductPanelData.prototype;

        p.init = function () {
            var s = this;
            s.active = false;
            s.roleGridActive = false;
            s.propertyGridActive = false;
            s.changed = false;

            s.data = {
                "records": []
            };

            s.batchData = {
                productId: 0,
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                    roleList: [],
                    propertyList: [],
                    regionList: [],
                    propertyGroupList: [],
                    removedPropertyList: [],
                    isAssignedNewPropertyByDefault: false
                }
            };

            s.batchBMData = {
                productId: 34,
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                    roleList: [],
                    propertyList: [],
                    regionList: [],
                    propertyGroupList: [],
                    removedPropertyList: [],
                    isAssignedNewPropertyByDefault: false
                }
            };

            s.roles = [];
            s.properties = [];
            s.propertyGroups = [];
            s.isAllProperties = false;
            s._batchData = angular.copy(s.batchData);
            s._batchBMData = angular.copy(s.batchBMData);
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

        p.setRoleGridActive = function (bool) {
            var s = this;
            s.roleGridActive = bool;
            return s;
        };

        p.isRoleGridActive = function () {
            var s = this;
            return s.roleGridActive;
        };

        p.setPropertyGridActive = function (bool) {
            var s = this;
            s.propertyGridActive = bool;
            return s;
        };

        p.isPropertyGridActive = function () {
            var s = this;
            return s.propertyGridActive;
        };


        p.getData = function (productId) {
            var s = this,
                hasRoleSelected = false,
                hasPropertySelected = false,
                hasPropertyGroupSelected = false,
                aoFamilyProduct = false;

            s.batchData = angular.copy(s._batchData);

            var roles = dataSyncManager.getProductRolesData(productId);
            var properties = dataSyncManager.getProductPropertiesData(productId);
            var propertyGroups = dataSyncManager.getProductPropertyGroupData(productId);
            var bmroles = "";
            if (productId == "30") {
                bmroles = dataSyncManager.getProductBenchMarkRolesData("34");
            }

            s.batchData.productId = productId;

            if (productId == "29" || productId == "30" || productId == "31" || productId == "32" ||
                productId == "51" || productId == "52" || productId == "53" || productId == "54") {
                aoFamilyProduct = true;
            }

            if (roles !== undefined && roles.length) {
                s.batchData.inputJson.roleList = [];
                roles.forEach(function (role) {
                    if (role.isAssigned) {
                        if (aoFamilyProduct) {
                            s.batchData.inputJson.roleList.push(role.name);
                        }
                        else {
                            s.batchData.inputJson.roleList.push(role.id);
                        }
                    }
                });

                hasRoleSelected = s.batchData.inputJson.roleList.length > 0;
            }

            if (properties !== undefined && properties.length) {
                s.batchData.inputJson.propertyList = [];

                if (dataSyncManager.isProductAllProperties(productId)) {
                    if (productId == "14" || productId == "3" || productId == "23") {
                        s.batchData.inputJson.propertyList.push("-1");
                    }
                    else {
                        s.batchData.inputJson.propertyList.push("all");
                    }
                }
                else {
                    properties.forEach(function (prop) {
                        if (prop.isAssigned) {
                            s.batchData.inputJson.propertyList.push(prop.id);
                        }
                        if (!prop.isAssigned && prop.originalProperty) {
                            s.batchData.inputJson.removedPropertyList.push(prop.id);
                        }
                    });

                    if (productId == "9") {
                        s.batchData.inputJson.isAssignedNewPropertyByDefault = dataSyncManager.isProductNewPropertyByDefault(productId);
                    }
                }

                hasPropertySelected = s.batchData.inputJson.propertyList.length > 0;
            }

            if (propertyGroups !== undefined && propertyGroups.length) {
                s.batchData.inputJson.regionList = [];
                s.batchData.inputJson.propertyGroupList = [];

                propertyGroups.forEach(function (group) {
                    if (group.isAssigned) {
                        if (aoFamilyProduct) {
                            s.batchData.inputJson.propertyGroupList.push(group.id);
                        }
                        else {
                            s.batchData.inputJson.regionList.push(group.id);
                        }
                    }
                });

                hasPropertyGroupSelected = s.batchData.inputJson.propertyGroupList.length > 0;
            }

            if (productId == "30" && bmroles !== undefined && bmroles.length > 0) {
                s.data = angular.copy(s._data);
                s.batchBMData = angular.copy(s._batchBMData);

                s.batchBMData.inputJson.roleList = [];
                s.batchBMData.inputJson.propertyList = [];
                s.batchBMData.inputJson.propertyList = s.batchData.inputJson.propertyList;

                bmroles.forEach(function (role) {
                    if (role.isAssigned) {
                        s.batchBMData.inputJson.roleList.push(role.name);
                    }
                });

                s.data.records.push(s.batchData);
                s.data.records.push(s.batchBMData);
            }

            if (productId == "10") {
                hasRoleSelected = true;
            }

            if (productId == "3" && !dataSyncManager.isProductDependencyDataNeeded(productId)) {
                hasPropertySelected = true;
                s.batchData.inputJson.propertyList = [];
            }

            if (aoFamilyProduct) {
                if (productId == "30" && bmroles && bmroles.length > 0) {
                    if (hasRoleSelected && hasPropertySelected) {
                        return s.data.records;
                    }
                }
                else if (hasRoleSelected && (hasPropertySelected || hasPropertyGroupSelected)) {
                    return s.batchData;
                }
            }

            if (productId == "39") {
                hasPropertySelected = true;
            }

            if (hasRoleSelected && hasPropertySelected) {
                return s.batchData;
            }

            return null;
        };

        p.gridReset = function () {
            var s = this;
            s.isAllProperties = false;
            s.roleGridActive = false;
            s.propertyGridActive = false;
        };

        p.reset = function () {
            var s = this;

            s.roles = [];
            s.properties = [];
            s.isAllProperties = false;
            s.roleGridActive = false;
            s.propertyGridActive = false;
            s.active = false;
            s.changed = false;
            s.data = angular.copy(s._data);
        };

        return new ProductPanelData();
    }

    angular
        .module("settings")
        .factory("productPanelDataModel", ["productDataSyncManager", factory]);
})(angular);
