//  Product Family

(function (angular) {
    "use strict";

    function factory() {
        function ProductFamilyModel() {
            var s = this;
            s.init();
        }

        var p = ProductFamilyModel.prototype;

        p.init = function () {
            var s = this;

            s.data = {
                id: "",
                name: "",
                productItems: []                
            };

            s.state = {
                isActive: true,
                isSelectAll: false
            };
        };

        // Setters

        p.setData = function (data) {
            var s = this;
            angular.extend(s.data, data || {});
            return s;
        };

        p.setSelectAllState = function(flag) {
            var s = this;
            s.state.isSelectAll = flag;
            return s;
        };

        p.setActiveState = function(flag) {
            var s = this;
            s.state.isActive = flag;
            return s;
        };

        p.setProductSelectState = function(isSelectAll, product) {
            product.setSelected(isSelectAll);
        };

        // Getters

        p.getId = function () {
            var s = this;
            return s.data.id;
        };

        // Assertions

        p.hasProducts = function () {
            var s = this;
            return s.data.productItems && s.data.productItems.length > 0;
        };

        // Actions

        p.addProduct = function(item) {
            var s = this;
            s.data.productItems.push(item);
            return s;
        };

        p.initStates = function() {
            var s = this;
            if(s.hasProducts()) {
                var selectedCounter = 0;
                angular.forEach(s.data.productItems, function(currProduct) {
                    if(currProduct.isSelected()) {
                        s.setActiveState(true);
                        selectedCounter++;
                    }
                });

                if(selectedCounter == s.data.productItems.length) {
                    s.setSelectAllState(true);
                }
            }

            return s;
        };

        p.selectAllProducts = function(isSelectAll) {
            var s = this;
            if(s.hasProducts()) {
                s.data.productItems.forEach(
                    s.setProductSelectState.bind(null, isSelectAll));
            }
        };

        p.toggleSelectAll = function(flag) {
            var s = this,
                isSelectAll = !s.state.isSelectAll;

            if(angular.isDefined(flag)) {
                isSelectAll = flag;
            }
            
            s.setSelectAllState(isSelectAll);
            s.selectAllProducts(isSelectAll);

            if(isSelectAll) { //open the panel
                s.setActiveState(true);
            }

            return isSelectAll;
        };

        p.togglePanelState = function() {
            var s = this,
                isPanelActive = !s.state.isActive;

            s.setActiveState(isPanelActive);

            return isPanelActive;
        };

        p.destroyProducts = function() {
            var s = this;
            angular.forEach(s.data.productItems, function(currProduct){
                currProduct.destroy();
            });
        };

        p.destroy = function() {
            var s = this;
            s.destroyProducts();
            s.data = undefined;
            s = undefined;
        };


        return function(familyId, familyName) {
            var inst = new ProductFamilyModel();

            inst.setData({
                id: familyId,
                name: familyName
            });

            return inst;
        };
    }

    angular
        .module("settings")
        .factory("personaProductFamily", [
            factory
        ]);

})(angular);