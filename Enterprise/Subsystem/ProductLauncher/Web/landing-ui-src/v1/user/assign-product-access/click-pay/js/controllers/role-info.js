//  Role Info  Controller

(function (angular, undefined) {
    "use strict";

    function CPRoleInfoCtrl($scope, compAside, cpModel, llcAside, siteAside) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.showAside = function (record) {
          
            cpModel.setName(record.name);
            cpModel.setRoleID(record.id);

            if(record.orgType.toLowerCase() === 'company'){                
                compAside.show();
            }

            if(record.orgType.toLowerCase() === 'site'){                
                siteAside.show();
            }

            if(record.orgType.toLowerCase() === 'owner'){                
                llcAside.show();
            }
        };

        vm.destroy = function () {
            vm.destWatch();
            compAside = undefined;
            llcAside = undefined;
            siteAside = undefined;
            cpModel = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("CPRoleInfoCtrl", [
            "$scope",
            "cpCompListAside",
            "cpRoleModel",
            "cpLlcListAside",
            "cpSiteListAside",
            CPRoleInfoCtrl
        ]);
})(angular);
