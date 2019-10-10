//  new Roles  Model

(function(angular, undefined) {
    "use strict";

    function factory(
       
        $filter
    ) {
        var model = {},
            cfg = {
                recordsPerPage: 10
            };

        model.init = function() {

            return model;
        };


        model.gridInit = function() {
           
        };



        model.setData = function(data) {
            model.data = data;
        };

        model.getData = function() {
            return model.data;
        };

        model.getNewAssignedData = function() {
            var newAssigned = [];
            
            model.getData().records.forEach(function (grp) {
                grp.subGroupList.forEach(function (subGrp) {
                   subGrp.rightsList.forEach(function (right) {
                        
                        if (right != undefined && right.isAssigned === true) {
                            newAssigned.push(right);                     
                        }
                    }); 
                });
            });

            return newAssigned;
        };


        model.getSelectedRecords = function() {
            //return model.grid.getSelectionChanges();
        };

        model.checkIsSelected = function() {
            var sel = false;
            model.getData().records.forEach(function (grp) {
                grp.subGroupList.forEach(function (subGrp) {
                   subGrp.rightsList.forEach(function (right) {
                        
                        if (right != undefined && right.isAssigned === true) {
                            sel = true;                            
                        }
                    }); 
                });
            });
                        
            return sel;
        };


        model.extendData = function (data) {
            // var i=0;
            data.records.forEach(function (item) {
                // if(i===0){
                //     angular.extend(item,{
                //         showHideIcon: "fa-angle-up",
                //         showHide: "show",
                //         isVisible: true
                //     });
                // }else{
                    angular.extend(item,{
                        showHideIcon: "fa-angle-down",
                        showHide: "hide",
                        isVisible: true
                    });
                // }
                
                // var j=0;

                item.subGroupList.forEach(function (subitem) {
                    // if(i===0 && j===0){
                    //     angular.extend(subitem,{
                    //         showHideIcon1: "fa-angle-up",
                    //         showHide1: "show",
                    //         isVisible: true                    
                    //     });
                    // }else{
                        angular.extend(subitem,{
                            showHideIcon1: "fa-angle-down",
                            showHide1: "hide",
                            isVisible: true
                        });
                    // }
                    // j++;


                    subitem.rightsList.forEach(function (right) {
                        angular.extend(right,{                            
                            isVisible: true                    
                        });
                        
                    });

                });
                // i++;



            });
              return data;  
        };   

        model.setDataErr = function(data) {
            logc("Error = > ", data);
        };

         model.searchFilter = function(inp) {  
            if(inp === "" || inp === undefined)
             {
                model.resetFilter();
             }else{
                model.getData().records.forEach(function (grp) {
                    grp.isVisible = true;
                    var i = grp.mainName.toLowerCase().indexOf(inp);
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
            grp.subGroupList.forEach(function (subGrp) {
                subGrp.isVisible = true;
                var i = subGrp.subName.toLowerCase().indexOf(inp);
                if(i === -1){
                    subGrp.isVisible = false;
                    subGrp.isVisible = model.searchRight(subGrp, inp);
                    if(subGrp.isVisible === true){
                        sel = true;  
                    }
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
            model.getData().records.forEach(function (grp) {
                grp.isVisible = true;
                grp.subGroupList.forEach(function (subGrp) {
                   subGrp.isVisible = true;
                   subGrp.rightsList.forEach(function (right) {                        
                        right.isVisible = true;   
                    }); 
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
        .factory("spndmgmtNewRightsModel", [            
            "$filter",
            factory
        ]);
})(angular);