function displayMessage(title,duration,bgColor,textColor,borderColor) {
    const message ='<div class="box inline-ad pop-up-message '+ borderColor + ' ' + bgColor + '" style="display:none">' +
        '<div class="box-body">' +
        '<div style="color: #757576; position:absolute; top:8px; right:8px;" onclick="$(\'#pop-up-message-wrapper\').empty()"><i class="rp-icon-delete-2"></i></div>'+
        '<h5 class="m-b-0 '+ textColor +'">' + title + '</h5>' +
        '</div>' +
        '</div>';
    var extendedDuration = parseFloat(duration) + 1000;
    $('#pop-up-message-wrapper').append(message);
    $('.pop-up-message').fadeIn('slow');
    console.log(extendedDuration);
    setTimeout(
        function(){
            $('.pop-up-message').fadeOut('slow');
        }, duration
    );
    setTimeout(
        function(){
            $('#pop-up-message-wrapper').empty();
        }, extendedDuration
    );
}