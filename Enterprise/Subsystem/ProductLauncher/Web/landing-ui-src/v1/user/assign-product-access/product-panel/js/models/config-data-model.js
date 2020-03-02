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

        p.setProperties = function (propertiesData) {
            var s = this;
            s.properties = propertiesData;
        };


        p.getGridConfigTypes = function (gridData) {
            var s = this, config = [];
           // logc("griddata--", gridData,gridData.Type);

            if(gridData.Type === "MultiSelectGrid"){
                gridData.Controls.forEach(function (ctrl){
                  if (ctrl.Type === "GridColums") {
                    ctrl.Controls.forEach(function (item) {
                      config.push({
                                "key" : item.DataSource,
                                "type" : s.isType(item.Type),
                                "text": item.DisplayName,
                                "idKey": "id",
                                "templateUrl" : s.getTemplate( s.isControl(item.Type) )
                        });
                    });
                  }
                });
            }
            return config;
        };

        p.getRadioConfig = function (jsonData) {
            var s = this, cnfg = [], cnfgs = [];
//logc("jsonData radio", jsonData);
             if(jsonData && jsonData.Controls){
                jsonData.Controls.forEach(function (tabGrp) {
                      //if(tabGrp.Type === "TabGroup"){
                            tabGrp.Controls.forEach(function (item) {
                               // logc("radio item",item);
                                //cnfg = [];
                                if(item.Type === 'RadioButton' ){
                                    cnfgs.push({
                                        "key" : item.DataSource,
                                        "type" : s.isControl(item.Type),
                                        "text": item.DisplayName
                                    });
                                }
                                // item.Controls.forEach(function (ctrl) {
                                //     logc("radio ctrl", ctrl);

                                // });
                                // if(cnfg.length > 0){
                                //     cnfgs.push(cnfg);
                                // }
                            });
                       // }
                });
            }
logc("jsonData radio", cnfgs);
            return cnfgs;
        };


        p.isType = function (type) {
            var s = this;
            if(type === 'Label'){
                return 'text';
            }
            else if(type === 'RadioButton' || type === 'CheckBox' || type === 'Dropdown'){
                return 'custom';
            }

            return '';
        };

        p.isControl = function (type) {
            var s = this;
            if(type === 'Label'){
                return '';
            }
            else if(type === 'RadioButton'){
                return 'radio';
            }
            else if(type === 'CheckBox'){
                return 'checkbox';
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
            });
           return main;
        };

        p.getTemplate = function(type){
            var html = '', url ='';
            if(type === 'radio'){
                // html = '<label class="md-check dark-bluebox" ng-controller="ClientPortalRolesRadioCtrl as cprrc">' +
                //                 '<input type="radio" name="client-portal-role" ' +
                //     ' ng-disabled="record.disabled" ng-model="record.isAssigned" '+
                //     ' ng-change="cprrc.publishRoleChange(record)" ng-value="true" ' +
                //     ' id="client-portal-role-{{record.id}}" class="has-value"> ' +
                //     ' <i class="primary"></i> ' +
                // '</label>';
                url = "user/assign-product-access/product-panel/templates/property-radio.html";
            }

            // $templateCache.put(url , html);
            // console.log("temp", $templateCache.get());
            // return url;
            // html = '<radiogrid></radiogrid>';
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
