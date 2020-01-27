'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsRaulBadge = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Badge');
    }
    render() {
        return (core.h("docs-element", { title: "Badges" }, core.h("div", { slot: "overview", class: "r-content" }, core.h("docs-showcase", null, core.h("p", { class: "font-md" }, core.h("raul-badge", { icon: "add-circle-1", content: "88" }), " ", core.h("code", null, "variant=\"primary\"")), core.h("p", { class: "font-md" }, core.h("raul-badge", { icon: "add-circle-1", content: "88", variant: "error" }), " ", core.h("code", null, "variant=\"error\"")), core.h("p", { class: "font-md" }, core.h("raul-badge", { icon: "add-circle-1", content: "88", variant: "warning" }), " ", core.h("code", null, "variant=\"warning\"")), core.h("p", { class: "font-md" }, core.h("raul-badge", { icon: "add-circle-1", content: "88", variant: "success" }), " ", core.h("code", null, "variant=\"success\"")))), core.h("div", { slot: "design" }, "Design Guidelines Stuff"), core.h("div", { slot: "api" }, core.h("docs-readme", { component: "raul-badge" }), core.h("docs-preview", { component: "raul-badge" }), core.h("docs-interface", { component: "raul-badge" }))));
    }
};

exports.docs_raul_badge = DocsRaulBadge;
