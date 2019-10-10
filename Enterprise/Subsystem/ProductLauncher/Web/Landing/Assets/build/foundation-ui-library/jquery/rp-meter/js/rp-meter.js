
$(function() {

    var rpMeter = function(element,options){
        element.find(".rp-fail-section").css("width", options['failThreshold'] + "%");
        element.find(".rp-conditional-section").css("width", options['conditionalThreshold'] + "%");
        element.find(".rp-pass-section").css("width", (100 - (options['failThreshold'] + options['conditionalThreshold'])) + "%");

        if(options['startPoint']){
            element.find(".rp-start-point").html(options['startPoint']);
        }

        if(options['failPoint']){
            element.find(".rp-fail-point").html(options['failPoint']);
            element.find(".rp-fail-point").css("left", (options['failThreshold'] - 6) + "%");
        }

        if(options['conditionalPoint']){
            element.find(".rp-conditional-point").html(options['conditionalPoint']);
            element.find(".rp-conditional-point").css("left", (options['failThreshold'] + options['conditionalThreshold'] - 6) + "%");
        }

        if(options['endPoint']){
            element.find(".rp-end-point").html(options['endPoint'])
        }

        if(options['userPlacementType']){
            if(options['userPlacementType'] === "caret"){
                element.find(".rp-user-mark-caret").show();
                if(options['userPlacement']){
                    element.find(".rp-user-mark-caret").css("left", options['userPlacement'] + "%");
                }
            } else if (options['userPlacementType'] === "bar") {
                element.find(".rp-user-mark-bar").show();
                if(options['userPlacement']){
                    element.find(".rp-user-mark-bar").css("left", options['userPlacement'] + "%");
                }
            }
        }


        if(options['failTitle']){
            element.find(".rp-fail-text").html(options['failTitle']);
            element.find(".rp-fail-text").css("width", options['failThreshold'] + "%");
        } else {
            element.find(".rp-fail-text").html("");
            element.find(".rp-fail-text").css("width", options['failThreshold'] + "%");
        }

        if(options['conditionalTitle']){
            element.find(".rp-conditional-text").html(options['conditionalTitle']);
            element.find(".rp-conditional-text").css("width", options['conditionalThreshold'] + "%");
        } else {
            element.find(".rp-conditional-text").html("");
            element.find(".rp-conditional-text").css("width", options['conditionalThreshold'] + "%");
        }

        if(options['passTitle']){
            element.find(".rp-pass-text").html(options['passTitle']);
            element.find(".rp-pass-text").css("width", (100 - (options['failThreshold'] + options['conditionalThreshold'])) + "%");
        } else {
            element.find(".rp-pass-text").html("");
            element.find(".rp-pass-text").css("width", (100 - (options['failThreshold'] + options['conditionalThreshold'])) + "%");
        }
    };

    $.fn.rpMeter = function (options) {
        rpMeter(this,options);
    };

});