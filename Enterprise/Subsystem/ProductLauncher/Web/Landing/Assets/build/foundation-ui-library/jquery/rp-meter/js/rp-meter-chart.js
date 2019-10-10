
$(function() {

    var rpMeterChart = function(element,options){
        $.each(options.data, function( index, value ) {
            element.css("overflow", "hidden");
            element.css("height", options.height);
            element.css("border-radius", options.borderRadius);
            element.append("<div style='display:inline-block; height:"+ options.height +"; background-color:"+ value.color +"; width:"+ value.percent +"%'></div>");
        });
    };

    $.fn.rpMeterChart = function (options) {
        rpMeterChart(this,options);
    };

});