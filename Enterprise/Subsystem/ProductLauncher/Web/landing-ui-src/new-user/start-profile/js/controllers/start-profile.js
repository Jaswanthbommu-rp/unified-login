//  Start Profile Controller

(function (angular) {
    "use strict";

    function StartProfileCtrl($scope, $window, $filter, $http, ENV, rpCookie, userModel, startProfileFormModel, startProfileFormConfig, startProfileFormData, startProfileSvc, startProfileOptions, jobTitleSvc, phoneTypeSvc, userDetSvc, profileSvc) {
        var vm = this;

        vm.init = function () {
            vm.errorMsg = "";

            vm.destroyCtrl = $scope.$on("$destroy", vm.destroy);
            vm.optionsWatch = $scope.$watch(startProfileOptions.isReady, vm.initForm);

            vm.initData();

            startProfileFormModel.setData(startProfileFormData);
            startProfileFormConfig.setMethodsSrc(vm);
            vm.startProfileData = startProfileFormData;
            vm.startProfileFormConfig = startProfileFormConfig;

            logc("userModel", userModel);
            vm.isFormDisplay = false;
            vm.initOptions();

        };

        vm.setForm = function (form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
            }
        };

        vm.initData = function () {

            var params = {
                enterpriseUserName: userModel.getEnterpriseUserName()
            };

            userDetSvc.get(params, vm.onDataReady, vm.setDataErr);

        };

        vm.profiletData = function (realpageId) {

            // var authorization = rpCookie.read('access_token');
            // logc("authorization", authorization);
            // if (authorization !== undefined) {
            //     authorization = 'Bearer ' + authorization;
            //     $http.defaults.headers.common.Authorization = authorization;
            // }
            vm.authorize();
            var params = {
                realPageId: realpageId
            };

            profileSvc.get(params, vm.onProfDataReady, vm.setDataErr);

        };

        vm.authorize = function () {

            var authorization = rpCookie.read('access_token');
            if (authorization !== undefined) {
                authorization = 'Bearer ' + authorization;
                $http.defaults.headers.common.Authorization = authorization;
            }
        };

        vm.onDataReady = function (resp) {
            if (resp.isError === false) {
                if (resp.records.length > 0) {
                    vm.profiletData(resp.records[0].realPageId);
                }
            }
        };

        vm.onProfDataReady = function (resp) {
            if (resp.status.success === true) {
                startProfileFormData.telecomNumbers = vm.getPhones(resp.data.telecommunicationNumber);
                startProfileFormData.electronicEmails = vm.getEmails(resp.data.emailContacts);
                startProfileFormData.industryJobTitle = resp.data.partyRole.roleTypeId;
                startProfileFormData.companyJobTitle = resp.data.title;
            }
        };

        vm.getPhones = function (phones) {
            phones.forEach(function (ph) {
                var phonenumber = ph.areaCode + ph.phoneNumber;
                ph.areaCode = "";
                ph.phoneNumber = phonenumber;
            });
            return phones;
        };

        vm.getEmails = function (emails) {
            var a = 0;
            emails.forEach(function (data) {
                if (data.contactMechanismUsageType.contactMechanismUsageTypeId !== 302) {
                    emails.splice(a, 1);
                }
                a++;
            });
            return emails;
        };

        vm.setDataErr = function (resp) {
            logc("eresp", resp);
        };

        vm.checkPhoneValid = function (item) {
            logc("item", item);
        };


        vm.initOptions = function () {
            jobTitleSvc.getList(vm.setJobTitles);
            phoneTypeSvc.getList(vm.setPhoneTypes);
        };

        vm.setJobTitles = function (response) {
            if (response.data) {
                startProfileOptions.initOptions("industryJobTitle", response.data);
            }
            else {
                logc("Error: Unable to get job title options"); //TODO
            }
        };

        vm.setPhoneTypes = function (response) {
            if (response.data) {
                startProfileOptions.initOptions("phoneType", response.data);
            }
            else {
                logc("Error: Unable to get phone type options"); //TODO
            }
        };

        vm.initForm = function (flag) {
            if (flag) {
                vm.optionsWatch(); //remove watcher
                vm.setSelectOptions();
                vm.setDefaultUsername();

                vm.isFormDisplay = true;
            }
        };

        vm.setDefaultUsername = function () {
            var defaultUsername = userModel.getEnterpriseUserName() || "";
            startProfileFormModel.setUsername(defaultUsername);
        };

        vm.setSelectOptions = function () {
            vm.startProfileFormConfig
                .setOptions("industryJobTitle", startProfileOptions.getOptions("industryJobTitle"))
                .setOptions("phoneType", startProfileOptions.getOptions("phoneType"));
        };

        vm.submitStartProfile = function (form) {
            if (form.$valid) {
                startProfileFormModel.setSubmitBtnDisabled();

                var ps = vm.getPhoneNumbers(startProfileFormData.telecomNumbers);
                var phoneNumbers = startProfileFormModel.getParsedPhoneNumbers(ps);
                startProfileSvc.save(startProfileFormData, phoneNumbers, startProfileFormData.electronicEmails)
                    .then(vm.processStartProfileForm)
                    .finally(vm.setSubmitBtnEnabled);
            }
            else {
                form.$setSubmitted();
            }
        };

        vm.getPhoneNumbers = function (phones) {
            var phs = [];
            phones.forEach(function (ph) {
                phs.push(ph);
            });

            return JSON.parse(JSON.stringify(phs));
        };

        vm.processStartProfileForm = function (data) {
            if (!angular.isUndefined(data.errorMessage) && data.errorMessage.trim().length === 0) {
                vm.redirectToLogin();
            }
            else {
                vm.errorMsg = data.errorMessage;
            }
        };

        vm.redirectToLogin = function () {
            $window.location.replace("/home/?msgId=200");
        };

        vm.skipStarterProfile = function () {
            vm.redirectToLogin();
        };

        vm.setSubmitBtnEnabled = function () {
            startProfileFormModel.setSubmitBtnDisabled(false);
        };

        vm.addPhone = function (form) {
            startProfileFormModel.addPhone();
        };

        vm.delPhone = function (item) {
            startProfileFormModel.delPhone(item);
        };

        vm.onChangePh = function (item) {
            // if (item.length === 10) {
            //     startProfileFormModel.getParsedPhoneNumber(item);
            // }
        };

        vm.destroy = function () {
            if (angular.isFunction(vm.optionsWatch)) {
                vm.optionsWatch();
            }
            vm.optionsWatch = undefined;

            startProfileFormModel.reset();
            userModel.reset();


            vm.destroyCtrl();
            vm = undefined;
        };

        vm.init();
    }

    angular
        .module("new-user")
        .controller("StartProfileCtrl", [
            "$scope",
            "$window",
            "$filter",
            "$http", 
            "ENV",
            "rpCookie",
            "userModel",
            "startProfileFormModel",
            "startProfileFormConfig",
            "startProfileFormData",
            "startProfileSvc",
            "startProfileOptionsModel",
            "industryJobTitleSvc",
            "phoneTypeTitleSvc",
            "getUserSvc",
            "userProfileDataSvc",
            StartProfileCtrl
        ]);
})(angular);