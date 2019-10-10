(function(angular, undefined) {
    "use strict";

    function factory(       
        $filter
    ) {
        var model = {};

        model.init = function() {
            model.data = {
                orderTimeOut: "0",
                invoiceTimeOut: "0",
                isOrderReminder : false,
                isInvoiceReminder : false
            };
            return model;
        };

       
        model.setData = function(data) {                              
            model.data.orderTimeOut = data.order_workflow_timeout === "" ? "0" : data.order_workflow_timeout;
            model.data.invoiceTimeOut = data.invoice_workflow_timeout === "" ? "0" : data.invoice_workflow_timeout ;
            model.data.isOrderReminder = data.order_endorse_email_reminder_flag === "0" ? false : true;
            model.data.isInvoiceReminder = data.invoice_endorse_email_reminder_flag === "0" ? false : true;
        
        };

        model.getData = function() {
            return model.data;
        };

        model.reset = function() {            
            model.data = {
                orderTimeOut: "0",
                invoiceTimeOut: "0",
                isOrderReminder : false,
                isInvoiceReminder : false
            };
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("spndmgmtCloneRoleWfModel", [           
            "$filter",
            factory
        ]);
})(angular);