//  Options Cache Model

(function (angular, undefined) {
    "use strict";

    function factory($resource, eventStream) {
        function OptionsCache() {
            var s = this;
            s.init();
        }

        var p = OptionsCache.prototype;

        p.init = function () {
            var s = this;
            s.options = [];
            s.reqData = {};
            s.isBusy = false;
            s.hasData = false;
            s.ready = eventStream();
        };

        p.get = function (callback) {
            var s = this;
            if (s.hasData) {
                callback(s.options);
            }
            else {
                if (!s.isBusy) {
                    s.getData();
                    s.isBusy = true;
                }
                s.ready.subscribe(callback);
            }
            return s;
        };

        p.getData = function () {
            var s = this;
            $resource(s.url).get(s.reqData, s.setData.bind(s));
            return s;
        };

        p.getOptionsFromResponse = function (resp) {
            return resp.data;
        };

        p.setData = function (resp) {
            var s = this,
                options = s.getOptionsFromResponse(resp);

            s.isBusy = false;
            s.hasData = true;
            s.options = s.options.concat(options);
            s.ready.publish(s.options);
            s.ready.destroy();
            s.ready = undefined;
            return s;
        };

        p.setDefaultOptions = function (options) {
            var s = this;
            s.options = options;
            return s;
        };

        p.setReqData = function (data) {
            var s = this;
            s.reqData = data;
            return s;
        };

        p.setUrl = function (url) {
            var s = this;
            s.url = url;
            return s;
        };

        p.destroy = function () {
            var s = this;
            s.options.flush();
            s.options = undefined;
        };

        return function (url) {
            return (new OptionsCache()).setUrl(url);
        };
    }

    angular
        .module("settings")
        .factory("optionsCache", [
            "$resource",
            "eventStream",
            factory
        ]);
})(angular);
