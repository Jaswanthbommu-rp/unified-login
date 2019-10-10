//  Activity Log Model

(function (angular, undefined) {
    "use strict";

    function factory(activityLogItem) {
        function ActivityLogModel() {
            var s = this;
            s.init();
        }

        var p = ActivityLogModel.prototype;

        p.init = function () {
            var s = this;
            s.data = [];
            s.activeData = [];
        };

        p.setData = function (data) {
            var s = this;
            s.data = [];
            data.forEach(function (item) {
                s.data.push(activityLogItem(item));

            });

            s.activeData = s.data;

            return s;
        };

        p.showUserActivity = function () {
            var s = this;

            s.activeData = s.data.filter(function (item) {
                return item.isUserActivity();
            });

            return s;
        };

        p.hasActivities = function () {
            var s = this;
            return s.data.length;
        };

        p.reset = function () {
            var s = this;
            s.data = [];
        };

        return new ActivityLogModel();
    }

    angular
        .module("settings")
        .factory("activityLogModel", ["activityLogItemModel", factory]);
})(angular);
