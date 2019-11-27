//  User Details Controller

(function (angular, undefined) {
    "use strict";

    function UserDetailsCtrl($scope, $q, $filter, $params, timeout, moment, model, userSvc, userTypes, formConfig, passReq, popoverConfig, tabs, tabsModel, userStatus, assignProductsModel, session, userTimeZones,
        contactMethod, industryJobTitle, phoneTypeTitle, security, helpData, persona, impersonate, switchConfig, changeUserType, chgUserTypeModal, pubsub, externalUserModal, externalUserSvc, existingUserModal, chkEmailModel, existingNoEmailUserModal) {
        var vm = this,
            lang = $filter("userDetailsText"),
            helpWidget = document.querySelector('raul-unified-help'),
            existingFirstName = "",
            existingLastName = "",
            existingMiddleName = "",
            existingLoginName = "",
            existingExtUser = "",
            existingUserTypeId = "";

        vm.init = function () {
            vm.isInit = true;
            vm.model = model;            
            vm.helpMode = helpWidget.helpPageId;
            vm.security = security;
            vm.customFieldList = [];
            vm.assignProducts = assignProductsModel;
            vm.passReqPopoverConfig = popoverConfig;
            vm.userTypes = {
                regularUser: 401,
                regularUserNoEmail: 404,
                superUser: 402,
                externalUser: 405
            };
            vm.newTypeId = 0;
            vm.changeUserTypeSub = angular.noop;

            vm.formConfig = formConfig.setMethodsSrc(vm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);

            vm.activeDeactiveSwitch = switchConfig({
                disabled: !security.isAllowed("activatedeactivateUser")
            });

            vm.activeDeactiveSwitchIDP = switchConfig({
                disabled: vm.isExternalUser()
            });

                        
            if (persona.isReady()) {
                 vm.register(); //.getUserTypeOptions();
                 vm.personaWatch = angular.noop;
                 vm.getExternalUserData();
            }
            else {
                vm.personaWatch = persona.subscribe(function(){
                    vm.register(); //.getUserTypeOptions();
                    vm.getExternalUserData();
                });
            }
            
            if (!$params.realPageId) {                
                vm.getUserTypeOptions();
            }
            chkEmailModel.setIsBusy(false);   

            // vm.register().getUserTypeOptions(); //.getUsertimeZoneOptions();
            vm.activeWatch = $scope.$watch(vm.isReady, vm.setControlsState);
            vm.activeWatchImp = $scope.$watch(vm.isImpReady, vm.setControlsStateImp);
            if (!model.userExists()) {
                vm.getProfilePhoneTypes().getProfileContactMethods().getProfileJobtitles();
            }

            vm.cloneReadyWatch = pubsub.subscribe("settings.userDataReady", vm.onCloneReady);
            vm.ThirdPartyWatch = pubsub.subscribe("settings.3rdParty", vm.set3rdPartyIDP);
            vm.resetUserTypeOptions = pubsub.subscribe("settings.resetUserTypeOptions",  vm.getUserTypeOptions);
            vm.noEmailValidation = pubsub.subscribe("settings.noEmailValidationUpdate", vm.noEmailValidationUpdate);

            vm.formWatch = $scope.$watch("userDetails.userDetailsForm", vm.setForm);
            
        };

        vm.setForm = function(form) {
            if (form) {
                vm.form = form;
                model.setForm(form);                
                vm.formWatch();
            }
        };

        vm.noEmailValidationUpdate = function () {                    
            vm.formConfig.notificationEmail.required = false;
            vm.userDetailsForm.notificationEmail.$validate();
        };

        vm.onCloneReady = function () {
            if (vm.isReady()) {
                vm.set3rdPartyIDP();
            }
        };

        vm.isReady = function () {
            return model.isReady();
        };

        vm.isImpReady = function () {
            return impersonate.isReady();
        };

        vm.isExternalUser = function () {
            return model.isExternalUser();
        };

        vm.updateTabsMenu = function () {
            var tabs = ["userDetails"];

            if (!model.isDisabled() && userStatus.isRegularUser()) {
                tabs.push("productAccess");
            }

            if (model.userExists() && !model.isClonedUser() && !model.is3rdPartyIDP() && !vm.isExternalUser()  ) {
                
                tabs.push("securityQuestions");
                tabs.push("resetPassword");
                tabs.push("activityLog");
            }

            if (model.userExists() && !model.isClonedUser() &&  (vm.isExternalUser() && vm.getExternalUserData() === undefined) ) {

                tabs.push("securityQuestions");
                tabs.push("resetPassword");
                tabs.push("activityLog");
            }

            //tabs.push("activityLog");

            tabsModel.setTabs(tabs).activateTab("userDetails");
        };

        vm.updateTabsMenuExternalUser = function (data) {

            if(data){
                if(data.restricted !== null && data.restricted !== undefined && data.restricted.tabs !== undefined){

                    var tabs = [];
                    tabsModel.tabsList.forEach(function (item) {
                        tabs.push(item.id);
                    });

                    if(data.restricted.tabs.indexOf('securityQuestions') !== -1 ){
                       var i = tabs.indexOf('securityQuestions');                    
                        if(i > 0){
                            tabs.splice(i, 1);
                        }
                    }

                    if(data.restricted.tabs.indexOf('resetPassword') !== -1 ){                    
                        var j = tabs.indexOf('resetPassword');
                        if(j > 0){
                            tabs.splice(j, 1);
                        }
                    }

                    tabsModel.setTabs(tabs).activateTab("userDetails");
                }
            }
            
           
        };

               

        // Getters

        vm.getUserTypeOptions = function () {
            var reqData = {
                roleTypeName: "user role",
                loginName: model.data.userLogin.loginName
            };

            vm.userTyepReq = userTypes.get(reqData, vm.initUserTypeOptions);

            return vm;
        };

        vm.getUserIntials = function () {
            return model.getInitials();
        };

        vm.getUsertimeZoneOptions = function () {
            var reqData = {};

            vm.userTimeZoneReq = userTimeZones.get(reqData, vm.initUserTimeZoneOptions);
            return vm;
        };


        vm.getProfilePhoneTypes = function () {
            phoneTypeTitle.getList(vm.initphoneTypeOptions);
            return vm;
        };

        vm.getProfileContactMethods = function () {
            contactMethod.getList(vm.initContactMethodOptions);
            return vm;
        };

        vm.getProfileJobtitles = function () {
            industryJobTitle.getList(vm.initJobTitleOptions);
            return vm;
        };

        vm.getCustomFieldsData = function () {
            var list = [];

            vm.model.customFieldList.forEach(function (item) {
                list.push(item.getData());
            });
            return list;
        };

        // Actions

        vm.checkLoginName = function (loginName) {                        
            if (userStatus.loginNameIsEmail()) {
                var isValid = model.loginNameIsValidEmail();
                vm.validateLoginNameReq[isValid ? "resolve" : "reject"]();
            }
            else {
                vm.validateLoginNameReq.resolve();
            }
        };

        vm.checkPasswordMatch = function () {
            vm.passMatchCheck = $q.defer();
            timeout.cancel(vm.checkTimer);
            vm.checkTimer = timeout(vm.validatePasswordMatch, 10);
            return vm.passMatchCheck.promise;
        };

        vm.focusInvalidField = function () {
            $scope.focusInvalidField.focus();
        };

        vm.hidePasswordRequirements = function () {
            $scope.passReqPopover.hide();
        };

        vm.initUserTypeOptions = function (resp) {
            //Regular User  or cloneduser remove user admin option
            if (model.isClonedUser() || persona.isPersonaIsRegularUser()) {
                resp.data = resp.data.filter(function (item) {
                    return item.partyRoleTypeId !== 402;
                });
            }

            // Realpage Employee user type
            if (resp.data != null && resp.data.length > 0) {
                var id = resp.data[0].partyRoleTypeId;
                if (id === 403) {
                    model.setUserTypeDefConfig(id);
                }

                vm.set3rdPartyIDP();
            }

            
            // if(vm.isExternalUser()){
                // vm.updateTabsMenuExternalUser();
                // vm.setExternalUserControl(true);
            // }

            formConfig.setUserTypeOptions(resp.data);
        };

        vm.set3rdPartyIDP = function () {
            if (vm.model.data.userTypeId === 405) {
                model.set3rdPartyIDP(false);
                vm.activeDeactiveSwitchIDP.setDisabled(true);
            }
        };

        vm.initUserTimeZoneOptions = function (resp) {
            formConfig.setUserTimeZoneOptions(resp.data);
        };

        vm.initphoneTypeOptions = function (resp) {
            formConfig.setPhoneTypeOptions(resp.data);
        };

        vm.initContactMethodOptions = function (resp) {
            formConfig.setContactMethodOptions(resp.data);
        };

        vm.initJobTitleOptions = function (resp) {
            formConfig.setJobTitleOptions(resp.data);
        };
        // ToDo: Check if admin user
        vm.onEffectiveDateChange = function (data) {
            //&& (session.getRealPageId() !== $params.realPageId)
            if (model.isDisabled()) {
                var limit = moment().startOf('day');
                formConfig.setFromDateMinLimit(limit);
            }

            vm.updateExpiresLimit(data);
        };

        vm.onSaveError = function () {
            vm.saveReq.reject({
                success: false,
                tabName: "userDetails"
            });
        };

        vm.onSaveSuccess = function (resp) {
            if (resp.status.success) {
                vm.saveReq.resolve({
                    success: true,
                    tabName: "userDetails"
                });
            }
            else {
                vm.saveReq.reject({
                    success: false,
                    tabName: "userDetails",
                    msg: lang(resp.status.errorCode)
                });
            }
        };

        vm.onTabActive = function () {
            helpWidget.helpPageId = vm.helpMode;
            vm.active = true;
            return vm;
        };

        vm.onTabInactive = function () {
            vm.active = false;
            return vm;
        };

        vm.onUserTypeChange = function (userTypeId) {
            var bPrompt = true;
            if ($params.realPageId && (userTypeId !== model.getOrgUserTypeId())) {
                if (userTypeId === vm.userTypes.regularUserNoEmail) {
                    if(model.isClonedUser()){
                            bPrompt = false;
                             vm.processUserTypeChange(userTypeId);
                        }else{
                            changeUserType.setChangeMode(changeUserType.changeModes.ToNoEmail);
                     }                    
                }
                else if (userStatus.isSuperUser()) {
                    if (userTypeId === vm.userTypes.externalUser) {
                        changeUserType.setChangeMode(changeUserType.changeModes.SuperToExternal);
                    }
                    else if (userTypeId === vm.userTypes.regularUser) {                        
                        changeUserType.setChangeMode(changeUserType.changeModes.SuperToRegular);                        
                    }
                }
                else if (userStatus.isRegularUserNoEmail()) {
                    if (userTypeId === vm.userTypes.regularUser) {
                        if(model.isClonedUser()){
                            bPrompt = false;
                            vm.processUserTypeChange(userTypeId);
                        }else{
                            changeUserType.setChangeMode(changeUserType.changeModes.NoEmailToRegular);
                        }
                    }
                    else if (userTypeId === vm.userTypes.superUser) {
                        changeUserType.setChangeMode(changeUserType.changeModes.NoEmailToSuper);
                    }
                    else if (userTypeId === vm.userTypes.externalUser) {
                        if(model.isClonedUser()){
                            bPrompt = false;
                            vm.processUserTypeChange(userTypeId);
                        }else{
                            changeUserType.setChangeMode(changeUserType.changeModes.NoEmailToExternal);
                        }
                    }
                }
                else if (userStatus.isRegularUser() && !userStatus.isExternalUser()) {
                    if (userTypeId === vm.userTypes.externalUser ) {
                        if(model.isClonedUser()){
                            bPrompt = false;
                            vm.processUserTypeChange(userTypeId);
                        }else{
                            changeUserType.setChangeMode(changeUserType.changeModes.RegularToExternal);
                        }
                    }
                    else if (userTypeId === vm.userTypes.superUser) {
                        changeUserType.setChangeMode(changeUserType.changeModes.RegularToSuper);
                    }
                }
                else if (userStatus.isExternalUser()) {
                    if (userTypeId === vm.userTypes.regularUser ) {
                        if(model.isClonedUser()){
                            bPrompt = false;
                            vm.processUserTypeChange(userTypeId);
                        }else{
                            changeUserType.setChangeMode(changeUserType.changeModes.ExternalToRegular);
                        }    
                    }
                    else if (userTypeId === vm.userTypes.superUser) {
                        changeUserType.setChangeMode(changeUserType.changeModes.ExternalToSuper);
                    }
                }
                vm.newTypeId = userTypeId;
                if(bPrompt){
                    vm.promptUserTypeChange();
                }
            }
            else {
                vm.processUserTypeChange(userTypeId);
            }
        };

        vm.processUserTypeChange = function (userTypeId) {
            userStatus.setStatusId(userTypeId);
            vm.updateTabsMenu();
            
            if (vm.isExternalUser()) {
                model.set3rdPartyIDP(false);
                            
                if(model.existingUser && model.data.realPageId !== "00000000-0000-0000-0000-000000000000" && vm.getExternalUserData()){
                    if(!vm.isEditingSelf() && existingExtUser){
                        vm.setExternalUserControl(true);
                    }else{
                        vm.setExternalUserControl(false);
                    }
                }
            
            }
            else {               
                model.set3rdPartyIDP(model.is3rdPartyIDP());
            
                
                if(existingUserTypeId === vm.model.data.userTypeId){
                    vm.setExternalUserControlLoad(model.getExternalUserData());
                }else{
                    vm.setExternalUserControl(false);    
                }
            
            }

            
            vm.activeDeactiveSwitchIDP.setDisabled(vm.isExternalUser());

            formConfig.setLoginNameErrorReqdText(userTypeId);
            formConfig.setEmailRequired(model.emailIsReqd());

            vm.userDetailsForm.loginName.$validate();
        };

        vm.promptUserTypeChange = function () {
            // subscribe to events from confirmation model
            if (vm.changeUserTypeSub == angular.noop) {
                vm.changeUserTypeSub = changeUserType.subscribe(vm.onUserTypeChangeDecision);
            }
            chgUserTypeModal.show();
        };

        vm.onUserTypeChangeDecision = function (data) {
            chgUserTypeModal.hide();
            if (data.status === "confirmedDemoteAdmin" || data.status === "confirmedDemoteAdminToExternal") {
                vm.assignProducts.setAdminResetFlag(true);
                model.setAdminResetFlag(true);
                vm.processUserTypeChange(vm.newTypeId);
            }
            else if (data.status === "confirmedPromoteRegular" || data.status === "confirmedRegularWithEmail" || data.status === "confirmedPromoteExternal" || data.status === "confirmedRegularExternal" || data.status === "confirmedExternalWithEmail") {
                vm.assignProducts.setAdminResetFlag(false);
                model.setAdminResetFlag(false);
                vm.processUserTypeChange(vm.newTypeId);
            }
            else {
                vm.resetUserTypeSelect();
                changeUserType.setChangeMode("none");
            }
        };

        vm.resetUserTypeSelect = function () {
            model.data.userTypeId = userStatus.getStatusId();
        };

        vm.register = function () {
            tabs.register({
                ctrl: vm,
                name: "userDetails"
            });

            return vm;
        };

        vm.saveData = function () {
            model.CustomFields = vm.getCustomFieldsData();
            var data = model.getData(),
                method = model.userExists() ? "update" : "save";

            if (data.telecommunicationNumber !== undefined) {
                var ps = vm.getPhoneNumbers(data.telecommunicationNumber);
                data.telecommunicationNumber = model.getParsedPhoneNumbers(ps);
            }


            if (model.isClonedUser()) {
                method = "save";
            }

            vm.saveReq = $q.defer();
            userSvc[method](data, vm.onSaveSuccess, vm.onSaveError);

            return vm.saveReq.promise;
        };

        vm.getPhoneNumbers = function (phones) {
            var phs = [];
            if (phones !== undefined) {

                phones.forEach(function (ph) {
                    phs.push(ph);
                });
            }

            return JSON.parse(JSON.stringify(phs));
        };

        vm.checkPhoneValid = function (item) {};

        vm.setControlsState = function (bool) {
            if (bool) {
                formConfig.setControlsDisabledState(false);
                if ($params) {
                    if (vm.isEditingSelf()) {
                        formConfig.setControlsDisabledState(true);
                    }

                    if (security.isAllowed("viewUser")) {
                        formConfig.setControlsDisabledState(true);
                    }

                    if ((impersonate.isUserImpersonated()) && vm.isEditingSelf()) {
                        formConfig.setControlsDisabledState(true);
                    }
                }
            }

            return vm;
        };

        vm.setControlsStateImp = function () {
            if (impersonate.isUserImpersonated() && vm.isEditingSelf()) {
                formConfig.setControlsDisabledState(true);
            }
        };

        vm.showErrors = function () {
            vm.userDetailsForm.$setSubmitted();
            timeout($scope.focusInvalidField.focus, 100);
        };

        vm.showPasswordCopyValidationError = function () {
            return !model.passwordCopyIsEmpty();
        };

        vm.showPasswordRequirements = function () {
            $scope.passReqPopover.show();
        };

        vm.showPasswordValidationError = function () {
            return !model.passwordIsEmpty();
        };

        vm.updateExpiresLimit = function (data) {
            var thruDateMin = false;

            if (data) {
                thruDateMin = data.clone().add(1, "days");

                if (model.thruDateIsBefore(data)) {
                    model.setThruDate("");
                }
            }

            formConfig.setThruDateMinLimit(thruDateMin);
        };

        vm.validateLoginName = function (loginName) {
            vm.validateLoginNameReq = $q.defer();
            timeout(vm.checkLoginName, 10);
            userStatus.setLoginName(loginName);
            return vm.validateLoginNameReq.promise;
        };

        vm.validatePasswordMatch = function () {
            var match = model.passwordsMatch(),
                method = match ? "resolve" : "reject";

            if (vm.passMatchCheck) {
                vm.passMatchCheck[method]();
                vm.passMatchCheck = undefined;
            }
        };

        vm.validatePasswordCopy = function (password) {
            vm.userDetailsForm.passwordCopy.$validate();
        };

        // Assertions
        vm.canAddEditCustomFields = function () {
            if (!model.userExists()) {
                return true;
            }
            else {
                return (!model.isSuperUser() && vm.isEditingSelf()) ? false : true;
            }
        };

        vm.emailIsReqd = function () {
            return model.emailIsReqd();
        };
        
        vm.passwordIsReqd = function () {
            if (model.isClonedUser()) {
                return model.passwordIsReqd();
            }
            else {
                return model.userExists() ? false : model.passwordIsReqd();
            }
        };

        vm.hasSaveFn = function () {
            if (model.isSuperUser()) {
                return true;
            }
            else {
                if (vm.isUserWithViewEditPwdRightOnly()) {
                    return false;
                }
                else {
                    return !vm.isEditingSelf();
                }
            }
        };

        vm.isCloned = function () {
            return model.isClonedUser();
        };

        vm.isDisabled = function () {
            return model.isDisabled();
        };

        vm.isDirty = function () {
            return vm.userDetailsForm.$dirty;
        };

        vm.isEditingSelf = function () {
            return session.getRealPageId() === $params.realPageId;
        };

        vm.isUserWithViewEditPwdRightOnly = function () {
            return security.isAllowed("viewUser") && security.isAllowed("editPassWord");
        };

        vm.canEditAccessData = function () {
            return !(vm.isEditingSelf() || vm.isUserWithViewEditPwdRightOnly());
        };

        vm.isValid = function () {
            return vm.userDetailsForm.$valid;
        };

        vm.userExists = function () {
            return model.userExists();
        };

        vm.validatePassword = function (password) {
            return passReq.update(password).isValid();
        };

        vm.addPhone = function (form) {
            model.addPhone();
        };

        vm.delPhone = function (item) {
            model.delPhone(item);
        };

        vm.setFirstNameDisabled = function (bool) {
            formConfig.setFirstNameDisabled(bool);
        };

        vm.setMiddleNameDisabled = function (bool) {
            formConfig.setMiddleNameDisabled(bool);
        };

        vm.setLastNameDisabled = function (bool) {
            formConfig.setLastNameDisabled(bool);
        };

        vm.setLoginNameDisabled = function (bool) {
            formConfig.setLoginNameDisabled(bool);
        };

        vm.setExternalUserControl = function (bool) {
            vm.setFirstNameDisabled(bool);
            vm.setMiddleNameDisabled(bool);
            vm.setLastNameDisabled(bool);
            vm.setLoginNameDisabled(bool);
        };

        vm.openExternalUserModal = function () {    
            
            chkEmailModel.setIsBusy(true);                     
            
            if(vm.userDetailsForm.loginName.$valid){                
                 if(vm.userDetailsForm.loginName.$pristine === false ){
                    vm.getDataOnBlur();                                   
                 }else{
                    chkEmailModel.setIsBusy(false);                        
                 }
            }else{        
                chkEmailModel.setIsBusy(false);         
                
                vm.userDetailsForm.loginName.$validate();
                vm.userDetailsForm.$setSubmitted();                
            }

        };

        vm.getExternalUserData = function() {            
            if(model.isReady()){                
                vm.getData();
                vm.modelWatch = angular.noop;     
            }else{
                vm.modelWatch = model.subscribe(vm.getData);
            }
        };

        vm.setExistingData = function () {
            existingLoginName = model.data.userLogin.loginName;
            existingExtUser = vm.isExternalUser();
            existingUserTypeId = vm.model.data.userTypeId;
            existingFirstName = vm.model.data.lastName;
            existingLastName = vm.model.data.lastNamelastName;
            existingMiddleName = vm.model.data.middleName;
        };

        vm.getData =function () {
                var userRPId = "";   
                vm.getUserTypeOptions();  
                if(vm.isInit){ 
                    vm.setExistingData();          
                }
                vm.isInit = false;
                
                if(model.existingUser && model.data.realPageId !== "00000000-0000-0000-0000-000000000000"){
                    userRPId = model.data.realPageId;
                }

                var params = { 
                    loginName : model.data.userLogin.loginName,
                    OrganizationRealPageId : persona.getOrgRealPageID(),
                    userRealPageId : userRPId                    
                };

                if(model.data.userLogin.loginName !== null){
                    externalUserSvc.getData(params)
                    .then(vm.setData, vm.setDataErr);
                }

                
        };

        vm.setData = function(resp) {       
            model.setExternalUserData(resp.data);           
            vm.updateTabsMenuExternalUser(resp.data);
            vm.setExternalUserControlLoad(resp.data);            
        };

        vm.setExternalUserControlLoad =function (data) {

            if(data){
                if(data.restricted !== null && data.restricted !== undefined  && data.restricted.fields !== undefined){

                    if(data.restricted.fields.indexOf('FirstName') !== -1 ){
                        vm.setFirstNameDisabled(true);
                    }
                    if(data.restricted.fields.indexOf('MiddleName') !== -1 ){
                        vm.setMiddleNameDisabled(true);
                    }
                    if(data.restricted.fields.indexOf('LastName') !== -1 ){
                        vm.setLastNameDisabled(true);
                    }
                    if(data.restricted.fields.indexOf('LoginName') !== -1 ){
                        vm.setLoginNameDisabled(true);
                    }
                } 
            }           
        };

        vm.setDataErr = function(data) {
            logc("Error: ", data);
        };


        vm.getDataOnBlur =function () {
                var userRPId = "";  
                                    
                if(model.existingUser && model.data.realPageId !== "00000000-0000-0000-0000-000000000000"){
                    userRPId = model.data.realPageId;
                }

                var params = { 
                    loginName : model.data.userLogin.loginName,
                    OrganizationRealPageId : persona.getOrgRealPageID(),
                    userRealPageId : userRPId                    
                };

                externalUserSvc.getData(params)
                    .then(vm.setDataOnBlur, vm.setDataErr);
        };

        vm.setDataOnBlur = function(resp) {  
            if(resp.data.userExists === false ){     
                vm.getUserTypeOptions();
            }

           model.setExternalUserData(resp.data);
           var isModalOpen = false;
           
           // Not an Employee
           if(model.data.userTypeId !== 403){


               if(resp.data.userExistsNotAvailable === true && resp.data.userExists === true  && (model.data.realPageId === "" || model.data.realPageId === "00000000-0000-0000-0000-000000000000" )){
                    vm.showExistingUserModal(true, resp.data.person.realPageId);
               }else{

                   if(resp.data.userExistsNotAvailable === false && resp.data.userExists === true && resp.data.userExistsAsNoEmail === false && resp.data.userExistsInThisOrganization === true && (model.data.realPageId !== "" && model.data.realPageId !== "00000000-0000-0000-0000-000000000000" && model.data.realPageId !== resp.data.person.realPageId)){                  
                        vm.showExistingUserModal(true, resp.data.person.realPageId);
                   }

                   if(resp.data.userExistsNotAvailable === false && resp.data.userExists === true && resp.data.userExistsAsNoEmail === false && resp.data.userExistsInThisOrganization === false && (model.data.realPageId !== "" && model.data.realPageId !== "00000000-0000-0000-0000-000000000000" && model.data.realPageId !== resp.data.person.realPageId)){                   
                        vm.showExistingUserModal(false, resp.data.person.realPageId);
                   }

                   if(resp.data.userExistsNotAvailable === false && resp.data.userExists === true && resp.data.userExistsAsNoEmail === true && resp.data.userExistsInThisOrganization === true && (model.data.realPageId !== "" && model.data.realPageId !== "00000000-0000-0000-0000-000000000000" && model.data.realPageId !== resp.data.person.realPageId)){                   
                        existingNoEmailUserModal.show();
                   }

                   if(resp.data.userExistsNotAvailable === false && resp.data.userExists === true && resp.data.userExistsAsNoEmail === false && resp.data.userExistsInThisOrganization === false && (model.data.realPageId === "" || model.data.realPageId === "00000000-0000-0000-0000-000000000000" )){                   
                        isModalOpen = true;
                        externalUserModal.show();
                   }

                   if(resp.data.userExistsNotAvailable === false && resp.data.userExists === true && resp.data.userExistsAsNoEmail === false && resp.data.userExistsInThisOrganization === true && (model.data.realPageId === "" || model.data.realPageId === "00000000-0000-0000-0000-000000000000" )){                                   
                        vm.showExistingUserModal(true, resp.data.person.realPageId);
                   }

                   if(resp.data.userExistsNotAvailable === true && resp.data.userExistsAsNoEmail === false && ( (resp.data.person !== null && model.data.realPageId !== resp.data.person.realPageId) || (resp.data.person === null) ) ){                                   
                        vm.showExistingUserModal(false, "");
                   }

                   if(  resp.data.userExists === true && resp.data.userExistsAsNoEmail === true && (model.data.realPageId === "" || model.data.realPageId === "00000000-0000-0000-0000-000000000000" )){                                       
                        existingNoEmailUserModal.show();
                   }

               }                          
               
           }else{

              if(resp.data.userExistsNotAvailable === true && resp.data.userExists === true  && (model.data.realPageId === "" || model.data.realPageId === "00000000-0000-0000-0000-000000000000" )){
                    vm.showExistingUserModal(true, resp.data.person.realPageId);
               }               

               if(resp.data.userExists === true && resp.data.userExistsAsNoEmail === false && resp.data.userExistsInThisOrganization === true && (model.data.realPageId !== "" && model.data.realPageId !== "00000000-0000-0000-0000-000000000000" && model.data.realPageId !== resp.data.person.realPageId)){                  
                    vm.showExistingUserModal(true, resp.data.person.realPageId);
               }

               if(resp.data.userExists === true && resp.data.userExistsAsNoEmail === false && resp.data.userExistsInThisOrganization === false && (model.data.realPageId !== "" && model.data.realPageId !== "00000000-0000-0000-0000-000000000000" && model.data.realPageId !== resp.data.person.realPageId)){                   
                    vm.showExistingUserModal(false, resp.data.person.realPageId);
               }

               if(resp.data.userExists === true && resp.data.userExistsAsNoEmail === false && resp.data.userExistsInThisOrganization === true && (model.data.realPageId === "" || model.data.realPageId === "00000000-0000-0000-0000-000000000000" )){                                   
                    vm.showExistingUserModal(true, resp.data.person.realPageId);
               }

               if(resp.data.userExists === true && resp.data.userExistsAsNoEmail === false && resp.data.userExistsInThisOrganization === false && (model.data.realPageId === "" || model.data.realPageId === "00000000-0000-0000-0000-000000000000" )){                   
                    vm.showExistingUserModal(false, resp.data.person.realPageId);
               }

           }

           if(isModalOpen === false){
                chkEmailModel.setIsBusy(false);  
           }
           
        };

         vm.showExistingUserModal = function(bool, rpId) {  
            if(bool){
                model.setExistingUserLink(rpId);
            }
            model.setShowExistingUserLink(bool);
            existingUserModal.show();
         };



        // Reset/Destroy

        vm.destroy = function () {
            model.reset();
            vm.destWatch();
            vm.activeWatch();
            vm.activeWatchImp();
            vm.cloneReadyWatch();
            vm.personaWatch();
            vm.modelWatch();
            vm.ThirdPartyWatch();
            vm.resetUserTypeOptions();
            vm.noEmailValidation();
            vm.customFieldList = [];
            passReq.reset();
            userStatus.reset();
            vm.userTyepReq.$cancelRequest();
            //vm.userTimeZoneReq.$cancelRequest();
            vm.assignProducts.reset();
            vm.changeUserTypeSub();
            vm = undefined;
            lang = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("UserDetailsCtrl", [
            "$scope",
            "$q",
            "$filter",
            "$stateParams",
            "timeout",
            "moment",
            "userDetailsModel",
            "userSvc",
            "userTypesSvc",
            "userDetailsFormConfig",
            "passReqModel",
            "passReqPopoverConfig",
            "userTabsManager",
            "userTabsModel",
            "userStatusModel",
            "assignProductsModel",
            "userSessionModel",
            "userTimeZoneSvc",
            "contactMethodSvc",
            "industryJobTitleSvc",
            "phoneTypeTitleSvc",
            "routeSecurity",
            "rpGhHelpData",
            "personaDetails",
            "userImpersonated",
            "rpSwitchConfig",
            "changeUserTypeConfirmationModel",
            "chgUserTypeModal",
            "pubsub",
            "externalUserModal",
            "externalUserSvc",
            "existingUserModal",
            "chkEmailModel",
            "existingNoEmailUserModal",
            UserDetailsCtrl
        ]);
})(angular);