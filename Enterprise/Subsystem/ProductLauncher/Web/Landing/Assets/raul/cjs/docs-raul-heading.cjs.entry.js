'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsRaulheading = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('heading');
    }
    render() {
        return (core.h("docs-element", { title: "Heading" }, core.h("div", { slot: "overview" }, core.h("docs-readme", { component: "raul-heading" })), core.h("div", { slot: "design" }, "Design Guidelines Stuff"), core.h("div", { slot: "api" }, core.h("docs-preview", { component: "raul-heading", content: "Lorem ipsum dolor sit amet" }), core.h("docs-showcase", null), core.h("docs-interface", { component: "raul-heading" }))));
    }
};

exports.docs_raul_heading = DocsRaulheading;
