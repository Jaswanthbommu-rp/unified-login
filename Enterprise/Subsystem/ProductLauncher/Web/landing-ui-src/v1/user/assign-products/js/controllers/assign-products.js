//  Assign Products Controller

(function (angular, undefined) {
    "use strict";

    function AssignProductsCtrl($scope, $params, model, svc, productAccess, panels, userStatus, pubsub, security, persona, aoStatus) {
        var vm = this;

        vm.init = function () {
            vm.security = security;
            vm.model = model;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(model.isActive.bind(model), vm.loadData);
            vm.userStatusWatch = userStatus.subscribe(vm.onUserStatusChange);
            vm.userAssignedProductsWatch = pubsub.subscribe("user.assignedproducts", vm.onGetData);
        };

        vm.loadData = function (active) {
            var realPageId;
            if (!model.adminResetFlag) {
                realPageId = $params.realPageId;
            }
            if (active) {
                if (model.isEmpty()) {
                    var reqData = {
                        personRealPageId: realPageId,
                        accessFilter: "UserDetails",
                        loginName: userStatus.getLoginName()
                    };

                    svc.get(reqData, vm.onGetData);
                }
                else {
                    vm.onDataReady();
                }
            }
        };

        vm.onGetData = function (resp) {
            if (model.isEmpty()) {
                var soln = model.setData(resp.data);
            }
            vm.setAOProductAccess(resp.data);
            vm.onDataReady();
        };

        vm.onDataReady = function () {
            if (!userStatus.isSuperUser()) {
                var soln = model.getDefaultSoln();
                vm.selectSoln(soln);
            }
        };

        vm.onUserStatusChange = function () {
            if (userStatus.isSuperUser()) {
                model.reset();
            }
            pubsub.publish("productpanel.userTypeChanged");

            if (model.adminResetFlag && (userStatus.statusId === 401 || userStatus.statusId === 405)) {
                if (!angular.equals({}, model.data)) {
                    logc("model data`", model);
                    model.families.forEach(function (family) {
                        family.selectAll.selected = false;
                    });

                    model.data.forEach(function (family) {
                        family.solutions.forEach(function (soln) {
                            soln.isAssigned = false;
                            if (soln.solutionId === 503 && soln.productId === 3) {
                                soln.isAssigned = true;
                            }
                        });
                    });
                }
            }

            // UnCheck the products which do not support  RegularUser NoEmail
            if (userStatus.statusId === 404) {
                if (!angular.equals({}, model.data)) {

                    model.data.forEach(function (family) {
                        if (family.familyId === 200 || family.familyId === 300 || family.familyId === 400 || family.familyId === 500) {
                            family.solutions.forEach(function (soln) {
                                if (soln.solutionId === 310 && soln.productId === 47) {
                                    soln.isAssigned = false;
                                }
                                if (soln.solutionId === 501 && soln.productId === 14) {
                                    soln.isAssigned = false;
                                }
                                // if ( (soln.solutionId === 402 && soln.productId === 29) || (soln.solutionId === 404 && soln.productId === 31) || (soln.solutionId === 403 && soln.productId === 30) || (soln.solutionId === 401 && soln.productId === 32) )  {
                                //     soln.isAssigned = false;
                                // }
                                if (soln.solutionId === 206 && soln.productId === 48) {
                                    soln.isAssigned = false;
                                }
                            });
                        }
                    });

                }


                // UnCheck the selectAll
                model.families.forEach(function (family) {
                    if (family.familyId === 200 || family.data.familyId === 300 || family.data.familyId === 400 || family.data.familyId === 500) {
                        family.solutions.forEach(function (soln) {
                            if (soln.data.solutionId === 310 && soln.data.productId === 47) {
                                family.selectAll.selected = false;
                                model.selectSoln(soln);
                                // Remove validation for notification email
                                pubsub.publish("settings.noEmailValidationUpdate");
                            }
                            if (soln.data.solutionId === 501 && soln.data.productId === 14) {
                                family.selectAll.selected = false;
                                model.selectSoln(soln);
                            }
                            if (soln.data.solutionId === 402 && soln.data.productId === 29 && userStatus.isExternalUser()) {
                                family.selectAll.selected = false;
                                model.selectSoln(soln);
                            }
                            if (soln.data.solutionId === 206 && soln.data.productId === 48) {
                                logc("payments");
                                family.selectAll.selected = false;
                                model.selectSoln(soln);
                                pubsub.publish("settings.noEmailValidationUpdate");
                            }
                        });
                    }
                });
            }

            if (userStatus.isExternalUser() && !angular.equals({}, model.data)) {
                model.data.forEach(function (family) {
                    if (family.familyId === 400) {
                        family.solutions.forEach(function (soln) {
                            if (soln.solutionId === 402 && soln.productId === 29) {
                                soln.isAssigned = false;
                                soln.disabled = true;
                            }
                        });
                    }
                });
            }
        };

        vm.isFamilyAdministration = function (familyId) {
            return familyId === 500;
        };

        vm.isDisabled = function (soln) {
            //Disable if user has viewuser right
            if (security.isAllowed("viewUser")) {
                return true;
            }

            if (soln.data.lockOnProductAccess) {
                return true;
            }

            if (soln.data.productNotAvailableForRegularUserNoEmail) {
                pubsub.publish("pa.regUserNoEmailNotAllowed", !userStatus.loginNameIsEmail());
                soln.data.disabled = !userStatus.loginNameIsEmail();
                return !userStatus.loginNameIsEmail();
            }

            if (soln.data.solutionId === 402 && soln.data.productId === 29 && userStatus.isExternalUser()) {
                return true;
            }

            return false;
        };

        vm.setAOProductAccess = function (data) {
            if (data) {
                data.forEach(function (family) {
                    if (family.familyId === 400) {
                        family.solutions.forEach(function (soln) {
                            if (userStatus.isExternalUser()) {
                                if (soln.solutionId === 402 && soln.productId === 29) {
                                    soln.isAssigned = false;
                                }
                            }

                            if (soln.products === "Benchmarking" && soln.solutionId === 403) {
                                aoStatus.setBenchmarkProductAccess(true);
                            }

                            if (userStatus.isRegularUserNoEmail()) {
                                pubsub.publish("ao.regUserNoEmailNotAllowed", soln.productNotAvailableForRegularUserNoEmail);
                            }
                        });
                    }
                });
            }
        };


        vm.selectSoln = function (soln) {
            panels.initState();
            model.selectSoln(soln);
            productAccess.selectSoln(soln);
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.userAssignedProductsWatch();
            model.reset();
            aoStatus.reset();
            vm.userStatusWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("AssignProductsCtrl", [
            "$scope",
            "$stateParams",
            "assignProductsModel",
            "assignProductsSvc",
            "assignProductAccessModel",
            "familyAccordionPanels",
            "userStatusModel",
            "pubsub",
            "routeSecurity",
            "personaDetails",
            "aOStatusModel",
            AssignProductsCtrl
        ]);
})(angular);
