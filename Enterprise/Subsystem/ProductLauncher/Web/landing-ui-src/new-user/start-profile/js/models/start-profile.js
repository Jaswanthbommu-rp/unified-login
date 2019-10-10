//  Start Profile Form Model

(function(angular) {
    "use strict";

    function factory(baseForm, dataObj) {
        var model = baseForm();

        model.setSubmitBtnDisabled = function(state) {
            model.form.submitBtnDisabled = state === undefined || state === true ? true : false;

            return model;
        };

        model.setUsername = function(username) {
            model.form.username = username;
        };

        model.getParsedPhoneNumbers = function(phones) {
            phones.forEach(function(ph) {
                var phone = ph.phoneNumber;
                if (phone !== "") {
                    ph.countryCode = ""; //TODO assumed for MVP, assign correct value eventually
                    ph.areaCode = phone.slice(0, -7);
                    ph.phoneNumber = phone.slice(3);
                }
            });
            return phones;
        };

        model.addPhone = function() {
            var s = this;
            if (dataObj.telecomNumbers.length > 0) {                
                var emptyFlag = false;
                dataObj.telecomNumbers.forEach(function(item) {
                    if (item.phoneNumber === "") {
                        emptyFlag = true;
                    }
                });
                if (emptyFlag === true) { return s; }
            }

            var data = {
                "partyContactMechanismId": 0,
                "contactMechanismId": 0,
                "countryCode": "",
                "areaCode": "",
                "phoneNumber": "",
                "isDeleted": false,
                "contactMechanismUsageType": {
                    "contactMechanismUsageTypeId": null,
                    "parentContactMechanismUsageTypeId": 200,
                    "name": ""
                }
            };
            
            dataObj.telecomNumbers.push(data);
           
            return s;
        };

        model.delPhone = function(itemDel) {
            var s = this;
           
            itemDel.isDeleted = true;
            var a = 0;
            dataObj.telecomNumbers.forEach(function(item) {
                if (item.isDeleted === true) {
                    dataObj.telecomNumbers.splice(a, 1);
                    return;
                }
                a++;
            });

            return s;
        };

        return model;
    }

    angular
        .module("new-user")
        .factory("startProfileFormModel", [
            "baseForm",
            "startProfileFormData",
            factory
        ]);
})(angular);