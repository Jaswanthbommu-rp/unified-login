//  Activity Log Payload Model

(function (angular, undefined) {
    "use strict";

    function factory($stateParams, moment) {
        function ActivityLogPayloadModel() {
            var s = this;
            s.init();
        }

        var p = ActivityLogPayloadModel.prototype;

        p.init = function () {
            var s = this;
            s.activityPayload = undefined;
            s.startDatePayload = undefined;
            s.endDatePayload = undefined;
            s.keywordPayload = undefined;
            s.timezoneOffset = undefined;
            s.timeStr = '18:25:03';
            s.setDateRange("1-CM");
            s.setStartDate(moment().startOf('month'));
            s.setEndDate(moment());
            s.setSortOrderPayload();
        };

        p.buildPayload = function (singleUser, exportFlag) {
            var s = this;
            var payload = {
                "activitySearchCriteria": [],
                "rowsPerPage": 0,
                "pageNumber": 1
            };
            payload.sortOrder = s.sortOrderPayload.sortOrder;
            payload.sortOrderColumnName = s.sortOrderPayload.sortOrderColumnName;
            if (singleUser) {
                payload.activitySearchCriteria.push(s.getFromUserIdPayload());
            }
            payload.activitySearchCriteria.push(s.startDatePayload);
            payload.activitySearchCriteria.push(s.endDatePayload);
            if (s.keywordPayload) {
                payload.activitySearchCriteria.push(s.keywordPayload);
            }
            if (s.activityPayload) {
                payload.activitySearchCriteria.push(s.activityPayload);
            }
            if (exportFlag) {
                // payload.offsetMinutes = s.timezoneOffset;
                payload.activitySearchCriteria.push(s.timezoneOffsetPayload);
                payload.rowsPerPage = 0;
            }
            return payload;
        };

        p.setSortOrderPayload = function (sortOrder) {
            var s = this;
            var payload = "";
            if (sortOrder) {
                var sortOrderParts = sortOrder.split("-");
                s.sortOrderPayload = {
                    "sortOrder": sortOrderParts[1],
                    "sortOrderColumnName": sortOrderParts[0]
                };
            }
            else {
                s.sortOrderPayload = {
                    "sortOrder": "desc",
                    "sortOrderColumnName": "ApplicationTimeStamp"
                };
            }
        };

        p.getFromUserIdPayload = function () {
            var s = this;
            var payload = {
                "name": "RealpageId",
                // "name": "FromUserId",
                "value": $stateParams.realPageId
            };
            return payload;
        };

        p.buildKeywordPayload = function (keyword) {
            var s = this;
            var keywordPayload = {};

            if (keyword) {
                keywordPayload = {
                    "name": "message",
                    "value": keyword
                };
            }
            else {
                keywordPayload = undefined;
            }

            s.keywordPayload = keywordPayload;
        };

        p.buildActivityPayload = function (activityValue) {
            var s = this;
            var activityPayload = {};
            if (activityValue != "") {
                activityPayload = {
                    "name": "LogTypeId",
                    "value": activityValue
                };
            }
            else {
                activityPayload = undefined;
            }
            s.activityPayload = activityPayload;
        };
        p.setStartDate=function(startDate){
            var s = this,
             offset = moment().utcOffset(),
            dateStr= moment(startDate).subtract(offset, "m").format("YYYY-MM-DD");
            var startdate = dateStr + ' ' +  s.timeStr;
            s.startDatePayload = {
                "name": "StartDate",
                "value": startdate
            };
            
        };
        p.setEndDate=function(endDate){
            var s = this,
            offset = moment().utcOffset(),
            dateStr= moment(endDate).subtract(offset, "m").format("YYYY-MM-DD");
            var enddate = dateStr + ' ' +  s.timeStr;
            s.endDatePayload = {
                "name": "EndDate",
                "value": enddate
            };
        };
        p.setDateRange = function (daterange) {
            var s = this;
            var startDate, endDate;
            var dateAdjust = daterange.split("-");
            var offset = moment().utcOffset();
            s.timezoneOffsetPayload = {
                "name": "OffsetMinutes",
                "value": offset
            };
            if (dateAdjust[1] === "M" || dateAdjust[1] === "w") {
                startDate = moment().subtract(dateAdjust[0], dateAdjust[1]).startOf(dateAdjust[1]).subtract(offset, "m").format("YYYY-MM-DD HH:mm:ss");
                endDate = moment().subtract(1, dateAdjust[1]).endOf(dateAdjust[1]).subtract(offset, "m").format("YYYY-MM-DD HH:mm:ss");
            }
            else if (dateAdjust[1] === "CM") {
                startDate = moment().startOf('M').subtract(offset, "m").format("YYYY-MM-DD HH:mm:ss");

                endDate = moment().endOf('day').subtract(offset, "m").format("YYYY-MM-DD HH:mm:ss");
            }
            else if (dateAdjust[1] === "yday") {
                startDate = moment().subtract((dateAdjust[0]), "d").startOf('day').subtract(offset, "m").format("YYYY-MM-DD HH:mm:ss");
                endDate = moment().subtract((dateAdjust[0]), "d").endOf('day').subtract(offset, "m").format("YYYY-MM-DD HH:mm:ss");
            }
            else if (dateAdjust[1] === "ytd") {
                startDate = moment().subtract((dateAdjust[0]), "d").startOf('year').subtract(offset, "m").format("YYYY-MM-DD HH:mm:ss");
                endDate = moment().endOf('day').subtract(offset, "m").format("YYYY-MM-DD HH:mm:ss");
            }
            else {
                startDate = moment().subtract((dateAdjust[0]), dateAdjust[1]).startOf('day').subtract(offset, "m").format("YYYY-MM-DD HH:mm:ss");
                endDate = moment().endOf('day').subtract(offset, "m").format("YYYY-MM-DD HH:mm:ss");
            }
            s.startDatePayload = {
                "name": "StartDate",
                "value": startDate
            };
            s.endDatePayload = {
                "name": "EndDate",
                "value": endDate
            };
        };

        p.reset = function () {
            var s = this;
            s.activityPayload = undefined;
            s.startDatePayload = undefined;
            s.endDatePayload = undefined;
            s.keywordPayload = undefined;
            s.setDateRange("1-CM");
            s.setSortOrderPayload();
        };

        return new ActivityLogPayloadModel();
    }

    angular
        .module("settings")
        .factory("activityLogPayloadModel", [
            "$stateParams",
            "moment",
            factory
        ]);
})(angular);
