//  Product Access Model

(function (angular, undefined) {
    "use strict";

    function factory($params, products, productAccess, session, security, templateModel) {
        function ProductAccessModel() {
            var s = this;
            s.init();
        }

        var p = ProductAccessModel.prototype;

        p.init = function () {
            var s = this;
            s.incompleteSolutionsList = [];
        };

        // Getters

        p.getData = function () {
            var s = this,
                newUser = !s.isExistingUser();

            return s[newUser ? "getNewUserData" : "getExistingUserData"]();
        };

        p.getNewUserData = function () {
            var s = this,
                data = {},
                solns = products.getAssignedSolns();

            data.productBatch = [];

            solns.forEach(function (soln) {
                var key = soln.getKey(),
                    productId = soln.data.productId,
                    isDisabled = soln.isProductDisabled(),
                    prodData = productAccess.getAccessData(key, productId);
                logc("getAccessData", prodData, key, productId);
                if (prodData && !isDisabled) {
                    if (angular.isArray(prodData) && prodData.length > 0) {
                        //For AO Prduct family loop through the company data
                        prodData.forEach(function (batch) {
                            data.productBatch.push(batch);
                        });
                    }
                    else {
                        data.productBatch.push(prodData);
                    }
                }
            });

            return data;
        };

        p.getExistingUserData = function () {
            var s = this,
                data = {},
                solns = products.getSolns();

            data.productBatch = [];

            solns.forEach(function (soln) {
                var prodData,
                    key = soln.getKey(),
                    isAssigned = soln.isAssigned(),
                    isDisabled = soln.isProductDisabled(),
                    productId = soln.data.productId,
                    touched = soln.wasTouched(),
                    assignmentChanged = soln.assignmentChanged(),
                    accessChanged = productAccess.accessChanged(key);
                logc("getAccessData", prodData, key, productId);
                // logc("touched", touched);
                // logc("assignmentChanged", assignmentChanged);
                // logc("accessChanged", accessChanged);
                if (assignmentChanged && !isDisabled) {
                    if (isAssigned) {
                        prodData = productAccess.getAccessData(key, productId);
                        logc("prodData1", prodData, productId);
                        if (angular.isArray(prodData) && prodData.length > 0) {
                            prodData.forEach(function (data) {
                                data.inputJson.isAssigned = isAssigned;
                            });
                        }
                        else {
                            prodData.inputJson.isAssigned = isAssigned;
                        }

                    }
                    else {
                        prodData = {
                            inputJson: {
                                isAssigned: false,
                            },

                            productId: soln.getProductId()
                        };
                    }

                    //data.productBatch.push(prodData);
                    if (prodData) {
                        if (angular.isArray(prodData) && prodData.length > 0) {
                            //For AO Prduct family loop through the company data
                            prodData.forEach(function (batch) {
                                data.productBatch.push(batch);
                            });
                        }
                        else {
                            data.productBatch.push(prodData);
                        }
                    }
                }
                else if (templateModel.isProductExists(productId)) {
                    if (isAssigned && touched && !isDisabled) {
                        prodData = productAccess.getAccessData(key, productId);
                        logc("prodData2", prodData, productId);
                        //prodData.inputJson.isAssigned = isAssigned;
                        //data.productBatch.push(prodData);
                        if (angular.isArray(prodData) && prodData.length > 0) {
                            prodData.forEach(function (batch) {
                                batch.inputJson.isAssigned = isAssigned;
                                data.productBatch.push(batch);
                            });
                        }
                        else {
                            prodData.inputJson.isAssigned = isAssigned;
                            data.productBatch.push(prodData);
                        }
                    }
                }
                else if (isAssigned && accessChanged && !isDisabled) {
                    prodData = productAccess.getAccessData(key, productId);
                    if (angular.isArray(prodData) && prodData.length > 0) {
                        //For AO Prduct family loop through the company data
                        prodData.forEach(function (batch) {
                            data.productBatch.push(batch);
                        });
                    }
                    else {
                        data.productBatch.push(prodData);
                    }
                }
            });

            return data;
        };

        p.getIncompleteSolutionsList = function () {
            var s = this;
            return s.incompleteSolutionsList;
        };

        // Setters
        p.editingSelf = function () {
            return session.getRealPageId() === $params.realPageId;
        };

        p.setActive = function () {
            var s = this;
            products.setActive();
            return s;
        };

        // Assertions

        p.isExistingUser = function () {
            var s = this;
            return !!$params.realPageId;
        };

        p.isValid = function () {
            var s = this,
                newUser = !s.isExistingUser();

            return s[newUser ? "isValidNewUser" : "isValidExistingUser"]();
        };

        p.isValidExistingUser = function () {
            var s = this,
                solns = products.getSolns();

            s.incompleteSolutionsList = [];

            if (!security.isAllowed("viewUser")) {
                solns.forEach(function (soln) {
                    var key = soln.getKey(),
                        touched = soln.wasTouched(),
                        isAssigned = soln.isAssigned(),
                        productId = soln.data.productId,
                        assignmentChanged = soln.assignmentChanged(),
                        missingData = productAccess.getAccessData(key, productId) === null;

                    if (missingData && isAssigned && (touched || assignmentChanged)) {
                        s.incompleteSolutionsList.push(soln);
                    }
                });
            }

            return s.incompleteSolutionsList.empty();
        };

        p.isValidNewUser = function () {
            var s = this,
                solns = products.getAssignedSolns();

            s.incompleteSolutionsList = [];

            solns.forEach(function (soln) {
                var key = soln.getKey(),
                    productId = soln.data.productId,
                    data = productAccess.getAccessData(key, productId);

                if (data === null) {
                    s.incompleteSolutionsList.push(soln);
                }
            });

            return s.incompleteSolutionsList.empty();
        };

        // Destroy/Reset

        p.reset = function () {
            var s = this;
            s.active = false;
            productAccess.reset();
            s.incompleteSolutionsList = [];
            products.setActive(false).reset();
        };

        return new ProductAccessModel();
    }

    angular
        .module("settings")
        .factory("productAccessModel", [
            "$stateParams",
            "assignProductsModel",
            "assignProductAccessModel",
            "userSessionModel",
            "routeSecurity",
            "productTemplateModel",
            factory
        ]);
})(angular);
