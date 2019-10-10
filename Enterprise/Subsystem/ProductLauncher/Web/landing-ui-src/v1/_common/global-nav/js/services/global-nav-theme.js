//  Global Nav Theme Service

(function (angular, undefined) {
    "use strict";

    function GlobalNavThemeSvc($resource, ENV, pubsub, userModel) {
        var svc = this,
            partyID = "",
            url = ENV.landingAPI + "api/configurationsettings",
            actions,
            params = {},
            currentUserSetting = {};

        actions = {
            get: {
                method: "GET",
                cancellable: true
            },
            save: {
                method: "PUT",
                cancellable: true
            }
        };

        var userPrefSvc = $resource(url, params, actions);

        svc.initComplete = false;
        svc.getSettingComplete = false;

        svc.init = function () {
            if (userModel.isReady()) {
                svc.onUserModelReady();
                svc.userModelnWatch = angular.noop;
            }
            else {
                svc.userModelWatch = userModel.subscribe(svc.onUserModelReady);
            }
        };

        svc.onUserModelReady = function () {
            params.PartyId = userModel.getPartyId();
            if (!svc.initComplete && !svc.getSettingComplete && params.PartyId) {
                svc.getSettingComplete = true;
                pubsub.subscribe("gn.themeUpdate", svc.savePref);
                userPrefSvc.get(params, svc.onGetData);
            }
        };

        svc.onGetData = function (resp) {
            var themeColor = "";
            for (var i = 0; i < resp.data.length; i++) {
                if (resp.data[i].settingName.toLowerCase() == "themecolor") {
                    currentUserSetting = resp.data[i];
                    themeColor = resp.data[i].value.toLowerCase();
                    break;
                }
            }
            pubsub.publish("gn.themeInit", {
                dark: themeColor == "dark"
            });
            svc.initComplete = true;
        };

        svc.savePref = function (data) {
            if (svc.initComplete) {
                currentUserSetting.value = (data.dark) ? "dark" : "light";
                userPrefSvc.save(currentUserSetting);
            }
        };
    }

    angular
        .module("settings")
        .service("globalNavThemeSvc", [
            "$resource",
            "ENV",
            "pubsub",
            "userSessionModel",
            GlobalNavThemeSvc
        ]);
})(angular);
