//  Activity Log Item Model

(function (angular, undefined) {
    "use strict";

    function factory(moment) {
        function ActivityLogItemModel() {
            var s = this;
            s.init();
        }

        var p = ActivityLogItemModel.prototype;

        p.init = function () {
            var s = this;
            s.data = {
                activityId: -1,
                logCategoryName: null,
                logActivityTypeName: null,
                message: null,
                fromUserLoginName: null,
                fromUserLoginId: 0,
                toUserLoginName: null,
                toUserLoginId: 0,
                organizationId: 0,
                productName: null,
                productModuleName: null,
                productModuleStepName: null,
                serverName: null,
                applicationTimestamp: null,
                activityDate: null,
                additionalInformation: null,
                isUserDetails: true,
                isUserActivity: true
            };
        };

        p.setData = function (data) {
            var s = this;
            angular.extend(s.data , data || {});
            s.setActivityDate();
            return s;
        };

        p.setActivityDate = function () {
            var s = this;
            var hours,
                activityDate = s.data.applicationTimestamp;

            if (activityDate) {
                activityDate = moment(activityDate).toDate();
            }
            else {
                activityDate = "";
            }

            s.data.activityDate = activityDate;
        };

        p.isUserActivity = function () {
            var s = this;
            return s.data.isUserActivity;
        };

        p.destroy = function () {
            var s = this;
            s.data = undefined;
        };

        return function (data) {
            return (new ActivityLogItemModel()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("activityLogItemModel", ["moment", factory]);
})(angular);
