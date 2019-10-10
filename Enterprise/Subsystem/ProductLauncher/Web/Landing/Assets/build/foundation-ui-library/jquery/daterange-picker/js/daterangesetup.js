function cb(ele, start, end) {
    ele.html(start.format('M/D/YY') + ' - ' + end.format('M/D/YY'));
}

setTimeout(function(){
    $('.daterangepicker').find('li[data-range-key="Last 30 Days"]').click();
    cb($('.daterange'), moment().subtract(29, 'days'), moment());
}, 500);

$('.daterange').daterangepicker({
    autoApply: true,
    ranges: {
        'Today': [moment(), moment()],
        'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
        'Last 7 Days': [moment().subtract(6, 'days'), moment()],
        'Last 30 Days': [moment().subtract(29, 'days'), moment()],
        'This Month': [moment().startOf('month'), moment().endOf('month')],
        'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
    }
}, cb);
