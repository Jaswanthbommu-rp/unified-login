//  Tabs  Config
// Products commenented will show Coming Soon on UI
(function(angular, undefined) {
    "use strict";

    function ProductFamiliesData($filter) {

        var data,
            svc = this;

        svc.data = data = [{
                familyId: 100,
                familyName: "property-management",

                solutions: [{
                        solutionId: 101,
                        solutionName: "OneSite"
                    },
                    {
                        solutionId: 102,
                        solutionName: "realpage-accounting"
                    },
                    {
                        solutionId: 104,
                        solutionName: "spend-management"
                    },
                    // {
                    //     solutionId: 105,
                    //     solutionName: "vendor-services"
                    // }
                ]
            },
            {
                familyId: 200,
                familyName: "resident-services",
                solutions: [
                    // {
                    //     solutionId: 201,
                    //     solutionName: "active-building"
                    // },
                    // {
                    //     solutionId: 205,
                    //     solutionName: "utility-management"
                    // }
                ]
            },
            {
                familyId: 300,
                familyName: "lease-management",
                solutions: [
                    // {
                    //     solutionId: 305,
                    //     solutionName: "lead2lease"
                    // },
                    // {
                    //     solutionId: 302,
                    //     solutionName: "prospect-contact-center"
                    // },
                    // {
                    //     solutionId: 303,
                    //     solutionName: "marketing center"
                    // },
                    // {
                    //     solutionId: 306,
                    //     solutionName: "omni-channel"
                    // }
                ]
            },
            {
                familyId: 400,
                familyName: "asset-optimization",
                solutions: [
                    // {
                    //     solutionId: 406,
                    //     solutionName: "asset-optimization"
                    // },
                    // {
                    //     solutionId: 401,
                    //     solutionName: "yieldstar"
                    // }
                ]
            },
            {
                familyId: 500,
                familyName: "administration",
                solutions: [
                    {
                        solutionId: 503,
                        solutionName: "unified-login"
                    },
                    // {
                    //     solutionId: 501,
                    //     solutionName: "client-portal"
                    // }
                ]
            },
            // {
            //     familyId: 600,
            //     familyName: "internal-tools",
            //     solutions: [
            //         {
            //             solutionId: 601,
            //             solutionName: "research-application"
            //         }
            //     ]
            // }
        ];


        svc.getData = function() {
            return data;
        };


        svc.getFamilyName = function(id) {
            var family;
            angular.forEach(svc.data, function(item) {
                if (item.familyId == id) {
                    family = item.familyName;
                }
            });
            return family;
        };

        svc.getSolutionName = function(familyid, id) {
            var solution;
            angular.forEach(svc.data, function(item) {
                if (item.familyId == familyid) {
                    angular.forEach(item.solutions, function(sol) {
                        if (sol.solutionId == id) {
                            solution = sol.solutionName;
                        }
                    });
                }
            });
            return solution;
        };

    }

    angular
        .module("settings")
        .service("productFamiliesData", [
            "$filter",
            ProductFamiliesData
        ]);
})(angular);
