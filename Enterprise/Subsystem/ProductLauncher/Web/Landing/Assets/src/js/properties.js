/****
 Created by henry hedges 3/30/17 - for property index blade
 ***/
// Navigation function - allows the photo button and list button at the top function effectively as navigation
$('.data-swap-btn').on('click', function(){
    /* fix RAUL bug where there is no logic to add active class */
    $('.data-swap-btn').removeClass('active');
    $(this).addClass('active');

    /* reads what the data-open target is, and hides all of the class then opens the appropriate one */
    var thisOpen = $(this).attr('data-open');
    $('.data-swap-loc').hide();
    $(thisOpen).show();
});


//allows the checkboxes to all be checked by the checkbox at the top
$('#check-all').click(function (e) {
    var inputGroup = $(this).data('input-group');
    var $input = $('input.' + inputGroup + '');
    //fyi - this was using 'let' instead of 'var' but was breaking in Safari
    if (e.target.checked) {
        for (var i = 0, len = $input.length; i < len; i++) {
            if ($input[i].type === 'checkbox') $input[i].checked = true;
        }
    } else {
        for (var j = 0, leng = $input.length; j < leng; j++) {
            if ($input[j].type === 'checkbox') $input[j].checked = false;
        }
    }
});





// function that stops propagation of click event for checkbox inside of dropdown
// $('#dd-cbox').on('click', function(e){ e.stopPropagation(); })


