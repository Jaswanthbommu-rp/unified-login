//  Sync Manager Model

(function (angular, undefined) {
    "use strict";

    function factory(pubsub, security, persona) {
        function ProductDataSyncManager() {
            var s = this;
            s.init();
        }

        var p = ProductDataSyncManager.prototype;

        p.init = function () {
            var s = this;

            s.productControlsMap = {};
            s.groupMap = {};
            s.propertyMap = {};
            s.roleMap = {};


            s.productControlsList = {
                products:[]
            };
            //s.productControlsList = [];
            s.groupList = [];
            s.propertyList =[];
            s.productsTouched = [];
            s.originalPropertyList = [];
            s.roleList = [];
            // s.roleList = {
            //     roles:[]
            // };

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

        p.getProductsTouched = function (){
            var s = this;
            return s.productsTouched;
        };

        p.getProductControlsList = function (){
            var s = this;
            return s.productControlsList;
        };

        p.getPropertyList = function () {
            var s = this;
            return s.propertyList;
        };

        p.getOriginalPropertyList = function () {
            var s = this;
            return s.originalPropertyList;
        };

        p.getRoleList = function () {
            var s = this;
            return s.roleList;
        };
        // Setters


        p.setPropertyList = function (list, key) {
            var s = this;
            s.propertyList = list;
            s.renderPropertyMap(key);
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

        p.setProductDataSelectKey = function (key) {
            var s = this;
            s.productDataSelectKey = key;
            return s;
        };

        p.setProductControlsList = function (list) {
            var s = this;
           s.productControlsList.products.push(list);
           s.renderProductControlsMap();
           return s;
        };

        p.setRoleList = function (list, key) {
            var s = this;
            s.roleList= list;
            s.renderRoleMap(key);
            return s;
        };

        p.setRoleSelectKey = function (key) {
            var s = this;
            s.roleSelectKey = key;
            return s;
        };


        p.getPageDisplayName = function (product) {
            var s = this,
                pageDisplayName = "";
            if (s.productControlsMap['product' + product] !== undefined)
            {
                pageDisplayName = s.productControlsMap['product' + product].displayName;
            }

            return pageDisplayName;
        };

        p.getProductControls = function (product) {
            var s = this,
                productControlList;

            if (s.productControlsMap['product' + product] !== undefined)
            {
                productControlList = s.productControlsMap['product' + product].control;
            }

            return productControlList;
        };

        p.getProductRolesData = function (product) {
            var s = this,
                productRolesList;

            if (s.roleMap['product' + product] !== undefined)
            {
                productRolesList = s.roleMap['product' + product].roles;
            }
            return productRolesList;
        };

        p.getProductPropertiesData = function (product) {
            var s = this,
                productPropertiesList;

            if (s.propertyMap['product' + product] !== undefined)
            {
                productPropertiesList = s.propertyMap['product' + product].properties;
            }

            return productPropertiesList;
        };

         p.updateProductAllProperties = function (product, value) {
            var s = this,
                productPropertiesList;

            if (s.propertyMap['product' + product] !== undefined)
            {
               s.propertyMap['product' + product].allProperties = value;
            }

           // return productPropertiesList;
        };

         p.isProductAllProperties = function (product) {
            var s = this;

            if (s.propertyMap['product' + product] !== undefined)
            {
               return s.propertyMap['product' + product].allProperties ;
            }

            return false;
        };

        p.selectedRoleSync = function (key, record) {
            var s = this,
                roleData,
                selectedRole,
                selectState = false;

            roleData = s.roleMap['product' + key].roles;

             roleData.forEach(function (item) {
                item.isAssigned = false;
                item.isAssigned = item.id == record.id;
             });

            // if (s.productsTouched.indexOf(key) !== -1){
            //     s.productsTouched.push(key);
            // }
            return s;
        };

        p.selectedPropertySync = function (key, record) {
            var s = this,
               propertyData;

            propertyData = s.propertyMap['product' + key].properties;

            propertyData.forEach(function (item) {
                item.isAssigned = false;
                item.isAssigned = item.id == record.id;
            });

            // if (s.productsTouched.indexOf(key) !== -1){
            //     s.productsTouched.push(key);
            // }

            return s;
        };

        p.selectedPropertiesSync = function (property) {
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

        p.allPropertiesSync = function (companyId, selected) {
            var s = this,
                propertyList,
                selectState = false,
                assignedCount = 0,
                totalCount = 0;

            propertyList = s.propertyMap['company' + companyId].property;

            propertyList.properties.forEach(function (item) {
                item["isAssigned"] = selected;
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


        // p.selectedRoleSync = function (key, data) {
        //     var s = this,
        //         roleData,
        //         groupdata,
        //         selectedRole,
        //         selectState = false;

        //     roleData = s.roleMap['company' + key].role;

        //     if (data && data.length > 0) {

        //         roleData.forEach(function (item) {
        //             item.isAssigned = false;
        //             data.forEach(function (role) {
        //                 if (item.name === role.name) {
        //                     item.isAssigned = role.isAssigned;
        //                 }
        //             });
        //         });

        //         roleData.forEach(function (item) {
        //             if (item["isAssigned"]) {
        //                 selectState = true;
        //             }
        //         });

        //         s.roleList.forEach(function (item) {
        //             if (item.companyId === key) {
        //                 item.isAssigned = selectState;
        //                 selectedRole = item;
        //             }
        //         });

        //     }

        //     return s;
        // };

         p.renderProductControlsMap = function () {
            var s = this;

            if (!angular.equals({}, s.productControlsList)) {
                s.productControlsList.products.forEach(function (product) {
                    s.productControlsMap['product' + product.Id] = {
                        control: product,
                        displayName: product.DisplayName,
                        controls: []
                    };
                });
            }
        };

         p.renderPropertyMap = function (key) {
            var s = this;
            if (!angular.equals({}, s.propertyList)) {
                s.propertyMap['product' + key] = {
                    properties: s.propertyList,
                    allProperties: false
                };
            }
        };

         p.renderRoleMap = function (key) {
            var s = this;

            if (!angular.equals({}, s.roleList)) {
                s.roleMap['product' + key] = {
                    roles: s.roleList
                };
            }
        };

        p.renderMap = function (key) {
            var s = this;

            if (!s.roleList.empty()) {
                s.roleList.forEach(function (role) {
                    s.roleMap['company' + role.companyId] = {
                        role: role.roles,
                        roles: []
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
            s.productControlsList = [];
            s.productControlsMap = {};
        };

        return new ProductDataSyncManager();
    }

    angular
        .module("settings")
        .factory("productDataSyncManager", [
            "pubsub",
            "routeSecurity",
            "personaDetails",
            factory
        ]);
})(angular);
