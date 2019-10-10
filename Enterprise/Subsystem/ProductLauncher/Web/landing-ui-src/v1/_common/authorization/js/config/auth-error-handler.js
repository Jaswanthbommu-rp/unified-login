//  Global Authorization Error Handler

(function (angular) {
	"use strict";

	function httpInterceptor($provide, $httpProvider) {
		function factory($q, $window, cookie) {
			return {
				response: function (response) {
					return response || $q.when(response);
				},

				responseError: function (rejection) {
					if (rejection.status === 401) {
						logc("401: Access Denied!");
						cookie.erase('access_token');
						$window.location.href = '/home/signout';
					}
					return $q.reject(rejection);
				}
			};
		}

		$provide.factory('httpInterceptor', ['$q', '$window', 'rpCookie', factory]);

		$httpProvider.interceptors.push('httpInterceptor');
	}

	angular
		.module("rpAuthorization")
		.config(['$provide', '$httpProvider', httpInterceptor]);
})(angular);

