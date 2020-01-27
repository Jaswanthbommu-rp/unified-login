'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsRaulSwitch = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Switch');
    }
    render() {
        return (core.h("docs-element", { title: "Switch" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("raul-switch", { "label-text": "Regular switch" }, core.h("input", { type: 'checkbox', id: 'switch1', checked: true })), core.h("raul-switch", { "label-text": "Disabled switch" }, core.h("input", { type: 'checkbox', id: 'switch2', disabled: true })), core.h("raul-switch", { "label-text": "Small switch", small: true }, core.h("input", { type: 'checkbox', id: 'switch3', checked: true, disabled: true })))), core.h("div", { slot: "design" }, core.h("docs-readme", { component: "raul-switch" })), core.h("div", { slot: "api" }, core.h("docs-preview", { component: "raul-switch", content: "<input type='checkbox' id='switch' checked disabled />" }), core.h("docs-interface", { component: "raul-switch" }))));
    }
};

exports.docs_raul_switch = DocsRaulSwitch;
