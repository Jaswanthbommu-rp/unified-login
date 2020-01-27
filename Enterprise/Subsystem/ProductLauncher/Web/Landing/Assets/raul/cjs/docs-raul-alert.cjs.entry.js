'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsRaulAlert = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Alert');
    }
    render() {
        return (core.h("docs-element", { title: 'Alert' }, core.h("div", { slot: 'overview' }, core.h("docs-readme", { component: 'raul-alert' }), core.h("docs-showcase", null, core.h("div", { class: 'mb-10 md:flex justify-between' }, core.h("div", { class: "sm:w-full md:w-1/3 my-3" }, core.h("raul-alert", { heading: 'Information alert', content: 'This is an information alert', ctaMessage: 'Optional', ctaUrl: 'https://www.google.com/', rounded: true })), core.h("div", { class: "sm:w-full md:w-1/3 md:mx-10 my-3" }, core.h("raul-alert", { heading: 'Information alert', content: 'This is an information alert', rounded: true })), core.h("div", { class: "sm:w-full md:w-1/3 my-3" }, core.h("raul-alert", { heading: 'Information alert', rounded: true }))), core.h("div", { class: 'mb-10 md:flex justify-between' }, core.h("div", { class: "sm:w-full md:w-1/3 my-3" }, core.h("raul-alert", { variant: 'success', heading: 'Success alert', content: 'This is an success alert', ctaMessage: 'Optional', ctaUrl: 'https://www.google.com/' })), core.h("div", { class: "sm:w-full md:w-1/3 md:mx-10 my-3" }, core.h("raul-alert", { variant: 'success', heading: 'Success alert', content: 'This is an success alert' })), core.h("div", { class: "sm:w-full md:w-1/3 my-3" }, core.h("raul-alert", { variant: 'success', heading: 'Success alert' }))), core.h("div", { class: 'mb-10 md:flex justify-between' }, core.h("div", { class: "sm:w-full md:w-1/3 my-3" }, core.h("raul-alert", { variant: 'warning', heading: 'Warning alert', content: 'This is an warning alert', ctaMessage: 'Optional', ctaUrl: 'https://www.google.com/' })), core.h("div", { class: "sm:w-full md:w-1/3 md:mx-10 my-3" }, core.h("raul-alert", { variant: 'warning', heading: 'Warning alert', content: 'This is an warning alert' })), core.h("div", { class: "sm:w-full md:w-1/3 my-3" }, core.h("raul-alert", { variant: 'warning', heading: 'Warning alert' }))), core.h("div", { class: 'mb-10 md:flex justify-between' }, core.h("div", { class: "sm:w-full md:w-1/3 my-3" }, core.h("raul-alert", { variant: 'danger', heading: 'Danger alert', content: 'This is an danger alert', ctaMessage: 'Optional', ctaUrl: 'https://www.google.com/', rounded: true })), core.h("div", { class: "sm:w-full md:w-1/3 md:mx-10 my-3" }, core.h("raul-alert", { variant: 'danger', heading: 'Danger alert', content: 'This is an danger alert', rounded: true })), core.h("div", { class: "sm:w-full md:w-1/3 my-3" }, core.h("raul-alert", { variant: 'danger', heading: 'Danger alert', rounded: true }))))), core.h("div", { slot: 'design' }), core.h("div", { slot: 'api' })));
    }
};

exports.docs_raul_alert = DocsRaulAlert;
