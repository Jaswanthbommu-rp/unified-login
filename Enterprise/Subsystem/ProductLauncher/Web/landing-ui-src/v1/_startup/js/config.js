//  App Startup

(function () {
    "use strict";

    RealPage.startup.load({
        appName: "settings",

        files: [
            {
                js: "../{{cdnVer}}/lib/app/js/scripts.min.js",
                css: "../{{cdnVer}}/lib/app/css/styles.min.css"
            },
            {
                js: "app/js/scripts.min.js",
                css: "app/css/styles.min.css"
            }
        ]
    });
})();
