//  Persona - Assign Products Controller

(function (angular) {
    "use strict";

    function AddProductsCtrl($scope, addProductFormData, addProductFormModel, addProductFormConfig,
                persona, personaProducts, productItem, productFamily, productsSvc, rpWatchList) {
        var vm = this;

        vm.init = function () {
            vm.watchList = rpWatchList();
            vm.watchList.add($scope.$on("$destroy", vm.destroy));

            addProductFormModel.setData(addProductFormData);
            addProductFormConfig.setMethodsSrc(vm);

            vm.productFormConfig = addProductFormConfig;
            vm.productFormData = addProductFormData;
            vm.personaProducts = personaProducts;

            vm.preIndex = "fam";

            vm.personaWatch = angular.noop;

            if (persona.isReady()) {
                vm.initData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.initData);
            }
        };

        vm.initData = function () {
            vm.toggleLoadingState(true);
            var realPageId = persona.getOrgRealPageID();
            productsSvc.products(realPageId, false)
                .get(vm.initProducts, vm.completedLoading);

            //TODO replace when Aeds check-in events
            // personaDetails.events.update.subscribe(vm.isPersonaDetailsReady);
        };

        // vm.isPersonaDetailsReady = function(realPageId) {
        //     productsSvc.products(realPageId, false)
        //         .get(vm.initProducts)
        //         .$promise
        //         .finally(vm.completedLoading);
        // };

        vm.completedLoading = function () {
            vm.toggleLoadingState(false);
        };

        vm.initProducts = function (response) {
            vm.createProducts(response.data);

            productsSvc.families()
                .get(vm.initProductFamilies)
                .$promise
                .finally(vm.completedLoading);
        };

        vm.initProductFamilies = function (response) {
            var familyObj = vm.createProductFamilies(response.data),
                familyList = [];

            vm.linkProductsWithFamilies(familyObj);

            angular.forEach(familyObj, function (currFamily) {
                currFamily.initStates();
                familyList.push(currFamily);
            });

            personaProducts.setFamilies(familyList);
        };

        vm.createProductFamilies = function (arr) {
            var familyObj = {};
            angular.forEach(arr, function (familyJson) {
                if (!familyJson.parentProductTypeId) {
                    var newFamily = productFamily(
                        familyJson.productTypeId,
                        familyJson.name
                    );

                    //prepends id to make sure that the ID will not be interpreted as an array index
                    familyObj[vm.preIndex + newFamily.getId()] = newFamily;
                }
            });

            return familyObj;
        };

        vm.linkProductsWithFamilies = function (familyObj) {
            var products = personaProducts.getProducts();
            angular.forEach(products, function (currProduct) {
                var productFamilyId = vm.preIndex + currProduct.getFamilyId();
                if (angular.isDefined(familyObj[productFamilyId])) {
                    familyObj[productFamilyId].addProduct(currProduct);
                }
            });
        };

        vm.createProducts = function (arr) {
            var productList = [];
            angular.forEach(arr, function (productJson) {
                if (productJson.familyId !== 0) {
                    var newProduct = productItem({
                        id: productJson.productId,
                        name: productJson.productName,
                        subsolution: productJson.subsolution,
                        productFamily: productJson.familyId
                    });
                    productList.push(newProduct);
                }
            });

            personaProducts.setProducts(productList);
        };

        vm.selectAllProducts = function () {
            var isSelected = addProductFormModel.isSelectAll(),
                families = personaProducts.getFamilies();

            if (isSelected) {
                addProductFormModel.clearFilter();
            }

            if (families && families.length > 0) {
                angular.forEach(families, function (currFamily) {
                    currFamily.toggleSelectAll(isSelected);
                });
            }
        };

        vm.selectAllItemsInFamily = function (currFamily) {
            var isSelected = currFamily.toggleSelectAll();
            if (!isSelected) {
                addProductFormModel.setSelectAll(isSelected);
            }
        };

        vm.selectProduct = function (prodItem, productFamily) {
            var isSelected = prodItem.isSelected();
            if (!isSelected) {
                productFamily.setSelectAllState(isSelected);
                addProductFormModel.setSelectAll(isSelected);
            }
        };

        vm.clearSelectAllProducts = function () {
            addProductFormModel.setSelectAll(false);
        };

        vm.toggleLoadingState = function (flag) {
            vm.isLoading = flag;
        };

        vm.destroy = function () {
            addProductFormModel.reset();

            vm.watchList.destroy();
            vm.productFormData.clearData();
            vm.watchList = undefined;

            vm.personaProducts = undefined;
            vm.productFormData = undefined;
            vm.productFormConfig = undefined;
            vm.personaWatch();
            vm = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("AddProductsCtrl", [
            "$scope",
            "addProductFormData",
            "addProductFormModel",
            "addProductFormConfig",
            "personaDetails",
            "personaProducts",
            "personaProductItem",
            "personaProductFamily",
            "personaProductsSvc",
            "rpWatchList",
            AddProductsCtrl
        ]);

})(angular);
