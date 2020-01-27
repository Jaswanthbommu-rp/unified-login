'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsRaulPagingBar = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Paging Bar');
    }
    render() {
        return (core.h("docs-element", { title: "Paging Bar" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("raul-paging-bar", { "total-rows": "55" }))), core.h("div", { slot: "design" }, core.h("docs-readme", { component: "raul-paging-bar" })), core.h("div", { slot: "api" }, core.h("docs-preview", { component: "raul-paging-bar", content: "Paging Bar" }), core.h("docs-interface", { component: "raul-paging-bar" }))));
    }
};

exports.docs_raul_paging_bar = DocsRaulPagingBar;
