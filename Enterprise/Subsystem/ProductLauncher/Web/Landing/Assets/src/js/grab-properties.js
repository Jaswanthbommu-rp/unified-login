$('.js-grab-properties').on('click', function () {
    var selectedPropertiesArray = [];
    var $propertyItems = $('.property-group-item');
    var noPropertiesSelectedMsg = '<span class="d-inline-block alert b-warning no-radius p-a-sm" role="alert"><i class="text-warning fa fa-exclamation-triangle m-r-sm"></i>No properties have been selected.</span>';
    var tempFeatureNotAvaliable = '<span class="d-inline-block alert b-warning no-radius p-a-sm" role="alert"><i class="text-warning fa fa-exclamation-triangle m-r-sm"></i>At this time, the selection of more than one property is not supported.</span>';
    var activeTagCTA = '<button type="submit" class="w-md btn primary rounded p-x-md m-y" >Apply Tag</button>';
    var inactiveTagCTA = '<button type="submit" class="w-md btn disabled rounded p-x-md m-y" disabled >Apply Tag</button>';
    var activeReportsCTA = '<button type="submit" class="w-md btn primary rounded p-x-md m-y" >Create Report</button>';
    var inactiveReportsCTA = '<button type="submit" class="w-md btn disabled rounded p-x-md m-y" disabled >Create Report</button>';

    $propertyItems.each(function (i, item) {
        if($(this).is(':checked')) {
//                    console.log($(this.data));
            var thisPropID = $(this).val();
            var thisPropName = $(this).data('property-name');
            var htmlPropTag = '<span class="label secondary-02 m-r-sm m-b-sm d-inline-block" id="' + thisPropID + '"> ' + thisPropName + '</span>';

//                    output.innerHTML = htmlButton + output.innerHTML;
            selectedPropertiesArray.push(htmlPropTag);
//                    console.log(htmlPropTag);

        }
    });
//            console.log('Array contents:', selectedPropertiesArray.length);
    $('.js-collected-properties').html("");
//            Remove once we support selecting more than one property
    $('#modal-cta-reports-button').html("");
    $('#modal-cta-tag-button').html("");

//            Remove check for more than 1 selected property once we support selecting more than one property
    if(selectedPropertiesArray.length > 1) {
        $('.js-collected-properties').append(selectedPropertiesArray);
        $('.js-collected-properties').prepend(tempFeatureNotAvaliable);
        $('#modal-cta-reports-button').append(inactiveReportsCTA);
        $('#modal-cta-tag-button').append(inactiveTagCTA);
    } else if(selectedPropertiesArray.length > 0) {
        $('.js-collected-properties').append(selectedPropertiesArray);
        $('#modal-cta-reports-button').append(activeReportsCTA);
        $('#modal-cta-tag-button').append(activeTagCTA);
    } else {
        $('.js-collected-properties').append(noPropertiesSelectedMsg);
        $('#modal-cta-reports-button').append(inactiveReportsCTA);
        $('#modal-cta-tag-button').append(inactiveTagCTA);
    }


});