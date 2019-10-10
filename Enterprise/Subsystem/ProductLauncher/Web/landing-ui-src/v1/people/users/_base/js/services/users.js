// Users  Service

(function (angular) {
    "use strict";

    function userListSvc($resource, ENV) {
        var svc = {};

        svc.url = {
            userList: ENV.landingAPI + "api/persons",
            updateUserLogin: ENV.landingAPI + "api/userlogin/status",
            updateBatchUserLogin: ENV.landingAPI + "api/userlogins",
            resendInvitation: ENV.landingAPI + "api/userlogins/resendinvitation",
        };

        svc.UserList = function () {
            var actions = {
                getList: {
                    method: "GET",
                    cancellable: true
                }
            };

            return $resource(svc.url.userList, {}, actions);
        };

        svc.updateUser = function (params, data) {
            var actions = {
                update: {
                    method: "POST"
                }
            };
            return $resource(svc.url.updateUserLogin, {}, actions).update(params, data).$promise;
        };

        svc.batchUpdateUsers = function (params, data) {
            var actions = {
                patchUpdate: {
                    method: "PATCH"
                }
            };

            return $resource(svc.url.updateBatchUserLogin, {}, actions).patchUpdate(params, data).$promise;
        };

        svc.resendInvitation = function (params, data) {
            var actions = {
                update: {
                    method: "POST"
                }
            };
            return $resource(svc.url.resendInvitation, {}, actions).update(params, data).$promise;
        };

        //svc.UserLogin = function(isUpdateAll, updateStatus, realPageIds) {
        //    var actions = {
        //        update: {
        //            method: "PUT"
        //        }
        //    },
        //    params = {
        //        updateType: null
        //    },
        //    updateObj = {
        //        userLoginStatusType: updateStatus
        //    };

        //    if(isUpdateAll) {
        //        params.updateType = "allRecords";
        //    } else {
        //        params.updateType = "batch";

        //        updateObj.userLoginWithRealPageIdList = [];
        //        angular.forEach(realPageIds, function(currRealPageId) {
        //            updateObj.userLoginWithRealPageIdList.push({
        //                realPageId: currRealPageId
        //            });
        //        });
        //    }

        //    return $resource(svc.url.updateUserLogin, params, actions).update(updateObj).$promise;
        //};


        return svc;
    }

    angular
         .module("settings")
         .factory("userListSvc", [
             "$resource",
             "ENV",
             userListSvc
         ]);
})(angular);
