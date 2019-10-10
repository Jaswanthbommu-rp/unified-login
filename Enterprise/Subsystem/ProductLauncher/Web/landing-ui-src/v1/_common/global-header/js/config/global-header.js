//  Global Header Config

(function (angular) {
    "use strict";

    function config(headerModel, cdnVer) {
        headerModel.extendData({
            showLogo: true,
			logoLink: "",
            logoImg1Src: cdnVer + "/lib/realpage/global-header/images/rp-logo-24x22.svg"
        });

        headerModel.setToolbarIcons({
            homeIcon: {
                url: "#/",
                active: true
            },

            appSwitcher: {
                active: true
            }
        });
    }

    angular
        .module("settings")
		.run(["rpGlobalHeaderModel", "cdnVer", config]);
})(angular);
