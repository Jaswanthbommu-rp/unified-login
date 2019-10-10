// Location Service Config

(function (angular) {
    "use strict";

    function config(prov, ENV) {
        // var helpUrl = ENV.helpUrl;
        var helpUrl = ENV.helpUrl;

        var data = {
            home: {
                vr: "40",
                pg: "ul-home",
                scrVer: "350",
                helpUrl: helpUrl,
                // cs: ""
            },
            "people.users": {
                vr: "40",
                pg: "ul-userlist",
                scrVer: "350",
                helpUrl: helpUrl,
                // cs: ""
            },
            "user.add": {
                vr: "40",
                pg: "ul-addUser",
                scrVer: "350",
                helpUrl: helpUrl,
                context: {
                    productAccess: {
                        pg: "ul-productAccess"
                    }
                }
            },
            "user.edit": {
                vr: "40",
                pg: "ul-editUser",
                scrVer: "350",
                helpUrl: helpUrl,
                context: {
                    productAccess: {
                        pg: "ul-productAccess"
                    },
                    securityQuestions: {
                        pg: "ul-securityQuestions"
                    },
                    password: {
                        pg: "ul-password"
                    },
                    activityLog: {
                        pg: "ul-activity"
                    }
                }
            },
            "roles-and-rights.roles": {
                vr: "40",
                pg: "ul-roles",
                scrVer: "350",
                helpUrl: helpUrl,
                // cs: ""
            },
            "roles-and-rights.rights": {
                vr: "40",
                pg: "ul-rights",
                scrVer: "350",
                helpUrl: helpUrl,
                // cs: ""
            }
        };

        prov.setData(data);
    }

    angular
        .module("settings")
        .config(["rpGhHelpDataProvider", "ENV", config]);
})(angular);
