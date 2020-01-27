'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsIntroduction = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Introduction', false);
    }
    render() {
        return (core.h("div", { class: "docs-page-container" }, core.h("div", { class: "docs-page-header" }, core.h("docs-markdown", null, `
                # Introduction
            `)), core.h("div", { class: "docs-page-content" }, "Content here.")));
    }
};

exports.docs_introduction = DocsIntroduction;
