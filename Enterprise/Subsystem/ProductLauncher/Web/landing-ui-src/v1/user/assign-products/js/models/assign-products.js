//  Assign Products Model

(function (angular, undefined) {
    "use strict";

    function factory(familyModel, formConfig) {
        function AssignProductsModel() {
            var s = this;
            s.init();
        }

        var p = AssignProductsModel.prototype;

        p.init = function () {
            var s = this;
            s.data = {};
            s.families = [];
            s.active = false;
            s.notificationEmailReqd = false;
            s.adminResetFlag = false;
        };

        // Getters

        p.getSolns = function () {
            var s = this,
                solns = [];

            s.families.forEach(function (family) {
                solns = solns.concat(family.getSolns());
            });

            return solns;
        };

        p.getAssignedSolns = function () {
            var s = this,
                solns = [];

            s.families.forEach(function (family) {
                solns = solns.concat(family.getAssignedSolns());
            });

            return solns;
        };

        p.getDefaultSoln = function () {
            var s = this;
            var fam = s.getAdminFam();
            return s.getAdminSol(fam);
        };

        p.getAdminFam = function () {
            var s = this;
            var adminFam;
            s.families.forEach(function (fam) {
                if (fam.data.familyId === 500) {
                    adminFam = fam;
                }
            });
            return adminFam === undefined ? s.families.first() : adminFam;
        };

        p.getAdminSol = function (fam) {
            var s = this;
            var adminSol;
            fam.solutions.forEach(function (sol) {
                if (sol.data.solutionId === 503) {
                    adminSol = sol;
                }
            });
            return adminSol === undefined ? s.getSolns().first() : adminSol;
        };

        // Setters

        p.setActive = function (bool) {
            var s = this;
            s.active = bool === undefined ? true : bool;
            return s;
        };

        p.setData = function (data) {
            var s = this;
            var adminData;
            s.data = data || [];

            data.forEach(function (famData) {
                if (famData.familyId === 500) {
                    adminData = famData;
                }
                else {
                    s.families.push(familyModel(famData));
                }
            });

            s.families.push(familyModel(adminData));

            return s;
        };

        p.setAdminResetFlag = function(bool)  {
            var s = this;
            s.adminResetFlag = bool;
        };
        
        // Assertions

        p.isActive = function () {
            var s = this;
            return s.active;
        };

        // Actions

        p.findSolnById = function (id) {
            var soln,
                s = this;

            s.families.forEach(function (family) {
                if (!soln) {
                    soln = family.findSolnById(id);
                }
            });

            return soln;
        };

        p.isEmpty = function () {
            var s = this;
            return s.families.length === 0;
        };

        p.selectSoln = function (soln) {
            var s = this;
            // Check to see if the selected product requires a notification email address for a regular user (no email)
            
            if (soln.data.notificationEmailRequiredForUserWithNoEmail)
            {
                formConfig.setNotificationEmailRequired(soln.isAssigned(), soln.getTitleText());
                s.notificationEmailReqd = soln.isAssigned();
            }

            s.families.forEach(function (family) {
                family.selectSoln(soln);
            });

            return s;
        };

        p.destroyFamily = function (family) {
            family.destroy();
        };

        p.reset = function () {
            var s = this;
            s.data = {};
            s.active = false;
            s.notificationEmailReqd = false;
            s.adminResetFlag = false;
            formConfig.clearNotificationEmail();
            s.families.forEach(s.destroyFamily);
            s.families.flush();
        };

        return new AssignProductsModel();
    }

    angular
        .module("settings")
        .factory("assignProductsModel", [
            "assignProductFamilyModel",
            "userDetailsFormConfig",
            factory
        ]);
})(angular);
