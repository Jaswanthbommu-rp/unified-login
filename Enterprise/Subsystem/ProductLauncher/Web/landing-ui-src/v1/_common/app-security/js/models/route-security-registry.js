//  Route Security Data

(function (angular, undefined) {
    "use strict";

    function factory() {
        var registry = {
            people: {
                routeId: "Userslist",
                exp: "^/people/users$",
                routeData: {
                    editUser: ["Edit User"],
                    cloneUser: ["Clone User"],
                    createUser: ["Create User"],
                    lockUnlockUser: ["Lock/Unlock User"],
                    viewUser: ["View User"],
                    activatedeactivateUser: ["Activate Deactivate User"],
                    resendInvitation: ["Resend Invitation"],
                    importUsers: ["Import users"]

                },
            },
            activityLog: {
                routeId: "ActivityLog",
                // routeId: "Userslist",
                exp: "^/people/activity-log",
                routeData: {
                    viewActivity: ["View Audit Trail"],
                    // viewActivity: ["View User"],
                }
            },
            editUser: {
                routeId: "EditUser",
                exp: "^/user/[a-z0-9\-]+/[a-z]+/edit$",
                routeData: {
                    editSelf: ["Edit Own Profile"],
                    editOther: ["Edit Other User Profile"],
                    editUser: ["Edit User"],
                    viewUser: ["View User"],
                    viewActivity: ["View Audit Trail"],
                    viewAuditTrailUserData: ["View Audit Trail User Data"],
                    editPassWord: ["Edit Password"],
                    activatedeactivateUser: ["Activate Deactivate User"]
                }
            },
             cloneUser: {
                routeId: "CloneUser",
                exp: "^/user/[a-z0-9\-]+/clone$",
                routeData: {
                    editUser: ["Edit User"]
                }
            },
            addUser: {
                routeId: "AddUser",
                exp: "^/user/add$",
                routeData: {
                    addUser: ["Create User"]
                }
            },
            viewRoles: {
                routeId: "RolesAndRights",
                exp: "^/roles-and-rights/roles$",
                routeData: {
                    manageRoleRight: ["Manage Role Right"],
                    viewRoleRight: ["View Role Right"]
                }
            },
            viewRights: {
                routeId: "RolesAndRights",
                exp: "^/roles-and-rights/rights$",
                routeData: {
                    viewRights: ["View Rights"]
                }
            },
            viewSupportTool: {
                routeId: "SupportTool",
                exp: "^/employee-access$",
                routeData: {
                    viewUnifiedPlatform: ["Access to Unified Platform"],
                    // viewUnifiedSettings: ["Access to Unified Settings"],
                    viewonlysupporttoolaccess: ["View Only Support Tool Access"]
                }
            },

        };

        Object.freeze(registry);

        return registry;
    }

    angular
        .module("settings")
        .factory("routeSecurityRegistry", [factory]);
})(angular);
