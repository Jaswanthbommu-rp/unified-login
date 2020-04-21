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
                    removedPropertyList: [],
                    isAssignedNewPropertyByDefault: false
                }
            };

            s.roles = [];
            s.properties = [];
            s.isAllProperties = false;
            s._batchData = angular.copy(s.batchData);
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


        // p.setRoleRights = function (list) {
        //     var s = this;
        //     s.roleRights = [];
        //     list.forEach(function (item) {
        //         s.roleRights.push(item.rightNickName.toLowerCase());
        //     });
        //     return s;
        // };


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

        //  p.getRoleRights = function () {
        //     var s = this;
        //     return s.roleRights;
        // };

        p.getData = function (productId) {
            var s = this,
                hasRoleSelected = false,
                hasPropertySelected = false;
            s.batchData = angular.copy(s._batchData);

            var roles = dataSyncManager.getProductRolesData(productId);
            var properties = dataSyncManager.getProductPropertiesData(productId);

            s.batchData.productId = productId;

            if (roles !== undefined && roles.length) {
                s.batchData.inputJson.roleList = [];
                roles.forEach(function (role) {
                    if (role.isAssigned) {
                        s.batchData.inputJson.roleList.push(role.id);
                    }
                });

                hasRoleSelected = s.batchData.inputJson.roleList.length > 0;
            }

            if (properties !== undefined && properties.length) {
                s.batchData.inputJson.propertyList = [];

                if (dataSyncManager.isProductAllProperties(productId)) {
                    if (productId == "14" && productId == "3") {
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
            //s.data.records.push(s.padata);

            if (productId == "10") {
                hasRoleSelected = true;
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
