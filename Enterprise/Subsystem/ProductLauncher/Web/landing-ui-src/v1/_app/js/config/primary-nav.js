// Configure Primary Navigation

(function(angular) {
    'use strict';

    function config(primaryNavModel) {
        var navData = [{
            text: 'Home',
            href: '#/',
            icon: 'rp-icon-home'
        }, {
            text: 'Company',
            href: '#/company',
            icon: 'rp-icon-building'
        }, {
            text: 'Properties',
            href: '#/properties',
            icon: 'rp-icon-business-center-3'
        }, {
            text: 'Products',
            href: '#/products',
            icon: 'rp-icon-tools'
        }, {
            text: 'People',
            href: '#/people',
            icon: 'rp-icon-users'
        }, {
            text: 'Roles',
            href: '#/roles',
            icon: 'rp-icon-user-profile'
        }, {
            text: 'Rights',
            href: '#/rights',
            icon: 'rp-icon-key'
        }];

        primaryNavModel.setNav(navData);
    }

    angular
        .module('settings')
        .config(['primaryNavProvider', config]);
})(angular);
