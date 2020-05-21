(function (angular, undefined) {
    "use strict";

    function factory(eventStream, ptSvc) {
        function ProductTemplateModel() {
            var s = this;
            s.init();
        }

        var p = ProductTemplateModel.prototype;

        p.init = function () {
            var s = this;
             s.state = {
                busy: false,
                ready: false
            };

            s.productTemplates = [];
            s.update = eventStream();
        };


        p.isReady = function (){
            var s = this;
            return s.state.ready;
        };

        p.isProductExists = function (productId) {
            var s = this;
            if (s.productTemplates.indexOf(productId) !== -1) {
                return true;
            }
            return false;
        };

         p.setTemplateProducts = function (products) {
            var s = this;

            products.data.forEach(function (item) {
                logc(item);
                s.productTemplates.push(item.productId);
            });
            return s;
        };

        p.loadProductTemplates = function () {
            var s = this,
                error = s.onLoadPTError.bind(s),
                success = s.onLoadPTSuccess.bind(s);

            if (!s.state.ready && !s.state.busy) {
                s.state.busy = true;
                ptSvc.get(success, error);
            }

            return s;
        };

        p.onLoadPTError = function () {
            var s = this;
             s.state.busy = false;
            logw("productTemplates load failed!");
            return s;
        };

        p.onLoadPTSuccess = function (resp) {
            var s = this;
            s.state.ready = true;
            s.state.busy = false;
            s.setTemplateProducts(resp);
            s.update.publish(resp);
            return s;
        };

         p.subscribe = function (callback) {
            var s = this;
            return s.update.subscribe.apply(s.update, arguments);
        };

        // Assertions

        p.isReady = function () {
            var s = this;
            return s.state.ready;
        };

        p.reload = function () {
            var s = this;
            s.reset().loadProductTemplates();
            return s;
        };

        p.reset = function () {
            var s = this;
            s.productTemplates = {};
            s.state.ready = false;
             return s;
        };

        return new ProductTemplateModel();
    }

    angular
        .module("settings")
        .factory("productTemplateModel", [
            "eventStream",
            "productTemplatesSvc",
            factory
    ]);
})(angular);
