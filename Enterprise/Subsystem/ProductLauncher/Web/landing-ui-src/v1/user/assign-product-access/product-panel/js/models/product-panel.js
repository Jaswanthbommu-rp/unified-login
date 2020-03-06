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
                    propertyList: []
                }
            };

            s.roles = [];
            s.properties = [];
            s.isAllProperties = false;
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

        p.setProperties = function (propertiesData) {
            var s = this;
            s.properties = propertiesData;
        };

        p.setRoles = function (rolesData) {
            var s = this;
            s.roles = rolesData;
        };


        p.setAllProperty = function (val) {
            var s = this;
            s.isAllProperties = val;

        };

        p.getRoles = function () {
            var s = this;
            return s.roles;
        };

        p.getProperties = function () {
            var s = this;
            return s.properties;
        };

         p.getData = function (productId) {
            var s = this,
                hasRoleSelected = false,
                hasPropertySelected = false;
//ogc("getdata",productId);
          var roles = dataSyncManager.getProductRolesData(productId);
          var properties = dataSyncManager.getProductPropertiesData(productId);
logc("getData", roles, properties);
           s.batchData.productId = productId;

           if (roles && roles.length) {
                s.batchData.inputJson.roleList = [];
                roles.forEach(function (role) {
                    logc("ppl role", role);
                    if (role.isAssigned) {
                        s.batchData.inputJson.roleList.push(role.id);
                    }
                });

                hasRoleSelected = s.batchData.inputJson.roleList.length > 0;
            }

            if (properties && properties.length) {
                s.batchData.inputJson.propertyList = [];

                if (s.isAllProperties) {
                    s.batchData.inputJson.propertyList.push("-1");
                }
                else {
                    properties.forEach(function (prop) {
                        if (prop.isAssigned) {
                            s.batchData.inputJson.propertyList.push(prop.id);
                        }
                    });
                }

                hasPropertySelected = s.batchData.inputJson.propertyList.length > 0;
            }
            //s.data.records.push(s.padata);
logc("cpl productbatch data", s.batchData);

            if (hasRoleSelected && hasPropertySelected) {
                return s.batchData;
            }

            return null;
        };

        // p.getData = function () {
        //     var s = this,
        //         hasRoleSelected = false,
        //         hasPropertySelected = false;

        //     var products = dataSyncManager.getProductsTouched();
        //     if (products && products.length) {
        //         products.forEach(function (product) {
        //           var roles = dataSyncManager.getProductRolesData(product);
        //           var properties = dataSyncManager.getProductPropertiesData(product);

        //            s.batchData.productId = product;
        //            if (roles && roles.length) {
        //                 s.batchData.inputJson.roleList = [];
        //                 s.roles.forEach(function (role) {
        //                     if (role.isAssigned) {
        //                         s.batchData.inputJson.roleList.push(role.id);
        //                     }
        //                 });

        //                 hasRoleSelected = s.batchData.inputJson.roleList.length > 0;
        //             }

        //             if (properties && properties.length) {
        //                 s.batchData.inputJson.propertyList = [];

        //                 if (s.isAllProperties) {
        //                     s.batchData.inputJson.propertyList.push("-1");
        //                 }
        //                 else {
        //                     s.properties.forEach(function (prop) {
        //                         if (prop.isAssigned) {
        //                             s.batchData.inputJson.propertyList.push(prop.id);
        //                         }
        //                     });
        //                 }

        //                 hasPropertySelected = s.batchData.inputJson.propertyList.length > 0;
        //             }
        //             s.data.records.push(s.padata);
        //         });
        //     }


        //     if (hasRoleSelected && hasPropertySelected) {
        //         return s.data;
        //     }

        //     return null;
        // };

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
