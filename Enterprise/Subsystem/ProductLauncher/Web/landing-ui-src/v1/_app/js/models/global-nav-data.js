//Global Navigation Data

(function (angular) {
    "use strict";

    var cnst = {
        "dashboard": {
            title: "Home",
            pageId: "home",
            url: "/home",
            icon: "places-home-1"
        },
        "people": {
            title: "People",
            icon: "user",
            pageId: "people",
            submenu: {
                classname: "menu-3",
                items: [
                    {
                        linkId: "people",
                        title: "Users",
                        url: "../people/users"
                    },
                    {
                        linkId: "activity log",
                        title: "User Activity Log",
                        url: "../people/activity-log"
                    }
                ]
            }
        },
        "roles and rights": {
            title: "Roles & Rights",
            pageId: "roles-and-rights",
            url: "#/roles-and-rights/roles",
            icon: "key-1"
        },
    };

    Object.freeze(cnst);


    angular
        .module("settings")
        .constant("globalNavData", cnst);
})(angular);
