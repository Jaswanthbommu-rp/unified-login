
/*jshint esversion: 6 */
$(document).on('click', '#showVideoTutToggle', function () {
    ShowHideUserSkipToggle($(this).is(':checked'));
    ShowHideNewUsersRows($('#newuserSkipToggle').is(':checked'));
});


$(document).on('click', '.skipcontrolTrigger', function () {

    if ($('.skipcontrolTrigger:checked').length > 0) {
        $('.skipControlWrap').show();
    } else {
        $('.skipControlWrap').hide();
        $('#newuserSkipToggle').attr('checked', false);

    }

});


$(document).on('click', '#newuserSkipToggle', function () {
    ShowHideNewUsersRows($(this).is(':checked'));
});


let ShowHideUserSkipToggleNew = (val) => {

    if (val) {
        $('.skipControlWrap').show();
    } else {
        $('.skipControlWrap').hide();
    }

};


let ShowHideUserSkipToggle = (newUser, migrateUser) => {

    if (newUser || migrateUser) {
        $('.skipControlWrap').show();
    } else {
        $('.skipControlWrap').hide();
    }

};

let ShowHideNewUsersRows = (val) => {
    if (val) {
        $('#AllowNewUsersRows').show();
    } else {
        $('#AllowNewUsersRows').hide();
    }

};

// Video tutorials stting page Data population 

var company;
var companyId;
var simonLearnId = 0;
var rightToManage = $('body').attr('data-manageright') === 'True' ? true : false;
var videoTutorialsUrl = "api/SimonLearn";

$(document).ready(function () {
    company = $.sessionStorage.get('company');
    companyId = company.CompanyId;
    var companyName = company.CompanyName;

    var setCompanySettingUrl = "/setting/companysettings?CompanyId=" + companyId + "&CompanyName=" + companyName;
    var setSettingUrl = "/setting?CompanyId=" + companyId + "&CompanyName=" + companyName;

    //for breadcrumbs links
    $('.h-t-settings').attr('href', setSettingUrl);
    $('.h-t-companysettings').attr('href', setCompanySettingUrl);


    $(document).on('click', '#VideoSettingCancelBtn', function () {
        location.href = setCompanySettingUrl;
    });





    let videoTutorialsGetUrl = videoTutorialsUrl + "?companyId=" + companyId;

    return Promise.resolve(userAPIService('GET', '', videoTutorialsGetUrl, '')).then(function (response) {

        if (response !== null && response.length > 0) {
            LoadVideoValues(response);
        } else {
            LoadVideoDefaulValues();
        }

    }).catch(function (error) {
        console.log(error)

        });

});



let LoadVideoDefaulValues = () => {

    $('#showVideoTutToggle').prop('checked', true);
    ShowHideUserSkipToggle(true);
    $('#newuserSkipToggle').prop('checked', false);
    $('#numberSkipVideo').prop(3);
    $('#skipVideoAfterPercentage').val(100);
    $('#markVideoCompleteAfterPercentage').val(100);

};

let LoadVideoValues = (response) => {
    simonLearnId = response[0].settingId;

    if (!rightToManage) {
        $('#showVideoTutToggle').prop('checked', response[0].showVideoForNewUser).attr("disabled", true);
        $('#showVideoTutMigrateToggle').prop('checked', response[0].showVideoForMigratedUser).attr("disabled", true);
        ShowHideUserSkipToggle(response[0].showVideoForNewUser, response[0].showVideoForMigratedUser);
        $('#newuserSkipToggle').prop('checked', response[0].allowSkipVideos).attr("disabled", true);
        ShowHideNewUsersRows(response[0].allowSkipVideos);
        $('#numberSkipVideo').val(response[0].noOfTimesUserSkipVideo).attr("disabled", true);
        $('#skipVideoAfterPercentage').val(response[0].skipVideoAfterPercentage).attr("disabled", true);
        $('#markVideoCompleteAfterPercentage').val(response[0].markVideoCompleteAfterPercentage).attr("disabled", true);
        $('.i-p-action-btns').remove();
    } else {
        $('#showVideoTutToggle').prop('checked', response[0].showVideoForNewUser);
        $('#showVideoTutMigrateToggle').prop('checked', response[0].showVideoForMigratedUser);
        ShowHideUserSkipToggle(response[0].showVideoForNewUser, response[0].showVideoForMigratedUser);
        $('#newuserSkipToggle').prop('checked', response[0].allowSkipVideos);
        ShowHideNewUsersRows(response[0].allowSkipVideos);
        $('#numberSkipVideo').val(response[0].noOfTimesUserSkipVideo);
        $('#skipVideoAfterPercentage').val(response[0].skipVideoAfterPercentage);
        $('#markVideoCompleteAfterPercentage').val(response[0].markVideoCompleteAfterPercentage);
    }
};



$(document).on('click', '.raul-notification-close', function () {
    $(this).closest('.raul-notification-container').removeClass('raul-notification-slide-show').addClass('raul-notification-slide-hide');
});




var simonVideoReqDto;
$(document).on('click', '#saveVideoSetting', function () {



    if (!$('#newuserSkipToggle').is(':checked')) {
        simonVideoReqDto = getReqDto();
        saveValues();
    } else if (!$('#showVideoTutToggle').is(':checked') && !$('#showVideoTutMigrateToggle').is(':checked')) {
        simonVideoReqDto = getReqDtoDefault();
        saveValues();
    } else {
        $('#video-settings').parsley().validate();

        if ($('#video-settings').parsley().isValid()) {

            simonVideoReqDto = getReqDto();
            saveValues();
        }
    }


});


function saveValues() {

    if (parseInt(simonLearnId) > 0) {
        //Call PUT Method 
        updateValues('PUT', videoTutorialsUrl + "/" + simonLearnId, simonVideoReqDto);
    } else {

        updateValues('POST', videoTutorialsUrl, simonVideoReqDto);
    }

}

var getReqDto = function getReqDto() {
    var skipChecked = $('#newuserSkipToggle').is(':checked');
    var simonVideoReqDto = {
        "showVideoForNewUser": $('#showVideoTutToggle').is(':checked'),
        "showVideoForMigratedUser": $('#showVideoTutMigrateToggle').is(':checked'),
        "allowSkipVideos": skipChecked,
        "noOfTimesUserSkipVideo": skipChecked ? Math.round( parseFloat(+$('#numberSkipVideo').val()) ) : 3,
        "skipVideoAfterPercentage": skipChecked ? Math.round( parseFloat(+$('#skipVideoAfterPercentage').val()) ) : 100,
            "markVideoCompleteAfterPercentage": skipChecked ? Math.round( parseFloat(+$('#markVideoCompleteAfterPercentage').val()) ) : 100,
        "companyId": companyId
    };
    return simonVideoReqDto;
};

var getReqDtoDefault = function getReqDtoDefault() {
    var skipChecked = $('#showVideoTutToggle').is(':checked') && $('#showVideoTutMigrateToggle').is(':checked');
    var simonVideoReqDto1 = {
        "showVideoForNewUser": $('#showVideoTutToggle').is(':checked'),
        "showVideoForMigratedUser": $('#showVideoTutMigrateToggle').is(':checked'),
        "allowSkipVideos": $('#newuserSkipToggle').is(':checked'),
        "noOfTimesUserSkipVideo": skipChecked ? Math.round( parseFloat(+$('#numberSkipVideo').val()) ) : 3,
        "skipVideoAfterPercentage": skipChecked ? Math.round( parseFloat(+$('#skipVideoAfterPercentage').val()) ) : 100,
        "markVideoCompleteAfterPercentage": skipChecked ? Math.round( parseFloat(+$('#markVideoCompleteAfterPercentage').val()) ) : 100,
        "companyId": companyId
    };
    return simonVideoReqDto1;
};





var updateValues = function updateValues(method, url, obj) {

    userAPIService(method, '', url, obj).then(function (response) {
        console.log(response.status)
        if (response.error) {
            throw response;
          } else {

            $('.raul-notification').remove();

            new RAUL.Notification({
                type: 'success',
                title: 'Data Saved Successfully',
                preventDisplay: false,
                ttl: 2000,
                showCloseButton: false,
            });
           
            return response;
        }
    }).then(response => {
        let videoTutorialsGetUrl = videoTutorialsUrl + "?companyId=" + companyId;

        return userAPIService('GET', '', videoTutorialsGetUrl, '');
        
    }).then(function (response) {

        if (response !== null && response.length > 0) {
            LoadVideoValues(response);
        } else {
            LoadVideoDefaulValues();
        }

    }).catch(function (response) {
        $('.raul-notification').remove();

        new RAUL.Notification({
            type: 'danger',
            title: response.error.message,
            preventDisplay: false,
            ttl: 2000,
            showCloseButton: false,
        });


    });
};