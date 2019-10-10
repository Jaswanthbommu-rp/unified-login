// Raul Client Portal Service

(function (angular, undefined) {
	"use strict";

	function RaulSwitchCompanySvc() {
		var svc = this;

		svc.activate = function () {
			svc.api.show();
		};

		svc.setElemApi = function (elemApi) {
			svc.api = elemApi;
		};
	}

	angular.module("settings")
		.service("raulSwitchCompanySvc", [
			RaulSwitchCompanySvc
		]);
})(angular);