//  User Request Status Message Model

(function (angular, undefined) {
    "use strict";

    function factory($filter) {
        function UserReqStatusMsgModel() {
            var s = this;
            s.init();
        }

        var p = UserReqStatusMsgModel.prototype;

        p.init = function () {
            var s = this;

            s.data = {
                msg: "",
                title: "",
                success: true,
                detailErrorMsgs: [],
                returnPath: ""
            };

            s.lang = $filter("userBaseText");
            s.dataCopy = angular.copy(s.data);
        };

        // Setters

        p.setData = function (data) {
            var s = this,
                success = true,
                mainMsgKey = "statusMsg.msg",
                titleKey = "statusMsg.title";

            s.data.detailErrorMsgs = [];

            data.tabStatus.forEach(function (state) {
                success = success && state.success;

                if (!state.success) {
                    if (state.errorCode)
                    {
                        state.msg = s.lang(state.errorCode);
                    }
                    s.data.detailErrorMsgs.push(state.msg);
                }
            });

            titleKey += success ? ".success" : ".error";
            mainMsgKey += data.userExists ? ".editUser" : ".addUser";
            mainMsgKey += success ? ".success" : ".error";

            s.data.success = success;
            s.data.msg = s.lang(mainMsgKey);
            s.data.title = s.lang(titleKey);

            return s;
        };

        // Assertions

        p.onGoToUsers = function () {
            var s = this;
            return s.data.success;
        };

        // Reset

        p.reset = function () {
            var s = this;
            s.data = angular.copy(s.dataCopy);
        };

        return new UserReqStatusMsgModel();
    }

    angular
        .module("settings")
        .factory("userReqStatusMsgModel", ["$filter", factory]);
})(angular);
