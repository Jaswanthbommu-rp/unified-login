/****

Created 07/26/2017

Displays and configures dropzones based on dropzone.js -> this requires dropzone.js to function.

Modified 08/31/2017

****/

// Turn 'there can only be one' off for Dropzone.
Dropzone.autoDiscover = false;

Dropzone.__proto__.getOrientation = function(height, width){
    var ratio = height / width,
        returnObj = {};

    if ( ratio > 1){
    // h>w
        returnObj.type = 'portrait';
    } else if ( ratio < 1 && ratio > 0){
    // h<w
        returnObj.type = 'landscape';
    } else {
    // h=w
        returnObj.type = 'square';
    }

    returnObj.ratio = ratio;

    return returnObj;
};

Dropzone.__proto__.logoProportion = function(previewsContainer, file, returnObjBool){

    var dzImage = previewsContainer.find('.dz-preview'),
        orientationObj = Dropzone.getOrientation( file.height, file.width ),
        orientation = orientationObj.type,
        ratio = orientationObj.ratio;

        // console.log('orientation object ', orientationObj);
        
    if ( orientation === 'portrait' ) {

        previewsContainer.width( function(idx, width){
            var previewsHeight = previewsContainer.height();
            return previewsHeight / ratio;
        });

        dzImage.removeClass('centered');

    } else if ( orientation === 'landscape' ) {

        previewsContainer.width( function(idx, width){
            var dzImageRatioW = width / file.width;

            return $(this).height() / ratio;
        });

        previewsContainer.find('.dz-image').height(function(idx, height){
            var height = height ? height : $(this).closest('.dz-preview').height(),
                dzImageRatio = height / file.height,
                dzImageW = $(this).width();

            // console.log('previewsContainer', previewsContainer);
            // console.log('this', $(this));
            // console.log('height', height);
            // console.log('width', dzImageW);
            // console.log('dzImageRatio', dzImageRatio);
            // console.log('(height * ( $(this).width() / file.width ) ) / dzImageRatio', (height * ( $(this).width() / file.width ) ) / dzImageRatio);

            if ( height / dzImageW > ratio ){
                return ( height * ( $(this).width() / file.width ) ) / dzImageRatio;
            } else {
                return height;
            }

        })

        dzImage.addClass('centered');
    } else {

        previewsContainer.width( function(idx, width){
            return $(this).height();
        });

        dzImage.removeClass('centered');
    }

    if (returnObjBool){
        return {
            width: previewsContainer.width(),
            height: previewsContainer.height()
        }
    }

}

// Modifications are from - http://www.dropzonejs.com/
function createDropZone(DZLocation, submitUrl, acceptedFileTypes, previewsContainerBool, imageUrl, otherModsObj, heightWidthRatioCallback){
    var dropZoneObject = {
            url: submitUrl,
            acceptedFiles: acceptedFileTypes || '.png,.jpg,.jpeg' ,
            previewsContainer: previewsContainerBool ? DZLocation + '-preview.dropzone-previews' : null,
            maxFiles: 1,
            init: function(){
                var imgUrl = imageUrl,
                    thisdropzone = this;

                this.on('thumbnail',function(file){
                    // console.log('thubnail')
                    var dz = this;
                    $(file.previewElement).click(function(e){
                        dz.hiddenFileInput.click();
                    })

                }); 

                this.on('change',function(event){
                    // console.log('change',event);
                });

                this.on('maxfilesexceeded',function(file){
                    this.removeAllFiles();
                    this.addFile(file);
                });

                this.on('addedfile', function(){
                    // console.log('addedfile');
                })

                this.on('success', function(file, response){

                    // console.log('great success uploading image');

                    var previewsContainer = $(this.previewsContainer),
                        dzWarnMess = $( DZLocation + '-warning-message');
                        
                    if ( heightWidthRatioCallback( file.height, file.width ) ){
                        
                        if ( previewsContainer.hasClass('b-accent') ){
                            previewsContainer.removeClass('b-accent');
                            dzWarnMess.hide();
                        }
                        previewsContainer.css('border-color', 'green');

                    } else {

                        if ( previewsContainer.hasClass('b-accent') === false ){
                            previewsContainer.addClass('b-accent');
                            dzWarnMess.show();
                        }

                    }

                    if ( previewsContainer.attr('id') === 'dropzone-widget-logo-preview'){
                        Dropzone.logoProportion(previewsContainer, file);
                    }

                    if (/proactive/.test(this.previewsContainer.id)){
                        reloadProactiveChat();
                    }
                });

                this.on('error', function(file, response) {
                    var previewsContainer = $(this.previewsContainer);
                    
                    if (response !== "You can not upload any more files."){
                        $(file.previewElement).find('.dz-error-message').text('There was an error with your upload, please try again. ' + response);
                        previewsContainer.css('border-color', 'red');                        
                    } else {
                        previewsContainer.css('border-color', 'black');                        
                    }
                });

                this.on('complete',function(){
                    // console.log('complete');
                })

                if (imgUrl !== null && imgUrl !== undefined && imgUrl !== ""){

                    var file = {
                        name: imgUrl,
                        size: 1000,
                        status: Dropzone.ADDED,
                        accepted: true,
                        url: imgUrl
                    }, img = new Image();

                    // only for initial image load to take advantage of listeners
                    img.onload = function(){
                        file['width'] = this.width;
                        file['height'] = this.height;

                        thisdropzone.emit("addedfile", file);                         
                        thisdropzone.emit("thumbnail", file, imgUrl);
                        thisdropzone.emit("complete", file);
                        thisdropzone.emit("success", file);
                        thisdropzone.files.push(file);
                    }

                    img.src = imgUrl;

                }

                $(DZLocation).find('.dz-preview').click(function(e){
                    thisdropzone.hiddenFileInput.click();
                });

            },
            resize: function(file, width, height){
                var returnObj = {},
                    previewsContainer = $(this.previewsContainer),
                    ratio = file.height / file.width,
                    trgWidth = width,
                    trgHeight = height;

                if ( $('#conditional-invite-photo.no-logo')[0] ){
                    returnObj['trgWidth'] = 220;
                    returnObj['trgHeight'] = 200;
                }

                if ( previewsContainer.attr('id') === 'dropzone-widget-logo-preview' || previewsContainer.attr('id') === 'dropzone-proactive-logo'){
                    var logoProp = Dropzone.logoProportion(previewsContainer, file, true);

                    trgWidth = logoProp.width;
                    trgHeight = logoProp.height;
                }

                returnObj['srcWidth'] = file.width;
                returnObj['srcHeight'] = file.height;
                returnObj['trgWidth'] = trgWidth;
                returnObj['trgHeight'] = trgHeight;

                // console.log('return obj -- resize', returnObj);

                return returnObj;
            }

        };
         
    for ( var key in otherModsObj ){
         dropZoneObject[ key ] = otherModsObj[ key ];
    }

    return new Dropzone( DZLocation, dropZoneObject );
}