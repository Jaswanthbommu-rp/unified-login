(function (angular, undefined) {
    "use strict";

    function radio($templateCache, $compile) {
        function link(scope, elem, attr) {
            var child,
                column,
                dir = {},
                childHtml;

            dir.init = function () {
                
               
                    // childHtml = '<label class="md-check dark-bluebox" ng-controller="ClientPortalRolesRadioCtrl as cprrc">' +
                    //             '<input type="radio" name="client-portal-role" ' +
                    //         ' ng-disabled="record.disabled" ng-model="record.isAssigned" '+
                    //         ' ng-change="cprrc.publishRoleChange(record)" ng-value="true" ' +
                    //         ' id="client-portal-role-{{record.id}}" class="has-value"> ' +
                    //         ' <i class="primary"></i> ' +
                    //     '</label>';
                    // child = angular.element(childHtml);

                    // child = $compile(child)(scope);
                    // elem.html("").append(child);
                

                dir.destWatch = scope.$on("$destroy", dir.destroy);
            };

            dir.destroy = function () {
                dir.destWatch();
                elem.html("").remove();
                dir = undefined;
                elem = undefined;
                scope = undefined;
                child = undefined;
                childHtml = undefined;
            };

            dir.init();
        }

        // return {            
        //     restrict: 'E',   
        //     controller: 'ClientPortalRolesRadioCtrl',
        //     controllerAs: 'cprrc',          
        //     template: '<label class="md-check dark-bluebox" >' +
        //                         '<input type="radio" name="client-portal-role" ' +
        //                     ' ng-disabled="record.disabled" ng-model="record.isAssigned" '+
        //                     ' ng-change="cprrc.publishRoleChange(record)" ng-value="true" ' +
        //                     ' id="client-portal-role-{{record.id}}" class="has-value"> ' +
        //                     ' <i class="primary"></i> ' +
        //                 '</label>'
        // };

        return{
            link: function(scope, element){
                var template = "<div>RadioB</div>";
                var linkFn = $compile(template);
                var content = linkFn(scope);
                element.append(content);
            }
        };
    }

    angular
        .module("settings")
        .directive('radiogrid', ['$templateCache', '$compile', radio]);
})(angular);