'use strict';

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

/*jshint esversion: 6 */

var CustomFields = function () {
    function CustomFields() {
        _classCallCheck(this, CustomFields);

        this.customFields = $.sessionStorage.get('customFields');
        this.accessUrl = 'api/CustomFields';
        this.notification = new Notification();
    }

    _createClass(CustomFields, [{
        key: 'getCustomFields',
        value: function getCustomFields() {
            var company = $.sessionStorage.get('company');
            var CompanyId = company.CompanyId,
                CompanyName = company.CompanyName;


            return userAPIService('GET', '', this.accessUrl + '?companyId=' + CompanyId, '').then(function (response) {
                return response;
            });
        }
    }, {
        key: 'populateCustomFieldsTable',
        value: function populateCustomFieldsTable(datatable, customFields) {

            //Clear the table before populating it
            datatable.fnClearTable();

            if (customFields) {
                //Populate the table with custom fields
                customFields.sort(function (a, b) {
                    return a.sequence - b.sequence;
                }).forEach(function (field) {
                    var statusColumn = '<div class="raul-table-status-column ' + (field.isFieldEnabled ? 'raul-table-status-column-active' : '') + '">' + '<div>' + (field.isFieldEnabled ? 'Enabled' : 'Disabled') + '</div>' + '</div>';
                    var minCharText = !field.minCharlength && !field.maxCharlength ? '' : '<span> | ' + (field.minCharlength + ' - ' + field.maxCharlength + ' characters</span>');
                    var infoTooltip = '<div class="custom-field-tooltip">' + '<a><img src="/home/Assets/build/images/settings-icons/settings-notifications.svg" class="info-icon"></a>' + '</div>' + '<div class="custom-field-tooltip-content">' + '<div class="second-line-text">' + minCharText + '</div>' + statusColumn + '</div>';
                    var controlPanelColumn = _rightToManage ? '<div class="control-panel-wrapper clearfix">' + infoTooltip + '<div class="control-panel-drag-drop" data-id=' + field.id + '><img src="/home/Assets/build/images/settings-icons/move.svg" alt=""></div>' + '<div class="control-panel-edit" data-id=' + field.id + '><img src="/home/Assets/build/images/settings-icons/pencil-1.svg" alt=""></div>' : '';

                    datatable.fnAddData(['<div class="raul-datatable-main-info">' + field.sequence + '.' + '</div>', '<div class="raul-datatable-main-info">' + '<div>' + field.fieldName + '</div>' + '<div class="second-line-text">' + field.fieldType + ' | ' + (field.isRequiredField ? 'Required' : 'Optional') + '<span class="custom-field-length">' + minCharText + '</span></div>' + '</div>', statusColumn, controlPanelColumn + '</div>']);
                });
            }
        }
    }, {
        key: 'addCustomField',
        value: function addCustomField(fieldData) {
            return userAPIService('POST', '', this.accessUrl, fieldData).then(function (response) {
                return response;
            });
        }
    }, {
        key: 'updateCustomField',
        value: function updateCustomField(fieldId, fieldData) {
            return userAPIService('PUT', '', this.accessUrl + '/' + fieldId, fieldData).then(function (response) {
                return response;
            });
        }
    }, {
        key: 'updateFieldsSequence',
        value: function updateFieldsSequence(fields) {
            return userAPIService('PUT', '', this.accessUrl + '/sequence', fields);
        }
    }, {
        key: 'closeModal',
        value: function closeModal(modal, form) {
            form.attr('data-field-id', '');
            form[0].reset();
            form.parsley().reset();
            modal.modal('hide');
        }
    }, {
        key: 'sendSequenceUpdate',
        value: function sendSequenceUpdate(fields) {
            var _this = this;

            this.updateFieldsSequence(fields).then(function (response) {
                if (response.error) {
                    throw response;
                }
                return customFields.getCustomFields();
            }).then(function (fieldsData) {
                $.sessionStorage.set('customFields', fieldsData);
            }).catch(function (response) {
                if (response.details) {
                    var details = response.details;
                    if (details.length > 0 && details[0].code === "DuplicateSequence") {
                        $('.raul-notification').remove();

                        _this.notification.create('sequenceError');
                    }
                }
            });
        }
    }]);

    return CustomFields;
}();

var _rightToManage = false;

var datatable = void 0;

var customFieldIntialize = function customFieldIntialize() {
    var customFields = new CustomFields();
    new Promise(function (resolve, reject) {

        return customFields.getCustomFields().then(function (response) {
            $.sessionStorage.set('customFields', response);
            resolve(response);
            var data = { "settingid": response[0].id };
            recentlyUsedSettings(data);
        });
    }).then(function (fields) {
        var customSettingsTable = $('#settings-custom-fields');
        datatable = customSettingsTable.dataTable();
        customFields.populateCustomFieldsTable(datatable, fields);
    });
};

var testSequenceError = function testSequenceError(updatedFields) {
    var customFields = new CustomFields();

    var CompanyId = $.sessionStorage.get('company').CompanyId;
    customFields.sendSequenceUpdate({ customFieldReqDto: updatedFields, CompanyId: CompanyId });
};

$(document).ready(function () {

    var customFields = new CustomFields();
    //let customSettingsTable = $('#settings-custom-fields');
    // var datatable = customSettingsTable.dataTable();
    var newCustomFiedsForm = $("#new-custom-field-form");
    var newCustomFieldsModal = $('#custom-fields-modal');

    //let fieldsData = $.sessionStorage.get('customFields');

    _rightToManage = $('body').attr('data-manageright') === 'True' ? true : false;
    if (!_rightToManage) {
        $('#create-custom-field').css("display", "none");
    }

    //CUSTOM PARSLEY VALIDATOR
    if (window.ParsleyValidator && !window.Parsley.hasValidator('customfieldname')) {
        window.Parsley.addValidator('customfieldname', function (value, fieldId) {
            var fieldsData = $.sessionStorage.get('customFields');

            return !fieldsData.some(function (field) {
                if (fieldId === field.id) return false;

                return field.fieldName.toLowerCase() === value.toLowerCase();
            });
        }).addMessage('en', 'customfieldname', 'Field name already exists');
    }

    if (window.ParsleyValidator && !window.Parsley.hasValidator('greaterorequal')) {
        window.Parsley.addValidator('greaterorequal', function (value, fieldId) {
            var minValue = $('#new-field-min-char').val();
            var maxValue = $('#new-field-max-char').val();

            if (maxValue && minValue) {
                return parseInt(maxValue) >= parseInt(minValue);
            } else {
                return true;
            }
        }).addMessage('en', 'greaterorequal', 'This value should be equal or greater than min value');
    }

    //EVENT Handlers
    $('#create-custom-field').click(function (e) {
        if (!_rightToManage) return;

        $('.custom-fields-required-toggle').removeClass('hidden'); //show required/optional toggle
        newCustomFieldsModal.find('.custom-fields-modal-header').text('New Custom Field');
        newCustomFieldsModal.find('.custom-field-title').text('Custom User Field');
        $('#create-custom-field-btn').text('Create');
        newCustomFiedsForm.attr('data-field-id', '');
        $('.dropdown-item').removeClass('checked');
        $('#drpalpnum').addClass('checked');
        $('#drpCustom-fieldsTitle').text('Alphanumeric');

        newCustomFieldsModal.modal('show');
    });

    $(document).on('click', '.control-panel-edit', function () {
        //populate form with custom field data

        if (!_rightToManage) return;

        //hide required/optional toggle by default
        $('.custom-fields-required-toggle').addClass('hidden');
        $('.dropdown-item').removeClass('checked');

        var fieldId = $(this).attr('data-id');
        var customFields = $.sessionStorage.get('customFields');

        var fieldToUpdate = customFields.find(function (field) {
            return field.id === +fieldId;
        });
        newCustomFiedsForm.attr('data-field-id', fieldToUpdate.id);

        newCustomFieldsModal.find('.custom-fields-modal-header').text('Edit Custom Field');
        newCustomFieldsModal.find('.custom-field-title').text(fieldToUpdate.fieldName);
        newCustomFieldsModal.find('input[name=fieldName]').val(fieldToUpdate.fieldName).attr('data-parsley-customfieldname', fieldId);
        $('#create-custom-field-btn').text('Update');

        var minCharlength = fieldToUpdate.minCharlength ? fieldToUpdate.minCharlength : '';
        var maxCharlength = fieldToUpdate.maxCharlength ? fieldToUpdate.maxCharlength : '';

        $('#new-field-min-char').val(minCharlength);
        $('#new-field-max-char').val(maxCharlength);
        newCustomFieldsModal.find('input[name=isRequiredField]').prop('checked', fieldToUpdate.isRequiredField);
        newCustomFieldsModal.find('input[name=isFieldEnabled]').prop('checked', fieldToUpdate.isFieldEnabled);
        newCustomFieldsModal.find('span[name=fieldType]').text(fieldToUpdate.fieldType.charAt(0).toUpperCase() + fieldToUpdate.fieldType.substring(1)).change();
        if (fieldToUpdate.fieldType.charAt(0).toUpperCase() + fieldToUpdate.fieldType.substring(1) == "Numeric") {
            $('#drpnum').addClass('checked');
        } else {
            $('#drpalpnum').addClass('checked');
        }
        if (fieldToUpdate.isFieldEnabled) {
            // if field is enabled, show required/optional toggle
            $('.custom-fields-required-toggle').removeClass('hidden');
        }

        newCustomFieldsModal.modal('show');
    });

    newCustomFieldsModal.find('.close').click(function (e) {
        customFields.closeModal(newCustomFieldsModal, newCustomFiedsForm);
    });

    newCustomFieldsModal.find('.cancel-custom-field-btn').click(function (e) {
        customFields.closeModal(newCustomFieldsModal, newCustomFiedsForm);
    });

    newCustomFiedsForm.submit(function (event) {
        var _this2 = this;

        if (event.keyCode == 13) {
            event.preventDefault();
            return false;
        }

        event.preventDefault();
        var fieldId = +$(this).attr('data-field-id');

        if ($(this).parsley().isValid()) {
            var data = createDataObject($(this));
            var company = $.sessionStorage.get('company');
            var CompanyId = company.CompanyId,
                CompanyName = company.CompanyName;


            data.isFieldEnabled = Boolean(data.isFieldEnabled);
            data.fieldType = $('#drpCustom-fieldsTitle').text().toLocaleLowerCase(); //data.fieldType.toLowerCase();
            data.isRequiredField = Boolean(data.isRequiredField);
            data.maxCharlength = data.maxCharlength ? Math.round(parseFloat(+data.maxCharlength)) : 1024; // if there is no value specified set max to 1024
            data.minCharlength = data.minCharlength ? Math.round(parseFloat(+data.minCharlength)) : 1; // if there is no value specified set min to 1
            data.companyId = CompanyId.toString();

            if (!fieldId) {
                customFields.addCustomField(data).then(function (newCustomField) {
                    if (newCustomField.error) {
                        throw newCustomField;
                    }

                    return customFields.getCustomFields();
                }).then(function (fieldsData) {
                    $.sessionStorage.set('customFields', fieldsData);
                    customFields.populateCustomFieldsTable(datatable, fieldsData);

                    // reset form and close modal
                    customFields.closeModal(newCustomFieldsModal, newCustomFiedsForm);
                }).catch(function (err) {
                    var details = err.details;

                    details.forEach(function (errorDetail) {
                        $('[name=\'' + errorDetail.target + '\']').parsley().addError('customValidationError', { message: errorDetail.message });
                    });
                });
            } else {
                //update existing Custom user field
                var customFieldsData = $.sessionStorage.get('customFields');

                var sequence = customFieldsData.find(function (item) {
                    return item.id === fieldId;
                }).sequence;
                data = Object.assign(data, { sequence: sequence });

                customFields.updateCustomField(fieldId, data).then(function (updatedCustomField) {
                    if (updatedCustomField.error) {
                        throw updatedCustomField;
                    }

                    return customFields.getCustomFields();
                }).then(function (fieldsData) {
                    $.sessionStorage.set('customFields', fieldsData);
                    customFields.populateCustomFieldsTable(datatable, fieldsData);

                    // reset form and close modal
                    customFields.closeModal(newCustomFieldsModal, $(_this2));
                }).catch(function (err) {
                    var details = err.details;

                    details.forEach(function (errorDetail) {
                        $('[name=\'' + errorDetail.target + '\']').parsley().addError('customValidationError', { message: errorDetail.message });
                    });
                });
            }
        }
    });

    //Drag&Drop handler
    $('#settings-custom-fields').on('row-reorder.dt', function (e, diff, edit) {
        if (!diff.length) return;
        var company = $.sessionStorage.get('company');
        var CompanyId = company.CompanyId,
            CompanyName = company.CompanyName;


        var updatedFields = diff.map(function (row) {
            var sequence = $.parseHTML(row.newData)[0].innerHTML;
            var id = row.node.querySelector('.control-panel-drag-drop').getAttribute('data-id');

            return {
                id: parseInt(id),
                sequence: parseInt(sequence)
            };
        });

        customFields.sendSequenceUpdate({ customFieldReqDto: updatedFields, CompanyId: CompanyId });
    });

    //Remove custom validation errors on fields change
    newCustomFiedsForm.on('change', 'input', function () {
        $(this).parsley().removeError('customValidationError');
    });

    //if there is no right to manage settings - disable drag & drop

    if (!_rightToManage) {

        $('#settings-custom-fields').on('mousedown', '.control-panel-drag-drop', function (e) {
            return false;
        });
    }

    $(document).on('keydown', function (e) {
        if (e.keyCode === 27) {
            customFields.closeModal(newCustomFieldsModal, newCustomFiedsForm);
        }
    });

    //if field is disabled, hide required/optional toggle
    $('input[name=isFieldEnabled]').on('change', function (e) {
        $('.custom-fields-required-toggle').toggleClass('hidden', !this.checked);
        $('.custom-fields-required-toggle.hidden').find('[name=isRequiredField]').prop('checked', this.checked);
    });
});