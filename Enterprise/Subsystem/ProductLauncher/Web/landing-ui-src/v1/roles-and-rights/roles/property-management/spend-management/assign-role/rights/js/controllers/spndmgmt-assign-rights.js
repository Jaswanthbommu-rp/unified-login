//  rights Controller

(function(angular, undefined) {
    "use strict";

    function SpndMgmtAssignRightsCtrl(
        $scope,
        pubsub,
        model,
        dataSvc,
        tabsManager,
        $q,
        saveSvc,
        user,
        tabsContext,
        persona,
        wfModel,
        formConfig
    ) {
        var vm = this;
        vm.isError = false;
        vm.init = function() {

            vm.model = model;
            vm.model.gridInit();
            tabsManager.registerTab({
                id: "01",
                ctrl: vm
            });

            formConfig.setMethodsSrc(vm);
            vm.rightConfig = formConfig;

            vm.state = tabsManager.getTabState("01");

            vm.isPageActive = true;
            wfModel.setData(tabsContext.get().data);
            vm.isError = false;
            vm.formWatch = $scope.$watch("assignRightsForm", vm.setForm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.loadData = function() {

            var params = {
                editorPersonaId: persona.getId(),
                assignedToRoleOnly: false,
                roleId: tabsContext.get().data.id
            };

            dataSvc.getData(params)
                .then(vm.setDataFromSvc, model.setDataErr);
        };

        vm.setDataFromSvc = function(data) {
            model.extendData(data);
            model.setData(data);
            model.setExistAssignedData(data);

        };


        vm.setForm = function(form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
            }
        };

        vm.setSubmitted = function() {
            vm.form.$setSubmitted();
            return vm;
        };


        vm.onTabActive = function() {
            vm.loadData();
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

        vm.setError = function(val) {
            vm.isError = val;
        };


        vm.onUpdate = function(assignRole) {
            var newAssigned = model.getNewAssignedData();
            var unAssigned = model.getUnAssignedData();
            
            var rightsList = [];

            newAssigned.forEach(function(item) {
                rightsList.push(item);
            });

            unAssigned.forEach(function(item) {
                rightsList.push(item);
            });

            vm.updateDeferred = $q.defer();

            var parm = {
                "editorPersonaId": persona.getId(),
                "roleId": model.roleData.data.id,
            };


            var inputData = {
                "roleName": assignRole.assignRoleName,
                "roleDesc": assignRole.assignRoleDesc,
                //"isMarketPlaceAdmin" : "",
                "orderWorkflowTimeout": wfModel.getData().orderTimeOut,
                "invoiceWorkflowTimeout": wfModel.getData().invoiceTimeOut,
                //"supplierWorkflowTimeout" : "",   
                "OrderEndorseEmailReminderFlag": wfModel.getData().isOrderReminder,
                "InvoiceEndorseEmailReminderFlag": wfModel.getData().isInvoiceReminder,
                "rightsList": rightsList

            };


            saveSvc.save(parm, inputData).$promise
                .then(vm.onUpdateSuccess, vm.onUpdateError);

            return vm.updateDeferred.promise;
        };


        vm.onUpdateError = function(resp) {
            vm.saveError = true;
            vm.updateDeferred.reject();
        };

        vm.onUpdateSuccess = function(resp) {

            vm.saveError = false;
            vm.form.$setUntouched();
            vm.updateDeferred.resolve();
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

        vm.showIcon = function(val, item) {
            item.showHideIcon = val === 0 ? 'fa-angle-up' : 'fa-angle-down';
            item.showHide = val === 0 ? 'show' : 'hide';
        };

        vm.showIconToggle = function(item) {

            item.showHideIcon = item.showHideIcon === 'fa-angle-down' ? 'fa-angle-up' : 'fa-angle-down';
            item.showHide = item.showHide === 'hide' ? 'show' : 'hide';
            var i = 0;
            item.subGroupList.forEach(function(subItem) {
                if (item.showHide === 'show') {
                    if (i === 0) {
                        subItem.showHideIcon1 = 'fa-angle-up';
                        subItem.showHide1 = 'show';
                    } else {
                        subItem.showHideIcon1 = 'fa-angle-down';
                        subItem.showHide1 = 'hide';
                    }

                } else {
                    subItem.showHideIcon1 = item.showHideIcon;
                    subItem.showHide1 = item.showHide;
                }

                i++;
            });
        };

        vm.showIconToggle1 = function(item) {
            item.showHideIcon1 = item.showHideIcon1 === 'fa-angle-down' ? 'fa-angle-up' : 'fa-angle-down';
            item.showHide1 = item.showHide1 === 'hide' ? 'show' : 'hide';
        };


        vm.showIcon1 = function(val, subval1) {
            var flag = false;
            if (val === 0 && subval1 === 0) {
                flag = true;
            }

            return flag === true ? 'fa-angle-up' : 'fa-angle-down';
        };

        vm.showRow = function(val, subval1, subval2) {
            var flag = false;
            if (val === 0 && subval1 === 0) {
                flag = true;
            }

            return flag === true ? 'show' : 'hide';
        };

        vm.showRow1 = function(val, subval1) {
            var flag = false;
            if (val === 0) {
                flag = true;
            }

            return flag === true ? 'show' : 'hide';
        };

        vm.searchRight = function(inp) {
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
            wfModel.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("SpndMgmtAssignRightsCtrl", [
            "$scope",
            "pubsub",
            "spndmgmtAssignRightsModel",
            "spndMgmtAssignRightsSvc",
            "spndMgmtAssignRoleTabsManager",
            "$q",
            "spndMgmtAssignRightSavesvc",
            "userSessionModel",
            "spndmgmtAssignTabsContext",
            "personaDetails",
            "spndmgmtAssignRoleWfModel",
            "spndmgmtAssignRoleFormConfig",
            SpndMgmtAssignRightsCtrl
        ]);
})(angular);