//  Lazy Load Config

(function (angular) {
    "use strict";

    function config(resolveModule) {
        var modules, appConfig,
            appName = "settings";

        modules = {
            "user.base": [],
            "user.add": ["js"],
            "user.edit": ["js"],
            "user.clone": ["js"],
            "user.user-profile": [],
            "user.user-details": [],
            "user.password-requirements": ["css", "js"],
            "user.product-access": ["css", "js"],
            "user.reset-password": [],
            "user.assign-products": [],
            "user.assign-product-access.bundle": [],
            "user.security-questions": [],
            "user.user-preference": ["css","js"],

            "error.base": ["css", "js"],

            "common.user-image": ["css", "js"],
            "common.notification-lang": ["js", "lang"],
            "common.user.user-profile": [],
            "common.user.user-password": ["js"],
            "common.security-questions": ["js"],

            "home.base": ["css", "js", "lang"],
            "home.user-resources": ["css", "js", "lang"],

            "manage-profile.bundle": [],

            "access-easy-lms.bundle": ["css","js"],

            "access-learning-portal.bundle": ["css","js"],

            "people.activity-log": ["css", "js"],
            "people.users.base": ["css", "js", "lang"],
            "people.change-password": ["css", "js", "lang"],
            "people.user-details": ["css", "js"],

            "people.user.base": [],
            "people.user.persona.base": [],
            "people.user.persona.add-products": [],
            "people.user.persona.add-products.onesite": ["css", "js"],
            "people.user.persona.add-products.marketing-center": ["css", "js"],
            "people.user.persona.add-products.accounting": ["css", "js"],
            "people.user.persona.add-products.spend-management": ["css", "js"],
            "people.user.persona.add-products.vendor-compliance": ["css", "js"],
            "people.user.persona-navigation": ["js", "lang"],
            "people.user.user-profile": [],

            "people.user.add": [],
            "people.user.edit": [],
            "people.user.view": ["css", "js"],

            "people.user.product-properties": ["css", "js"],
            "people.user.product-roles": ["css", "js"],

            "products.base": ["css", "js", "lang"],
            "products.favorites": ["js"],
            "product.base": ["css", "js"],
            "product.lrc": ["css", "js"],
            "product.lrc.settings": ["css", "js"],

            "roles-and-rights.base": ["css", "js"],
            "roles-and-rights.rights.bundle": ["css", "js"],
            "roles-and-rights.roles.bundle": ["css", "js"],
            "roles-and-rights.enterprise-roles.bundle": ["css", "js"],

            "employee-access.base": ["css", "js"],
        };

        appConfig = {
            appName: appName,
            modules: modules,
            basePath: ""
        };

        resolveModule
            .setLazyLoad(appName, appConfig);
    }

    angular
        .module("settings")
        .config(["rpResolveModuleProvider", config]);
})(angular);
