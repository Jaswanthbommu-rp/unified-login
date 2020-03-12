//  External User Modal

(function (angular, undefined) {
    "use strict";

    function factory(modal) {
        return modal("user/user-details/templates/existing-user-noemail-other-comp-modal.html");
    }

    angular
        .module("settings")
        .factory("existingNoEmailUserOthCompModal", ["rpModalModel", factory]);
})(angular);
