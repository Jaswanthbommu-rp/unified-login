'use strict';

// Load this after Parsley for additional comparison validators

// gt, gte, lt, lte, notequalto extra validators
var parseRequirement = function parseRequirement(requirement) {
    if (isNaN(+requirement)) return parseFloat(jQuery(requirement).val());else return +requirement;
};

// Greater than validator
window.Parsley.addValidator('gt', {
    validateString: function validateString(value, requirement) {
        return parseFloat(value) > parseRequirement(requirement);
    },
    priority: 32
});

// Greater than or equal to validator
window.Parsley.addValidator('gte', {
    validateString: function validateString(value, requirement) {
        return parseFloat(value) >= parseRequirement(requirement);
    },
    priority: 32,
    messages: {
        en: 'This value should be equal or greater than min value'
    }
});

// Less than validator
window.Parsley.addValidator('lt', {
    validateString: function validateString(value, requirement) {
        return parseFloat(value) < parseRequirement(requirement);
    },
    priority: 32
});

// Less than or equal to validator
window.Parsley.addValidator('lte', {
    validateString: function validateString(value, requirement) {
        return parseFloat(value) <= parseRequirement(requirement);
    },
    priority: 32
});