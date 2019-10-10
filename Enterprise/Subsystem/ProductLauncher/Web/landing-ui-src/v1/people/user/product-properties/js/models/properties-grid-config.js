// Product Item Properties Access Grid Configuration

(function (angular) {
    "use strict";

    function gridFactory(rpGridConfig, gridConst) {
        var gridConfig = rpGridConfig();

        //define colummn config
        gridConfig.get = function() {
            return [{
                key: gridConst.isSelected.id,
                type: gridConst.isSelected.type,
            }, {
                key: gridConst.property.id,
                type: gridConst.property.type,
            }, {
                key: gridConst.city.id,
                type: gridConst.city.type,
            }, {
                key: gridConst.state.id,
                type: gridConst.state.type,
            }];
        };

        //define header config
        gridConfig.getHeaders = function() {
            return [[{
                key: gridConst.isSelected.id,
                text: gridConst.isSelected.text,
                enabled: gridConst.isSelected.isEnabled
            }, {
                key: gridConst.property.id,
                text: gridConst.property.text,
                isSortable: gridConst.property.isSortable
            }, {
                key: gridConst.city.id,
                text: gridConst.city.text,
                isSortable: gridConst.city.isSortable
            }, {
                key: gridConst.state.id,
                text: gridConst.state.text,
                isSortable: gridConst.state.isSortable
            }]];
        };

        return gridConfig;
    }

    angular
        .module("settings")
        .factory("productPropertiesGridConfig", [
                "rpGridConfig",
                "productPropertiesGridConst",
                gridFactory
        ]);

})(angular);
