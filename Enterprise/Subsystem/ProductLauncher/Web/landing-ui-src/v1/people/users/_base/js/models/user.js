(function (angular, undefined) {
    "use strict";

    function factory() {
        var defaultUserRecord = {
            realPageId: -1,
            isSelected: false,
            imgSrc: null,
            fullName: null,
            username: null,
            productCount: -1,
            propertyCount: -1,
            lastLogin: null,
            lockStatus: false,
            accountStatus: null,
            userType: null
        };

        return function (record) {
            record = record || {};

            var user = angular.extend(this, defaultUserRecord, record);

            return user;

        };
    }

    angular
        .module("settings")
        .factory("userListRowModel", [factory]);
})(angular);
