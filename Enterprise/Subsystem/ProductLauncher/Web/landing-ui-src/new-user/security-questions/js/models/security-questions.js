//  Security Questions Form Model

(function (angular) {
    "use strict";

    function factory(baseForm) {
        var model = baseForm();

        model.setSubmitBtnDisabled = function (state) {
            model.form.submitBtnDisabled = state === undefined || state === true ? true : false;

            return model;
        };

        return model;
    }

    angular
        .module("new-user")
        .factory("securityQuestionsFormModel", [
        	"baseForm",
        	factory
        ]);
})(angular);
