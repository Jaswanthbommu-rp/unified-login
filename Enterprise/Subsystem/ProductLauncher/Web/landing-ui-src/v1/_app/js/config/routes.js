//  Configure Routes

(function (angular) {
    "use strict";

    function files(list) {
        var baseFiles = [
            "settings.common.notification-lang",
             "settings.manage-profile.bundle"
        ];

        return baseFiles.concat(list || []);
    }

    function config(RoutesProvider) {
        var routes = {};

        routes["home"] = {
            url: "/",
            controller: "HomeCtrl as page",
            resolve: ["appLayout", "checkUserStatus"],
            lazyLoad: [{
                rerun: true,

                serie: true,

                files: files([
                    "lib.realpage.popover",
                    "lib.realpage.carousel",
                    "settings.common.user-image",
                    "settings.home.base",
                    "settings.access-learning-portal.bundle",
                    "settings.access-easy-lms.bundle"
                ])
            }]
        };

        routes["user"] = {
            abstract: true,
            url: "/user",
            resolve: ["security", "appLayout", "checkUserStatus"],
            lazyLoad: [{
                rerun: true,

                serie: true,

                files: files([
                    "lib.realpage.select-all",
                    "lib.realpage.popover",
                    "settings.user.base",
                    "settings.user.password-requirements",
                    "settings.user.user-details",
                    "settings.user.user-profile",
                    "settings.user.product-access",
                    "settings.user.reset-password",
                    "settings.user.security-questions",
                    "settings.user.assign-products",
                    "settings.user.assign-product-access.bundle"
                ])
            }]
        };

        routes["user.add"] = {
            url: "/add",
            controller: "AddUserCtrl as addUser",
            lazyLoad: [{
                rerun: true,

                serie: true,

                files: [
                    "settings.user.add",

                    "settings.common.user-image"
                ]
            }]
        };

        routes["user.edit"] = {
            url: "/:realPageId/:link/edit",
            controller: "EditUserCtrl as editUser",
            lazyLoad: [{
                rerun: true,

                serie: true,

                files: [
                   // "settings.common.security-questions",
                    "settings.user.edit",

                    "settings.common.user-image",
                ]
            }]
        };

        routes["user.clone"] = {
            url: "/:realPageId/clone",
            controller: "CloneUserCtrl as cloneUser",
            lazyLoad: [{
                rerun: true,

                serie: true,

                files: [
                    "settings.user.clone"
                ]
            }]
        };

        routes["people"] = {
            url: "/people",
            abstract: true,
            resolve: ["appLayout"],
            lazyLoad: [{
                rerun: true,

                serie: true,

                files: files([
                    "lib.moment.timezone"
                ])
            }]
        };

        routes["people.users"] = {
            url: "/users",
            controller: "PeopleUsersCtrl as page",
            resolve: ["security", "checkUserStatus"],
            lazyLoad: [{ //grid dependencies
                serie: true,
                rerun: true,
                files: [
                    "lib.realpage.form-common",
                    "lib.realpage.grid-controls",
                    "settings.common.user-image",
                    "settings.people.users.base"
                ]
            }]
        };

        routes["people.activity-log"] = {
            url: "/activity-log",
            controller: "AllActivityLogCtrl as al",
            resolve: ["security", "checkUserStatus"],
            lazyLoad: [{
                serie: true,
                rerun: true,
                files: [
                    "lib.realpage.form-common",
                    "settings.common.user-image",
                    "settings.people.activity-log"
                ]
            }]
        };

        routes["people.change-password"] = {
            url: "/users/:userId/change-password",
            controller: "ChangePwdController as page",
            lazyLoad: [{
                files: [
                    // "settings.common.user.user-password",
                    "settings.people.change-password"
                ]
            }]
        };

        routes["products"] = {
            url: "/products",
            abstract: true,
            controller: "ProductsCtrl as page",
            resolve: ["appLayout", "checkUserStatus"],
            lazyLoad: [{
                serie: true,

                rerun: true,

                files: files([
                    "settings.products.base"
                ])
            }]
        };

        routes["products.all"] = {
            url: ""
        };

        routes["products.favorites"] = {
            url: "/favorites"
        };

        routes["product"] = {
            url: "/product",
            abstract: true,
            controller: "ProductCtrl as page",
            resolve: ["appLayout", "checkUserStatus"],
            lazyLoad: [{
                rerun: true,

                serie: true,

                files: files([
                    "settings.product.base"
                ])
            }]
        };

        var products = [
            "lrc"
        ];

        products.forEach(function (product) {
            var name = product.camelize();

            routes["product." + product] = {
                url: "/" + product,
                abstract: true,
                controller: name.ucfirst() + "Ctrl as page",
                lazyLoad: [{
                    rerun: true,

                    files: [
                        "settings.product." + product
                    ]
                }]
            };
        });

        routes["product.lrc.settings"] = {
            url: "",
            controller: "LrcSettingsCtrl as page",
            lazyLoad: [{
                files: [
                    "settings.product.lrc.settings"
                ]
            }]
        };

        routes["product.lrc.settings.resident-management"] = {
            url: "/settings/resident-management"
        };

        routes["product.lrc.settings.resident-management.general"] = {
            url: "/general"
        };

        routes["product.lrc.users"] = {
            url: "/users"
        };

        routes["product.lrc.activity"] = {
            url: "/activity"
        };

        routes["employee-access"] = {
            url: "/employee-access",
            //abstract: true,
            resolve: ["appLayout", "security", "checkUserStatus"],
            // controller: "EmployeeAccessCtrl as page",
            lazyLoad: [{
                serie: true,
                rerun: true,
                files: [
                    "settings.employee-access.base",
                    "settings.user.base"
                ]
            }]
        };

        routes["roles-and-rights"] = {
            url: "/roles-and-rights",
            abstract: true,
            resolve: ["appLayout", "checkUserStatus", "security"],
            controller: "RolesRightsCtrl as page",
            lazyLoad: [{
                serie: true,
                rerun: true,
                files: [
                    "lib.realpage.complex-grid",
                    "lib.realpage.complex-grid-pagination",
                    "settings.roles-and-rights.base",
                    "settings.user.base"
                ]
            }]
        };


        routes["roles-and-rights.rights"] = {
            url: "/rights",
            lazyLoad: [{
                serie: true,
                rerun: true,
                files: [
                    "settings.roles-and-rights.rights.bundle"
                ]
            }]
        };

        routes["roles-and-rights.roles"] = {
            url: "/roles",
            lazyLoad: [{
                serie: true,
                rerun: true,
                files: [
                    "settings.roles-and-rights.roles.bundle"
                ]
            }]
        };

        routes["roles-and-rights.enterprise-roles"] = {
            url: "/enterprise-roles",
            lazyLoad: [{
                serie: true,
                rerun: true,
                files: [
                    "settings.roles-and-rights.enterprise-roles.bundle"
                ]
            }]
        };

        routes["error"] = {
            url: "/error",
            abstract: true,
            controller: "ErrorCtrl as page",
            lazyLoad: [{
                files: files([
                    "settings.error.base"
                ])
            }]
        };

        routes["error.access-denied"] = {
            url: "/access-denied"
        };

        //RoutesProvider.setTemplateUrlPrefix("settings/").setRoutes(routes).setDefault("/");
		RoutesProvider.setRoutes(routes).setDefault("/");
    }

    angular
        .module("settings")
        .config(["rpRoutesProvider", config]);
})(angular);
