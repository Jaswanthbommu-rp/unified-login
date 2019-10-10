(function ($, MODULE_CONFIG) {
    "use strict";

    $.fn.uiJp = function () {

        var lists = this;

        lists.each(function () {
            var self = $(this);
            var options = eval('[' + self.attr('ui-options') + ']');
            if ($.isPlainObject(options[0])) {
                options[0] = $.extend({}, options[0]);
            }

            uiLoad.load(MODULE_CONFIG[self.attr('ui-jp')]).then(function () {
                if ($.fn.dataTable && $.fn.dataTable.isDataTable(self)) {//!!!for some reason uiLoad "loads" 2 times, here we check if dataTable is already initialised to prevent popup datatables error in IE
                   return;
                };

                self[self.attr('ui-jp')].apply(self, options);
            });
        });

        return lists;
    };

})(jQuery, MODULE_CONFIG);
