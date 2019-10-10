//  Resource Card Model

(function (angular, undefined) {
    "use strict";

    function factory(appLangTranslate, types, methodsRepo) {
        function ResourceCardModel() {
            var s = this;
            s.init();
        }

        var p = ResourceCardModel.prototype;

        p.init = function () {
            var s = this;
            s.linksList = [];
            s.methods = methodsRepo();
            s.lang = appLangTranslate("dashboard").translate;
            s.data = {};
        };

        p.getData = function () {
            var s = this;
            return s.data;
        };

        p.getLinkTarget = function () {
            var s = this;
            if (s.data.isNewTab) {
                return s.data.titleUniqueId;
            }
            return undefined;
        };

        p.getLinkType = function () {
            var s = this;
            return s;
        };

        p.setSrc = function (src) {
            var s = this;
            s.methods.setSrc(src);
            return s;
        };

        p.setData = function (item) {
            var s = this,
                langKey = "userResources.text.",
                itemKey = "prod" + item.productId,
                linkData = types[itemKey];
            s.data = item;
            s.data.text = s.lang(langKey + itemKey);

            if (linkData.type == "method") {
                s.data.method = s.methods.get(linkData.methodName);
            }
            else if (linkData.type == "url") {
                s.data.url = item.productUrl;
                s.data.newWin = item.isNewTab;
            }

            return s;
        };

        p.isDisabled = function () {
            var s = this;
            return s.data.productStatus !== 8;
        };


        p.reset = function () {
            var s = this;
            s.data = {};
        };

        return function (data) {
            return (new ResourceCardModel()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("resourceCardModel", [
            "appLangTranslate",
            "userResourceLinkTypes",
            "rpMethodsRepo",
            factory
        ]);
})(angular);
