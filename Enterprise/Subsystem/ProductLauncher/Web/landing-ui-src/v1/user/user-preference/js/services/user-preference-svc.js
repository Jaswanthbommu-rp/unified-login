(function (angular, undefined) {
  "use strict";

  function UserPreferenceSvc(ENV, $resource) {
    var svc = this;

    svc.getCategories = function () {
      var url = ENV.notificationsApiRoot + "/User/preferences/categories",
        actions = {
          get: {
            method: "GET",
            cancellable: true,
          },
        };

      return $resource(url, {}, actions);
    };

    svc.getLookupData = function () {
      var url = ENV.notificationsApiRoot + "/Lookup",
        actions = {
          get: {
            method: "GET",
            cancellable: true,
          },
        };

      return $resource(url, {}, actions);
    };

    svc.getUserProducts = function (realPageId) {

        var url = ENV.landingAPI + "/user/" + realPageId + "/products",
        actions = {
          get: {
            method: "GET",
            cancellable: true,
          },
        };

      return $resource(url, {}, actions);
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
