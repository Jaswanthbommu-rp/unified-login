// Change Password Notification

(function (angular) {
    "use strict";

    function ChangePasswordNotificationSvc($filter, $interval, notification) {
        var svc = this,
            severity = { //severity level returned by the server
                none: "none",
                info: "information",
                warn: "warning",
                critical: "critical"
            };

        svc.pwdExpired = false;

        svc.setPwdExpired = function (bool) {
            svc.pwdExpired = bool;
        };

        svc.replaceNumber = function (countdown, msg) {
            return msg.replace("###", countdown);
        };

        //notification type is based on pNotify notification types
        svc.getNotificationType = function (severityLevel) {
            var level = severityLevel.toLowerCase();
            if (level == severity.info) {
                return "info";
            }
            else if (level == severity.warn) {
                return "warning";
            }
            else if (level == severity.critical) {
                return "error";
            }
            return null;
        };

        svc.notifyPwdExpiration = function (countdown, severityLevel, callback) {
            var msg = "",
                checkCount = 0,
                filter = $filter("notificationText");

            svc.langCheckInterval = $interval(function () {
                checkCount++;

                var ready = !!filter("pwd_notice_change_pwd_last_msg");

                if (ready || checkCount > 10) {
                    $interval.cancel(svc.langCheckInterval);

                    if (ready) {
                        if (countdown > 1) {
                            msg = svc.replaceNumber(countdown, filter("pwd_notice_change_pwd_msg"));
                        }
                        else {
                            msg = filter("pwd_notice_change_pwd_last_msg");
                        }

                        svc.showPwdExpirationNfn({
                            msg: msg,
                            severityLevel: severityLevel,
                            moreDetailsText: filter("pwd_notice_change_pwd"),
                            callback: callback
                        });
                    }
                    else {
                        logw("Lang check expired");
                    }
                }
            }, 100);
        };

        svc.showPwdExpirationNfn = function (data) {
            svc.pwdNotification = notification({
                width: "450px",
                hide: false,
                type: svc.getNotificationType(data.severityLevel),
                closeBtnEnabled: true,
                moreDetailsEnabled: true,
                closer: true,
                msgText: data.msg,
                moreDetailsCallback: data.callback,
                moreDetailsText: data.moreDetailsText,
                moreDetailsClassName: "change-password-link"
            });
        };

        svc.notifyPwdExpired = function (callback) {
            if (svc.langCheckIntervalExp) {
                return;
            }

            var msg = "",
                checkCount = 0,
                filter = $filter("notificationText");

            svc.langCheckIntervalExp = $interval(function () {
                checkCount++;

                var ready = !!filter("pwd_notice_pwd_expired_msg");


                if (ready || checkCount > 10) {
                    $interval.cancel(svc.langCheckIntervalExp);
                    svc.langCheckIntervalExp = undefined;

                    if (ready && svc.pwdExpired) {

                        msg = filter("pwd_notice_pwd_expired_msg");

                        svc.showPwdExpiredNfn({
                            msg: msg
                        });

                        svc.pwdExpired = false;
                    }
                    else {
                        logw("couldn't find text, count exceeded");
                    }
                }
            }, 100);
        };

        svc.showPwdExpiredNfn = function (data) {
            svc.pwdNotification = notification({
                width: "380px",
                type: "error",
                closer: false,
                // closeBtnEnabled: true,
                moreDetailsEnabled: false,
                msgText: data.msg
            });
        };

        svc.closePwdNotification = function () {
            if (svc.pwdNotification) {
                svc.pwdNotification.close();
            }
        };
    }

    angular
        .module("settings")
        .service("userAccountNotificationSvc", [
            "$filter",
            "$interval",
            "rpNotificationModel",
            ChangePasswordNotificationSvc
        ]);
})(angular);
