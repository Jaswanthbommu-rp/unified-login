
(function (angular) {
    "use strict";

    var links = {
        productLearningPortal: "https://www.realpagelearning.com/",
        clientPortal: "https://www.realpage.com/clientportal",
        realpageMain: "https://www.realpage.com"
    };

    angular
        .module("settings")
        .value("externalLinks", links);

})(angular);