
//  Edit User Controller

(function (angular) {
    "use strict";

    function EditUserCtrl($scope, $location, $stateParams, $filter, editUserSummaryModel, 
            userFormData, userFormModel, userFormState, personaModel, userStates,
			manageUserModel, editUserSvc, updateUserSvc, rpWatchList, moment, notifsSvc, userProfileModel) {
    	var vm = this;

    	vm.init = function() {
    		vm.watchList = rpWatchList();
            vm.watchList.add($scope.$on("$destroy", vm.destroy));

            vm.additionalPersonaWatch = $scope.$on("rpManageUserAdditionalPersona", vm.additionalPersona);
            vm.loadWatch = $scope.$watchCollection("page.isLoading", vm.verifyAllLoaded);

            userProfileModel.subscribe(vm.userProfileUpdated);
            manageUserModel.setState(userStates.EDIT_USER);
            userFormModel.setData(userFormData);

            vm.formModel = userFormModel;
            vm.formState = userFormState.state;

    		vm.realPageId = $stateParams.userId;
            vm.model = editUserSummaryModel;
            
            vm.isLoading = {
            	userProfile: false,
				userDetails: false,
				userPersonas: false
            };

    		vm.initUserDetails();
    	};

        vm.userProfileUpdated = function () {
            vm.dataReady = false;
            vm.loadWatch = $scope.$watchCollection("page.isLoading", vm.verifyAllLoaded);
            vm.initUserDetails();
        };

    	vm.initUserDetails = function() {
    		vm.setLoading();

            userFormModel.setRealPageId(vm.realPageId);

    		editUserSvc.getUserProfile(vm.realPageId)
                .then(vm.setUserProfileData)
                .finally(vm.setUserProfileLoaded);

            editUserSvc.getUserDetails(vm.realPageId)
                .then(vm.setUserDetails)
                .finally(vm.setUserDetailsLoaded);

            editUserSvc.getUserPersonas(vm.realPageId)
                .then(vm.setUserPersonas)
                .finally(vm.setUserPersonasLoaded);
    	};

        vm.additionalPersona = function(evt) {
            evt.stopPropagation();
            $scope.$broadcast("rpAdditionalPersona");            
               
            vm.destroyPersonaWatch();
        };

        vm.setUserProfileData = function(response) {
        	if(response.isError) {
                editUserSummaryModel.setError(response);
            } else {
                editUserSummaryModel.setData(response);
                userProfileModel.setData(response.userSummary);
                vm.setUserDetailsFromSummary(response.userSummary);
            }
        };

        vm.setUserDetailsFromSummary = function(summary) {
            if(summary.notificationEmail) {
                userFormModel.setNotificationEmail(summary.notificationEmail.addressString, summary.notificationEmail.contactMechanismId);
            }

            userFormModel.setUserType(summary.userType);
        };

		vm.setUserDetails = function(response) {
            
            if(response){
                var startDate = "",
                    endDate = "";

                if(response.fromDate) {
                    startDate = moment(response.fromDate);
                }
                if(response.thruDate) {
                    endDate = moment(response.thruDate);
                }
                
                userFormModel.updateData({
                    username: response.loginName,
                    startDate: startDate,
                    endDate: endDate,
                    isEnabled: response.isActive
                });                
            }
		};

		vm.setUserPersonas = function(respPersonaArr) {
			if(respPersonaArr && respPersonaArr.length > 0){
                var personaList = [];
                angular.forEach(respPersonaArr, vm.addExistingPersona.bind(null, personaList));
                
                if(personaList.length > 1) {
                    userFormState.setHasAddedPersona(true);                     
                }
                $scope.$broadcast("rpInit:personaList", personaList);
            }
		};

        vm.addExistingPersona = function(list, personaData) {
            var initPersonaData = angular.extend({
                type: personaData.personaEnvironmentTypeId,
                personaTypeId: personaData.personaTypeId
            }, personaData || {});

            if(personaData.fromDate) {
                initPersonaData.startDate = moment(personaData.fromDate);
            }
            if(personaData.thruDate) {
                initPersonaData.endDate = moment(personaData.thruDate);
            }

            list.push(personaModel(initPersonaData));
        };

        vm.setLoading = function() {
        	angular.forEach(vm.isLoading, function(val, key) {
        		vm.isLoading[key] = true;
        	});

    		userFormState.setIsLoading();        	
        };

        vm.setUserProfileLoaded = function() {
        	vm.isLoading.userProfile = false;
        };

		vm.setUserDetailsLoaded = function() {
			vm.isLoading.userDetails = false;
		};

		vm.setUserPersonasLoaded = function() {
			vm.isLoading.userPersonas = false;
		};

        vm.updateTabName = function(evt) {
            //TODO update tab name
            // logc("udpateTabName: %s", event.tabName);
        };

        vm.updateUser = function() {
            var form = userFormState.state.userDetailsForm;

            $scope.$broadcast("rpUpdate:personas");
            
            if (form.$valid) {
                userFormState.setIsLoading();
                updateUserSvc.save(userFormData)
                    .then(vm.processEditUserForm, vm.updateUserErrorCallback)
                    .finally(vm.enableSubmitBtn);
            } else {
                form.$setSubmitted();
            }
        };

        vm.processEditUserForm = function(response) {
            if(response && response.status && response.status.success) {
                $location.path("people/user/" + response.data.realPageId);
            } else {
                notifsSvc.notify({
                    text: response.status.errorMsg,
                    type: "error"
                });
            }
        };

        vm.updateUserErrorCallback = function(response) {
            var message = response.data.data.errorMessage || $filter("editUserText")("err_update_user");
            notifsSvc.notify({
                text: message,
                type: "error"
            });
        };

        vm.enableSubmitBtn = function () {
            userFormState.setIsLoading(false);
        };

		vm.verifyAllLoaded = function(newObj) {
			var flag = false;
			angular.forEach(newObj, function(isLoading) {
				flag = flag || isLoading;
			});

			if(flag === false) {
                $scope.$broadcast("rpInit:userDetails");
                vm.dataReady = true;
				userFormState.setIsLoading(false);
				vm.destroyLoadWatch();				
			}
		};

        vm.cancel = function() {
            //TODO add checker are you sure? 
            $location.path(manageUserModel.getRedirectLink());
        };

        vm.destroyLoadWatch = function() {
            if(vm.loadWatch) {
                vm.loadWatch();
                vm.loadWatch = undefined;
            }
        };

        vm.destroyPersonaWatch = function() {
            if(vm.additionalPersonaWatch) {
                vm.additionalPersonaWatch();
                vm.additionalPersonaWatch = undefined;
            }
        };

    	vm.destroy = function() {
    		vm.watchList.destroy();
            vm.watchList = undefined;

            vm.destroyLoadWatch();
            vm.destroyPersonaWatch();

            userFormModel.reset();
            userFormState.reset();
            
    		vm.realPageId = undefined;
            vm.formState = undefined;
    		vm = undefined;
    	};

    	vm.init();

    }

    angular
        .module("settings")
        .controller("EditUserCtrl", [
            "$scope",
            "$location",
            "$stateParams",
            "$filter",
            "editUserSummaryModel",
            "manageUserFormData",
            "manageUserFormModel",
            "manageUserFormState",
            "personaModel",
            "userStates",
            "manageUserModel",
            "editUserSvc",
            "updateUserSvc",
            "rpWatchList",
            "moment",
            "notificationService",
            "userProfileModel",
        	EditUserCtrl
        ]);
})(angular);