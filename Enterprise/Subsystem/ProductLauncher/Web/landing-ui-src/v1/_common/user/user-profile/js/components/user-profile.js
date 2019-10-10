//  Product Item Roles Access Component

(function (angular) {
    "use strict";

    function UserProfileComponent($location,pubsub, userProfileModel) {
        var ctrl = this;

        ctrl.$onChanges = function ($evt) {
            if ($evt && $evt.data && $evt.data.currentValue) {
                ctrl.userSummary = $evt.data.currentValue.userSummary;
            }
        };

        ctrl.$onDestroy = function () {
            ctrl.userSummary = undefined;
            ctrl = undefined;
        };

        ctrl.manageProfile = function () {
             $location.path("#/user/" + ctrl.userSummary.realPageId + "/edit").replace();
        };
    }

    angular
        .module("settings")
        .component("gbUserProfile", {
            templateUrl: "common/user/user-profile/templates/user-profile.html",
            bindings: {
                data: "<"
            },
            controller: [
                "$location",
                "pubsub",
                "userProfileModel",
                UserProfileComponent
            ]
        });

})(angular);
