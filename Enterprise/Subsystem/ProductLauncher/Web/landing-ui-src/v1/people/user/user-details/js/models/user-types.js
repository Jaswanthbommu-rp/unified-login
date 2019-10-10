//  Add User Form Model

(function (angular) {
    "use strict";

    function factory($filter) {
        var model = {},
            data = {};

        model.init = function() {
            data.options = [{
                //regular
                label: $filter("userDetailsText")("user_detail_regular_user"),
                value: 0
            }, {
                //regular w/o email
                label: $filter("userDetailsText")("user_detail_regular_no_email_user"),
                value: 2
            }, {
                //RealPage System Administrator
                label: $filter("userDetailsText")("user_detail_super_user"),
                value: 1
            },{
                //RealPage System Administrator
                label: $filter("userDetailsText")("user_detail_employee"),
                value: 1
            }, {
                //SDE
                label: $filter("userDetailsText")("user_detail_sde"),
                value: 3
            }, {
                //external user
                label: $filter("userDetailsText")("user_detail_external_user"),
                value: 4
            }, {
                //RealPage Employee
                label: $filter("userDetailsText")("user_detail_rp_employee"),
                value: 5
            }];

            model.data = data;
            return model;
        };
            

        model.getOptions = function() {
            return model.data.options; //TODO TEMPORARY... should eventually come from the service instead
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("userTypes", [
            "$filter",
            factory
        ]);
})(angular);
