(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
        	"cancel",
			"clone_user",
			"update_user",

            "err_update_user"
        ];

        appLangKeys.app("people.user.edit").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
