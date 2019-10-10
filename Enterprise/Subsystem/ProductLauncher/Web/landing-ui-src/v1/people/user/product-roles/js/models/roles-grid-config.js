// Product Item Roles Access Grid Configuration

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
                key: gridConst.role.id,
                type: gridConst.role.type
            }, {
                key: gridConst.roleType.id,
                type: gridConst.roleType.type
            }];
        };

        //define header config
        gridConfig.getHeaders = function() {
            return [[{
                key: gridConst.isSelected.id,
                text: gridConst.isSelected.text,
                enabled: gridConst.isSelected.isEnabled
            }, {
                key: gridConst.role.id,
                text: gridConst.role.text,
                isSortable: gridConst.role.isSortable
            }, {
                key: gridConst.roleType.id,
                text: gridConst.roleType.text,
                isSortable: gridConst.roleType.isSortable
            }]];
        };

        return gridConfig;
    }

    angular
        .module("settings")
        .factory("productRolesGridConfig", [
                "rpGridConfig",
                "productRolesGridConst",
                gridFactory
        ]);

})(angular);
