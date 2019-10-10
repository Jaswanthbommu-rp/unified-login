//  rights Controller

(function(angular, undefined) {
    "use strict";

    function SpndMgmtCloneRightsCtrl(
        $scope,
        pubsub,
        model,
        dataSvc,
        tabsManager,
        $q,
        saveSvc,
        user,
        cloneTabsContext,
        notifySvc,
        persona,
        wfModel,
        formConfig,
        tabsContext
    ) {
        var vm = this;
        vm.isError = false;
        vm.init = function() {

            tabsManager.registerTab({
                id: "01",
                ctrl: vm
            });
            vm.model = model;

            formConfig.setMethodsSrc(vm);
            vm.rightConfig = formConfig;
            vm.state = tabsManager.getTabState("01");
            vm.isError = false;

            vm.isPageActive = true;
            wfModel.setData(tabsContext.get().data);

            vm.formWatch = $scope.$watch("cloneRightsTabForm", vm.setForm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.loadData = function() {
            var params = {
                editorPersonaId: persona.getId(),
                assignedToRoleOnly: false,
                roleId: cloneTabsContext.get().data.id
            };

            dataSvc.getData(params)
                .then(vm.setDataFromSvc, model.setDataErr);
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

        vm.setForm = function(form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
            }
        };

        vm.setDataFromSvc = function(data) {
            model.extendData(data);
            model.setData(data);
            model.setExistAssignedData(data);

        };

        vm.setSubmitted = function() {
            vm.form.$setSubmitted();
            return vm;
        };


        vm.onTabActive = function() {
            vm.loadData();
        };

        vm.onUpdate = function(cloneRole) {

            var newAssigned = model.getNewAssignedData();
            vm.updateDeferred = $q.defer();

            var parm = {
                "editorPersonaId": persona.getId(),
                "roleId": 0 //cloneRole.inheritRoleId
            };


            var inputData = {
                "roleName": cloneRole.roleName,
                "roleDesc": cloneRole.roleDesc,
                //"isMarketPlaceAdmin" : "",
                "orderWorkflowTimeout": wfModel.data.orderTimeOut,
                "invoiceWorkflowTimeout": wfModel.data.invoiceTimeOut,
                //"supplierWorkflowTimeout" : "",   
                "OrderEndorseEmailReminderFlag": wfModel.data.isOrderReminder,
                "InvoiceEndorseEmailReminderFlag": wfModel.data.isInvoiceReminder,
                "rightsList": newAssigned

            };


            saveSvc.save(parm, inputData).$promise
                .then(vm.onUpdateSuccess, vm.onUpdateError);

            return vm.updateDeferred.promise;
        };


        vm.onUpdateError = function(resp) {

            if (!angular.isUndefined(resp.errorReason) && resp.errorReason.trim().length === 0 && resp.isError === false) {
                vm.saveError = true;
                vm.updateDeferred.reject();
            } else {

                resp.errorReason = "Error occured";
                tabsManager.resetCounts();
                pubsub.publish("smSettings.cloneRoleError", resp);
            }
        };

        vm.onUpdateSuccess = function(resp) {

            if (!angular.isUndefined(resp.errorReason) && resp.errorReason.trim().length === 0 && resp.isError === false) {
                vm.saveError = false;
                vm.form.$setUntouched();
                vm.updateDeferred.resolve();
            } else {
                if (resp.errorReason.toLowerCase().indexOf('already been created') != -1) {
                    resp.errorReason = "Role already exists";
                }
                tabsManager.resetCounts();
                pubsub.publish("smSettings.cloneRoleError", resp);
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
        .controller("SpndMgmtCloneRightsCtrl", [
            "$scope",
            "pubsub",
            "spndmgmtCloneRightsModel",
            "spndMgmtCloneRightsSvc",
            "spndMgmtCloneRoleTabsManager",
            "$q",
            "spndMgmtCloneSaveRightsSvc",
            "userSessionModel",
            "spndmgmtCloneTabsContext",
            "notificationService",
            "personaDetails",
            "spndmgmtCloneRoleWfModel",
            "spndmgmtCloneRoleFormConfig",
            "spndmgmtCloneTabsContext",
            SpndMgmtCloneRightsCtrl
        ]);
})(angular);