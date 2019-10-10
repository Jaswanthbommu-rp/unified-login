//  rights menu Model

(function(angular, undefined) {
    "use strict";

    function factory(sideMenuModel,
        ProductFamiliesData,
        productsModel) {
        function RightsMenuModel() {
            var s = this;
            s.init();
        }

        var p = RightsMenuModel.prototype;

        p.init = function() {
            var s = this;
            s.sideMenuData = [];
        };

        p.setFamilies = function(data) {
            var s = this;
            s.families = data;
            return s;
        };

        p.getFamilies = function() {
            var s = this;
            return s.families;
        };

        p.getSidemenuList = function() {
            var s = this;
            return s.sideMenuData;
        };

        p.getRightsSideMenu = function() {
            var s = this;
            var families = s.getFamilies();
            s.sideMenuData = [];
            families.forEach(function(family) {
                var list = s.getSolList(family);
                // family.sideMenuList.setList(list);
            });

        };

        p.setActiveSol = function(sol) {
            var s = this;
            var data = s.getSidemenuList();
            
            var sel;
            data.forEach(function(item) {
                var solAct = productsModel.getActiveSol();
                item.active = false;
                item.solution.selected = false;

                if (solAct === "") {
                    if (sol.solutionId === item.solutionId) {
                        item.active = true;
                        sel = item;
                        item.solution.selected = true;
                    }
                } else {
                    if (solAct.solutionId === item.solutionId) {
                        item.active = true;
                        sel = item;
                        item.solution.selected = true;
                    }
                }
            });

            // return sel;
            return s;
        };


        p.getSolList = function(family) {
            var s = this;
            var list = [],
                famName, title, link, ext, famId;

            famId = family.data.familyId;
            famName = ProductFamiliesData.getFamilyName(famId);

            family.solutions.forEach(function(sol) {
                sol.selected = false;
                title = s.getTitle(sol, famId);
                link = s.getLink(famName, title);
                //list.length === 0 ? true : false,
                ext = {
                    active: false,
                    text: sol.titleId,
                    validators: {},
                    templateUrl: link,
                    solutionId: sol.solutionId,
                    solution: sol
                };

                list.push(ext);
                s.sideMenuData.push(ext);
            });

            return list;
        };

        p.getTitle = function(sol, famId) {
            var s = this;
            return sol.solutionId === undefined ? "" : ProductFamiliesData.getSolutionName(famId, sol.solutionId);
        };

        p.getLink = function(famName, title) {
            var s = this;
            var link = "roles-and-rights/rights/" + famName + "/" + title + "/index.html";
            link = title === undefined ? "roles-and-rights/rights/coming-soon/templates/index.html" : link;
            return link;
        };


        p.reset = function() {
            var s = this;
        };


        return new RightsMenuModel();
    }

    angular
        .module("settings")
        .factory("rightsMenuModel", ["solSideMenuModel", "productFamiliesData", "rolesAndRightsProductsModel", factory]);
})(angular);