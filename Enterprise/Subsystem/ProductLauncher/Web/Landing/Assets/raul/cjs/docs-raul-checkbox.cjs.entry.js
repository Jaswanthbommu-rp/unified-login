'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsRaulCheckobx = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Checkbox');
    }
    render() {
        return (core.h("docs-element", { title: "Checkbox" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null)), core.h("div", { slot: "design" }, core.h("docs-readme", { component: "raul-checkbox" })), core.h("div", { slot: "api" }, core.h("docs-preview", { component: "raul-checkbox", content: "<input type='checkbox' name='foo' value='bar' />" }), core.h("docs-interface", { component: "raul-checkbox" }))));
    }
};

exports.docs_raul_checkbox = DocsRaulCheckobx;
