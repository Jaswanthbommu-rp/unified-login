(function (angular, undefined) {
  "use strict";

  function UserPreferenceSvc(ENV, $resource, http) {
    var svc = this;

    svc.getCategories = function () {
     return http.get(ENV.notificationsApiRoot + "/User/preferences/categories");

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

        var url = ENV.landingAPI + "api/user/" + realPageId + "/products",
        actions = {
          get: {
            method: "GET",
            cancellable: true,
          },
        };

      return $resource(url, {}, actions);
    };

    svc.updatePeferences = function () {
      var url, defaults, actions;

       defaults = {};

       url = ENV.notificationsApiRoot + "/User/preferences";
       actions = {
            update: {
                method: "PUT",
                cancellable: true
            }
        };

        return $resource(url, defaults, actions);
    };
  }

  angular
    .module("settings")
    .service("UserPreferenceSvc", ["ENV", "$resource","$http", UserPreferenceSvc]);
})(angular);
