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
                config = [];
            // logc("griddata--", gridData,gridData.Type);
            if (gridData.type === "Multi Select Grid" || gridData.type === "Select Grid") {
                gridData.controls.forEach(function (item) {
                    config.push({
                        "key": item.dataSource,
                        "type": s.isType(item.type),
                        "text": item.displayName,
                        "idKey": "id",
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
                radioData.controls.forEach(function (tabGrp) {
                    tabGrp.controls.forEach(function (tab) {
                        tab.controls.forEach(function (item) {
                            if (item.type === 'Radio') {
                                cnfgs.push({
                                    "key": item.dataSource,
                                    "type": s.isControl(item.type),
                                    "text": item.displayName
                                });
                            }

                        });
                    });

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
                                    logc("attributes", item);
                                    if (item.key === "InfoIcon" && item.value === "Slide") {
                                        isSlideScreen = true;
                                    }
                                });
                            }
                            if (isSlideScreen) {
                                ctrl.controls.forEach(function (subCtrls) {
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

            logc("listaside", listasideConfig);
            return listasideConfig;
        };
        // p.getListAsideConfig = function (data) {
        //     var s = this,
        //         displayName = "",
        //         isSlideScreen = false,
        //         cnfg = [],
        //         cnfgs = [];

        //     if (data && data.controls) {
        //         data.controls.forEach(function (tabGrp) {
        //             tabGrp.controls.forEach(function (tab) {
        //                 tab.controls.forEach(function (tabData) {
        //                     logc("tabData", tabData);
        //                     tabData.controls.forEach(function (tabitem) {
        //                         logc("item", tabitem);
        //                         if (tabitem.type === 'Icon') {
        //                             if (tabitem.attributes !== null) {
        //                                 tabitem.attributes.forEach(function (item) {
        //                                     logc("attributes", item);
        //                                     if (item.key === "InfoIcon" && item.value === "Slide") {
        //                                         isSlideScreen = true;
        //                                     }
        //                                 });
        //                             }
        //                             if (isSlideScreen) {
        //                                 tabitem.controls.forEach(function (ctrls) {
        //                                     if (ctrls.type === "Grid") {
        //                                         displayName = ctrls.displayName;
        //                                         ctrls.controls.forEach(function (ctrl) {
        //                                             cnfgs.push({
        //                                                 "key": ctrl.dataSource,
        //                                                 "type": s.isType(ctrl.type),
        //                                                 "text": ctrl.displayName,
        //                                                 "idKey": "id",
        //                                                 "templateUrl": ""
        //                                             });
        //                                         });
        //                                     }
        //                                 });
        //                             }
        //                         }
        //                     });

        //                 });
        //             });

        //         });
        //     }

        //     logc("listaside", cnfgs);
        //     return cnfgs;
        // };

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

        p.getHeaders = function (tab) {
            var s = this;
            var hdr = [];
            tab.forEach(function (item) {
                if (item.type === 'text') {
                    hdr.push({
                        "key": item.key,
                        "text": item.text
                    });
                }
                if (item.type === 'custom') {
                    hdr.push({
                        "key": item.key,
                    });
                }
                else if (item.type === 'select') {
                    hdr.push({
                        "key": item.key,
                        "type": item.type,
                        "enabled": true
                    });
                }
            });
            return [hdr];
        };

        p.getFilters = function (tab) {
            var s = this;
            var fltr = [];
            tab.forEach(function (item) {
                if (item.type === 'text') {
                    fltr.push({
                        "key": item.key,
                        "text": item.text,
                        "type": item.type,
                        "placeholder": "Filter by " + item.text + " Name"
                    });
                }
                if (item.type === 'custom') {
                    fltr.push({
                        "key": item.key,
                    });
                }
                else if (item.type === 'select') {
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
            logc("type", type, tabName);
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
