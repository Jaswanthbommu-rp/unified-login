//  Product Solution Model

(function(angular, undefined) {
    "use strict";

    function factory($filter, pubsub, favorites) {
        function ProductSolutionModelExt() {
            var s = this;
            s.init();
        }

        var p = ProductSolutionModelExt.prototype;

        p.init = function() {
            var s = this;
            s.data = {};
            s.isActive = true;
        };

        // Setters

        p.setData = function(data) {
            var s = this;
            s.data = {
                titleID: data.titleID,
                productId: data.productId,
                productStatus: data.productStatus,
                hasAccess: data.isAssigned,
                solutionId: data.solutionId,
                productName: data.productName,
                solutionName: data.titleID,
                productDescription: data.products
            };

            return s;
        };

        // Getters

        p.getIconId = function() {
            var s = this;
            return "prod" + s.getProductId();
        };

        p.getId = function() {
            var s = this;
            return s.data.solutionId;
        };

        p.getProductId = function() {
            var s = this;
            return s.data.productId;
        };

        p.getSolutionName = function() {
            var s = this;
            return s.data.productName;
        };

        p.getWinId = function() {
            var s = this;
            return s.data.isNewTab ? ("win" + s.getId()) : undefined;
        };

        p.getFamilyId = function() {
            var s = this;
            return s.data.familyId;
        };

        // Actions

        // Assertions

        p.isProductDisabled = function() {
            var s = this;
            return s.data.productStatus != "8";
        };

        p.hasId = function(id) {
            var s = this;
            return s.getId() == id;
        };

        p.hasMatchingId = function(filter) {
            var s = this,
                isMatch = true;

            if (filter.prodFamily) {
                isMatch = isMatch && s.data.familyId == filter.prodFamily;
            }

            if (filter.prodSolution) {
                isMatch = isMatch && s.getId() == filter.prodSolution;
            }

            return isMatch;
        };

        p.hasMatchingText = function(filter) {
            var s = this,
                isMatch = true,
                st = filter.searchText.toLowerCase(),
                solnName = s.data.solutionName.toLowerCase();

            if (st) {
                isMatch = isMatch && solnName.indexOf(st) != -1;
            }

            return isMatch;
        };

        p.isFavorite = function() {
            var s = this;
            return s.data.isFavorite;
        };

        p.isRelevant = function(filter) {
            var s = this,
                matchId = s.hasMatchingId(filter),
                matchText = s.hasMatchingText(filter);

            s.isActive = matchId && matchText;

            return s.isActive;
        };

        // Destroy/Reset

        p.destroy = function() {
            var s = this;
            s.data = undefined;
        };

        return function(data) {
            return (new ProductSolutionModelExt()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("productSolutionModelExt", ["$filter", "pubsub", "favoritesSvc", factory]);
})(angular);