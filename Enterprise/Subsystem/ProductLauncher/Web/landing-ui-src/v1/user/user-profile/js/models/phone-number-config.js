//  Telecom Number Model

(function (angular, undefined) {
    "use strict";

    function factory($q, timeout, inputConfig, usageTypeConfig, filterType) {
        var index = 0;

        function TelecomNumberModel() {
            var s = this;
            index++;
            s.init();
        }

        var p = TelecomNumberModel.prototype;

        p.init = function () {
            var s = this;

            s.data = {};
            s.index = index;

            s.phoneNumberConfig = inputConfig({
                minlength: 10,
                maxlength: 10,
                pattern: /^[\d]+$/,
                id: "phone-number-" + s.index,
                inputFilter: filterType.numeric,
                fieldName: "phone-number-" + s.index,
                modelOptions: {
                    allowInvalid: true
                },

                errorMsgs: [
                    {
                        name: "isRequired",
                        text: "Phone number is required"
                    },
                    {
                        name: "minlength",
                        text: "A ten digit phone number is required"
                    },
                    {
                        name: "pattern",
                        text: "Phone number should only contain digits"
                    }
                ],

                onChange: s.onChange.bind(s),

                asyncValidators: {
                    isRequired: s.validatePhoneNumber.bind(s)
                }
            });

            s.usageTypeConfig = usageTypeConfig.get({
                id: "usage-type-" + s.index,
                fieldName: "usage-type-" + s.index,

                asyncValidators: {
                    isRequired: s.validateUsageType.bind(s)
                },

                errorMsgs: [
                    {
                        name: "isRequired",
                        text: "Usage type is required"
                    }
                ]
            });
        };

        // Getters

        p.getData = function () {
            var s = this,
                dd = s.derivedData;

            s.data.areaCode = "01";
            s.data.phoneNumber = dd.phoneNumber.slice(3);
            s.data.areaCode = dd.phoneNumber.slice(0, -7);

            return s.data;
        };

        p.getFieldNames = function () {
            var s = this;
            return ["phone-number-" + s.index, "usage-type-" + s.index];
        };

        // Setters

        p.setChangeCallback = function (callback) {
            var s = this;
            s.onChangeCallback = callback;
            return s;
        };

        p.setData = function (data) {
            var s = this,
                phoneNbr,
                hasPhone = data.areaCode !== undefined && data.phoneNumber !== undefined;

            s.data = data || {};
            phoneNbr = s.data.phoneNumber;

            s.derivedData = {
                phoneNumber: hasPhone ? (s.data.areaCode + phoneNbr) : ""
            };

            if (phoneNbr) {
                if (phoneNbr.length > 3) {
                    phoneNbr = phoneNbr.slice(0, 3) + '-' + phoneNbr.slice(3, 7);
                }
            }

            s.displayDerivedData = {
                phoneNumber: hasPhone ? ("(" + s.data.areaCode + ") " + phoneNbr) : ""
            };

            return s;
        };

        p.setPhoneNumberValidator = function (validator) {
            var s = this;
            s.phoneNumberValidator = validator;
            return s;
        };

        // Actions

        p.hasValidPhoneNumber = function () {
            var s = this,
                dd = s.derivedData;

            return dd.phoneNumber &&
                dd.phoneNumber.length == 10 &&
                dd.phoneNumber.match(/^[\d]+$/);
        };

        p.onChange = function (data) {
            var s = this;

            s.updatePhoneNumber(data);

            if (s.onChangeCallback) {
                s.onChangeCallback();
            }

            return s;
        };

        p.updatePhoneNumber = function (data) {
            var s = this;
            s.data.phoneNumber = data.slice(3);
            s.data.areaCode = data.slice(0, -7);
        };

        p.validatePhoneNumber = function () {
            var s = this,
                deferred = $q.defer();

            timeout(function () {
                var bool = s.phoneNumberIsValid();
                deferred[bool ? "resolve" : "reject"]();
            }, 50);

            return deferred.promise;
        };

        p.validateUsageType = function () {
            var s = this,
                deferred = $q.defer();

            timeout(function () {
                var bool = s.usageTypeIsValid();
                deferred[bool ? "resolve" : "reject"]();
            }, 50);

            return deferred.promise;
        };

        // Assertions

        p.phoneNumberIsValid = function () {
            var s = this;
            return s.phoneNumberValidator ? s.phoneNumberValidator() : true;
        };

        p.usageTypeIsValid = function () {
            var s = this,
                bool = true;

            if (s.derivedData.phoneNumber) {
                bool = !!s.data.contactMechanismUsageType.contactMechanismUsageTypeId;
            }
            else {
                bool = true;
            }

            return bool;
        };

        // Destroy / Reset

        p.destroy = function () {
            var s = this;
            s.usageTypeConfig.destroy();

            s.data = undefined;
            s.derivedData = undefined;
            s.usageTypeConfig = undefined;
            s.phoneNumberConfig = undefined;
        };

        return function (data) {
            return (new TelecomNumberModel()).setData(data);
        };
    }

    angular
        .module("settings")
        .factory("telecomNumberModel", [
            "$q",
            "timeout",
            "rpFormInputTextConfig",
            "usageTypeConfig",
            "rpInputFilterType",
            factory
        ]);
})(angular);
