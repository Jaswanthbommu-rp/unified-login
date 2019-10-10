// Raul Client Portal Service

(function (angular, undefined) {
	"use strict";

	function RaulClientPortalSvc() {
		var svc = this;

		svc.activate = function () {
			svc.api.show();
			return svc;
		};

		svc.setElemApi = function (elemApi) {
			svc.api = elemApi;
		};

		svc.setLink = function (url) {
			svc.api.setLink(url);
		};
	}

	angular.module("settings")
		.service("raulClientPortalSvc", [
			RaulClientPortalSvc
		]);
})(angular);