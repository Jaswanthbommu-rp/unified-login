// Assign Products Keys

(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
            "text.title",

            "text.familyTitle.100",
            "text.familyTitle.200",
            "text.familyTitle.300",
            "text.familyTitle.400",
            "text.familyTitle.500",
            "text.familyTitle.600",
            "text.familyTitle.700",

            "text.solnTitle.101",
            "text.solnTitle.102",
            "text.solnTitle.104",
            "text.solnTitle.105",
            "text.solnTitle.107",
            "text.solnTitle.110",
            "text.solnTitle.111",
            "text.solnTitle.112",
            "text.solnTitle.201",
            "text.solnTitle.204",
            "text.solnTitle.205",
            "text.solnTitle.206",
            "text.solnTitle.302",
            "text.solnTitle.303",
            "text.solnTitle.305",
            "text.solnTitle.306",
            "text.solnTitle.307",
            "text.solnTitle.308",
            "text.solnTitle.309",
            "text.solnTitle.310",
            "text.solnTitle.311",
            "text.solnTitle.401",
            "text.solnTitle.402",
            "text.solnTitle.403",
            "text.solnTitle.404",
            "text.solnTitle.406",
            "text.solnTitle.407",
            "text.solnTitle.408",
            "text.solnTitle.409",
            "text.solnTitle.410",
            "text.solnTitle.501",
            "text.solnTitle.503",
            "text.solnTitle.504",
            "text.solnTitle.505",
            "text.solnTitle.601",
            "text.solnTitle.701"
        ];

        appLangKeys.app("assignProducts").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
