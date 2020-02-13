

(function (angular, undefined) {
    "use strict";

    function factory() {
        function NewProductAccessModel() {
            var s = this;
            s.init();
        }

        var p = NewProductAccessModel.prototype;

        p.init = function () {
            var s = this;
            s.data = [];
        };

        p.setData = function (data) {
            var s = this;
            s.data = data;
            return s;
        };

        p.getData = function () {
            var s = this;            
            return s.data;
        };

        p.getPageTitle = function () {
            var s = this;            
            return s.data.PageDisplayName;
        };
        
        p.getTabsData = function () {
            var s = this;    
            var tabs = {};
            if(s.data && s.data.Tabs){
                s.data.Tabs.forEach(function (data) {
                    tabs[data.DisplayName] = {
                        id : data.DisplayName,
                        text : data.DisplayName,
                        isActive : true,
                        incUrl: "user/assign-product-access/new-product-access/templates/" + data.DisplayName + ".html",
                    };
                });        
            }
            return tabs;
        };



        p.reset = function () {
            var s = this;
            s.data = [];
        };

        return new NewProductAccessModel();
    }

    angular
        .module("settings")
        .factory("newProductAccessModel", [factory]);
})(angular);
