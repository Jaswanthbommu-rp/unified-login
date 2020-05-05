//Config Data model

(function (angular, undefined) {
    "use strict";

    function factory($templateCache) {
        function ConfigData() {
            var s = this;
            s.init();
        }

        var p = ConfigData.prototype;

        p.init = function () {
            var s = this;

        };

        p.getGridConfigTypes = function (gridData, tabName) {
            var s = this,
                filterType,
                config = [];
            // logc("griddata--", gridData,gridData.Type);
            if (gridData.type === "Multi Select Grid" || gridData.type === "Select Grid") {
                filterType = undefined;
                if (item.attributes !== null) {
                    item.attributes.forEach(function (data) {
                        if (data.key === "FilterType" && data.value === "menu") {
                            filterType = "menu";
                        }
                    });
                }

                gridData.controls.forEach(function (item) {
                    config.push({
                        "key": item.dataSource,
                        "type": s.isType(item.type),
                        "text": item.displayName,
                        "idKey": "id",
                        "filterType": filterType,
                        "templateUrl": s.getTemplate(s.isControl(item.type), tabName)
                    });
                });
            }

            return config;
        };

        p.getRadioConfig = function (radioData) {
            var s = this,
                cnfg = [],
                cnfgs = [];

            if (radioData && radioData.controls) {
                radioData.controls.forEach(function (item) {
                    if (item.type === 'Radio') {
                        cnfgs.push({
                            "key": item.dataSource,
                            "type": s.isControl(item.type),
                            "text": item.displayName
                        });
                    }
                });
            }

            return cnfgs;
        };

        p.getListAsideConfig = function (data) {
            var s = this,
                displayName = "",
                isSlideScreen = false,
                listasideConfig = {
                    displayName: "",
                    config: []
                },
                cnfg = [],
                cnfgs = [];

            if (data && data.controls) {
                data.controls.forEach(function (ctrl) {
                    if (ctrl.type === "Icon") {
                        if (ctrl.attributes !== null) {
                            ctrl.attributes.forEach(function (item) {
                                if (item.key === "InfoIcon" && item.value === "Slide") {
                                    isSlideScreen = true;
                                }
                            });
                        }
                        if (isSlideScreen) {
                            ctrl.controls.forEach(function (subCtrls) {
                                logc("sub controls", subCtrls);
                                if (subCtrls.type === "Grid") {
                                    listasideConfig.displayName = subCtrls.displayName;
                                    subCtrls.controls.forEach(function (gridCtrl) {
                                        listasideConfig.config.push({
                                            "key": gridCtrl.dataSource,
                                            "type": s.isType(gridCtrl.type),
                                            "text": gridCtrl.displayName,
                                            "idKey": "id"
                                        });

                                    });
                                }
                            });
                        }
                    }
                });
            }

            //logc("listaside", listasideConfig);
            return listasideConfig;
        };

        p.isType = function (type) {
            var s = this;
            if (type === 'Label') {
                return 'text';
            }
            else if (type === 'Radio' || type === 'Dropdown' || type === 'Icon') {
                return 'custom';
            }
            else if (type === 'CheckBox' || type === 'Checkbox') {
                return 'select';
            }

            return '';
        };

        p.isControl = function (type) {
            var s = this;
            if (type === 'Label') {
                return '';
            }
            else if (type === 'Radio') {
                return 'radio';
            }
            else if (type === 'CheckBox' || type === 'Checkbox') {
                return 'check';
            }
            else if (type === 'Icon') {
                return 'icon';
            }
        };

        p.getHeaders = function (tab, val) {
            var s = this;
            var hdr = [];
            tab.forEach(function (item) {
                if (item.type === 'text') {
                    hdr.push({
                        "key": item.key,
                        "text": item.text
                    });
                }
                else if (item.type === 'custom') {
                    hdr.push({
                        "key": item.key,
                    });
                }
                else if (item.type === 'select') {
                    hdr.push({
                        "key": item.key,
                        "type": item.type,
                        "enabled": val
                    });
                }
            });
            return [hdr];
        };

        p.getFilters = function (tab) {
            var s = this;
            var fltr = [];
            tab.forEach(function (item) {
                if (item.type === 'text' && item.filterType === undefined) {
                    fltr.push({
                        "key": item.key,
                        "text": item.text,
                        "type": item.type,
                        "placeholder": "Filter by " + item.text + " Name"
                    });
                }

                if (item.type === 'text' && item.key === "roletype" && item.filterType === "menu") {
                    fltr.push({
                        "key": item.key,
                        "value": "",
                        "type": "menu",
                        "options": [{
                                value: "",
                                name: "All"
                            },
                            {
                                value: "Custom",
                                name: "Custom"
                            },
                            {
                                value: "System",
                                name: "System"
                            }
                        ]
                    });
                }

                if (item.type === 'select' || (item.type === 'custom' && item.key !== 'InfoIcon')) {
                    fltr.push({
                        "key": item.key,
                        "type": "menu",
                        "value": "",
                        "options": [
                            {
                                value: "",
                                name: "All"
                            },
                            {
                                value: true,
                                name: "Selected"
                            },
                            {
                                value: false,
                                name: "Not Selected"
                            }
                        ]
                    });
                }
            });
            return fltr;
        };

        p.getMain = function (tab) {
            var s = this;
            var main = [];
            tab.forEach(function (item) {
                if (item.type === 'text') {
                    main.push({
                        "key": item.key,
                        "type": item.type,
                    });
                }
                if (item.type === 'custom') {
                    main.push({
                        "key": item.key,
                        "type": item.type,
                        "templateUrl": item.templateUrl,
                        "idKey": item.idKey
                    });
                }
                else if (item.type === 'select') {
                    main.push({
                        "key": item.key,
                        "type": item.type,
                        "idKey": item.idKey
                    });
                }
            });
            return main;
        };

        p.getTemplate = function (type, tabName) {
            var html = '',
                url = '';
            //logc("type", type, tabName);
            if (type === 'radio') {
                url = "user/assign-product-access/product-panel/templates/" + tabName.toLowerCase() + "-radio.html";
            }
            else if (type === 'icon') {
                url = "user/assign-product-access/product-panel/templates/product-panel-info-icon.html";
            }

            return url;
        };



        p.reset = function () {
            var s = this;

        };

        return new ConfigData();
    }

    angular
        .module("settings")
        .factory("configDataModel", ['$templateCache', factory]);
})(angular);
