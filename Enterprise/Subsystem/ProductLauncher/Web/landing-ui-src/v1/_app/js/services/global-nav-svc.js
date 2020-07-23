//  Global Nav Service

(function (angular, undefined) {
    "use strict";

    function GlobalNavSvc($resource, ENV, navModel, navData, cookie, persona) {
        var svc = this,
            complete = false,
            personaReady = false,
            navListReady = false,
            settingsAdded = false;
        svc.personaWatch = angular.noop;


        svc.init = function () {
            if (!complete) {
                complete = true;
                settingsAdded = false;
                var res = $resource(ENV.landingAPI + "api/SideMenu/rights");
                res.get(svc.setData);
                svc.personaWatch();
                svc.personaWatch = persona.subscribe(svc.updatePersonaReady);
            }
        };

        svc.setData = function (response) {

            var navList = [],
                rights = response.data.rights,
                extLinkKeys = ["property", "company"];

            rights.forEach(function (item, key) {
                rights[key] = item.toLowerCase();
            });

            angular.forEach(navData, function (item, key) {
                if (rights.contains(key)) {
                    if (item.submenu) {
                        item = svc.updateSubmenu(item, rights);
                    }

                    navList.push(item);

                    if (extLinkKeys.contains(key)) {
                        svc.updateLink(item);
                    }

                }
            });

            navModel.setData(navList);
            svc.updateNavListReady();
        };

        svc.updatePersonaReady = function () {
            personaReady = true;
            svc.updateSettings();
        };

        svc.updateNavListReady = function () {
            navListReady = true;
            svc.updateSettings();
        };

        svc.updateSubmenu = function (item, rights) {
            var links = [];

            item.submenu.items.forEach(function (item) {
                if (rights.contains(item.linkId)) {
                    links.push(item);
                }
            });
            return angular.extend({}, item, {
                items: links
            });
        };

        svc.updateLink = function (data) {
            var token = cookie.read("access_token");
            data.labelLink = data.labelLink + "?token=" + token;
        };

        svc.updateSettings = function (data) {
            if (personaReady && navListReady) {
                svc.updateNotificationsLink();
                svc.updatePlatformAlertsLink();
                svc.updateSettingsLink();

                var leftNav = document.querySelector('omnibar-navigation');
                leftNav.items = navModel.data;
            }
        };

        svc.updateSettingsLink = function () {
            if (persona.isReady()) {
                var compName = persona.getCompanyName();
                var rpId = persona.getPersonaRealPageID();
                var compId = persona.getBooksMasterId();

                var setting = {
                    title: "Settings",
                    pageId: "settings",
                    icon: "cog-gear-settings",
                    items: [
                        {
                            linkId: "manage settings",
                            title: "Manage Settings",
                            url: "/settings"
                                },
                        {
                            linkId: "settings activity log",
                            title: "Settings Activity Log",
                            url: "/settings/activity-log"
                                }
                            ]
                };

                if (!(compId === -1 || compId === -2) && (persona.hasViewOnlySettingsAccess() || persona.hasManagePlatFormSecurity() || persona.hasManageCustomFields() || persona.hasManageUnifiedSettings())) {
                    if (navModel.data !== undefined && (navModel.data.length > 0) && !settingsAdded) {
                        if (persona.hasManageSettingsTemplates()) {

                            setting.items.splice(1, 0, {
                                linkId: "Manage Templates",
                                title: "Manage Templates",
                                url: "/settings/templates"
                            });
                        }

                        navModel.data.push(setting);
                        settingsAdded = true;
                        svc.personaWatch();
                    }
                }

                // Cog for Employee Company ONLY
                if ((compId === -1) && (persona.hasAccessSettingsAdmin())) {

                    if (navModel.data !== undefined && (navModel.data.length > 0) && !settingsAdded) {
                        var tmpl = {
                            linkId: "Manage Templates",
                            title: "Manage Templates",
                            url: "/settings/templates"
                        };
                        setting.items.push(tmpl);
                        navModel.data.push(setting);
                        settingsAdded = true;
                        svc.personaWatch();
                    }
                }
            }
        };

        svc.updateNotificationsLink = function () {

            var compName = persona.getCompanyName();
            var rpId = persona.getPersonaRealPageID();
            var compId = persona.getBooksMasterId();
            var notifications = {
                title: "Configurations",
                icon: "wrench-screwdriver",
                pageId: "Configurations",
                classname: "menu-3",
                items: [{
                    linkId: "Notifications",
                    title: "Notifications",
                    url: "/home/notifications/configuration"
                }]
            };

            if ((persona.hasnotificationsAccess())) {

                if (navModel.data !== undefined && (navModel.data.length > 0)) {

                    navModel.data.push(notifications);
                    svc.personaWatch();
                }
            }
        };

        svc.updatePlatformAlertsLink = function () {
            var platformAlerts = {
                title: "Platform Alerts",
                icon: "alarm-timeout",
                pageId: "platform-alerts",
                url: "/notifications/platformalerts"
            };

            if ((persona.hasPlatformAlertsAccess())) {
                if (navModel.data !== undefined && (navModel.data.length > 0)) {
                    navModel.data.push(platformAlerts);
                    svc.personaWatch();
                }
            }
        };

        svc.reset = function () {
            complete = false;
            return svc;
        };
    }

    angular
        .module("settings")
        .service("globalNavSvc", [
            "$resource",
            "ENV",
            "rpGlobalNavModel",
            "globalNavData",
            "rpCookie",
            "personaDetails",
            GlobalNavSvc
        ]);
})(angular);
