//  rights Model

(function(angular, undefined) {
    "use strict";

    function factory(
        $filter,        
        pubsub,
        svc,
        assignRolesAside,
        tabsContext,
        user,
        $modal,        
        persona

    ) {
        var model = {},
            cfg = {
                recordsPerPage: 10
            };

        model.asideScope = {};

        model.init = function() {
            model.getDataSvc();            
            return model;
        };                
       
        model.getDataSvc = function() {
            var params = { editorPersonaId: persona.getId() };
            svc.get(params, model.setDataFromSvc, model.setDataErr);
        };

        model.getSelectedRecords = function() {
            return model.grid.getSelectionChanges();
        };

        model.assignRolestoRights = function(record) {
            tabsContext.set({ type: "assign", data: record });
            assignRolesAside.show();
        };


        model.setRights = function(data) {
            model.data = data.records;
        };

        model.getRights = function() {
            return model.data;
        };

        model.setDataFromSvc = function(data) {   
            model.extendData(data);               
            model.setRights(data);            
        };    

        model.extendData = function (data) {
            // var i=0;
            data.records.forEach(function (item) {
                // if(i===0){
                //     angular.extend(item,{
                //         showHideIcon: "fa-angle-up",
                //         showHide: "show"
                //     });
                // }else{
                    angular.extend(item,{
                        showHideIcon: "fa-angle-down",
                        showHide: "hide"
                    });
                // }
                
                // var j=0;

                item.subGroupList.forEach(function (subitem) {
                    // if(i===0 && j===0){
                    //     angular.extend(subitem,{
                    //         showHideIcon1: "fa-angle-up",
                    //         showHide1: "show"                    
                    //     });
                    // }else{
                        angular.extend(subitem,{
                            showHideIcon1: "fa-angle-down",
                            showHide1: "hide"
                        });
                    // }
                    // j++;
                });
                // i++;
            });
              return data;  
        };    
       
        model.getFilteredData = function(val) {
            var filObj = $filter("filter")(model.getRoles(), {
                centerName: val
            },true);
            return filObj;
        };

        model.setDataErr = function(data) {
            logc("Error: ", data);
        };
        
        model.reset = function() {                        
            // model.assignUpdate();            
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("rolAndRhtSpndMgmtRightsModel", [
            "$filter",            
            "pubsub",
            "spndmgmtRightsSvc",
            "assignRolesToRightsAside",
            "assignRolesToRightsContext",
            "userSessionModel",
            "$modal",            
            "personaDetails",
            factory
        ]);
})(angular);