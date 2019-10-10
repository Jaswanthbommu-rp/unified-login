(function (angular, undefined) {
    "use strict";

    function factory(
        $filter,        
        msgModal,
        saveSvc,
        $q        
    ) {
        var model = {};
            
        model.init = function () {             
            return model;
        };

       

        model.openModal = function (data) {            
             model.update(data);
             msgModal.show();
        };

        model.update = function(data) {
           
            model.updateDeferred = $q.defer();

            var parm = {
                 "organizationRealPageId": data.companyRealPageId                
            };

            var inputData = {
                  // "organizationRealPageId": data.companyRealPageId                 
            };
           
            saveSvc.save(parm, inputData).$promise
                .then(model.onUpdateSuccess, model.onUpdateError);

            return model.updateDeferred.promise;
        };

        
        model.onUpdateError = function(resp) {            
            logc("success resp",resp);
        };

        model.onUpdateSuccess = function(resp) {            
            logc("error resp",resp);
        };


        model.reset = function () {
            
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("empAccessCompActionModalModel", [
            "$filter",            
            "compAdminRefreshMsgModal",       
            "compAdminRefreshMsgSvc",
            "$q", 
            factory
        ]);
})(angular);
