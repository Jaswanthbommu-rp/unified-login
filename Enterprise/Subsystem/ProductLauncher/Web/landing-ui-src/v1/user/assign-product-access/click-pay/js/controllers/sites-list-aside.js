//  Sites List Controller

(function (angular, undefined) {
    "use strict";

    function CPSiteListAsideCtrl($scope, aside, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, roleModel, userModel, persona, userDetailsModel, cpDataModel, pubsub, $filter, security) {
        var vm = this,
            asideGrid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();
            vm.isError = false;

        vm.init = function () {

        	vm.sites = [];
            vm.selSites = [];
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
            vm.formWatch = $scope.$watch("siteForm", vm.setForm);
            
            return vm;
        };

        vm.loadData = function () {
            asideGrid.busy(true);
            var params = {
                    productType: "ClickPay",                    
                    editorPersonaId: persona.getId(),
                    subjectPersonaId: userDetailsModel.getPersonaId(),            
                    organizationRoleId: roleModel.getRoleID(),
                    organizationType: "site",                           
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

             if(vm.selSites.length > 0){
                  vm.selSites.forEach(function (item) {

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

                    vm.selSites.push(o);
                 }  
            }else{

                var o1 = {
                    organizationId :  val.id,
                    roleId : roleModel.getRoleID(),
                    isAssigned : val.isAssigned
                };

                vm.selSites.push(o1);            

            }

                
        };

        vm.gridSelectAllChange = function(val) {
                          
            if (val != undefined) {

                vm.asideGrid.data.records.forEach(function(site) {

                    if (vm.selSites != undefined && vm.selSites.length > 0) {

                        var records = $filter("filter")(vm.selSites, {
                            organizationId: site.id
                        }, true);

                        if (records != undefined && records.length === 0) {
                            var o = {
                                organizationId :  site.id,
                                roleId : roleModel.getRoleID(),
                                isAssigned : site.isAssigned
                            };

                            vm.selSites.push(o);
                        }else{
                            records.forEach(function(item) {
                                item.isAssigned = site.isAssigned;
                            });
                        }
                    }else{

                        vm.selSites =  vm.selSites === undefined ? [] : vm.selSites;
                        var o1 = {
                                organizationId :  site.id,
                                roleId : roleModel.getRoleID(),
                                isAssigned : site.isAssigned
                            };

                        vm.selSites.push(o1);

                    }                    
                });

               
            }
        };

        vm.setData = function (resp) {        	        	
        	if (resp.records && resp.records.length > 0) {        	
            	vm.sites = resp.records;            
                vm.setOrigSites(resp.records); // Keep original records
            	vm.mergeSites(vm.sites);

                if(vm.hasAccess()){
                    
                     vm.sites.forEach(function(item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }


                asideGrid.busy(false);
                gridPagination.setData(vm.sites).goToPage({
                    number: 0
                });

            }

            // if (resp.isError) {
            //     vm.isSitesError = true;
            // }
        };

        vm.mergeSites = function (origSites) {
        	var sites = cpDataModel.getProperties();
        	
        	if(sites != undefined && sites.length > 0){
        		sites.forEach(function (item) {        			
        			if(item.roleId === roleModel.getRoleID()){
        			  var records = $filter("filter")(origSites, {
		                  id: item.organizationId
			            });

			            if (records != undefined && records.length > 0) {
			                records[0].isAssigned = item.isAssigned;
			            }
			        }
        		});
        	}
        };

        vm.setOrigSites = function (data) {
            var orgdata = JSON.parse(JSON.stringify(data));
            cpDataModel.setOrgProperties(orgdata);   
        };

        vm.update = function () {
        	
        	var sitesArr = vm.selSites;
            
            var assignedSites = cpDataModel.getProperties();
            assignedSites =  assignedSites === undefined ? [] : assignedSites;
           
            assignedSites.forEach(function (item) {

                var records = $filter("filter")(sitesArr, {
                    organizationId: item.organizationId,
                    roleId: item.roleId
                });

                if (records != undefined && records.length === 0) {                        
                    sitesArr.push(item);
                }else{
                    records.forEach(function(val) {
                        item.isAssigned = val;
                    });
                }                 
               
           });

           // remove any item from sitesArr  which did not change when compared to original list 
           var origProp = cpDataModel.getOrgProperties(); 
           origProp.forEach(function (item) {                
                var i=0;
                sitesArr.forEach(function (site) {
                    if( site.organizationId === item.id && site.isAssigned === item.isAssigned && roleModel.getRoleID() === site.roleId){
                        
                        sitesArr.splice(i,1);
                        return;
                    }
                    i++;
                });
                
               
           }); 

           var assigned = $filter("filter")(vm.sites, {
                        isAssigned: true
                        }); 
        	
        	if( assigned.length === 0){ //sitesArr.length === 0 &&
                vm.setError(true);
                return;
            } 

        	cpDataModel.setProperties(sitesArr);	
        	aside.hide();
        	// pubsub.publish("clickpay.properties", sitesArr);
        };

        vm.setError = function(val) {
            vm.isError = val;
        };

        vm.isDirty = function() {
            return vm.form.$dirty;
        };

        vm.isValid = function() {
            return vm.form.$valid;
        };


        vm.hasAccess =  function (){            
            return security.isAllowed("viewUser")  || !persona.hasManageClickPayProductAccess();
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
        .controller("CPSiteListAsideCtrl", [
            "$scope",
            "cpSiteListAside",
            "CPSiteListAsideSvc",
            "rpGridModel",
            "CPSiteAsideGrigConfig",
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
            CPSiteListAsideCtrl
        ]);
})(angular);
