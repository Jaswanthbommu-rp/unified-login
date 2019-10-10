(function(angular) {
	"use strict";

	var userPasswordStateModel = function($filter) {
        var form = {},
            keys = {
                char_count_check: {
                    val: "is_in_range",
                    lbl: $filter("changePasswordText")("label.charRange")
                },
                uppercase_check: {
                    val: "has_capital_letter",
                    lbl: $filter("changePasswordText")("label.upperCase")
                },
                lowercase_check: {
                    val: "has_small_letter",
                    lbl: $filter("changePasswordText")("label.lowerCase")
                },
                number_check: {
                    val: "has_number",
                    lbl: $filter("changePasswordText")("label.numerical")
                },
                special_char_check: {
                    val: "has_special_characters",
                    lbl: $filter("changePasswordText")("label.specialCharacter")
                },
                history_check: {
                    val: "is_unique",
                    lbl: $filter("changePasswordText")("label.historyRecord")
                }
            },
            regex = {
                uppercase_check: /[A-Z]/,
                lowercase_check: /[a-z]/,
                number_check: /[0-9]/,
                special_char_check: /[!"#\$%&'\(\)\*\+,-\./:;<=>\?@\[\\\]\^_`\{\|\}~ ]/ //OWASP's password special characters: https://www.owasp.org/index.php/Password_special_characters
            };

        form.state = {};
        form.settings = {
            minCharacters: 8,
            maxCharacers: 20
        };

        form.init = function(settings) {
            var state = form.state;

            if(settings) {
                angular.extend(form.settings, settings);
            }

            keys.char_count_check.lbl = keys.char_count_check.lbl.replace("{{min}}", form.settings.minCharacters);
            keys.char_count_check.lbl = keys.char_count_check.lbl.replace("{{max}}", form.settings.maxCharacers);

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
                label: keys.history_check.lbl,
                value: false
            };

            return form.state;
        };

        form.isPasswordValid = function(newPassword) {
            var state = form.state;

            if(state[keys.char_count_check.val] === undefined) {
                return false;
            }

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
            return val && (val.length >= form.settings.minCharacters) && (val.length <= form.settings.maxCharacers);
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

        form.updateUniqueness = function(val) {
            form.state[keys.history_check.val].value = val;
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
        .module("settings")
        .factory("userPasswordState", [
            "$filter",
            userPasswordStateModel
        ]);
        
})(angular);