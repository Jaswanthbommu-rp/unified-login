'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsRaulStatusIndicator = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Status Indicator');
    }
    render() {
        return (core.h("docs-element", { title: "Status Indicator" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("div", { class: "flex flex-wrap -mx-2" }, core.h("div", { class: "flex-none px-2" }, core.h("raul-status-indicator", null)), core.h("div", { class: "flex-none px-2" }, core.h("raul-status-indicator", { variant: "destructive" })), core.h("div", { class: "flex-none px-2" }, core.h("raul-status-indicator", { variant: "success" })), core.h("div", { class: "flex-none px-2" }, core.h("raul-status-indicator", { variant: "warning" }))))), core.h("div", { slot: "design" }, core.h("docs-readme", { component: "raul-status-indicator" })), core.h("div", { slot: "api" }, core.h("docs-preview", { component: "raul-status-indicator", content: "Status Indicator" }), core.h("docs-interface", { component: "raul-status-indicator" }))));
    }
};

exports.docs_raul_status_indicator = DocsRaulStatusIndicator;
