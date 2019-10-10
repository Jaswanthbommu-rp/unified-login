(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
	        "cancel",
			"create_user",
			"user_details"
        ];

        appLangKeys.app("people.user.add").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
