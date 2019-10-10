//  Change Usertype Confirmation Model

(function (angular, undefined) {
    "use strict";

    function factory(eventStream) {
        function ChangeUserType() {
            var s = this;
            s.init();
        }

        var p = ChangeUserType.prototype;

        p.init = function () {
            var s = this;
            s.stream = eventStream();
            s.changeMode = "none";
            s.changeModes = {
            	ToNoEmail: "ToNoEmail",
            	SuperToRegular: "SuperToRegular",
            	RegularToSuper: "RegularToSuper",
            	NoEmailToRegular: "NoEmailToRegular",
                NoEmailToSuper: "NoEmailToSuper",
                NoEmailToExternal: "NoEmailToExternal",

                SuperToExternal: "SuperToExternal",
                ExternalToSuper: "ExternalToSuper",
                ExternalToRegular: "ExternalToRegular",
                RegularToExternal: "RegularToExternal",
                
            };
        };

        p.publish = function () {
        	var s = this;
        	s.stream.publish.apply(s.stream, arguments);
        	return s;
        };

        p.subscribe = function () {
        	var s = this;
        	return s.stream.subscribe.apply(s.stream, arguments);
        };

        p.setChangeMode = function (changeMode) {
        	var s = this;
        	s.changeMode = changeMode;
        };

        p.chgSuperToRegular = function () {
        	var s = this;
        	return (s.changeMode === s.changeModes.SuperToRegular);
        };

        p.chgToNoEmail = function () {
        	var s = this;
        	return (s.changeMode === s.changeModes.ToNoEmail);
        };

        p.chgRegularToSuper = function () {
        	var s = this;
        	return (s.changeMode === s.changeModes.RegularToSuper);
        };

        p.chgNoEmailToRegular = function () {
        	var s = this;
        	return (s.changeMode === s.changeModes.NoEmailToRegular);
        };

        p.chgNoEmailToExternal = function () {
            var s = this;
            return (s.changeMode === s.changeModes.NoEmailToExternal);
        };        

        p.chgNoEmailToSuper = function () {
            var s = this;
            return (s.changeMode === s.changeModes.NoEmailToSuper);
        };

        p.chgExternalToSuper = function () {
            var s = this;
            return (s.changeMode === s.changeModes.ExternalToSuper);
        };

        p.chgExternalToRegular = function () {
            var s = this;
            return (s.changeMode === s.changeModes.ExternalToRegular);
        };

        p.chgRegularToExternal = function () {
            var s = this;
            return (s.changeMode === s.changeModes.RegularToExternal);
        };

        p.chgSuperToExternal = function () {
            var s = this;
            return (s.changeMode === s.changeModes.SuperToExternal);
        };

                        
        p.destroy = function () {
            var s = this;
            s.stream.destroy();
            s.stream = undefined;
            s.changeMode = "";
        };

        return new ChangeUserType();
    }

    angular
        .module("settings")
        .factory("changeUserTypeConfirmationModel", ["eventStream", factory]);
})(angular);
