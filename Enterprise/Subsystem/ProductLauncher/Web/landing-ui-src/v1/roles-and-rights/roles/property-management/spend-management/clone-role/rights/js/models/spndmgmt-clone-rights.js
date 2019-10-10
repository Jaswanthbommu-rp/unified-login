//  Clone Roles  Model

(function(angular, undefined) {
    "use strict";

    function factory(        
        cloneTabsContext,
        user,
        $filter,
        persona
    ) {
        var model = {};

        model.init = function() {

            model.data = { rightName: "" };
            return model;
        };

        model.gridInit = function() {
           
        };

        model.getDataSvc = function(dataSvcGet) {
            var params = {
                editorPersonaId: persona.getId(),
                assignedToRoleOnly: false,
                roleId: cloneTabsContext.get().data.id
            };

            dataSvcGet.getData(params)
                .then(model.setDataFromSvc, model.setDataErr);
        };

        model.setData = function(data) {
            model.data = data;
        };

        model.getData = function() {
            return model.data;
        };

         model.setExistAssignedData = function(data) {
            model.existAssignedData = [];
            var arr = [];
            model.getData().records.forEach(function(grp) {
                grp.subGroupList.forEach(function(subGrp) {
                    subGrp.rightsList.forEach(function(right) {
                        if (right != undefined && (right.isAssigned === true || right.isWarnAssigned === true)) {
                            //model.existAssignedData.push(right);
                            arr.push(right);
                        }
                    });
                });
            });

            model.existAssignedData = JSON.parse(JSON.stringify(arr));

        };

        model.getExistAssignedData = function() {
            return model.existAssignedData;
        };

        model.getAllAssignedData = function() {

            var allAssigned = [];
            
             model.getData().records.forEach(function (grp) {
                grp.subGroupList.forEach(function (subGrp) {
                   subGrp.rightsList.forEach(function (right) {                        
                        if (right != undefined && right.isAssigned === true) {
                            allAssigned.push(right);                        
                        }
                    }); 
                });
            });

            return allAssigned;
        };

       
        // model.getNewAssignedData = function() {
        //     var newAssigned = [];
            
        //     model.getData().records.forEach(function (grp) {
        //         grp.subGroupList.forEach(function (subGrp) {
        //            subGrp.rightsList.forEach(function (right) {
                        
        //                 if (right != undefined && right.isAssigned === true) {
        //                     newAssigned.push(right);                     
        //                 }
        //             }); 
        //         });
        //     });

        //     return newAssigned;
        // };

        // model.getUnAssignedData = function() {
        //     var unAssignedData = [];
        //     var allAssigned = model.getAllAssignedData();
        //     var extAssigned = model.getExistAssignedData();

        //     extAssigned.forEach(function(item) {

        //         var i = allAssigned.indexOf(item);
        //         if (i === -1) {
        //             unAssignedData.push(item);
        //         }
        //     });
        //     return unAssignedData;
        // };

         model.getNewAssignedData = function() {
            var newAssigned = [];
            var allAssigned = model.getAllAssignedData();
            var extAssigned = model.getExistAssignedData();


            allAssigned.forEach(function(allitem) {
            //     var isNewItem = true;
            //     extAssigned.forEach(function(existItem) {

            //         if ((allitem.name === existItem.name)) {
            //             isNewItem = false;
            //         }

            //         if ((allitem.name === existItem.name) && (existItem.isWarnAssigned === false && allitem.isWarnAssigned === true)) {
            //             isNewItem = true;
            //         }
            //     });

            //     if ((isNewItem === true)) {
            //         newAssigned.push(allitem);
            //         return;
            //     }

            if (( allitem.isAssigned === true) ) {
                    newAssigned.push(allitem);                    
              }

            });

            return newAssigned;
        };

        // model.getUnAssignedData = function() {
        //     var unAssignedData = [];
        //     var allAssigned = model.getAllUnAssignedData();
        //     var extAssigned = model.getExistAssignedData();

        //     allAssigned.forEach(function(allitem) {
        //         extAssigned.forEach(function(existItem) {
        //             if (allitem.name === existItem.name) {
        //                 if ((existItem.isAssigned === true && allitem.isAssigned === false) || (existItem.isWarnAssigned === true && allitem.isWarnAssigned === false)) {
        //                     unAssignedData.push(allitem);
        //                     return;
        //                 }
        //             }
        //         });

        //     });

        //     return unAssignedData;            
        // };



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

                    subitem.rightsList.forEach(function (rt) {
                        angular.extend(rt,{                           
                            isVisible: true
                        });
                    });

                });
                // i++;
            });
              return data;  
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



        model.setDataErr = function(data) {
            logc("Error = > ", data);
        };

        model.resetGridFilters = function() {
            model.grid.resetFilters();
        };

        model.reset = function() {
            model.data.rightName = "";            
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("spndmgmtCloneRightsModel", [            
            "spndmgmtCloneTabsContext",
            "userSessionModel",
            "$filter",
            "personaDetails",
            factory
        ]);
})(angular);