//  rights Controller

(function(angular, undefined) {
    "use strict";

    function SpndMgmtNewRightsCtrl(
        $scope,
        pubsub,        
        model,
        dataSvc,        
        tabsManager,
        $q,
        saveSvc,
        user,
        persona,
        wfModel,
        formConfig
    ) {
        var vm = this;
        vm.isError = false;

        vm.init = function() {

            tabsManager.registerTab({
                id: "01",
                ctrl: vm
            });
            
            vm.state = tabsManager.getTabState("01");
            
            vm.model = model;

            formConfig.setMethodsSrc(vm);
            vm.rightConfig = formConfig;
            
            vm.isPageActive = true;
            vm.isError = false;

            vm.formWatch = $scope.$watch("newRightsTabForm", vm.setForm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.loadData = function() {

            var params = {
                editorPersonaId: persona.getId(),
                assignedToRoleOnly: false
            };

            dataSvc.getData(params)
                .then(vm.setDataFromSvc, model.setDataErr);
        };

        vm.onPageChange = function(data) {
            // logc(data);
        };

        vm.setDataFromSvc = function(data) {
            model.extendData(data);              
            model.setData(data);            
        };

        vm.setForm = function(form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
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

        vm.onUpdate = function(newRole) {

            var newAssigned = model.getNewAssignedData();   
            vm.updateDeferred = $q.defer();

            var parm = {
                "editorPersonaId": persona.getId(),
                "roleId": 0,
            };

            
            var inputData = {
                "roleName" : newRole.roleName,
                "roleDesc" : newRole.roleDesc,
                //"isMarketPlaceAdmin" : "",
                "orderWorkflowTimeout" : wfModel.data.orderTimeOut,
                "invoiceWorkflowTimeout" : wfModel.data.invoiceTimeOut,
                //"supplierWorkflowTimeout" : "",   
                "OrderEndorseEmailReminderFlag" : wfModel.data.isOrderReminder,
                "InvoiceEndorseEmailReminderFlag" : wfModel.data.isInvoiceReminder,                    
                "rightsList": newAssigned       
                
            };

            
            saveSvc.save(parm, inputData).$promise
                .then(vm.onUpdateSuccess, vm.onUpdateError);

            return vm.updateDeferred.promise;
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


        vm.onUpdateError = function(resp) {
            vm.saveError = true;
            vm.updateDeferred.reject();
        };

        vm.onUpdateSuccess = function(resp) {            
            if (!angular.isUndefined(resp.errorReason) && resp.errorReason.trim().length === 0 && resp.isError === false) {
                vm.saveError = false;
                vm.form.$setUntouched();
                vm.updateDeferred.resolve();
            } else {
                if(resp.errorReason.toLowerCase().indexOf('already been created') != -1){
                    resp.errorReason = "Role already exists";
                }
                tabsManager.resetCounts();
                pubsub.publish("smSettings.newRoleError", resp);
            }
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
            item.subGroupList.forEach(function (subItem) {
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
            //return val === 0 ? 'fa-angle-up' : 'fa-angle-down';
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

        vm.warningEnabled = function(rtObj) {
            rtObj.isAssigned = rtObj.isWarnAssigned === true ? true : rtObj.isAssigned;
            rtObj.value = rtObj.isWarnAssigned === true ? "-1" : rtObj.isAssigned === true ? "1" : "0";

        };

        vm.allowDenyEnabled = function(rtObj) {
            if (rtObj.isCompliance) {
                rtObj.isWarnAssigned = rtObj.isAssigned === false ? false : rtObj.isWarnAssigned;
                rtObj.value = rtObj.isAssigned === false ? "0" : "1";
            } else {
                rtObj.value = rtObj.isAssigned === false ? "0" : "1";
            }
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
        .controller("SpndMgmtNewRightsCtrl", [
            "$scope",
            "pubsub",            
            "spndmgmtNewRightsModel",
            "spndMgmtNewRightsSvc",            
            "spndMgmtNewRoleTabsManager",
            "$q",
            "spndMgmtNewSaveRightsSvc",
            "userSessionModel",
            "personaDetails",
            "spndmgmtNewRoleWfModel",
            "spndmgmtNewRoleFormConfig",
            SpndMgmtNewRightsCtrl
        ]);
})(angular);