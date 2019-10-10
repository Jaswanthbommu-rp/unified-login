(function (angular, undefined) {
    "use strict";

    function factory(
        $filter,
        gridConfig,
        gridModel,
        gridTransformSvc,
        gridPaginationModel,
        persona,
        dataSvcGet
    ) {
        var model = {},
            cfg = {
                recordsPerPage: 10
            };

        model.init = function () {
            model.search = "";
            model.searchUser = "";          
            return model;
        };

        model.initGrid = function () {
            var grid = gridModel(),
                gridTransform = gridTransformSvc(),
                gridPagination = gridPaginationModel();
            model.grid = grid;
            model.grid.busy(true);
            gridTransform.watch(grid);
            grid.setConfig(gridConfig);

            gridPagination.setConfig(cfg);
            gridPagination.setGrid(grid).trackSelection(gridConfig.getTrackSelectionConfig());

            model.gridPagination = gridPagination;
            if (persona.isReady()) {
                model.getDataSvc();
            }
            else {
                model.personaWatch = persona.subscribe(model.getDataSvc);
            }

            return model;
        };


        model.getDataSvc = function () {
            if (persona.isReady()) {
                var params = {
                    editorPersonaId: persona.getId(),
                    filter: model.search
                };
                dataSvcGet.get(params, model.setDataFromSvc, model.setDataErr);
            }
        };

        model.getSelectedRecords = function () {
            return model.grid.getSelectionChanges();
        };

        model.setData = function (data) {
            model.data = data;
        };

        model.getData = function () {
            return model.data;
        };

        model.setDataFromSvc = function (data) {
            model.extendData(data);
            model.setData(data);
            model.setFilteredDataToGrid(data.records);
        };

        model.extendData = function (data) {
            data.records.forEach(function (item) {
                angular.extend(item, {
                    more: "more",
                    filterData: item.companyName + " " + item.address + " " + item.city + " " + item.state + " " + item.country + " " + item.postalCode,
                    phoneNumber: item.phoneNumber === "" ? "" : model.formatPhone(item.phoneNumber)
                });
            });

        };

        model.setDataErr = function (data) {
            logc("Error = > ", data);
        };

        model.setFilteredDataToGrid = function (d) {
            var data = {
                records: d
            };
            model.gridPagination.flushBackupData();
            model.grid.busy(false);
            model.gridPagination.setData(data.records).goToPage({
                number: 0
            });
        };

        model.getFilteredData = function (inp) {
            var filObj = [];
            model.getData().records.forEach(function (item) {
                var i = item.filterData.toLowerCase().indexOf(inp);
                if (i !== -1) {
                    filObj.push(item);
                }
            });

            return filObj;
        };

        model.searchFilter = function (inp) {
            if (inp === "" || inp === undefined) {
                var data = model.getData().records;
                model.setFilteredDataToGrid(data);
            }
            else {
                var fildata = model.getFilteredData(inp);
                model.setFilteredDataToGrid(fildata);
            }

            return model;
        };

        model.formatPhone = function (tel) {
            if (!tel) {
                return '';
            }

            var value = tel.toString().trim().replace(/^\+/, '');

            if (value.match(/[^0-9]/)) {
                return tel;
            }

            var country, city, number;

            switch (value.length) {
            case 10: // +1PPP####### -> C (PPP) ###-####
                country = 1;
                city = value.slice(0, 3);
                number = value.slice(3);
                break;

            default:
                return tel;
            }

            if (country == 1) {
                country = "";
            }

            number = number.slice(0, 3) + '-' + number.slice(3);

            return (country + " (" + city + ") " + number).trim();
        };

        // model.getMenuData = function() {
        //     return [{
        //         label: "Company",
        //         value: "Company"
        //     },{
        //         label: "User",
        //         value: "User"
        //     }];
        // };


        model.reset = function () {
            model.personaWatch();
            model.search = "";
            model.grid.destroy();
            model.gridPagination.destroy();
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("empAccessModel", [
            "$filter",
            "empAccessCompGridConfig",
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "empAccessCompSvc",
            factory
        ]);
})(angular);
