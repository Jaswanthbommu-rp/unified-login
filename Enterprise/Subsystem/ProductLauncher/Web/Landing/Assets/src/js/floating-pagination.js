/***

 created 4/11/17 by Henry Hedges

 **/
$('.floating-pagination-bar .select2.select2-container').first().css("width","60px");
$('.floating-pagination-bar .select2.select2-container').last().css("width","80px");

$('.lg-pag li').not(':first').not(':last').each(function(item, el){

    $(el).click(function(e){
        e.preventDefault();
        $('.lg-pag li.active').removeClass('active');
        $(this).addClass('active');
    });
});



