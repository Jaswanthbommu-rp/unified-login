'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const myDate = new Date(2019, 10, 25);
const startDate = new Date(2019, 10, 6);
const endDate = new Date(2019, 11, 16);
const DocsRaulBadge = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Date picker');
    }
    render() {
        return (core.h("docs-element", { title: "Date picker" }, core.h("div", { ref: el => this.el = el, slot: 'overview' }, core.h("docs-readme", { component: 'raul-date-picker' }), core.h("docs-showcase", null, core.h("div", { class: 'mb-10' }, core.h("raul-date-picker", { variant: 'date' })), core.h("div", { class: 'mb-10' }, core.h("raul-date-picker", { variant: 'inline', label: 'An inline picker' })), core.h("div", { class: 'mb-10' }, core.h("raul-date-picker", { variant: 'date', label: 'A date picker with a label' })), core.h("div", { class: 'mb-10' }, core.h("raul-date-picker", { variant: 'date', label: 'A date picker with an initial date', initialDate: myDate })), core.h("div", { class: 'mb-10' }, core.h("raul-date-picker", { variant: 'date', label: 'A disabled date picker', disabled: true, initialDate: myDate })), core.h("div", { class: 'mb-10' }, core.h("raul-date-picker", { variant: 'date', label: 'A date picker with validation', error: "This field is a required field." })), core.h("div", { class: 'mb-10' }, core.h("raul-date-picker", { variant: 'range', label: 'A range picker' })), core.h("div", { class: 'mb-10' }, core.h("raul-date-picker", { variant: 'range', label: 'A range picker with initial range', initialStartDate: startDate, initialEndDate: endDate })), core.h("div", { class: 'mb-10' }, core.h("raul-date-picker", { variant: 'range', label: 'A range picker with validation', error: "This field is a required field." })))), core.h("div", { slot: 'design' }, "WIP"), core.h("div", { slot: 'api' }, core.h("docs-preview", { component: "raul-date-picker" }), core.h("docs-interface", { component: "raul-date-picker" }), core.h("docs-readme", { component: "raul-date-picker" }))));
    }
};

exports.docs_raul_date_picker = DocsRaulBadge;
