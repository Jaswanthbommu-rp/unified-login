    function reloadProactiveChat(){
        //close preview
        openPreview();
        //open preview
        openPreview();
    }

    // count characters in input field
    function countText(targetInput, labelID, maxChar){
        var text = targetInput.value,
            remainingCharLabel = document.getElementById(labelID),
            newValue;

        if ( text.length > maxChar ){
            targetInput.value = text.substring(0, maxChar);
        } 

        newValue = maxChar - targetInput.value.length;

        if ( newValue <= 0 ){
            remainingCharLabel.parentNode.style.setProperty('color','red');
        } else if ( remainingCharLabel !== null ){
            remainingCharLabel.parentNode.style.setProperty('color','');
        }
        // crossbrowser compatability for innerText
        if ( remainingCharLabel.textContent ) {
            remainingCharLabel.textContent = newValue;
        } else {
            remainingCharLabel.innerText = newValue;
        }

    };
    // get file data and show in <img> tag - used for pictures
    function readURL(input, targetElm) {
        if (typeof targetElm === 'string'){
            var targetElm = document.getElementById(targetElm);
        } 

        if (input.files && input.files[0]) {

            var reader = new FileReader();
            console.log('input files ',input.files)
            // listener, let the tag show the new image
            reader.onload = function (e) {
                e.preventDefault();
                console.log('file read event')
                targetElm.setAttribute('src', e.target.result);
            }

            reader.onerror = function(e){
                console.log('ERROR with file reading::: ', e)
            }

            console.log('input file[0]',input.files[0])
            // get data from the src file
            reader.readAsDataURL(input.files[0]); 

        } else {
            // file not loaded in file input
            var elm = document.getElementById( input.id + '-hidden' )
            targetElm.src = elm.value;
        }
    }

    function formatClr(hexcolor){
        return '#'+hexcolor;
    }

    // create a cloned version of a formatted component - used for proactive caht preview
    function makeInvitePreview(selectedOption){
        var flyout = document.getElementById('pc-preview-flyout'),
            preview = document.getElementById('pc-preview-full'),
            previewClone = preview.cloneNode(true),
            // current values
            inviteHeadline = document.getElementById('chat-headline-input'),
            inviteInvitation = document.getElementById('chat-invitation-input'),
            inviteTextBlurb = document.getElementById('chat-txt-blurb-input'),
            inviteChatBtnText = document.getElementById('chat-btn-text-input'),
            inviteBgClr = document.getElementById('chat-bg-clr'),
            inviteTextClr = document.getElementById('chat-txt-clr'),
            invitePrimaryClr = document.getElementById('chat-prim-clr'),
            inviteChatBtnTxtClr = document.getElementById('chat-btn-txt-clr'),
            // current cloned sections 
            textAndButtons = previewClone.getElementsByClassName('pc-preview-text')[0],
            chatMainBtn = previewClone.getElementsByClassName('chat-main-btn')[0],
            logoArea = previewClone.getElementsByClassName('pc-preview-logo')[0],
            // all selected type categories
            typeArray = JSON.parse(selectedOption.dataset.invitetype),
            hasLogo = false,
            allImages = 0;

        // append logo/photo/icon to the their respective locations
        for (var i = 0, len = typeArray.length; i < len; i++){  
            // get dropzone, then get file url from dz
            var elmFile = window[ document.getElementById( 'dropzone-proactive-' + typeArray[i] ).dataset.dzvar ].files[0];
            var imageElm = document.createElement('img');

            // readURL(elm, imageElm);
            // check if image exists, then read it
            if ( elmFile !== undefined ){
                imageElm.setAttribute('src', elmFile.url !== undefined && elmFile.url ? elmFile.url : elmFile.dataURL );
                allImages++;
            }

            if (typeArray[i] === 'logo'){
                // logo
                if ( hasLogo === false ){ hasLogo = true };

                if ( elmFile === undefined ) {
                    imageElm.setAttribute('src', '/build/assets/images/rp-logo-small.png');
                }

                imageElm.style.maxWidth = '120px';
                imageElm.style.display = 'inline-block';
                previewClone.getElementsByClassName('pc-preview-logo')[0].appendChild(imageElm);
            } else {
                // icon and photo
                if ( typeArray[i] === 'photo') { 

                    if ( elmFile === undefined ) {
                        imageElm.setAttribute('src', '/build/assets/images/property-imgs/p10.jpg');
                    }

                    imageElm.style.minWidth = '200px'; 
                } else {

                    if ( elmFile === undefined ) {
                        imageElm.setAttribute('src', '/build/assets/images/profile.png');
                    }

                    imageElm.style.maxHeight = '220px';
                    imageElm.style.maxWidth = '220px';
                }

                imageElm.style.height = '100%';
                previewClone.getElementsByClassName('pc-preview-image')[0].appendChild(imageElm);
            }
        }

        if ( !document.getElementById('image-vacant-warning') && (allImages !== typeArray.length) ){
            console.log('preview clone add warning')
            var imageVacantWarning = document.createElement('div'),
                imageVWStyle = imageVacantWarning.style,
                imageVWClassList = imageVacantWarning.classList;

            imageVacantWarning.setAttribute('id', 'image-vacant-warning');
            imageVacantWarning.textContent = "You have selected proactive chat with an image, but have not uploaded one. Please do that now.";
            imageVacantWarning.style.setProperty('border','2px solid');
            imageVacantWarning.style.setProperty('color', 'black');
            imageVacantWarning.classList.add('b-warning');
            imageVacantWarning.classList.add('m-b-1');
            imageVacantWarning.classList.add('p-a-sm');
            imageVacantWarning.classList.add('m-a-1');

            flyout.appendChild(imageVacantWarning);

        } else if ( document.getElementById('image-vacant-warning') && allImages === typeArray.length ){
            document.getElementById('image-vacant-warning').remove();
        }

        // add headline, invitation, and chat button text + styles
        var headline = document.createElement('h3'),
            invitation = document.createElement('p');

        previewClone.style.backgroundColor = inviteBgClr.value;

        chatMainBtn.backgroundColor = inviteChatBtnTxtClr.value;

        headline.innerText = inviteHeadline.value ? inviteHeadline.value : inviteHeadline.placeholder;
        headline.setAttribute('id','pc-preview-headline');
        headline.style.setProperty('color', formatClr(inviteTextClr.value), 'important');
        headline.style.setProperty('font-size','1.75rem');
        headline.setAttribute('class', 'm-b-1');

        invitation.innerText = inviteInvitation.value ? inviteInvitation.value : inviteInvitation.placeholder;
        invitation.setAttribute('class','m-b-0');
        invitation.setAttribute('id','pc-preview-invitation');
        invitation.style.setProperty('color', formatClr(inviteTextClr.value), 'important');
        invitation.style.setProperty('font-size','17px');

        chatMainBtn.innerText = inviteChatBtnText.value ? inviteChatBtnText.value : inviteChatBtnText.placeholder;
        chatMainBtn.style.setProperty('color', formatClr(inviteChatBtnTxtClr.value));
        chatMainBtn.style.setProperty('font-size','18px');
        chatMainBtn.style.setProperty('background-color', formatClr(invitePrimaryClr.value), 'important');
        chatMainBtn.setAttribute('id', 'pc-preview-main-btn');

        chatMainBtn.parentNode.children[1].style.setProperty('color', formatClr(invitePrimaryClr.value), 'important');
        chatMainBtn.parentNode.children[1].setAttribute('id','pc-preview-decline-btn')

        textAndButtons.prepend(invitation);
        textAndButtons.prepend(headline);

        // add text blurb to logo area
        if ( inviteTextBlurb.value && hasLogo ){ 
            var blurbElm = document.createElement('p');
            var blurbElmWrapper = document.createElement('div');
            var logoBuffer = hasLogo ? 'm-l-sm': '';

            blurbElmWrapper.append(blurbElm);
            blurbElmWrapper.setAttribute('style','display: block;');

            blurbElm.innerText = inviteTextBlurb.value;
            blurbElm.setAttribute('style','display: inline-block; font-style: italic;')
            blurbElm.style.color = formatClr(inviteTextClr.value);
            // blurbElm.style.setProperty('margin-bottom','0 !important');
            blurbElm.setAttribute('class','m-b-0 m-t-sm '+ logoBuffer );
            blurbElm.setAttribute('id', 'pc-preview-blurb-input');
            
            logoArea.classList.add('p-y-sm');
            logoArea.append(blurbElmWrapper);

        } else if ( !!(inviteTextBlurb.value) === false && hasLogo ){
            logoArea.classList.add('p-y-md');
        }

        if ( hasLogo ){
            logoArea.classList.add('p-x-md');
            logoArea.classList.add('w-full');
            logoArea.classList.add('b-t-lightgrey');
        }

        // change id to prevent collision
        flyout.style.setProperty('background-color', formatClr(inviteBgClr.value), 'important');
        previewClone.id = previewClone.id + '-clone';

        return previewClone;
    }

    // open a preview flyout that contains what the new proactive chat widget will look like
    function openPreview(){

        var previewFlyout = document.getElementById('pc-preview-flyout');

        if (previewFlyout.dataset.open === 'false'){      

            var chatInviteEx = document.getElementById('chat-invite-ex');
            var selectedInviteOption = chatInviteEx.options[chatInviteEx.selectedIndex];
            var invitePreview = makeInvitePreview(selectedInviteOption);
            var secondP2 = invitePreview.getElementsByClassName('pc-preview-block')[0];

            // show the flyout
            invitePreview.style.display = 'block';

            //add flags and styles
            previewFlyout.dataset.open = 'true';
            previewFlyout.classList.add('show-preview');
            previewFlyout.appendChild(invitePreview);

            invitePreview.style.setProperty('height', secondP2.clientHeight + 'px' );

        } else {
            var previewClone = document.getElementById('pc-preview-full-clone');

            // close the flyout and delete the clone
            previewFlyout.dataset.open = 'false';
            previewFlyout.classList.remove('show-preview');
            previewClone.parentNode.removeChild( previewClone );

        }
        return false;
    }

    var chatHeadlineInput = document.getElementById('chat-headline-input'),
        chatInvitationInput = document.getElementById('chat-invitation-input'),
        chatButtonInput = document.getElementById('chat-btn-text-input'),
        chatTextBlurbInput = document.getElementById('chat-txt-blurb-input');

    countText( chatHeadlineInput, 'chat-headline-text', chatHeadlineInput.dataset.charlimit );
    countText( chatInvitationInput, 'chat-invitation-text', chatInvitationInput.dataset.charlimit );
    countText( chatButtonInput, 'chat-button-text', chatButtonInput.dataset.charlimit );
    countText( chatTextBlurbInput, 'chat-text-blurb', chatTextBlurbInput.dataset.charlimit );


// live update proactive chat 
    $("#chat-headline-input").on('keyup', function(e){
        $("#pc-preview-headline").text(e.target.value)
    })
    $("#chat-invitation-input").on('keyup', function(e){
        $("#pc-preview-invitation").text(e.target.value)
    })
    $("#chat-btn-text-input").on('keyup', function(e){
        $("#pc-preview-main-btn").text(e.target.value)
    })
    $("#chat-txt-blurb-input").on('keyup', function(e){
        $("#pc-preview-blurb-input").text(e.target.value)
    })
    $("#chat-invite-ex").change(function(e){
        reloadProactiveChat();
    })
    $("#chat-prim-clr").change(function(e){
        var colorVal = '#' + $(this).val();
        console.log('value', $(this).val())
        $('#pc-preview-main-btn').css('background-color',colorVal);
        $('#pc-preview-decline-btn')[0].style.setProperty('color', colorVal ,'important');

    })
    $("#chat-bg-clr").change(function(e){
        $('.pc-preview-block').css('background-color', '#' + $(this).val());
    })
    $("#chat-txt-clr").change(function(e){
        var colorVal = '#' + $(this).val();
        var blurbInput = $("#pc-preview-blurb-input");
        console.log('value', $(this).val())

        $('#pc-preview-headline')[0].style.setProperty('color', colorVal ,'important');
        $('#pc-preview-invitation')[0].style.setProperty('color', colorVal ,'important');

        if ( blurbInput[0] ){
            blurbInput[0].style.setProperty('color', colorVal ,'important');
        }
    })
    $("#chat-btn-txt-clr").change(function(e){
        $('#pc-preview-main-btn')[0].style.setProperty('color', '#' + $(this).val() ,'important');
    })