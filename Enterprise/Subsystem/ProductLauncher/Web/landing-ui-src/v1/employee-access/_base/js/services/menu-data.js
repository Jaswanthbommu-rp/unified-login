//  Add User Form Model

(function (angular) {
    "use strict";

    function factory($filter, baseForm) {
        var model = baseForm();
       
      model.getMenuData = function() {
            return [{
                label: "Company",
                value: "Company"
            },{
                label: "User",
                value: "User"
            }];
        };
        

        return model;
    }

    angular
        .module("settings")
        .factory("empAccessMenuData", [
            "$filter",
            "baseForm",
            factory
        ]);
})(angular);
