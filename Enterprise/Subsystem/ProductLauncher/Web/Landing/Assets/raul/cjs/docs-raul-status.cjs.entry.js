'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsRaulStatus = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Status');
    }
    render() {
        return (core.h("docs-element", { title: "Status" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("div", { class: "flex flex-wrap -mx-2" }, core.h("div", { class: "flex-none p-2" }, core.h("raul-status", null, "Default")), core.h("div", { class: "flex-none p-2" }, core.h("raul-status", { variant: "destructive" }, "Destructive")), core.h("div", { class: "flex-none p-2" }, core.h("raul-status", { variant: "success" }, "Success")), core.h("div", { class: "flex-none p-2" }, core.h("raul-status", { variant: "warning" }, "Warning")), core.h("div", { class: "w-1/2 p-2" }, core.h("raul-status", null, "Really long text to test the component inside of a half width container"))))), core.h("div", { slot: "design" }, core.h("docs-readme", { component: "raul-status" })), core.h("div", { slot: "api" }, core.h("docs-preview", { component: "raul-status", content: "Status" }), core.h("docs-interface", { component: "raul-status" }))));
    }
};

exports.docs_raul_status = DocsRaulStatus;
