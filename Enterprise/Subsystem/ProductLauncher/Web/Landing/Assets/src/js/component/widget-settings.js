/***
    Widget Settings - chat and proactive chat
***/
/*
    based on the data elements in the clicked 'edit question', we will append all of the particuar questions to the edit questions module
*/


$(document).ready(function () {

    //cache animated elements
    var checkFooterElm = $('footer');
    var fixedFooter = $('.fixed-footer');

    $(window).scroll(function (e) {
        checkFooterElm.each(function (i, el) {
            var el = $(el);
            if (el.visible(true) && !fixedFooter.hasClass('pos-abt')) {
                fixedFooter.addClass('pos-abt');
            }
            else if(!el.visible(true))  {
                fixedFooter.removeClass('pos-abt');
            }
        });
    });

    //color preview
    let backgroundColor = $('.jscolor.background').val();
    let textColor = $('.jscolor.text').val();
    let fillColor = $('.jscolor.fill').val();
    let initialTitleCopy = $('#widget_title').val();
    $('.background-target').css('background',backgroundColor);
    $('.text-target').css('color',textColor);
    $('.fill-target').css('background',fillColor);
    $('.fill-text-target').css('color',fillColor);
    $('.js-widget-title-target').text(initialTitleCopy);
});

//color preview
$('.jscolor.background').change(function(){
    var color = $(this).val();
    $('.background-target').css('background','#' + color);
})
$('.jscolor.text').change(function(){
    var color = $(this).val();
    $('.text-target').css('color','#' + color);
})
$('.jscolor.fill').change(function(){
    var color = $(this).val();
    $('.fill-target').css('background','#' + color);
    $('.fill-text-target').css('color','#' + color);
})

//CTA preview
$('#widget_title').keyup(function () {
    var titleCopy = $(this).val();
    $('.js-widget-title-target').text(titleCopy);
})

$('.gen-code').click(function(e){
    e.preventDefault();
    // var widgetId = $('#widget_id').val();
    var widgetId = $(this).data('widget-id');

    if (widgetId){
        $('#gen-code-text').val('<script src="https://uc-widget.realpageuc.com/widget?wid='+ widgetId + '"></script>');
    } else {
        $('#gen-code-text').val('Please make sure to save the widget before generating a script tag!');
    }
})

$('#btn-copy-code-for-deployment').on('click', function(e){
    console.log('copying code for deployment');
    e.preventDefault();
    e.stopPropagation();
    el = document.getElementById('gen-code-text');
    el.focus();
    el.setSelectionRange(0,el.textLength);
    document.execCommand('copy');
});

$('.dmod-edit-pq').click(function(e){
    e.preventDefault();
    e.stopPropagation();
    console.log('CLICK EVENT: dmod-edit-pq has been clicked', e.target)
    var data = $(this).data().questions;
    //create html for the questions
    // var questionHTML = $('<div>',{ class:"happy-agent", name:"quesiton1", question:"{{$hellothere = 'hello'}}" }).text("{{$hellothere = 'hello'}}")
    console.log(data)
    // $('#edit-precom-form').append(questionHTML)
// append to element : edit-precom-form
})


function showAssociatedFileFields(targetIdx){
    var conditionalElms = $('.conditional-pictoral');

    conditionalElms.hide();

    switch (targetIdx){
        case 0:
            conditionalElms.filter(function(idx, elm){
                return $(elm).data('category') === 'icon';
            }).show();
            break;
        case 1:
            conditionalElms.filter(function(idx, elm){
                return $(elm).data('category') === 'logo';
            }).show();
            break;
        case 2:
            conditionalElms.filter(function(idx, elm){
                var elm = $(elm).data('category');
                return elm === 'icon' || elm === 'logo' ;
            }).show();
            break;
        case 3:
            conditionalElms.filter(function(idx, elm){
                var elm = $(elm).data('category');

                if ( elm === 'photo'){
                    $(this).addClass('no-logo');
                    return true;
                }
            }).show();
            break;
        case 4:
            conditionalElms.filter(function(idx, elm){
                var elm = $(elm).data('category');

                if (elm === 'photo'){
                    $(this).removeClass('no-logo');
                    return true;
                } else if (elm === 'logo'){
                    return true;
                }

            }).show();
            break;
        default:
            break;

    }
}

// shows 'file' elements based on loaded state
showAssociatedFileFields( $('#chat-invite-ex').prop('selectedIndex') );

//change shown file fields on 'select' field change
$('#chat-invite-ex').change(function(e){
    //TODO: make default setting for conditional elms dependent on current sleectiosn
    var targetIdx = $( e.target ).find(':selected').index();
    showAssociatedFileFields( targetIdx );
});

$('.template-dropdown-choices a').on('click', function(e){
    var thisChoice = $(this).attr('data-val');
    $('#templateName').val(thisChoice);
});


// carousel will change chat invite type select-element's value
$('#chat-invitation-examples').on('slid.bs.carousel',function(e){
    window.activeElm = $(this).find('.active');
    var chatInvite = $('#chat-invite-ex')

    chatInvite.val( activeElm[0].id );
    showAssociatedFileFields( chatInvite.prop('selectedIndex') );
});

// select element will change carousel's current image
$('#chat-invite-ex').change(function(e){
    // console.log(this);
    // console.log(e.target);
    // console.log(e.target.value);
    var selectedValue = $('#' + e.target.value).index();
    $('#chat-invitation-examples').carousel(selectedValue);
})

//check length od widget label text field
$('#widget-label-text').text( function(){
    var widget_title = document.getElementById('widget_title')
    return widget_title.dataset.charlimit - widget_title.value.length;
})

// console.log('all triggers',document.querySelectorAll("[name='chat[trigger]']"))

// change all trigger fields based on anyone of them changing
document.querySelectorAll("[name='chat[trigger]']").forEach(function(item){
    item.addEventListener('change', function(e){
        var inactivityTriggers = document.querySelectorAll("[name='chat[trigger]']")
        inactivityTriggers.forEach(function(trigger){
            if (trigger !== e.target){
                trigger.selectedIndex = e.target.selectedIndex;
            }
        });
    })
})

// change all position fields based on anyone of them changing
document.querySelectorAll("[name='chat[position]']").forEach(function(item){
    if (item.type === 'select-one'){
        item.addEventListener('change',function(e){
            var target = e.target;
            document.querySelectorAll("input[name='chat[position]']").forEach(function(radioBtn){
                if (radioBtn.value === target.options[ target.selectedIndex ].value){
                    radioBtn.checked = true;
                }
            })
        })
    } else if (item.type === 'radio'){
        item.addEventListener('change',function(e){
            var selElm = document.querySelectorAll( "select[name='chat[position]']" )[0]
            for ( var i = 0; i < selElm.length; i++ ){
                if ( selElm.options[i].value === e.target.value ){
                    selElm.selectedIndex = i;
                }
            }
        })
    }
})

// change all include_mobile fields based on anyone of them changing
document.querySelectorAll("[name='chat[include_mobile]']").forEach(function(item){
    item.addEventListener('change', function(e){
        var mobileSelElms = document.querySelectorAll("[name='chat[include_mobile]']")
        mobileSelElms.forEach(function(mobileSel){
            if (mobileSel !== e.target){
                mobileSel.selectedIndex = e.target.selectedIndex;
            }
        });
    })
})

// show splash screen option if more than one communication mode is selected
$('.comm-modes input[type="checkbox"]').click(function(e){
    var comModes = $('.comm-modes'),
        checkedElms = comModes.find('input[type="checkbox"]:checked').length,
        splash = $('#widget-splash-screen'),
        greeting = $('#widget-greeting'),
        modeNode = comModes.find('.def-comm-mode[data-mode="'+ $(this).data().mode +'"]');
    if (checkedElms > 1){

        splash.show();

        if ( splash.find('input').is(':checked') ){
            greeting.show();
        } else {
            greeting.hide();
        }

    } else {
        splash.hide();
        greeting.hide();
    }

    if ($(this).is(':checked')){
        modeNode.show();
        modeNode.find('input').prop('checked', false);

    } else {
        modeNode.hide();
        modeNode.find('input').prop('checked', false);
    }
})

$('#widget-splash-screen input').click(function(e){
    var target = $(e.target),
        isChecked = target.is(':checked'),
        greeting = $('#widget-greeting'),
        checkedElms = $('.comm-modes input[type="checkbox"]:checked').length,
        defCommMode = $('.def-comm-mode');

    if (isChecked && checkedElms > 1 ){
        greeting.show();
        defCommMode.hide();
    } else {
        greeting.hide();
        //determine which values to show
        if( $('input[data-mode="call"]').is(':checked') ) {
          $('div[data-mode="call"]').show();
        }
        if( $('input[data-mode="chat"]').is(':checked') ) {
          $('div[data-mode="chat"]').show();
        }
        if( $('input[data-mode="text"]').is(':checked') ) {
          $('div[data-mode="text"]').show();
        }
        if( $('input[data-mode="email"]').is(':checked') ) {
          $('div[data-mode="email"]').show();
        }
    }
})

// change logo on/off switch
$('#logo_active').change(function(e){
    var textElm = $('#logo_active_text'),
        logoDZ = $('#logo_dz');

    // can only be 'active' or 'inactive'
    if (textElm.text() === 'Off') {
        textElm.text('On');
        logoDZ.show();
    } else {
        textElm.text('Off');
        logoDZ.hide();
    };

});

$('.allow-item-checkbox').click(function(e){
    var elm = $(this),
        checkedElm = elm.find('input:checked')[0],
        expandElm = $( elm.data('select') );

    if ( checkedElm ){
        expandElm.removeClass('collapse');
    } else if ( checkedElm === undefined ){
        expandElm.addClass('collapse');
    }
})

/**
    Conversation Subjects
**/
$('.conversation-subject-setting').find('input[type=checkbox]').each(function(idx, elm){
    // watch for input being the only input checked
    $(elm).click(function(e){
        if ( $('.conversation-subject-setting').find('input[type=checkbox]:checked').length < 1 ){
            var subjectName = $(this).data().subjectname;
            // alert('You must select at least one subject. \n Please choose another subject before de-selecting ' + $(this).data().subjectname);
            displayMessage("'You must select at least one subject. \n Please choose another subject before de-selecting '" + subjectName + "' ","6000","warning","text-neutral", "b-warning");

            return false;
        }

        return true;
    })

    // toggle email/phone/text number input below subject
    $(elm).change(function(e){
        $(this.closest('tr').dataset.sibling).toggle();
    });
})
