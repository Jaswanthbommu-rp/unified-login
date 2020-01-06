(function(angular) {
	"use strict";

	var userPasswordStateModel = function($filter, regexlib) {
        var form = {},
            keys = {
                char_count_check: {
                    val: "is_in_range",
                    lbl: $filter("setPasswordText")("label.charRange")
                },
                uppercase_check: {
                    val: "has_capital_letter",
                    lbl: $filter("setPasswordText")("label.upperCase")
                },
                lowercase_check: {
                    val: "has_small_letter",
                    lbl: $filter("setPasswordText")("label.lowerCase")
                },
                number_check: {
                    val: "has_number",
                    lbl: $filter("setPasswordText")("label.numerical")
                },
                special_char_check: {
                    val: "has_special_characters",
                    lbl: $filter("setPasswordText")("label.specialCharacter")
                },
                history_check: {
                    val: "is_unique"
                }
            },
            regex = {
                uppercase_check: /[A-Z]/,
                lowercase_check: /[a-z]/,
                number_check: /[0-9]/,
                special_char_check: regexlib.password,
                whitespace_check: /\s/
            };

        form.state = {};
        form.settings = {
            minimumLength: 8,
            maximumLength: 20
        };

        form.init = function(settings) {
            var state = form.state;

            if(settings) {
                angular.extend(form.settings, settings);
            }

            keys.char_count_check.lbl = keys.char_count_check.lbl.replace("{{min}}", form.settings.minimumLength);
            keys.char_count_check.lbl = keys.char_count_check.lbl.replace("{{max}}", form.settings.maximumLength);

            //number of characters
            state[keys.char_count_check.val] = {
                label: keys.char_count_check.lbl,
                value: false
            };

            //uppercase
            state[keys.uppercase_check.val] = {
                label: keys.uppercase_check.lbl,
                value: false
            };

            //lowercase
            state[keys.lowercase_check.val] = {
                label: keys.lowercase_check.lbl,
                value: false
            };

            //number            
            state[keys.number_check.val] = {
                label: keys.number_check.lbl,
                value: false
            };

            //special character
            state[keys.special_char_check.val] = {
                label: keys.special_char_check.lbl,
                value: false
            };

            //unique to previous 5 passwords
            state[keys.history_check.val] = {
                value: false
            };

            return form.state;
        };

        form.isPasswordValid = function(newPassword) {
            var state = form.state;

            state[keys.char_count_check.val].value = form.isCountWithinRange(newPassword);
            state[keys.uppercase_check.val].value = form.hasUppercases(newPassword);
            state[keys.lowercase_check.val].value = form.hasLowercases(newPassword);
            state[keys.number_check.val].value = form.hasNumerics(newPassword);
            state[keys.special_char_check.val].value = form.hasValidCharacters(newPassword);

            var isValid = state[keys.char_count_check.val].value &&
                state[keys.uppercase_check.val].value &&
                state[keys.lowercase_check.val].value &&
                state[keys.number_check.val].value &&
                state[keys.special_char_check.val].value;

            return isValid;
        };

        form.isCountWithinRange = function(val) {
            return val && (val.length >= form.settings.minimumLength) && (val.length <= form.settings.maximumLength);
        };

        form.hasUppercases = function(val) {
            return val && regex.uppercase_check.test(val);
        };

        form.hasLowercases = function(val) {
            return val && regex.lowercase_check.test(val);
        };

        form.hasNumerics = function(val) {
            return val && regex.number_check.test(val);
        };

        form.hasValidCharacters = function(val) {
            return val && regex.special_char_check.test(val);
        };

        form.updateSettings = function(newPasswordSettings) {
            angular.extend(form.settings, newPasswordSettings);
        };

        form.getState = function() {
            return form.state;
        };

        form.getLabel = function() {
            return form.label;
        };

        form.reset = function() {
            form.state = {};
        };

        return form;
    };

    angular
        .module("new-user")
        .factory("userPasswordState", [
            "$filter",
            "gbRegex",
            userPasswordStateModel
        ]);
        
})(angular);