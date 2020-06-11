
(function (angular, undefined) {
  "use strict";

  function UserPreferenceCtrl(
    $q,
    $scope,
    $stateParams,
    pubsub,
    session,
    svc,
    model
  ) {
    var vm = this;

    vm.init = function () {
      vm.userPreferencesList = [];
      vm.open = true;
      vm.products =[{name:'Notification',isActive:true, id:1}];
      vm.model = model;
      vm.getCategories();
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
      svc.getUserProducts(realPageId).then(function(products){
         model.bindCategories(resp,products);
      });
    };

    vm.menuSelected = function(row) {
        model.data.products[0].items.forEach(function(element) {
          element.isSelected = element.name === row.name ? true : false;
        });
        model.getTypeListData(row.name);
    };

    vm.saveData = function(){
        var request = [];
        model.data.masterData.forEach(function(item) {
          item.userPreferences.forEach(function(preference) {
            request.push({
              categoryId: preference.categoryId,
              preferenceId: preference.preferenceId,
              isEnabled: preference.isEnabled
            });
          });
        });
        svc.updatePeferences(request).then(function(resp) {
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
      UserPreferenceCtrl,
    ]);
})(angular);
