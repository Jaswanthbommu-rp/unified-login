//  View User Controller

(function (angular) {
    "use strict";

    function ViewUserCtrl($scope, $stateParams, userSummaryModel, manageUserModel, userStates,
            userFormData, userFormModel, userFormState, personaModel,
			editUserSvc, rpWatchList, moment, userProfileModel) {
    	var vm = this;

    	vm.init = function() {
    		vm.watchList = rpWatchList();
            vm.watchList.add($scope.$on("$destroy", vm.destroy));

            userProfileModel.subscribe(vm.userProfileUpdated);
            manageUserModel.setState(userStates.VIEW_USER);
            userFormModel.setData(userFormData);

            vm.formState = userFormState.state;

    		vm.realPageId = $stateParams.userId;
            vm.model = userSummaryModel;

            vm.dataReady = false;
            vm.isLoading = {
                userProfile: false,
                userPersonas: false
            };
            vm.loadWatch = $scope.$watchCollection("page.isLoading", vm.verifyAllLoaded);

    		vm.initUserDetails();
    	};

        vm.userProfileUpdated = function () {
            vm.initUserDetails();
        };

    	vm.initUserDetails = function() {
            vm.setLoading();

            userFormModel.setRealPageId(vm.realPageId);

    		editUserSvc.getUserProfile(vm.realPageId)
                .then(vm.setUserProfileData);

            editUserSvc.getUserPersonas(vm.realPageId)
                .then(vm.setUserPersonas);
    	};

         vm.setLoading = function() {
            angular.forEach(vm.isLoading, function(val, key) {
                vm.isLoading[key] = true;
            });
        };

        vm.verifyAllLoaded = function(newObj) {
            var flag = false;
            angular.forEach(newObj, function(isLoading) {
                flag = flag || isLoading;
            });

            if(flag === false) {
                vm.dataReady = true;
                vm.destroyLoadWatch();              
            }
        };

        vm.destroyLoadWatch = function() {
            if(vm.loadWatch) {
                vm.loadWatch();
                vm.loadWatch = undefined;
            }
        };

        vm.setUserProfileData = function(response) {
        	if(response.isError) {
                userSummaryModel.setError(response);
            } else {
                userProfileModel.setData(response.userSummary);
                userSummaryModel.setData(response);
            }
            vm.isLoading.userProfile = false;
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
            vm.isLoading.userPersonas = false;
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

    	vm.destroy = function() {
    		vm.watchList.destroy();
            vm.watchList = undefined;

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
        .controller("ViewUserCtrl", [
            "$scope",
            "$stateParams",
            "viewUserSummaryModel",
            "manageUserModel",
            "userStates",
            "manageUserFormData",
            "manageUserFormModel",
            "manageUserFormState",
            "personaModel",
            "viewUserSvc",
            "rpWatchList",
            "moment",
            "userProfileModel",
        	ViewUserCtrl
        ]);
})(angular);