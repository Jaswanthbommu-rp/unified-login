//  Search Form Config

(function(angular, undefined) {
    "use strict";

    function factory(baseFormConfig, inputTextConfig) {
        var model = baseFormConfig();

        
         model.search = inputTextConfig({
            required: false,
            iconClass: "fa fa-search",
            placeholder : "Search by Company Name or Company Location",
            onChange: model.getMethod("filterInput"),
        });

         model.searchUser = inputTextConfig({
            required: false,
            iconClass: "fa fa-search",
            placeholder : "Search by Name, Username or Email",
            onChange: model.getMethod("filterInputUser"),            
            validators: {
                        minlengthchk: model.getMethod("checkChars")
                    },
            errorMsgs: [{
                name: "minlengthchk",
                text: "This search requires at least five characters"
            }]
        });

        

        return model;
    }

    angular
        .module("settings")
        .factory("empAccessFormConfig", [
            "baseFormConfig",
            "rpFormInputTextConfig",            
            factory
        ]);
})(angular);