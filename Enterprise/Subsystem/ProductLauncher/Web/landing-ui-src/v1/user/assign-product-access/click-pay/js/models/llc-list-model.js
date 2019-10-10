//  Llc  Model

(function(angular, undefined) {
    "use strict";

    function factory(
       
        $filter,
        cpDataModel
    ) {
        var model = {};

        model.init = function() {
            return model;
        };
       
        model.setData = function(data) {
            model.data = data;
        };

        model.getData = function() {
            return model.data;
        };

        model.getNewAssignedData = function() {
            var newAssigned = [];
            
            model.getData().forEach(function (grp) {
                grp.siteList.forEach(function (subGrp) {
                                           
                    if (subGrp != undefined && subGrp.isAssigned === true) {
                        newAssigned.push(subGrp);                     
                    }
                   
                });
            });

            return newAssigned;
        };


        model.getSelectedRecords = function() {
            //return model.grid.getSelectionChanges();
        };

        model.checkIsSelected = function() {
            var sel = false;
            model.getData().forEach(function (grp) {
                grp.siteList.forEach(function (subGrp) {
                                           
                    if (subGrp != undefined && subGrp.isAssigned === true) {
                        sel = true;                            
                    }
                   
                });
            });
                        
            return sel;
        };


        model.extendData = function (data) {
            
            data.forEach(function (item) {
                
                angular.extend(item,{
                    showHideIcon: "fa-angle-down",
                    showHide: "hide",
                    isVisible: true
                });
                

                item.siteList.forEach(function (subitem) {
                    
                    angular.extend(subitem,{
                        showHideIcon1: "fa-angle-down",
                        showHide1: "hide",
                        isVisible: true
                });
                   
                    // subitem.rightsList.forEach(function (right) {
                    //     angular.extend(right,{                            
                    //         isVisible: true                    
                    //     });
                        
                    // });

                });
                
            });
              return data;  
        };   

        model.setDataErr = function(data) {
            
        };

         model.searchFilter = function(inp) {  
            if(inp === "" || inp === undefined)
             {
                model.resetFilter();
             }else{
                model.getData().forEach(function (grp) {
                    grp.isVisible = true;
                    var i = grp.name.toLowerCase().indexOf(inp);
                    if(i === -1){
                        grp.isVisible = false;
                        grp.isVisible = model.searchSubGrp(grp, inp);
                    }else{
                        grp.isVisible = true;
                    }
                    
                });
            }
                        
            return model;
        };

        model.searchSubGrp = function(grp, inp) {        

            var sel = false;
            grp.siteList.forEach(function (subGrp) {
                subGrp.isVisible = true;
                var i = subGrp.name.toLowerCase().indexOf(inp);
                if(i === -1){
                    subGrp.isVisible = false;
                    // subGrp.isVisible = model.searchRight(subGrp, inp);
                    // if(subGrp.isVisible === true){
                    //     sel = true;  
                    // }
                }else{
                    sel = true;  
                    subGrp.isVisible = true;
                }
            });
          
                        
            return sel;
        };

        model.searchRight = function(subGrp, inp) {        

            var sel = false;            
            subGrp.rightsList.forEach(function (rt) {  
                rt.isVisible = true;                        
                var i = rt.right.toLowerCase().indexOf(inp);
                if(i === -1){
                    rt.isVisible = false;                    
                }else{
                    sel = true;  
                    rt.isVisible = true;   
                }
            }); 
                
                        
            return sel;
        };

        model.resetFilter = function() { 
            model.getData().forEach(function (grp) {
                grp.isVisible = true;
                grp.siteList.forEach(function (subGrp) {
                   subGrp.isVisible = true;
                   // subGrp.rightsList.forEach(function (right) {                        
                   //      right.isVisible = true;   
                   //  }); 
                });
            });
                        
            return model;
        };       

        model.reset = function() {            
            model.data = undefined;
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("cpLlcModel", [            
            "$filter",
            "clickPayProductAccessModel",
            factory
        ]);
})(angular);