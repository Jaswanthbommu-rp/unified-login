/*jshint esversion: 6 */

var responseData = "";
var pageResponse = "";
var ipurl = "";
var setCompanySettingUrl = "";
var companyName = "";
let notification = new Notification();

let buildIdentityProviderPage = () => {

    let company = $.sessionStorage.get('company');
    let companyId = company.CompanyId;

    companyName = company.CompanyName;

    var rightToManage = $('body').attr('data-manageright') === 'True' ? true : false;

    let settingPageID = $.sessionStorage.get('settingsPages');
    setCompanySettingUrl = "/setting/companysettings?CompanyId=" + companyId + "&CompanyName=" + companyName;


    let obj = settingPageID.find(function (obj) {
        return obj.name === 'System & Security';
    });

    if (!rightToManage) {
        $('#identity-provider .i-p-action-btns').hide();
    } else {
        $('#identity-provider .i-p-action-btns').show();
    }


    let ipControl = '';

    //select the type
    let selectedType = function (optionData) {
        var foundSelected = false,
            ddlOption = $('#IdentityProviderType .dropdown-row')[0],
            ddlGroup = $('#IdentityProviderType .dropdown-group'),
            dataOption,
            ddlOptionRow;
        $('#IdentityProviderType').find('.dropdown-item').removeClass('checked');
        // if user doesnot manage rights
        if (optionData) {
            ddlGroup.html('');
            for (let i = 0; i < optionData.length; i++) {
                ddlOptionRow = $(ddlOption).clone();
                dataOption = optionData[i].type;
                ddlOptionRow.find('.raul-form-input-text').text(dataOption);
                ddlOptionRow.find('input[name=form-style-second-section]').attr({ 'value': dataOption, 'data-text': dataOption });
                if (optionData[i].isTypeSelected === true && rightToManage) {
                    foundSelected = true;
                    ddlOptionRow.find('.dropdown-item').addClass('checked');
                    $('#IdentityProviderType').find('.dropdown-title').text(dataOption);
                    $("#IdentityProviderType #title-span").text(dataOption);
                }
                else if (rightToManage) {
                    foundSelected = true;
                }
                else if (optionData[i].isTypeSelected === true && !rightToManage) {
                    foundSelected = true;
                    if ($('#viewSelectValue').length < 1) {
                        $('.ip-select').hide().after('<div id="viewSelectValue" class="viewSelectValue">' + optionData[i].type + '</div>');
                    }

                }
                ddlGroup.append(ddlOptionRow);

            }

        }

        if (!foundSelected && $('#viewSelectValue').length < 1) {

            $('.ip-select').hide().after('<div id="viewSelectValue" class="viewSelectValue"> RealPage </div>');
        }

    };

    $('#IdentityProviderType').on('change', function () {
        $('.ipType').empty();
        let selectedValue = $('#IdentityProviderType').find('.dropdown-title').text().trim();
        $('#IdentityProviderType').find('.dropdown-item').removeClass('checked');
        $("#IdentityProviderType input[data-text=" + selectedValue + "]").closest(".dropdown-item").addClass('checked');
        buildForm(selectedValue, responseData);
    });

    //$(document).on('click', '#IdentityProviderType', function () {
    //    let selectedValue = $('#IdentityProviderType .dropdown-item.checked input[type=radio]').data('text');
    //    $("#IdentityProviderType #title-span").text(selectedValue);
    //});

    // Sort controlsList Sequence
    function SortBySequence(x, y) {
        return x.sequence - y.sequence;
    }

    // Build Form based on option values on change
    let buildForm = function (selectedValue, JsonData) {

        if (selectedValue === '') {
            selectedValue = 'RealPage';
        }
        let formFields = "";

        $.each(JsonData, function (index, value) {

            value.controlsList.sort(SortBySequence);

            if (value.type.trim() === selectedValue) {

                $.each(value.controlsList, function (ind, inputcontrols) {
                    let formcontrol = inputcontrols.controlType.toLowerCase();

                    switch (formcontrol) {
                        case 'label':
                            formFields += "<tr><td><div class='col-md-6 col-xs-12 pull-left'><label class='form-label'>" + inputcontrols.labelText + (inputcontrols.infoText !== null && inputcontrols.infoText.length > 0 ? "<a data-tooltip='" + inputcontrols.infoText + "' data-tooltip-pos='top'><img src='../Assets/build/images/settings-icons/settings-notifications.svg' width='20' class='info-icon'></a>" : " ") + "</label></div>";
                            formFields += "<div class='col-md-6 col-xs-12 pull-left' ><div class='pull-right ellipsis'><a href ='" + inputcontrols.linkLabelValue + "'+>" + inputcontrols.linkLabelText + "</a> <br/>" + (inputcontrols.allowCopyOption ? "<a class='copy-link text-primary'>Copy</a>" : "") + "</div></div></td ></tr >";
                            break;
                        case 'link':
                            formFields += "<tr><td><div class='col-md-6 col-xs-12 pull-left'><label class='form-label'>" + inputcontrols.labelText + (inputcontrols.infoText !== null && inputcontrols.infoText.length > 0 ? "<a data-tooltip='" + inputcontrols.infoText + "' data-tooltip-pos='top'><img src='../Assets/build/images/settings-icons/settings-notifications.svg' width='20' class='info-icon'></a>" : "") + "</label></div>";
                            formFields += "<div class='col-md-6 col-xs-12 pull-left'><div class='text-right text-primary'><a href='" + inputcontrols.linkLabelValue + "'>" + inputcontrols.linkLabelText + "</a>";
                            formFields += "</div></div></td ></tr >";
                            break;
                        case 'textarea':
                            formFields += "<tr><td><div class='col-md-4 col-xs-12 pull-left'><label class='form-label'>" + inputcontrols.labelText + (inputcontrols.infoText !== null && inputcontrols.infoText.length > 0 ? "<a data-tooltip='" + inputcontrols.infoText + "' data-tooltip-pos='top'><img src='../Assets/build/images/settings-icons/settings-notifications.svg' class='info-icon'></a>" : "") + "</label></div>";
                            if (!rightToManage) {
                                formFields += "<div class='col-md-8 col-xs-12 pull-right'><div>";
                                formFields += inputcontrols.controlValue !== '' && inputcontrols.controlValue !== null ? inputcontrols.controlValue : inputcontrols.controlDefaultValue !== '' && inputcontrols.controlDefaultValue !== null ? inputcontrols.controlDefaultValue : '';
                                formFields += "</div></div>";
                            } else {
                                formFields += "<div class='col-md-8 col-xs-12 pull-right'><div><textarea id='" + selectedValue + "_" + formcontrol + "_" + inputcontrols.sequence + "' cols='40' rows='5' name='textarea-value' class='form-control' placeholder='" + inputcontrols.controlValuePlaceHolderText + "'>";
                                formFields += inputcontrols.controlValue !== '' && inputcontrols.controlValue !== null ? inputcontrols.controlValue : inputcontrols.controlDefaultValue !== '' && inputcontrols.controlDefaultValue !== null ? inputcontrols.controlDefaultValue : '';
                                formFields += "</textarea></div></div>";
                            }
                            formFields += "</td ></tr >";
                            break;
                        case 'input':
                            formFields += "<tr><td><div class='col-md-6 col-xs-12 pull-left'><label class='form-label'>" + inputcontrols.labelText + " </label></div>";
                            if (!rightToManage) {
                                formFields += "<div class='col-md-5 col-xs-12 pull-right'><div>" + (inputcontrols.controlValue !== '' && inputcontrols.controlValue !== null ? inputcontrols.controlValue : inputcontrols.controlDefaultValue !== '' && inputcontrols.controlDefaultValue !== null ? inputcontrols.controlDefaultValue : '') + "</div></div>";
                            } else {
                                formFields += "<div class='col-md-5 col-xs-12 pull-right'><div><input type='text' id='" + selectedValue + "_" + formcontrol + "_" + inputcontrols.sequence + "' class='form-control' name='input-value' placeholder='" + inputcontrols.controlValuePlaceHolderText + "' value='" + (inputcontrols.controlValue !== '' && inputcontrols.controlValue !== null ? inputcontrols.controlValue : inputcontrols.controlDefaultValue !== '' && inputcontrols.controlDefaultValue !== null ? inputcontrols.controlDefaultValue : '') + "'></div></div>";
                            }
                            formFields += "</td ></tr >";
                            break;
                        // default: alert('Un Known');
                    }


                });
                $('.ipType').html(formFields);

            }

        });


    };

    return Promise.resolve(userAPIService('GET', '', 'api/Pages/' + obj.id + '/PartialPages?companyId=' + companyId + '', '')).then(function (response) {

        // get Identity Provider page by id
        let IdentityProviderPageId = response.find(function (page) {
            //debugger;
            return page.partialPageName === "Identity Provider";
        }).id;

        ipurl = 'api/Pages/' + obj.id + '/PartialPages/' + IdentityProviderPageId + '?companyId=' + companyId;
        return userAPIService('GET', '', ipurl, '');

    }).then(function (response) {
        responseData = response.identityProviderDto;
        //Call Recentlyusedsetting
        var data = { "settingid": responseData[0].id };
        recentlyUsedSettings(data);

        // Get select option values for Type Select
        selectedType(response.identityProviderDto);
    }).then(function () {
        // build form based on option value on change event 
        if (!rightToManage) {
            buildForm($('#viewSelectValue').text(), responseData);
        }
        else {
            buildForm($('#IdentityProviderType').find('.dropdown-title').text().trim(), responseData);
        }
        });




    $(document).on('click', '#identity-provider-cancel', function () {
        location.href = setCompanySettingUrl;
    });



};



$(document).on('click', '#identity-provider-save', function () {
    // var iptype = $('#IdentityProviderType').val();
    var iptype = $('#title-span').text();
    //console.log(responseData)

    let savObj = responseData.find(function (obj) {
        return obj.type === iptype;
    });


    for (var i = 0; i < responseData.length; i++) {
        //  console.log('sdfsdf' + responseData[i].type + "--" + savObj.type)
        if (responseData[i].type === savObj.type) {
            responseData[i].isTypeSelected = true;
        } else {
            responseData[i].isTypeSelected = false;
        }
    }


    for (var j = 0; j < savObj.controlsList.length; j++) {
        let formcontrol = savObj.controlsList[j].controlType.toLowerCase();

        switch (formcontrol) {
            case 'textarea':
            case 'input':
                savObj.controlsList[j].controlValue = $("#" + iptype + "_" + formcontrol + "_" + savObj.controlsList[j].sequence).val();
        }


    }

    var postObj = { "identityProviderDto": [] };
    var company = $.sessionStorage.get('company');
    var companyId = company.CompanyId;

    postObj.identityProviderDto = responseData;
    postObj.companyId = companyId;
    userAPIService('PUT', '', ipurl, postObj).then(function (response) {
        //Show scussee message
        $('.raul-notification').remove();

        notification.create('successIdentityProvider');

        return response;
    });

});

$(document).on('click', '.copy-link', function () {
    var copyText = $(this).closest('div').find('a:first-child').text();
    copy(copyText);
});

//copy text to clipboard
function copy(copyText) {
    var inp = document.createElement('input');
    document.body.appendChild(inp);
    inp.value = copyText;
    inp.select();
    document.execCommand('copy', false);
    inp.remove();
}


$(document).on('click', '#identity-provider-cancel', function () {
    location.href = setCompanySettingUrl;
});

