(function (angular, undefined) {
    "use strict";

    function RegisterProductModels(templateModel, acctModel, biModel, cpayModel, cpModel, cmsModel, diqModel, dmModel, elmsModel, ilmlaModel, ilmlmModel, intgmModel, iaModel, l2lModel, mkcModel, onsiteModel, onesiteModel, paModel, pmModel, pccModel, ppModel, riModel, raModel, rpModel, rmModel, spModel, smModel, uaModel, umModel, utmModel, vcModel) {
        var svc = this;

        svc.getProductModels = function () {
            var list = [];

            list.push({
                model: templateModel.isProductExists(1) ? ppModel : onesiteModel,
                key: "soln101",
                product: "1"
            });

            list.push({
                model: templateModel.isProductExists(3) ? ppModel : umModel,
                key: "soln503",
                product: "3"
            });

            list.push({
                model: templateModel.isProductExists(6) ? ppModel : l2lModel,
                key: "soln305",
                product: "6"
            });

            list.push({
                model: templateModel.isProductExists(8) ? ppModel : acctModel,
                key: "soln102",
                product: "8"
            });

            list.push({
                model: templateModel.isProductExists(9) ? ppModel : mkcModel,
                key: "soln303",
                product: "9"
            });

            list.push({
                model: templateModel.isProductExists(10) ? ppModel : pccModel,
                key: "soln302",
                product: "10"
            });


            list.push({
                model: templateModel.isProductExists(13) ? ppModel : smModel,
                key: "soln104",
                product: "13"
            });


            list.push({
                model: templateModel.isProductExists(14) ? ppModel : cpModel,
                key: "soln501",
                product: "14"
            });

            list.push({
                model: templateModel.isProductExists(15) ? ppModel : riModel,
                key: "soln204",
                product: "15"
            });

            list.push({
                model: templateModel.isProductExists(16) ? ppModel : vcModel,
                key: "soln105",
                product: "16"
            });

            list.push({
                model: templateModel.isProductExists(17) ? ppModel : rpModel,
                key: "soln201",
                product: "17"
            });


            list.push({
                model: templateModel.isProductExists(18) ? ppModel : utmModel,
                key: "soln205",
                product: "18"
            });

            list.push({
                model: templateModel.isProductExists(20) ? ppModel : dmModel,
                key: "soln110",
                product: "20"
            });


            list.push({
                model: templateModel.isProductExists(23) ? ppModel : onsiteModel,
                key: "soln307",
                product: "23"
            });


            list.push({
                model: templateModel.isProductExists(24) ? ppModel : raModel,
                key: "soln601",
                product: "24"
            });

            list.push({
                model: templateModel.isProductExists(25) ? ppModel : spModel,
                key: "soln504",
                product: "25"
            });

            list.push({
                model: templateModel.isProductExists(26) ? ppModel : uaModel,
                key: "soln107",
                product: "26"
            });

            list.push({
                model: templateModel.isProductExists(29) ? ppModel : biModel,
                key: "soln402",
                product: "29"
            });

            list.push({
                model: templateModel.isProductExists(30) ? ppModel : paModel,
                key: "soln403",
                product: "30"
            });


            list.push({
                model: templateModel.isProductExists(31) ? ppModel : iaModel,
                key: "soln404",
                product: "31"
            });

            list.push({
                model: templateModel.isProductExists(32) ? ppModel : rmModel,
                key: "soln401",
                product: "32"
            });

            list.push({
                model: templateModel.isProductExists(36) ? ppModel : elmsModel,
                key: "soln111",
                product: "36"
            });

            list.push({
                model: templateModel.isProductExists(39) ? ppModel : intgmModel,
                key: "soln505",
                product: "39"
            });

            list.push({
                model: templateModel.isProductExists(40) ? ppModel : ilmlmModel,
                key: "soln308",
                product: "40"
            });

            list.push({
                model: templateModel.isProductExists(41) ? ppModel : ilmlaModel,
                key: "soln309",
                product: "41"
            });

            list.push({
                model: templateModel.isProductExists(44) ? ppModel : pmModel,
                key: "soln701",
                product: "44"
            });

            list.push({
                model: templateModel.isProductExists(47) ? ppModel : diqModel,
                key: "soln310",
                product: "47"
            });

            list.push({
                model: templateModel.isProductExists(48) ? ppModel : cpModel,
                key: "soln206",
                product: "48"
            });

            if (templateModel.isProductExists(50)) {
                list.push({
                    model: ppModel,
                    key: "soln311",
                    product: "50"
                });

            }

            if (templateModel.isProductExists(51)) {
                list.push({
                    model: ppModel,
                    key: "soln407",
                    product: "51"
                });

            }

            if (templateModel.isProductExists(52)) {
                list.push({
                    model: ppModel,
                    key: "soln408",
                    product: "52"
                });
            }

            if (templateModel.isProductExists(53)) {
                list.push({
                    model: ppModel,
                    key: "soln409",
                    product: "53"
                });
            }

            if (templateModel.isProductExists(54)) {
                list.push({
                    model: ppModel,
                    key: "soln410",
                    product: "54"
                });
            }


            if (templateModel.isProductExists(55)) {
                list.push({
                    model: ppModel,
                    key: "soln112",
                    product: "55"
                });
            }

            list.push({
                model: cmsModel,
                key: "default"
            });

            return list;
        };
    }

    angular
        .module("settings")
        .service("registerProductModels", [
            "productTemplateModel",
            "AccountingDataModel",
            "businessIntelligenceDataModel",
            "clickPayProductAccessModel",
            "clientPortalDataModel",
            "comingSoonProductAccessModel",
            "depositAlternativeProductAccessModel",
            "documentManagementDataModel",
            "easyLMSProductAccessModel",
            "ilmLeadAnalyticsDataModel",
            "ilmLeadManagementDataModel",
            "integMktDataModel",
            "investmentAnalyticsDataModel",
            "lead2LeaseDataModel",
            "MarketingCenterDataModel",
            "onSiteDataModel",
            "onesiteDataModel",
            "performanceAnalyticsDataModel",
            "portfolioManagementDataModel",
            "prospectContactCenterDataModel",
            "productPanelDataModel",
            "rentersInsuranceDataModel",
            "resAppDataModel",
            "residentPortalsDataModel",
            "revenueManagementDataModel",
            "selfProvisioningPortalProductAccessModel",
            "spendManagementDataModel",
            "unifiedAmenitiesProductAccessModel",
            "userMgmtDataModel",
            "utilityManagementDataModel",
            "vendorComplianceDataModel",
            RegisterProductModels
        ]);
})(angular);
