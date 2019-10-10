// Raul Manage Profile Service

(function (angular, undefined) {
	"use strict";

	function RaulManageProfileSvc() {
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
		.service("raulManageProfileSvc", [
			RaulManageProfileSvc
		]);
})(angular);