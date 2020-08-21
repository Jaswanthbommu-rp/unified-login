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
            if (gridData.type === "Multi Select Grid" || gridData.type === "Select Grid" || gridData.type === "Grid" ) {
                filterType = undefined;
                gridData.controls.forEach(function (item) {
                    if (item.attributes !== null) {
                        item.attributes.forEach(function (data) {
                            if (data.key === "FilterType" && data.value === "menu") {
                                filterType = "menu";
                            }
                        });
                    }

                    config.push({
                        "key": item.dataSource,
                        "type": s.isType(item.type),
                        "text": item.displayName,
                        "idKey": "id",
                        "disabledKey": "disableSelection",
                        "filterType": filterType,
                        "templateUrl": s.getTemplate(s.isControl(item.type), tabName, item.displayName)
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

        p.getListAsideConfig = function (data, roleType, linkType) {
            var s = this,
                displayName = "",
                filterType,
                isSlideScreen = false,
                listasideConfig = {
                    displayName: "",
                    config: []
                },
                cnfg = [],
                cnfgs = [];

            if (data && data.controls) {
                data.controls.forEach(function (ctrl) {
                    if (ctrl.type === "Icon" || ctrl.type === "Link") {
                        if (ctrl.attributes !== null) {
                            ctrl.attributes.forEach(function (item) {
                                if ((item.key === "InfoIcon" || item.key === "AssignedProperties" || item.key === "AssignedGroups") && item.value === "Slide" && item.key.toLowerCase() === linkType){
                                    isSlideScreen = true;
                                }
                            });
                        }
                        if (isSlideScreen) {
                            ctrl.controls.forEach(function (subCtrls) {
                                filterType = undefined;
                                logc("sub controls", subCtrls);
                                if (subCtrls.type === "Grid" || subCtrls.type === "Multi Select Grid") {
                                    listasideConfig.displayName = roleType !== "" ?  roleType : subCtrls.displayName;
                                    subCtrls.controls.forEach(function (gridCtrl) {
                                        var columnName = (roleType !== "" && gridCtrl.dataSource === "name") ? roleType :  gridCtrl.displayName;

                                        if (gridCtrl.attributes !== null) {
                                            gridCtrl.attributes.forEach(function (data) {
                                                if (data.key === "FilterType" && data.value === "menu") {
                                                    filterType = "menu";
                                                }
                                            });
                                        }
                                        listasideConfig.config.push({
                                            "key": gridCtrl.dataSource,
                                            "type": s.isType(gridCtrl.type),
                                            "text": columnName,
                                            "filterType": filterType,
                                            "idKey": "id",
                                            "templateUrl": s.getTemplate(s.isControl(gridCtrl.type), gridCtrl.dataSource, gridCtrl.displayName)
                                        });
    
                                    });
                                    
                                    
                                }
                            });
                        }
                        isSlideScreen = false;
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
            else if (type === 'Radio' || type === 'Dropdown' || type === 'Icon' || type === 'Link') {
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
            else if (type === 'Link') {
                return 'link';
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
                    if(item.key === 'assignedProperties' || item.key === 'assignedGroups'){
                        hdr.push({
                            "key": item.key,
                            "text": item.text
                        });
                    }else{
                        hdr.push({
                            "key": item.key,
                        });
                    }
                   
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

        p.getFilters = function (tab, optionvalues) {
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

                if (item.type === 'text' && item.key === 'propertyType' && item.filterType === "menu") {
                    var items = [];
                    items.push({
                        value: "",
                        name: "All"
                    });
                    if(optionvalues.length > 0){
                        optionvalues.forEach(function (item) {
                            items.push({
                                value: item,
                                name: item
                            });
                        });
                    }
                    fltr.push({
                        "key": item.key,
                        "type": "menu",
                        "value": "",
                        "options": items
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

                if (item.type === 'select' || (item.type === 'custom' && item.key !== 'InfoIcon' && item.key !== 'assignedGroups' && item.key !== 'assignedProperties')) {
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
                if (item.type === 'custom' && (item.key === 'InfoIcon' || item.key === 'assignedProperties' || item.key === 'assignedGroups')) {
                    fltr.push({
                        "key": item.key,
                        "type": "",
                        "value": ""
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
                        "type": item.type
                    });
                }
                if (item.type === 'custom') {
                    main.push({
                        "key": item.key,
                        "type": item.type,
                        "templateUrl": item.templateUrl,
                        "idKey": item.idKey,
                        "disabledKey": item.disabledKey
                    });
                }
                else if (item.type === 'select') {
                    main.push({
                        "key": item.key,
                        "type": item.type,
                        "idKey": item.idKey,
                        "disabledKey": item.disabledKey
                    });
                }
            });
            return main;
        };

        p.getTemplate = function (type, tabName, displayName) {
            var html = '',
                url = '';
            //logc("type", type, tabName);
            if (type === 'radio') {
                url = "user/assign-product-access/product-panel/templates/" + tabName.toLowerCase() + "-radio.html";
            }
            else if (type === 'icon') {
                url = "user/assign-product-access/product-panel/templates/product-panel-info-icon.html";
            }
            else if (type === 'link' && displayName === 'Assigned Groups') {
                url = "user/assign-product-access/product-panel/templates/product-panel-label-group-link.html";
            }
            else if (type === 'link') {
                url = "user/assign-product-access/product-panel/templates/product-panel-label-link.html";
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
