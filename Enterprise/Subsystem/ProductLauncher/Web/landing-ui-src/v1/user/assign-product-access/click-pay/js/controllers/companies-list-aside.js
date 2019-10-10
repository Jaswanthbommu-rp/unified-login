//  Companies List Controller

(function (angular, undefined) {
    "use strict";

    function CPCompListAsideCtrl($scope, aside, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, roleModel, userModel, persona, userDetailsModel, cpDataModel, pubsub, $filter, security) {
        var vm = this,
            asideGrid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();
            vm.isError = false;

        vm.init = function () {

        	vm.companies = [];
            vm.selComps = [];
            vm.title = "";
            vm.subtitle = roleModel.getName();
            vm.asideGrid = asideGrid;
            gridTransform.watch(asideGrid);
            asideGrid.setConfig(gridConfig);
            gridPagination.setGrid(asideGrid);
            $scope.gridPagination = gridPagination;

            gridPagination.setConfig({
                recordsPerPage: 10
            });

            vm.personaWatch = angular.noop;

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.gridSelectionWatch = vm.asideGrid.subscribe("selectChange", vm.gridRowSelectionChange);
            vm.gridSelectAllWatch = vm.asideGrid.subscribe("selectAll", vm.gridSelectAllChange);
            vm.formWatch = $scope.$watch("compForm", vm.setForm);
            return vm;
        };

        vm.loadData = function () {

            asideGrid.busy(true);
            var params = {
                    productType: "ClickPay",                    
                    editorPersonaId: persona.getId(),
                    subjectPersonaId: userDetailsModel.getPersonaId(),            
                    organizationRoleId: roleModel.getRoleID(),
                    organizationType: "company",                           
            };

            vm.dataReq = dataSvc.get(params, vm.setData);
        };

        vm.setForm = function(form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
            }
        };

        vm.gridRowSelectionChange = function(val) {
                         
             if(val === undefined){return;}
             var flag = false;

             if(vm.selComps.length > 0){
                  vm.selComps.forEach(function (item) {

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

                    vm.selComps.push(o);
                 }  
            }else{

                var o1 = {
                    organizationId :  val.id,
                    roleId : roleModel.getRoleID(),
                    isAssigned : val.isAssigned
                };

                vm.selComps.push(o1);            

            }
                
        };

        vm.gridSelectAllChange = function(val) {
                          
            if (val != undefined) {

                vm.asideGrid.data.records.forEach(function(comp) {

                    if (vm.selComps != undefined && vm.selComps.length > 0) {

                        var records = $filter("filter")(vm.selComps, {
                            organizationId: comp.id
                        }, true);

                        if (records != undefined && records.length === 0) {
                            var o = {
                                organizationId :  comp.id,
                                roleId : roleModel.getRoleID(),
                                isAssigned : comp.isAssigned
                            };

                            vm.selComps.push(o);
                        }else{
                            records.forEach(function(item) {
                                item.isAssigned = comp.isAssigned;
                            });
                        }
                    }else{

                        vm.selComps =  vm.selComps === undefined ? [] : vm.selComps;
                        var o1 = {
                                organizationId :  comp.id,
                                roleId : roleModel.getRoleID(),
                                isAssigned : comp.isAssigned
                            };

                        vm.selComps.push(o1);

                    }                    
                });

            }
        };

        vm.setData = function (resp) {
        	
        	if (resp.records && resp.records.length > 0) {
                vm.companies = resp.records;
                vm.setOrigCompanies(resp.records); // Keep original records
                vm.mergeComps(vm.companies);
                
                if(vm.hasAccess()){
                	
                     vm.companies.forEach(function(item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }

                asideGrid.busy(false);
                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });
            }

            // if (resp.isError) {
            //     vm.isCompaniesError = true;
            // }
        };

        vm.mergeComps = function (origComps) {
        	var comps = cpDataModel.getCompanies();

        	if(comps != undefined && comps.length > 0){
        		comps.forEach(function (item) {
        			
        			if(item.roleId === roleModel.getRoleID()){
        			  var records = $filter("filter")(origComps, {
		                id: item.organizationId
			            });

			            if (records != undefined && records.length > 0) {
			                records[0].isAssigned = true;
			            }
			        }
        		});
        	}
        };

        vm.setOrigCompanies = function (data) {
            var orgdata = JSON.parse(JSON.stringify(data));
            cpDataModel.setOrigCompanies(orgdata);   
        };

        vm.update = function () {
        	
            
            var compsArr = vm.selComps;
            
            var assignedComps = cpDataModel.getCompanies();
            assignedComps =  assignedComps === undefined ? [] : assignedComps;
           
            assignedComps.forEach(function (item) {

                var records = $filter("filter")(compsArr, {
                    organizationId: item.organizationId,
                    roleId: item.roleId
                });

                if (records != undefined && records.length === 0) {                        
                    compsArr.push(item);
                }else{
                    records.forEach(function(val) {
                        item.isAssigned = val;
                    });
                }                 
               
           });

           // remove any item from compsArr  which did not change when compared to original list 
           var origProp = cpDataModel.getOrigCompanies(); 
           origProp.forEach(function (item) {                
                var i=0;
                compsArr.forEach(function (comp) {
                    if( comp.organizationId === item.id && comp.isAssigned === item.isAssigned && roleModel.getRoleID() === comp.roleId){                        
                        compsArr.splice(i,1);
                        return;
                    }
                    i++;
                });
                
               
           }); 

           var assigned = $filter("filter")(vm.companies, {
                        isAssigned: true
                        }); 
                       
            
            if( assigned.length === 0){ //compsArr.length === 0 &&
                vm.setError(true);
                return;
            } 

            cpDataModel.setCompanies(compsArr);    
            aside.hide();            
        	// pubsub.publish("clickpay.companies", compsArr);
        };

        vm.setError = function(val) {
            vm.isError = val;
        };

        vm.hasAccess =  function (){            
            return security.isAllowed("viewUser")  || !persona.hasManageClickPayProductAccess();
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

        vm.setChanged = function () {            
            cpDataModel.setChanged();
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.personaWatch();
            vm.gridSelectionWatch();
            vm.gridSelectAllWatch();
            asideGrid.destroy();
            gridTransform.destroy();
            gridPagination.destroy();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }

            vm = undefined;
            $scope = undefined;
            asideGrid = undefined;
            gridTransform = undefined;
            gridPagination = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("CPCompListAsideCtrl", [
            "$scope",
            "cpCompListAside",
            "CPCompListAsideSvc",
            "rpGridModel",
            "CPCompAsideGrigConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "cpRoleModel",
            "userSessionModel",
            "personaDetails",
            "userDetailsModel",
            "clickPayProductAccessModel",
            "pubsub",
            "$filter",
            "routeSecurity",
            CPCompListAsideCtrl
        ]);
})(angular);
