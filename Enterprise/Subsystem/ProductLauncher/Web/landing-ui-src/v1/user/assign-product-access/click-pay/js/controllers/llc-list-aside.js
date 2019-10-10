//  rights Controller

(function(angular, undefined) {
    "use strict";

    function CpLLCCtrl(
        $scope,
        pubsub,        
        model,        
        $q,   
        dataSvc,     
        user,
        persona,
        aside,
        roleModel,
        formConfig,
        userDetailsModel,
        $filter,
        cpDataModel,
        security
    ) {
        var vm = this;
        vm.isError = false;

        vm.init = function() {
            vm.llcs = [];
            vm.selLlcs = [];
            vm.isAllSelected = false;
            vm.title = "LLC";
            vm.model = model;
                       
            vm.isPageActive = true;
            
            vm.subtitle = roleModel.getName();

            formConfig.setMethodsSrc(vm);
            vm.formConfig = formConfig;

            vm.loadData();

            vm.formWatch = $scope.$watch("llcForm", vm.setForm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.loadData = function() {

            var params = {
                    productType: "ClickPay",                    
                    editorPersonaId: persona.getId(),
                    subjectPersonaId: userDetailsModel.getPersonaId(),            
                    organizationRoleId: roleModel.getRoleID(),
                    organizationType: "owner",                           
            };
           
            vm.dataReq = dataSvc.get(params, vm.setDataFromSvc);
        };

        vm.onPageChange = function(data) {
            
        };

        vm.setDataFromSvc = function(resp) {
            if (resp.records && resp.records.length > 0) {    
                vm.llcs = resp.records;
                vm.setOrigLLC(resp.records); // Keep original records  
                vm.mergeLlcs(vm.llcs);
                model.extendData(vm.llcs);     
                
                model.setData(vm.llcs);   

                 if(vm.hasAccess()){
                    
                     vm.llcs.forEach(function(item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }

                
                // var records = $filter("filter")(vm.llcs, {
                //         isAssigned: false
                //         });
                

                // vm.isAllSelected = records.length > 0 ? false : true;
                vm.isAllSelected = vm.isCheckAllSelected();
            } 

            // if (resp.isError) {
            //     vm.isLLCError = true;
            // }
                 
        };

        vm.setForm = function(form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
            }
        };

        vm.isCheckAllSelected = function() {
             var records = $filter("filter")(vm.llcs, {
                        isAssigned: false
                        });                

            return   records.length > 0 ? false : true;             
        };

        vm.setSubmitted = function() {
            vm.form.$setSubmitted();
            return vm;
        };

         vm.mergeLlcs = function (origLlcs) {
            var llcs = cpDataModel.getLlc();
            
            if(llcs != undefined && llcs.length > 0){
                llcs.forEach(function (item) {
                    
                    if(item.roleId === roleModel.getRoleID()){
                      var records = $filter("filter")(origLlcs, {
                        id: item.organizationId
                        });

                        if (records != undefined && records.length > 0) {
                            records[0].isAssigned = true;
                        }
                    }
                });
            }
        };

        vm.isChecked = function (val) {            
          return val === true ? "checked" : "";  
        };

        vm.setError = function(val) {
            vm.isError = val;
        };

        vm.setSubmitted = function() {
            vm.form.$setSubmitted();
            return vm;
        };

        vm.onTabActive = function() {
            vm.loadData();
        };

        vm.onUpdate = function() {
            
            
            var llcArr = vm.selLlcs;
                                   
            var assignedLlcs = cpDataModel.getLlc();
            assignedLlcs =  assignedLlcs === undefined ? [] : assignedLlcs;
           
            assignedLlcs.forEach(function (item) {

                var records = $filter("filter")(llcArr, {
                    organizationId: item.organizationId,
                    roleId: item.roleId
                });

                if (records != undefined && records.length === 0) {                        
                    llcArr.push(item);
                }else{
                    records.forEach(function(val) {
                        item.isAssigned = val;
                    });
                }                 
               
            });

           // remove any item from llcArr  which did not change when compared to original list 
            var origProp = cpDataModel.getOrgiLlc(); 
            origProp.forEach(function (item) {                
                var i=0;
                llcArr.forEach(function (llc) {
                    if( llc.organizationId === item.id && llc.isAssigned === item.isAssigned && roleModel.getRoleID() === llc.roleId){
                        
                        llcArr.splice(i,1);
                        return;
                    }
                    i++;
                });                
               
            });
            
            var assigned = $filter("filter")(vm.llcs, {
                        isAssigned: true
                        }); 

            if( assigned.length === 0){  //llcArr.length === 0 &&
                vm.setError(true);
                return;
            } 
            cpDataModel.setLlc(llcArr);  
            aside.hide();    
              
                                    
        };

        vm.checkIsSelected = function() {
            var isSel = model.checkIsSelected();
            if (isSel === true) {
                vm.setError(false);
            } else {
                vm.setError(true);
            }
            return isSel;
        };

        vm.setError = function (val) {
          vm.isError = val;  
        };
        

        vm.hasSaveError = function() {
            return vm.saveError;
        };

        vm.isDirty = function() {
            return vm.form.$dirty;
        };

        vm.isValid = function() {
            return vm.form.$valid;
        };

         vm.showIcon = function(val,item){
            item.showHideIcon =  val === 0 ? 'fa-angle-up' : 'fa-angle-down';
            item.showHide =  val === 0 ? 'show' : 'hide';
        };

        vm.showIconToggle = function(item){
            item.showHideIcon = item.showHideIcon === 'fa-angle-down' ? 'fa-angle-up' : 'fa-angle-down';
            item.showHide = item.showHide === 'hide' ? 'show' : 'hide';
            var i=0;
            item.siteList.forEach(function (subItem) {
                if(item.showHide === 'show'){
                    if(i===0){
                        subItem.showHideIcon1 = 'fa-angle-up';
                        subItem.showHide1 = 'show';
                    }else{
                        subItem.showHideIcon1 = 'fa-angle-down';
                        subItem.showHide1 = 'hide';
                    }

                }else{
                     subItem.showHideIcon1 = item.showHideIcon;
                     subItem.showHide1 = item.showHide;
                }
               
                i++;
            });
        };

        vm.showIconToggle1 = function(item){                            
            item.showHideIcon1 = item.showHideIcon1 === 'fa-angle-down' ? 'fa-angle-up' : 'fa-angle-down';
            item.showHide1 = item.showHide1 === 'hide' ? 'show' : 'hide';            
        };


        vm.showIcon1 = function(val, subval1){            
            var flag = false;
            if(val === 0 && subval1 === 0 ){                
                flag = true;
            }
            
            return flag === true ? 'fa-angle-up' : 'fa-angle-down';
        };

        vm.showRow = function(val, subval1, subval2){            
            var flag = false;
            if(val === 0 && subval1 === 0 ){                
                flag = true;
            }

            return flag === true ? 'show' : 'hide';
        };

        vm.showRow1 = function(val, subval1){            
            var flag = false;
            if(val === 0  ){                
                flag = true;
            }

            return flag === true ? 'show' : 'hide';
        };

        vm.searchRight = function (inp) {
            model.resetFilter();             
            inp = inp.toLowerCase();
            model.searchFilter(inp);
        };

        vm.onSelAllChange = function (val) {
            
              if (val != undefined) {

                vm.llcs.forEach(function(llc) {
                    llc.isAssigned = val;

                    if (vm.selLlcs != undefined && vm.selLlcs.length > 0) {

                        var records = $filter("filter")(vm.selLlcs, {
                            organizationId: llc.id
                        }, true);

                        if (records != undefined && records.length === 0) {
                            var o = {
                                organizationId :  llc.id,
                                roleId : roleModel.getRoleID(),
                                isAssigned : val
                            };

                            vm.selLlcs.push(o);
                        }else{
                            records.forEach(function(item) {
                                item.isAssigned = val;
                            });
                        }
                    }else{

                        vm.selLlcs =  vm.selLlcs === undefined ? [] : vm.selLlcs;
                        var o1 = {
                                organizationId :  llc.id,
                                roleId : roleModel.getRoleID(),
                                isAssigned : val
                            };

                        vm.selLlcs.push(o1);

                    }                    
                });

            }
        };

        
        vm.onChange = function(val) {
                        
             if(val === undefined){return;}
             var flag = false;

             if(vm.selLlcs.length > 0){
                  vm.selLlcs.forEach(function (item) {

                     if( val.id === item.organizationId  ){
                        item.isAssigned = val.isAssigned;
                        flag = true;

                     }            
            });

            if(!flag){

                    var o = {
                        organizationId :  val.id,
                        roleId : roleModel.getRoleID(),
                        isAssigned : val.isAssigned
                    };

                    vm.selLlcs.push(o);
                 }  
            }else{

                var o1 = {
                    organizationId :  val.id,
                    roleId : roleModel.getRoleID(),
                    isAssigned : val.isAssigned
                };

                vm.selLlcs.push(o1);            

            }

           vm.isAllSelected = vm.isCheckAllSelected();    
        };

        vm.setOrigLLC = function (data) {
            var orgdata = JSON.parse(JSON.stringify(data));
            cpDataModel.setOrigLlc(orgdata);   
        };

        vm.hasAccess =  function (){            
            return security.isAllowed("viewUser")  || !persona.hasManageClickPayProductAccess();
        };

        vm.setChanged = function () {            
            cpDataModel.setChanged();
        };

        vm.isDirty = function() {
            return vm.form.$dirty;
        };

        vm.isValid = function() {
            return vm.form.$valid;
        };

        vm.cancel = function () {
            aside.hide();
        };

        vm.destroy = function() {
            vm.destWatch();
            model.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("CpLLCCtrl", [
            "$scope",
            "pubsub",            
            "cpLlcModel",            
            "$q",
            "CpLLCSvc",
            "userSessionModel",
            "personaDetails",            
            "cpLlcListAside",
            "cpRoleModel",
            "cpFormConfig",
            "userDetailsModel",
            "$filter",
            "clickPayProductAccessModel",
            "routeSecurity",
            CpLLCCtrl
        ]);
})(angular);