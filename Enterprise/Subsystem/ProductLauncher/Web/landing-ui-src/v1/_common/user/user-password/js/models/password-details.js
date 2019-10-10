(function (angular) {
    "use strict";

    function factory($q, eventStream, userDetailsSvc, userModel) {
        var model = {};

        model.events = {
            update: eventStream()
        };

        model.getUpdateEvent = function () {
            return model.events.update;
        };

        model.load = function () {
            return model.forceLoad();
        };

        model.forceLoad = function () {
            if (!userModel) {
                userDetailsSvc.getUserProfile()
                    .then(model.setUserData);
            }
            else{
                model.setUserData(userModel.getData());
            }
        };

        model.setUserData = function (userData) {
            if (model.userData || userData) {
                if (model.userData) {
                    return model.userData;
                }
                if (userData) {
                    userModel.setData(userData.data);
                    model.events.update.publish(userData.data);
                    model.userData = userData.data;
                }
            }
            else {
                return false;
            }
        };

        return model;
    }

    angular
        .module("settings")
        .factory("passwordDetails", [
            "$q",
            "eventStream",
            "userDetailsSvc",
            "userSessionModel",
            factory
        ]);
})(angular);
