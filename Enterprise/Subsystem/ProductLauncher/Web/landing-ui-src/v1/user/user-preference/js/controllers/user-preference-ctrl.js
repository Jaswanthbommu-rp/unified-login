(function (angular, undefined) {
  "use strict";

  function UserPreferenceCtrl(
    $q,
    $scope,
    $stateParams,
    pubsub,
    session,
    svc,
    model,
    tabs
  ) {
    var vm = this;

    vm.init = function () {
      vm.userPreferencesList = [];
      vm.open = true;
      vm.destWatch = $scope.$on("$destroy", vm.destroy);
      vm.products = [{ name: "Notification", isActive: true, id: 1 }];
      vm.formWatch = $scope.$watch("userPreferenceForm", vm.setForm);
      vm.model = model;
      vm.register();
      vm.getCategories();
    };

    vm.setForm = function (form) {
        if (form) {
            vm.form = form;
            vm.formWatch();
        }
    };

    vm.setSubmitted = function () {
        vm.form.$setSubmitted();
    };

    vm.isDirty = function() {
        return model.isDirty();
    };

    vm.isValid = function() {
        return vm.isDirty() ? vm.form.$valid : true;
    };

    vm.hasSaveFn = function() {
        return vm.isDirty() && vm.isValid();
    };


    vm.getCategories = function () {
      svc.getLookupData().then(vm.bindLookUpData);

      svc.getCategories().then(vm.bindCategries);
    };

    vm.bindLookUpData = function (resp) {
      vm.LookUpData = resp;
      model.updatePreferenceList(resp);
    };

    vm.bindCategries = function (resp) {
      var realPageId = session.getRealPageId();
      svc.getUserProducts(realPageId).then(function (products) {
        model.bindCategories(resp, products);
      });
    };

    vm.menuSelected = function (row) {
      model.data.products[0].items.forEach(function (element) {
        element.isSelected = element.name === row.name ? true : false;
      });
      model.getTypeListData(row.name);
    };

    vm.saveData = function () {
      vm.saveReq = $q.defer();
      var request = [];
      if(model.data.masterData && model.data.masterData.length){
          model.data.masterData.forEach(function (item) {
            item.userPreferences.forEach(function (preference) {
              request.push({
                categoryId: preference.categoryId,
                preferenceId: preference.preferenceId,
                isEnabled: preference.isEnabled,
              });
            });
          });
          svc.updatePeferences(request).then(vm.onSaveSuccess, vm.onSaveError);
      }
      return vm.saveReq.promise;
    };

    vm.onSaveError = function () {
      vm.saveReq.reject({
        success: false,
        tabName: "userPreference",
      });
    };

    vm.onSaveSuccess = function (resp) {
      vm.saveReq.resolve({
        success: true,
        tabName: "userPreference",
      });
    };

    vm.register = function () {
      tabs.register({
        ctrl: vm,
        name: "userPreference",
      });
    };

    vm.destroy = function () {
      vm.destWatch();
      model.reset();
      vm = undefined;
      $scope = undefined;
    };

    vm.init();
  }

  angular
    .module("settings")
    .controller("UserPreferenceCtrl", [
      "$q",
      "$scope",
      "$stateParams",
      "pubsub",
      "userSessionModel",
      "UserPreferenceSvc",
      "UserPreferenceModel",
      "userTabsManager",
      UserPreferenceCtrl,
    ]);
})(angular);
