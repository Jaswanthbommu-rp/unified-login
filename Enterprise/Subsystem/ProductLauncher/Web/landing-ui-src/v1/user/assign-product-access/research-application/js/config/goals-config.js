//  New Role Form Config

(function (angular, undefined) {
    "use strict";

    function factory(baseFormConfig,  inputTextConfig) {
        var model = baseFormConfig();

        model.noDupMrg = inputTextConfig({           
            required: true,            
            minlength : 1,   
            maxlength : 5,
            pattern: /^[0-9]+$/i,
            errorMsgs: [{
                name: "required",
                text: "This field is required"
            },{
                name: "pattern",
                text: "Invalid input"
            }],
            onChange: model.getMethod("goalsChange")            
        });        

        model.spdProcssDup = inputTextConfig({
            required: true,            
            minlength : 1,   
            maxlength : 5,
            pattern: /^[0-9]+$/i,
            errorMsgs: [{
                name: "required",
                text: "This field is required"
            }],
            onChange: model.getMethod("goalsChange")             
        });        

        model.noIncmpRec = inputTextConfig({
            required: true,            
            minlength : 1,   
            maxlength : 5,
            pattern: /^[0-9]+$/i,
            errorMsgs: [{
                name: "required",
                text: "This field is required"
            }],
            onChange: model.getMethod("goalsChange")             
        });        

        model.noUnqRec = inputTextConfig({
            required: true,            
            minlength : 1,   
            maxlength : 5,
            pattern: /^[0-9]+$/i,
            errorMsgs: [{
                name: "required",
                text: "This field is required"
            }],
            onChange: model.getMethod("goalsChange")             
        });      

        model.noUpdMstr = inputTextConfig({
            required: true,            
            minlength : 1,   
            maxlength : 5,
            pattern: /^[0-9]+$/i,
            errorMsgs: [{
                name: "required",
                text: "This field is required"
            }],
            onChange: model.getMethod("goalsChange")             
        });       

        model.noRecEsc = inputTextConfig({
            required: true,            
            minlength : 1,   
            maxlength : 5,
            pattern: /^[0-9]+$/i,
            errorMsgs: [{
                name: "required",
                text: "This field is required"
            }],
            onChange: model.getMethod("goalsChange")             
        });         

        return model;
    }

    angular
        .module("settings")
        .factory("resAppGoalsFormConfig", [
            "baseFormConfig",            
            "rpFormInputTextConfig",            
            factory
        ]);
})(angular);
