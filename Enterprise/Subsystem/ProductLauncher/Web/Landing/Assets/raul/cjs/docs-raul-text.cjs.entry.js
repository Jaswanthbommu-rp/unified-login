'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsRaulText = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Text');
    }
    render() {
        return (core.h("div", { class: "docs-page-container" }, core.h("div", { class: "docs-page-header tabbed" }, core.h("docs-markdown", null, `
                # Text
            `), core.h("raul-tabs", { tabs: [
                { label: "OVERVIEW", name: "overview" },
                { label: "DESIGN GUIDELINES", name: "design-guidelines" },
                { label: "API", name: "api" },
            ], "active-tab": "overview" })), core.h("div", { class: "docs-page-content" }, core.h("docs-readme", { component: "raul-text" }), core.h("docs-preview", { component: "raul-text", content: "Lorem ipsum dolor sit amet" }), core.h("docs-showcase", null), core.h("docs-interface", { component: "raul-text" }))));
    }
};

exports.docs_raul_text = DocsRaulText;
