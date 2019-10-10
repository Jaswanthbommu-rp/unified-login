//  Leasing & Rents Settings Model

(function (angular) {
    "use strict";

    function factory() {
        var model = {},
            baseState;

        baseState = "product.lrc.settings.";

        model.sideMenuCollapsed = true;

        model.navData = [
        	{
        		id: "fees-and-deposits",
        		title: "Fees &amp; Deposits",
                sref: baseState + "fees-and-deposits",
        		subNav: [
        			{
        				title: "Concessions",
                        sref: baseState + "fees-and-deposits.concessions"
        			}, {
        				title: "Fees &amp; Additional Deposits",
                        sref: baseState + "fees-and-deposits.fees-and-additional-deposits"
        			}, {
        				title: "Group Fees",
                        sref: baseState + "fees-and-deposits.group-fees"
        			}
        		]
        	}, {
        		id: "resident-management",
        		title: "Resident Management",
                sref: baseState + "resident-management",
        		subNav: [
        			{
        				title: "General Settings",
        				sref: baseState + "resident-management.general"
        			}, {
        				title: "Utilities Settings",
                        sref: baseState + "resident-management.utilities"
        			}, {
        				title: "Move Out Settings",
                        sref: baseState + "resident-management.move-out"
        			}, {
        				title: "Reasons for Move Out",
                        sref: baseState + "resident-management.reasons-move-out"
        			}
        		]
        	}, {
                id: "amenities",
                title: "Amenities",
                subNav: []
            }, {
                id: "traffic-sources",
                title: "Traffic Sources",
                subNav: []
            }, {
                id: "leasing-setup",
                title: "Leasing Setup",
                subNav: []
            }, {
                id: "accounts-receivable",
                title: "Accounts Receivable",
                subNav: []
            }, {
                id: "transactions",
                title: "Transactions",
                subNav: []
            }, {
                id: "field-editing-controls",
                title: "Field Editing Controls",
                subNav: []
            }
        ];

        model.getSideMenuState = function () {
            return model.sideMenuCollapsed;
        };

        model.expandSideMenu = function () {
            model.sideMenuCollapsed = false;

            return model;
        };

        model.toggleSideMenu = function () {
            model.sideMenuCollapsed = !model.sideMenuCollapsed;

            return model;
        };

        return model;
    }

    angular
        .module("settings")
        .factory("lrcSettingsModel", [
        	factory
        ]);
})(angular);
