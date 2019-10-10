(function (angular) {
    "use strict";

    function factory($filter, tabsMenuModel) {
        var model = {};

        model.init = function(existingConfig) {
            model.scrollTabs = tabsMenuModel();

            var scrollTabsConfig = [];
            if(angular.isUndefined(existingConfig) && existingConfig !== null) {
                angular.forEach(existingConfig, function(currConfig) {
                    currConfig.id = model.assignId();
                    scrollTabsConfig.push(currConfig);
                });
            }            

            return model.scrollTabs.setData(scrollTabsConfig);
        };

        model.assignId = function(tabConfig) {
            return "persona-" + model.getCounter();
        };

        model.assignLabel = function(label) {
            if(angular.isUndefined(label) || label === null || label.length === 0) {
                return $filter("manageUserText")("default_persona_label") + " " + model.getCounter();
            }
            return label;
        };

        model.getCounter = function() {
            return model.scrollTabs.getData().length + 1;
        };

        model.getConfig = function() {
            return model.scrollTabs;
        };

        model.addNewTab = function(isActive, label) {
            var id = model.assignId(),
                scrollName = model.assignLabel(label);

            model.scrollTabs.addData({
                id: id,
                isActive: isActive || false,
                text: scrollName
            });

            return {
                id: id,
                name: scrollName
            };
        };

        model.destroy = function() {
            model.scrollTabs.destroy();
            model.scrollTabs = undefined;
        };

        return model;
    }

    angular
        .module("settings")
        .factory("personaScrollTabs", [
            "$filter",
            "rpScrollingTabsMenuModel",
        	factory
        ]);
})(angular);
