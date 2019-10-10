//  Regex

(function (angular, undefined) {
    "use strict";

    function factory(regex) {
        var model = angular.extend({}, regex);

        model.password = /[!"#\$%&'\(\)\*\+,-\.\/\:\;<=>\?@\[\\\]\^_`\{\|\}~ ]/;

        return model;
    }

    angular
        .module("gbShared")
        .factory("gbRegex", ["regex", factory]);
})(angular);
