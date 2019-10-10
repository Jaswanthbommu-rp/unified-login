function repositionResultOptions(){

    var parentTable = $(this);
    var resultOptions;

    parentTable.find('.dataTables_length').each( function () {
        resultOptions = $(this);

        parentTable.find('.dataTables_info').each( function () {
            resultOptions.appendTo(this);
        });

    });

}
var delay = (function(){
    var timer = 0;
    return function(callback, ms){
        clearTimeout (timer);
        timer = setTimeout(callback, ms);
    };
})();



$.extend( true, $.fn.dataTable.defaults, {
    "aLengthMenu": [[5, 10, 20, 50, 100, 500, -1], [5, 10, 20, 50, 100, 500, "All"]],
    "iDisplayLength" : 20,
    "bPaginate" : true,
    "bInfo" : true,
    "bAutoWidth" : false,
    "bLengthChange": true,
    "responsive": true,
    "bSort" : true,
    "preDrawCallback" : function() {
        var table = this.DataTable();
        var lastPageSize = table.page.info().end;
        var selectedPageSize = table.page.info().length;
        var recordsTotal = table.page.info().recordsTotal;
        var readyForDraw = true;

        window.test = table;

        //-1 means 'All'
        if (selectedPageSize === -1 && recordsTotal > 500) {
            readyForDraw = false;

            //set pageSize back to last size
            if (lastPageSize > 500) {
                lastPageSize = 5;
            }

            table.page.len(lastPageSize);
            $('#modal-show-all').modal('show');
        }

        return readyForDraw;
    },
    "initComplete" : function(){
        var JQtable = $(this.parents('.ui-data-table'));
        var table = this.DataTable();
        var resultOptions;

        // DataTable column search input
        JQtable.find('tfoot tr th').each(function () {
            var title = JQtable.find('thead th').eq($(this).index()).text();
            title = title.trim();
            $(this).html('<input type="text" class="filter form-control" placeholder="' + title + '" />');
        });

        //Hide filters that are not needed
        JQtable.find(".datatable-hide-filter").html('');

        table.columns().every( function () {
            var that = this;
            $( 'input', this.footer() ).keyup(function (e) {
                var thatInput = this;
                delay(function(){
                    if($.isNumeric(thatInput.value)){
                        var commaNumber = thatInput.value.replace(/(\d)(?=(\d\d\d)+(?!\d))/g, '$1,');
                        that.search(commaNumber);
                    }else{
                        that.search(thatInput.value);}
                    table.columns.adjust().draw();
                }, 500 );
            });
        });

        JQtable.closest(".box").find(".clear-btn").click(function() {
            table.search("").draw();
            JQtable.find(".filter").val("");
            table.columns().search("").draw();
        });

        JQtable.find('.dataTables_info').each( function () {

            var resultOptions;

            JQtable.find('.dataTables_length').each( function () {
                resultOptions = $(this);
                resultOptions.css('display','block');
                resultOptions.css('position','relative');

                JQtable.find('.dataTables_info').each( function () {
                    var parentElement = $(this).parent();
                    resultOptions.appendTo(parentElement);
                });

            });

        });

    }
} );