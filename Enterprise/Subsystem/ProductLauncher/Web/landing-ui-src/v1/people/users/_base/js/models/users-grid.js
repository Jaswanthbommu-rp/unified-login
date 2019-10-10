//  Properties Grid

(function (angular) {
    "use strict";

    function gridFactory(usersSvc, usersGridConfig, rpGridModel, rpGridTransform, User, moment, gridPaginationModel, userModel, security, usersGridConfigFactory, customFieldSvc) {
        var model = {},
            gridModel = null,
            gridTransform = null;

        model.init = function () {
            model.isCustomColFlag = true;
            model.customFieldsSettings = [];
            model.customFieldColName = "";
            model.gridModel = gridModel = rpGridModel();
            model.gridPagination = gridPaginationModel();
            gridModel.setConfig(usersGridConfig);
            gridModel.subscribe('filterBy', model.load);
            gridModel.subscribe('sortBy', model.load);
            model.isGridLoading = false;
            // gridModel.subscribe('paginate', grid.paginate);
            //
            model.gridPagination.setGrid(model.gridModel);
            model.gridPagination.setConfig({
                recordsPerPage: 50
            });

            model.currentFilter = {};

            gridTransform = rpGridTransform();
            gridTransform.watch(model.gridModel);
            usersGridConfig.setSrc(model);

            model.getCustomDataDataSvc();

            model.load();
            return model;
        };

        model.getData = function () {
            return model.gridModel.getData();
        };

        model.load = function () {
            gridModel.busy(true).flushData();
            model.cancelRequest();

            model.currentFilter = model.getQuery();
            model.request = usersSvc.UserList().getList(model.currentFilter, model.populateGrid);
        };

        model.getCustomDataDataSvc = function () {
            var params = {};
            customFieldSvc.get(params, model.setDataFromSvc, model.setDataErr);
        };

        model.setDataFromSvc = function (data) {
            if (data.records && data.records.length > 0) {
                model.isCustomFldSeqOneDisabled(data);
                model.setCustomFldSetData(data.records);
            }
            else {
                model.setIsCustomColVisible(false);
            }
            if (model.getIsCustomColVisible()) {
                model.setGridCustomFieldCol(model.customFieldColName);
            }
        };

        model.isCustomFldSeqOneDisabled = function (data) {
            model.isCustomColFlag = false;
            var i = 0;
            data.records.forEach(function (item) {
                if (i === 0) {
                    model.isCustomColFlag = true;
                    model.customFieldColName = item.fieldName;
                    i++;
                }
            });
        };

        model.setCustomFldSetData = function (data) {
            model.customFieldsSettings = data;
        };

        model.getCustomFldSetData = function () {
            return model.customFieldsSettings;
        };

        model.setDataErr = function (data) {
            logc("Error = > ", data);
        };

        model.getQuery = function (paramData) {
            var dataFilter = gridModel.paginationModel.data,
                filterByJson = {},
                sortByJson = {};

            if (dataFilter.filterBy) {
                filterByJson = model.getFilterByJSON(dataFilter.filterBy);
            }

            if (dataFilter.sortBy) {
                sortByJson = model.getSortByJSON(dataFilter.sortBy);
            }

            return {
                "datafilter.filterBy": filterByJson,
                "datafilter.sortBy": sortByJson,
                "datafilter.pages.resultsPerPage": dataFilter.pages.resultsPerPage,
                "datafilter.pages.startRow": dataFilter.pages.startRow,
                "include": "summaryCount, userLogin"
            };
        };

        model.getFilterByJSON = function (obj) {
            var returnObj = {};
            angular.forEach(obj, function (prop, key) {
                if (prop.toString().length > 0) {
                    switch (key) {
                    case "name":
                        returnObj.name = prop;
                        break;

                    case "userType":
                        returnObj.usertype = prop;
                        break;

                    case "status":
                        returnObj["status"] = prop;
                        break;

                    case "isLocked":
                        if (prop === true) {
                            returnObj["lockStatus"] = "locked";
                        }
                        else {
                            returnObj["lockStatus"] = "unlocked";
                        }
                        break;

                    case "product":
                        returnObj["productid"] = prop;
                        break;

                    case "property":
                        break; //TODO
                    }
                }
            });

            return returnObj;
        };

        model.getSortByJSON = function (obj) {
            var returnObj = {};
            angular.forEach(obj, function (prop, key) {
                if (prop.length > 0) {
                    switch (key) {
                    case "user":
                        returnObj.name = prop;
                        break;

                    case "productCount":
                        returnObj.products = prop;
                        break;

                    case "lastLogin":
                        returnObj.lastlogin = prop;
                        break;

                    case "propertyCount": //TODO not yet implemented in the service level, waiting for Blue Books integration
                        returnObj.properties = prop;
                        break;

                    case "accountStatus": //TODO not yet implemented in the service level, waiting for schema change
                        returnObj.status = prop;
                        break;

                    }
                }
            });

            return returnObj;
        };

        model.cancelRequest = function () {
            if (model.request !== undefined && model.request !== null) {
                model.request.$cancelRequest();
                model.request = null;
            }
        };

        model.populateGrid = function (response) {
            if (model.isCustomColFlag) {
                model.setCustomField(response);
            }
            var userList = model.prepareList(response);
            userList.totalRecords = userList.records.length;
            gridModel.setData(userList).busy(false);
            model.gridPagination.setData(userList.records).goToPage({
                number: 0
            });
        };

        model.filterList = function (filterData) {
            gridModel.getEvents().publish("filterBy", filterData);
        };

        model.filterByName = function (name, filterData) {
            var obj = angular.copy(filterData);
            obj.name = name;
            gridModel.getEvents().publish("filterBy", obj);
        };

        model.prepareList = function (response) {
            var gridRecords = [];

            if (response.status.success === true) {
                if (response && response.data.length > 0) {
                    angular.forEach(response.data, function (currRecord) {
                        //get summary counts
                        var recordProductCount = 0,
                            recordPropertyCount = 0;
                        if (currRecord.summaryCounts) {
                            recordPropertyCount = currRecord.summaryCounts.properties || 0;
                            recordProductCount = currRecord.summaryCounts.products || 0;
                        }

                        //convert dates
                        var hours,
                            lastLoginDate = currRecord.userLogin.lastLogin;

                        if (lastLoginDate) {
                            lastLoginDate = moment(lastLoginDate).toDate();
                            hours = lastLoginDate.getHours();
                            lastLoginDate.setHours(hours);
                        }
                        else {
                            lastLoginDate = "";
                        }

                        //create a new instance of user record
                        var newUser = new User({
                            realPageId: currRecord.realPageId,
                            user: currRecord.firstName + " " + currRecord.middleName + " " + currRecord.lastName,
                            username: currRecord.userLogin.loginName,
                            productCount: recordProductCount,
                            propertyCount: recordPropertyCount,
                            lastLogin: lastLoginDate,
                            lockStatus: currRecord.userLogin.isLocked,
                            expiredStatus: currRecord.userLogin.isExpired,
                            activeStatus: currRecord.userLogin.isActive,
                            pendingStatus: currRecord.userLogin.isPending,
                            accountStatus: currRecord.userLogin.status,
                            imgSrc: currRecord.avatar,
                            userProfileLink: "#/user/" + currRecord.userLogin.realPageId + "/UserList" + "/edit",
                            disableSelection: currRecord.realPageId === userModel.getRealPageId() ? true : false,
                            hasEditUserAccess: (security.isAllowed("editUser") || security.isAllowed("viewUser")) ? true : false,
                            userType: model.getUserType(currRecord.userLogin.userRoleType), //=== 402 ? "SuperUser" : "RegularUser",
                            userRoleType: currRecord.userLogin.userRoleType,
                            customFieldCol: currRecord.customFieldCol === undefined ? "" : currRecord.customFieldCol
                        });
                        gridRecords.push(newUser);
                    });
                }
            }
            return {
                records: gridRecords
            };
        };

        model.getUserType = function (userRoleType) {
            var ret = "RegularUser";
            switch (userRoleType) {
            case 402:
                ret = "SuperUser";
                break;

            case 403:
                ret = "RealPageEmployee";
                break;
            }

            return ret;
        };

        model.setCustomField = function (resp) {
            if (model.customFieldColName !== "") {
                resp.data.forEach(function (item) {
                    item.customFieldObj.forEach(function (cust) {
                        if (cust.fieldName === model.customFieldColName) {
                            angular.extend(item, {
                                "customFieldCol": cust.fieldValue
                            });
                            // return;
                        }
                    });
                });
            }
            else {
                model.customFieldsSettings.forEach(function (item) {
                    // if (item.sequence === 1) {
                    if (item.fieldName === model.customFieldColName) {
                        // model.customFieldColName = item.fieldName;
                        angular.extend(item, {
                            "customFieldCol": model.customFieldColName === "" ? "Custom Field" : model.customFieldColName
                        });
                    }
                });

            }

            // model.setGridCustomFieldCol(model.customFieldColName, "customFieldCol");
        };

        model.setGridCustomFieldCol = function (name) {
            var getKeys = usersGridConfig.get();
            var getHeaders = usersGridConfig.getHeaders();
            var data = {};

            getHeaders[0].forEach(function (item) {
                if (item.key === "customFieldCol") {
                    item.text = name;
                    //This makes the custom field sortable.  At this time it is not an option.
                    //Restore this line when the API is fixed to handle it.
                    // item.isSortable = name === "" ? false : true;
                }
            });

            data.getKeys = getKeys;
            data.getHeaders = getHeaders;

            usersGridConfig = usersGridConfigFactory(data);
            model.gridModel.setConfig(usersGridConfig);
            //model.setIsCustomColVisible(true); // for testing only
        };

        model.updateSelectAll = function (val) {
            if (gridModel.gridSelectModel === null || angular.isUndefined(gridModel.gridSelectModel)) {
                return;
            }

            var flag = (val === false) ? false : true;
            gridModel.gridSelectModel.updateSelected(flag);
            gridModel.gridSelectModel.publishState();
        };

        model.getIsCustomColVisible = function () {
            return model.isCustomColFlag;
        };


        model.setIsCustomColVisible = function (val) {
            model.isCustomColFlag = val;
        };

        model.updatedUserLogins = function (selectAll) {
            var userObj = [];

            if (selectAll !== "all") {
                gridModel.data.records.filter(function (item) {
                    if (item.isSelected) {
                        userObj.push({
                            "realPageId": item.realPageId,
                            "status": item.accountStatus
                        });
                    }
                });
            }

            return userObj;
        };

        model.getUserLoginsForEmailResent = function () {
            var userObj = [];

            gridModel.data.records.filter(function (item) {
                if (item.isSelected && item.userRoleType !== 404 && (item.accountStatus === "Expired" || item.accountStatus === "Pending")) {
                    userObj.push({
                        "realPageId": item.realPageId
                    });
                }
            });

            return userObj;
        };

        model.setIsGridLoading = function (flag) {
            model.isGridLoading = flag === undefined || flag;
            if (model.isGridLoading) {
                //gridModel.busy(true);
            }
        };

        model.hasSelectedLockedUsers = function () {
            var records = gridModel.getData().records;

            records = records.filter(function (record) {
                return record.lockStatus && record.isSelected;
            });

            return records.length !== 0;
        };

        model.destroy = function () {
            model.cancelRequest();
            model.request = undefined;
            model.currentFilter = undefined;

            model.gridModel.flushData();
            model.gridModel.destroy(); //TODO verify
            model.gridModel = gridModel = undefined;

            gridTransform.destroy();
            gridTransform = undefined;

            model.gridPagination.destroy();
            model.gridPagination = undefined;
        };

        return model;
    }

    angular
        .module("settings")
        .factory("userListGridModel", [
            "userListSvc",
            "userListGridConfig",
            "rpGridModel",
            "rpGridTransform",
            "userListRowModel",
            "moment",
            "rpGridPaginationModel",
            "userSessionModel",
            "routeSecurity",
            "userListGridConfigFactory",
            "usersCustomFieldSvc",
            gridFactory
        ]);
})(angular);
