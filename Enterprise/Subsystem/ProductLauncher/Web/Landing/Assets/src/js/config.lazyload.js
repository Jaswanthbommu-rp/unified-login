// lazyload config
var MODULE_CONFIG = {
    easyPieChart:   [
        'Assets/build/foundation-ui-library/jquery/easy-pie-chart/js/jquery-easypiechart.js'],
    sparkline:      [
        'Assets/build/foundation-ui-library/jquery/sparkline/js/jquery.sparkline.retina.js' ],
    plot:           [
        'Assets/build/foundation-ui-library/jquery/flot/js/jquery.flot.js',
        'Assets/build/foundation-ui-library/jquery/flot/js/jquery.flot.resize.js',
        'Assets/build/foundation-ui-library/jquery/flot/js/jquery.flot.pie.js',
        'Assets/build/foundation-ui-library/jquery/flot-tooltip/js/jquery.flot.tooltip.js',
        'Assets/build/foundation-ui-library/jquery/flot-spline/js/jquery.flot.spline.js',
        'Assets/build/foundation-ui-library/jquery/flot-orderbars/js/jquery.flot.orderBars.js'],
    vectorMap:      [
        'Assets/build/foundation-ui-library/jquery/jvectormap/js/jquery-jvectormap.js',
        'Assets/build/foundation-ui-library/jquery/jvectormap/css/jquery-jvectormap.css',
        'Assets/build/foundation-ui-library/jquery/jvectormap/js/jquery-jvectormap-world-mill-en.js',
        'Assets/build/foundation-ui-library/jquery/jvectormap/js/jquery-jvectormap-us-aea-en.js' ],
    contextMenu:    [
        'Assets/build/foundation-ui-library/jquery/jquery-context-menu/jquery.contextMenu.js',
        'Assets/build/foundation-ui-library/jquery/jquery-context-menu/jquery.contextMenu.css',
        'Assets/build/foundation-ui-library/jquery/jquery-context-menu/jquery.ui.position.js'],
    dataTable:      [
        '/Assets/build/foundation-ui-library/jquery/data-table/js/jquery.dataTables.js',
        '/Assets/build/foundation-ui-library/jquery/data-table/js/dataTables.bootstrap.js',
        '/Assets/build/foundation-ui-library/jquery/data-table/js/dataTables.responsive.js',
        '/Assets/build/foundation-ui-library/jquery/data-table/js/dataTables.fixedColumns.js',
        'https://cdn.datatables.net/v/dt/dt-1.10.16/rr-1.2.3/datatables.min.js',
        '/Assets/build/js/plugins/ui-dtpagination-input.js',
        '/Assets/build/js/ui-datatable-config.js',
        'https://cdn.datatables.net/v/dt/dt-1.10.16/rr-1.2.3/datatables.min.css',
        '/Assets/build/foundation-ui-library/jquery/data-table/css/dataTables-bootstrap.css',
        '/Assets/build/foundation-ui-library/jquery/data-table/css/fixedColumns-bootstrap.css',
        '/Assets/build/foundation-ui-library/jquery/data-table/css/responsive-bootstrap.css'],
    footable:       [
        'Assets/build/foundation-ui-library/jquery/footable/js/footable.all.js',
        'Assets/build/foundation-ui-library/jquery/footable/css/footable.css',
        'Assets/build/foundation-ui-library/jquery/footable/css/font-face.css'
    ],
    screenfull:     [
        'Assets/build/foundation-ui-library/jquery/screenfull/_base/js/screenfull.js'
    ],
    sortable:       [
        'Assets/build/foundation-ui-library/jquery/html-sortable/js/html-sortable.js'
    ],
    nestable:       [
        'Assets/build/foundation-ui-library/jquery/nestable/css/jquery-nestable.css',
        'Assets/build/foundation-ui-library/jquery/nestable/js/jquery.nestable.js'
    ],
    summernote:     [
        'Assets/build/foundation-ui-library/jquery/summernote/css/summernote.css',
        'Assets/build/foundation-ui-library/jquery/summernote/js/summernote.js'
    ],
    parsley:        [
        '/Assets/build/foundation-ui-library/jquery/parsleyjs/css/parsley.css',
        '/Assets/build/js/plugins/parsley.js',
        //'/Assets/build/foundation-ui-library/jquery/parsleyjs/js/parsley.js',
        '/Assets/build/js/plugins/parsley-validator-comparison.js',
    ],
    select2:        [
        'Assets/build/foundation-ui-library/jquery/select2/css/select2.css',
        'Assets/build/foundation-ui-library/jquery/select2-bootstrap-theme/css/select2-bootstrap.css',
        'Assets/build/foundation-ui-library/jquery/select2-bootstrap-theme/css/select2-bootstrap-4.css',
        'Assets/build/foundation-ui-library/jquery/select2/js/select2.js'
    ],
    datetimepicker: [
        'Assets/build/foundation-ui-library/jquery/eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.css',
        'Assets/build/foundation-ui-library/jquery/eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.dark.css',
        'Assets/build/foundation-ui-library/js/moment/_base/js/moment.js',
        'Assets/build/foundation-ui-library/jquery/eonasdan-bootstrap-datetimepicker/build/js/bootstrap-datetimepicker.min.js'
    ],
    daterangepicker: [
        'Assets/build/foundation-ui-library/jquery/daterange-picker/css/daterangepicker.css',
        'Assets/build/foundation-ui-library/jquery/daterange-picker/js/daterangepicker.js',
        'Assets/build/foundation-ui-library/jquery/daterange-picker/js/daterangesetup.js'
    ],
    chart:          [
        'Assets/build/foundation-ui-library/js/echarts/_base/js/echarts-all.js',
        'Assets/build/foundation-ui-library/js/echarts/_base/js/theme.js',
        'Assets/build/foundation-ui-library/js/echarts/_base/js/jquery.echarts.js'
    ],
    highcharts:      [
        'Assets/build/foundation-ui-library/jquery/highstock/highcharts/js/highstock.src.js',
        'Assets/build/foundation-ui-library/jquery/highstock/highcharts/js/highcharts-more.js',
        'Assets/build/foundation-ui-library/jquery/highstock/highcharts/js/modules/heatmap.js',
        'Assets/build/foundation-ui-library/jquery/highstock/highcharts/js/modules/exporting.js'
    ],
    bootstrapWizard:[
        'Assets/build/foundation-ui-library/jquery/twitter-bootstrap-wizard/jquery.bootstrap.wizard.js'
    ],
    fullCalendar:   [
        'Assets/build/foundation-ui-library/js/moment/_base/js/moment.js',
        'Assets/build/foundation-ui-library/jquery/fullcalendar/js/fullcalendar.js',
        'Assets/build/foundation-ui-library/jquery/fullcalendar/css/fullcalendar.css',
        'Assets/build/foundation-ui-library/jquery/fullcalendar/css/fullcalendar.theme.css',
        'Assets/build/js/plugins/calendar.js'
    ],
    dropzone:       [
        'Assets/build/foundation-ui-library/js/dropzone/_base/js/dropzone.js',
        'Assets/build/foundation-ui-library/js/dropzone/_base/css/dropzone.css'
    ],
    rpCarousel:     [
        'Assets/build/foundation-ui-library/jquery/rp-carousel/css/rp-carousel.css',
        'Assets/build/foundation-ui-library/jquery/rp-carousel/js/rp-carousel.js'
    ],
    tooltipster:    [
        'Assets/build/foundation-ui-library/jquery/tooltipster/css/tooltipster.bundle.min.css',
        'Assets/build/foundation-ui-library/jquery/tooltipster/css/tooltipster-sideTip-shadow.min.css',
        'Assets/build/foundation-ui-library/jquery/tooltipster/js/tooltipster.bundle.min.js'
    ],
    bootstrapSwitch: [
        'Assets/build/foundation-ui-library/bootstrap/bootstrap-switch/css/bootstrap-switch.min.css',
        'Assets/build/foundation-ui-library/bootstrap/bootstrap-switch/js/bootstrap-switch.min.js'
    ],
    rpAudio:     [
        'Assets/build/foundation-ui-library/jquery/rp-audio/css/rp-audio.css',
        'Assets/build/foundation-ui-library/jquery/rp-audio/js/rp-audio.js'
    ]
};