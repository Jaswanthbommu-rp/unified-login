'use strict';

// Side Shelf
$('.dmod-trigger').on('click', function () {
    console.log('dmod-trigger hit');
    var thisMod = $(this).attr('data-modalopen');
    var dataDir = $(thisMod).attr('data-direction');
    console.log('thisMod', thisMod);
    console.log('dataDir', dataDir);
    dmodOpen(thisMod, dataDir);
});

function dmodOpen(thisMod, dataDir) {
    $('.dmod-overlay').show();
    $('body').addClass('dmod-open');
    switch (dataDir) {
        case 'right':
            $(thisMod).show().animate({ "right": 0 }, "fast", function () {
                $(thisMod).addClass('active');
            });
            break;
    }
}

$('.dmod-close').on('click', function () {
    console.log('dmod-close hit');
    var thisParent = $(this).attr('data-ref');
    var dataDir = $(thisParent).attr('data-direction');
    console.log('thisParent', thisParent);
    console.log('dataDir', dataDir);
    dmodClose(thisParent, dataDir);
});

function dmodClose(thisParent, dataDir) {
    console.log('thisParent', thisParent);
    console.log('dataDir', dataDir);
    $('.dmod-overlay').hide();
    $('body').removeClass('dmod-open');
    switch (dataDir) {
        case 'right':
            console.log('case right hit');
            $(thisParent).animate({ "right": "-100%" }, "fast", function () {
                $(thisParent).hide().removeClass('active');
            });
            break;
    }
}

$('.dmod-overlay').on('click', function () {
    console.log('dmod-overlay click');
    $('body').removeClass('dmod-open');
    var openDmod = $('.dmod-modal.active');
    var openDmodId = openDmod.attr('id');
    var dataDir = $('#' + openDmodId).attr('data-direction');

    console.log('openDmod', openDmod);
    console.log('openDmodId', openDmodId);
    console.log('dataDir', dataDir);
    dmodClose('#' + openDmodId, dataDir);
});