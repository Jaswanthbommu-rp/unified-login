(function (angular, undefined) {
  "use strict";

  function UserPreferenceSvc(ENV, $resource) {
   
    var svc = this;

    svc.getCategories = function () {
      return $resource(
        ENV.notificationsApiRoot + "/User/preferences/categories"
      ).get().$promise;
    };

    svc.getLookupData = function () {
      return $resource(ENV.notificationsApiRoot + "/Lookup").get().$promise;
    };

    svc.getUserProducts = function (realPageId) {
      return $resource(
        ENV.landingAPI + "/user/" + realPageId + "/products"
      ).get().$promise;
    };

    svc.updatePeferences = function (request) {
      return $resource(
        ENV.notificationsApiRoot + "/User/preferences",
        request,
        {
          update: {
            method: "PUT",
          },
        }
      );
    };
  }

  angular
    .module("settings")
    .service("UserPreferenceSvc", ["ENV", "$resource", UserPreferenceSvc]);
})(angular);
