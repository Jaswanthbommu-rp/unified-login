
$(function() {
    var index = 0;

    var elementArray = {};

    var rpCarousel = function(element){

        var currIndex = 0;

        if(getIndexforElement(element)){
            currIndex = getIndexforElement(element);
        } else {
            index = index +1;
            currIndex = index;
        }
        elementArray[currIndex] = {};
        elementArray[currIndex].ele = $(element);
        elementArray[currIndex].items = $(elementArray[currIndex].ele).find('.items');
        //Set initial values
        elementArray[currIndex].totalItemsWidth = 0;
        elementArray[currIndex].leftHidden = 0;
        elementArray[currIndex].moveRight = 0;
        elementArray[currIndex].rightHidden = 0;
        elementArray[currIndex].visibleAreaWidth = 0;

        // figure out the visible area
        getVisibleArea(elementArray[currIndex].ele);

        //resize totalItemsContainer
        resizeTotalItemContainer(currIndex);

        // disabling prev link
        $(elementArray[currIndex].ele).find('.prev').addClass('hidden-xs-up');

        // hiding prev and next links by default
        $(elementArray[currIndex].ele).find('.prev').hide();
        $(elementArray[currIndex].ele).find('.next').hide();

        elementArray[currIndex].visibleAreaWidth = $(elementArray[currIndex].ele).find('.visible-area').width();
        //console.log("visible area width: " + elementArray[currIndex].visibleAreaWidth);

        // prev link function
        $(elementArray[currIndex].ele).find('.prev').click(function(){
            //console.log("======== TRYING TO GO PREVIOUS =========");
            rpCarouselPrevious(eval(currIndex));
        });

        // next link function
        $(elementArray[currIndex].ele).find('.next').click(function(){
            //console.log("======== TRYING TO GO NEXT =========");
            rpCarouselNext(currIndex);
        })
    };

    function resizeTotalItemContainer(currIndex){

        elementArray[currIndex].totalItemsWidth = 0;

        // to get totalItemsWidth
        $(elementArray[currIndex].items).children('li').each(function(){
            elementArray[currIndex].totalItemsWidth += $(this).width() + parseInt($(this).css('margin-right'));
            //console.log("element width: " + $(this).width());
            //console.log("current total length: " + elementArray[currIndex].totalItemsWidth);
        });

        // to set totalItemsWidth to parent UL
        $(elementArray[currIndex].items).css('width',elementArray[currIndex].totalItemsWidth);

        //show prev and next links if there are a lot of items
        if(elementArray[currIndex].totalItemsWidth > elementArray[currIndex].visibleAreaWidth){
            $(elementArray[currIndex].ele).find('.prev').show();
            $(elementArray[currIndex].ele).find('.next').show();
        }
    }


    function getVisibleArea(element){
        var selectedElement = getIndexforElement(element);
        resizeTotalItemContainer(selectedElement);

        elementArray[selectedElement].visibleAreaWidth = $(elementArray[selectedElement].ele).find('.visible-area').width();
        //console.log("visible area width: " + elementArray[selectedElement].visibleAreaWidth);

        //decide if arrows should show
        elementArray[selectedElement].leftHidden = 0;
        elementArray[selectedElement].rightHidden = elementArray[selectedElement].totalItemsWidth - (elementArray[selectedElement].visibleAreaWidth + elementArray[selectedElement].leftHidden);
        $(elementArray[selectedElement].items).css('transform', 'translateX(0px)');

        if(elementArray[selectedElement].rightHidden === 0){
            $(elementArray[selectedElement].ele).find('.next').addClass('hidden-xs-up');
        } else {
            $(elementArray[selectedElement].ele).find('.next').removeClass('hidden-xs-up');
        }

        $(elementArray[selectedElement].ele).find('.prev').addClass('hidden-xs-up');

    }

    function getIndexforElement(element){
        for (var key in elementArray) {
            var obj = elementArray[key];
            var currentName = element.attr("name");
            var comparingName = obj.ele.attr("name");
            if(currentName === comparingName){
                return key;
            }
        }
        return null;
    }

    function rpCarouselFindNext(element){
        var nextID = getIndexforElement($(element));
        rpCarouselNext(nextID);
    }

    function rpCarouselFindPrevious(element){
        var prevID = getIndexforElement($(element));
        rpCarouselPrevious(prevID);
    }

    function rpCarouselPrevious(selectedElement){



        elementArray[selectedElement].moveLeft = elementArray[selectedElement].leftHidden;

        if($(elementArray[selectedElement].ele).find('.next').hasClass('hidden-xs-up')){
            $(elementArray[selectedElement].ele).find('.next').removeClass('hidden-xs-up');
        }
        if(!$(elementArray[selectedElement].ele).hasClass('hidden-xs-up')) {
            if (elementArray[selectedElement].leftHidden <= elementArray[selectedElement].visibleAreaWidth) {
                elementArray[selectedElement].moveLeft = 0;
                $(elementArray[selectedElement].ele).find('.prev').addClass('hidden-xs-up');
            } else {
                elementArray[selectedElement].moveLeft -= elementArray[selectedElement].visibleAreaWidth;
            }
            $(elementArray[selectedElement].items).css('transform', 'translateX(-' + elementArray[selectedElement].moveLeft + 'px)');
            elementArray[selectedElement].leftHidden = elementArray[selectedElement].moveLeft;

            //console.log("clicked left");
            //console.log("elementArray[selectedElement].rightHidden: " + elementArray[selectedElement].rightHidden);
            //console.log("elementArray[selectedElement].leftHidden: " + elementArray[selectedElement].leftHidden);
            //console.log("elementArray[selectedElement].visibleAreaWidth: " + elementArray[selectedElement].visibleAreaWidth);
        }
    }

    function rpCarouselNext(selectedElement){



        if($(elementArray[selectedElement].ele).find('.prev').hasClass('hidden-xs-up')){
            $(elementArray[selectedElement].ele).find('.prev').removeClass('hidden-xs-up');
        }
        if(!$(elementArray[selectedElement].ele).hasClass('hidden-xs-up')) {
            elementArray[selectedElement].rightHidden = elementArray[selectedElement].totalItemsWidth - (elementArray[selectedElement].visibleAreaWidth + elementArray[selectedElement].leftHidden);
            if (elementArray[selectedElement].rightHidden < elementArray[selectedElement].visibleAreaWidth) {
                elementArray[selectedElement].moveRight = elementArray[selectedElement].totalItemsWidth - elementArray[selectedElement].visibleAreaWidth;
                //console.log("total width: " + elementArray[selectedElement].totalItemsWidth);
                //console.log("visible width: " + elementArray[selectedElement].visibleAreaWidth);
                $(elementArray[selectedElement].ele).find('.next').addClass('hidden-xs-up');
            } else {
                elementArray[selectedElement].moveRight = elementArray[selectedElement].leftHidden + elementArray[selectedElement].visibleAreaWidth;
            }
            $(elementArray[selectedElement].items).css('transform', 'translateX(-' + elementArray[selectedElement].moveRight + 'px)');
            elementArray[selectedElement].leftHidden = elementArray[selectedElement].moveRight;

            //console.log("clicked right");
            //console.log("elementArray[selectedElement].rightHidden: " + elementArray[selectedElement].rightHidden);
            //console.log("elementArray[selectedElement].leftHidden: " + elementArray[selectedElement].leftHidden);
            //console.log("elementArray[selectedElement].visibleAreaWidth: " + elementArray[selectedElement].visibleAreaWidth);

        }
    }

    $.fn.rpCarousel = function (options) {

        var carousels = $( ".rp-carousel" );

        $.each(carousels, function( index, element ) {
            if($(element).attr("name") === options.name){
                rpCarousel($(element));

                window.addEventListener('carouselResize', function() {
                    getVisibleArea($(element));
                });

                $( window ).resize(function() {
                    getVisibleArea($(element));
                });
            }
        });

    };

});