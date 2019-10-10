//  Document Management Events

(function (angular, undefined) {
    "use strict";

    function factory(eventsManager) {
        return eventsManager([
            "tabsListChange"
        ]);
    }

    angular
        .module("settings")
        .factory("dmEvents", ["eventsManager", factory]);
})(angular);
