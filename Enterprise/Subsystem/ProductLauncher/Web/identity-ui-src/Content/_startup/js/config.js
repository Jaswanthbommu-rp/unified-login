//  App Startup

(function () {
    "use strict";

    RealPage.startup.load({
        appName: "identity",

        files: [
            {
                js: "/login/{{cdnVer}}/lib/app/js/scripts.min.js",
				css: "/login/{{cdnVer}}/lib/app/css/styles.min.css"
            },
            {
				js: "/login/content/app/js/scripts.min.js",
				css: "/login/content/app/css/styles.min.css"
            }
        ]
    });
})();
