//  Product Solution Model

(function (angular, undefined) {
    "use strict";

    function factory($filter, pubsub, favorites) {
        function ProductSolutionModel() {
            var s = this;
            s.init();
        }

        var p = ProductSolutionModel.prototype;

        p.init = function () {
            var s = this;
            s.data = {};
            s.isActive = true;
        };

        // Setters

        p.setData = function (data) {
            var s = this;

            s.data = {
                titleID: data.titleID,
                familyId: data.familyId,
                isNewTab: data.isNewTab,
                familyName: data.family,
                productId: data.productId,
                productStatus: data.productStatus,
                hasAccess: data.hasAccess,
                isAllowFavorite: data.isAllowFavorite,
                learnMore: data.learnMore,
                solutionId: data.solutionId,
                productUrl: data.productUrl,
                isFavorite: data.isFavorite,
                isAppSwitcher: data.showInAppSwitcher,
                isProductFilter: data.showInUserListFilter,
                subsolution: data.subsolution,
                productName: data.productName,
                solutionName: data.productName,
                titleUniqueId: data.titleUniqueId,
                productDescription: data.productDescription
            };

            return s;
        };

        // Getters

        p.getIconId = function () {
            var s = this;
            return "prod" + s.getProductId();
        };

        p.getId = function () {
            var s = this;
            return s.data.solutionId;
        };

        p.getProductId = function () {
            var s = this;
            return s.data.productId;
        };

        p.getAppSwitcherStatus = function () {
            var s = this;
            return s.data.isAppSwitcher;
        };

        p.getProductFilterStatus = function () {
            var s = this;
            return s.data.isProductFilter;
        };

        p.getSolutionName = function () {
            var s = this;
            return s.data.productName;
        };

        p.getWinId = function () {
            var s = this;
            return s.data.isNewTab ? ("win" + s.getId()) : undefined;
        };

        p.getFamilyId = function () {
            var s = this;
            return s.data.familyId;
        };

        // Actions

        p.toggleFavStatus = function () {
            var params,
                s = this;

            s.data.isFavorite = !s.data.isFavorite;

            params = {
                name: "IsFavorite",
                productId: s.getProductId(),
                value: s.data.isFavorite ? 1 : 0
            };

            favorites.update(params, s.onFavoriteUpdateComplete.bind(s));

            return s;
        };

        p.onFavoriteUpdateComplete = function() {
            var s = this;
            pubsub.publish("productsData.reload");
            pubsub.publish("prodSolnFav.change");
            var omnibar = document.querySelector('omnibar-shell');
            omnibar.reload();
            return s;
        };

        // Assertions

        p.isProductDisabled = function () {
            var s = this;
            return s.data.productStatus != "8";
        };

        p.getLinkTarget = function () {
            var s = this;
            if(s.data.isNewTab) {
                return s.data.titleUniqueId;
            }
            return undefined;
        };

        p.hasId = function (id) {
            var s = this;
            return s.getId() == id;
        };

        p.hasMatchingId = function (filter) {
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

        p.hasMatchingText = function (filter) {
            var s = this,
                isMatch = true,
                st = filter.searchText.toLowerCase(),
                solnName = s.data.solutionName.toLowerCase();

            if (st) {
                isMatch = isMatch && solnName.indexOf(st) != -1;
            }

            return isMatch;
        };

        p.hasProductId = function (productId) {
            var s = this;
            return s.data.productId === productId;
        };

        p.isFavorite = function () {
            var s = this;
            return s.data.isFavorite;
        };

        p.isRelevant = function (filter) {
            var s = this,
                matchId = s.hasMatchingId(filter),
                matchText = s.hasMatchingText(filter);

            s.isActive = matchId && matchText;

            return s.isActive;
        };

        // Destroy/Reset

        p.destroy = function () {
            var s = this;
            s.data = undefined;
        };

        return function (data) {
            return (new ProductSolutionModel()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("productSolutionModel", [
            "$filter",
            "pubsub",
            "favoritesSvc",
            factory
        ]);
})(angular);
