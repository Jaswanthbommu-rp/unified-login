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

        // p.setProperties = function (propertiesData) {
        //     var s = this;
        //     s.properties = propertiesData;
        // };


        p.getGridConfigTypes = function (gridData, tabName) {
            var s = this, config = [];
           // logc("griddata--", gridData,gridData.Type);
            if(gridData.type === "MultiSelectGrid" || gridData.type === "Select Grid"){
                gridData.controls.forEach(function (item){
                    config.push({
                                "key" : item.dataSource,
                                "type" : s.isType(item.type),
                                "text": item.displayName,
                                "idKey": "id",
                                "templateUrl" : s.getTemplate( s.isControl(item.type), tabName)
                        });
                });
            }
            // if(gridData.type === "MultiSelectGrid" || gridData.type === "Select Grid"){
            //     gridData.controls.forEach(function (ctrl){
            //       if (ctrl.Type === "GridColums") {
            //         ctrl.Controls.forEach(function (item) {
            //           config.push({
            //                     "key" : item.DataSource,
            //                     "type" : s.isType(item.Type),
            //                     "text": item.DisplayName,
            //                     "idKey": "id",
            //                     "templateUrl" : s.getTemplate( s.isControl(item.Type), tabName)
            //             });
            //         });
            //       }
            //     });
            // }
            return config;
        };

        p.getRadioConfig = function (radioData) {
            var s = this, cnfg = [];
            logc("radioData radio", radioData);
             if(radioData && radioData.controls){
                radioData.controls.forEach(function (tabGrp) {
                    tabGrp.controls.forEach(function (tab) {
                      var cnfgs = [];  
                      tab.controls.forEach(function (item) {
                                logc("radio item",item);
                            if(item.type === 'Radio' ){
                                cnfgs.push({
                                    "key" : item.dataSource,
                                    "type" : s.isControl(item.type),
                                    "text": item.displayName
                                });
                            }

                        });
                      cnfg.push(cnfgs);
                    });

                });
            }
            return cnfg;
        };


        p.isType = function (type) {
            var s = this;
            if(type === 'Label'){
                return 'text';
            }
            else if(type === 'Radio'  || type === 'Dropdown' || type === 'Custom'){
                return 'custom';
            }
            else if(type === 'CheckBox' ){
                return 'select';
            }

            return '';
        };

        p.isControl = function (type) {
            var s = this;
            if(type === 'Label'){
                return '';
            }
            else if(type === 'Radio'){
                return 'radio';
            }
            else if(type === 'CheckBox'){
                return 'check';
            }
            else if(type === 'Custom'){
                return 'info';
            }
        };

        p.getHeaders = function (tab) {
            var s = this;
            var hdr = [];
            tab.forEach(function (item) {
                if(item.type === 'text'){
                    hdr.push({
                        "key" : item.key,
                        "text" : item.text
                    });
                }
                if(item.type === 'custom'){
                    hdr.push({
                        "key" : item.key,
                    });
                }
                else if(item.type === 'select'){
                    hdr.push({
                        "key" : item.key,
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
                if(item.type === 'text'){
                    fltr.push({
                        "key" : item.key,
                        "text" : item.text,
                        "type" : item.type,
                        "placeholder": "Filter by " + item.text + " Name"
                    });
                }
                if(item.type === 'custom'){
                    fltr.push({
                        "key" : item.key,
                    });
                }
                else if(item.type === 'select'){
                    fltr.push({
                        "key" : item.key,
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
                if(item.type === 'text'){
                    main.push({
                        "key" : item.key,
                        "type" : item.type,
                    });
                }
                if(item.type === 'custom'){
                    main.push({
                        "key" : item.key,
                        "type" : item.type,
                        "templateUrl" :  item.templateUrl,
                        "idKey" : item.idKey
                    });
                }
                else if(item.type === 'select'){
                    main.push({
                        "key" : item.key,
                        "type" : item.type,
                        "idKey" : item.idKey
                    });
                }
            });
           return main;
        };

        p.getTemplate = function(type, tabName){
            var html = '', url ='';

            if(type === 'radio'){
                url = "user/assign-product-access/product-panel/templates/" + tabName.toLowerCase() +"-radio.html";
            }
            
            if(type === 'info'){
                url = "user/assign-product-access/product-panel/templates/info-icon.html";
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
