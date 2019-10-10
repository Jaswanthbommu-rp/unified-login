//  Object Security Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function ObjectSecurity() {
            var s = this;
            s.init();
        }

        var p = ObjectSecurity.prototype;

        p.init = function () {
            var s = this;
            s.rightsData = {
                allowEdit: [],
                allowDelete: false
            };
            s.actionsData = {};
        };

        p.setData = function (data) {
            var s = this;
            s.rightsData = data || {};
            return s;
        };

        p.setActionsData = function (actionsData) {
            var s = this;
            s.actionsData = actionsData || {};
            return s;
        };

        p.actionAllowed = function (actionName) {
            var s = this,
                allowed = false,
                list = s.actionsData[actionName],
                allowEdit = s.rightsData.allowEdit;

            if (!allowEdit || !allowEdit.push) {
                logw("ObjectSecurity: Editables list was not found!");
            }
            else if (!list || !list.push) {
                logw("ObjectSecurity: Data for action name %s was not found!", actionName);
            }
            else {
                allowed = true;

                list.forEach(function (key) {
                    allowed = allowed && allowEdit.contains(key);
                });
            }

            return allowed;
        };

        p.deleteAllowed = function () {
            var s = this;

            if (s.rightsData.allowDelete === undefined) {
                logw("ObjectSecurity: Data for right to delete was not found!");
                s.rightsData.allowDelete = false;
            }

            return s.rightsData.allowDelete;
        };

        p.destroy = function () {
            var s = this;
            s.data = undefined;
            s.actionsData = undefined;
        };

        return function (data) {
            return (new ObjectSecurity()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("objectSecurityModel", [factory]);
})(angular);
