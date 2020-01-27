'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsUpgrading = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Upgrading', false);
    }
    render() {
        return (core.h("div", { class: "docs-page-container" }, core.h("div", { class: "docs-page-header" }, core.h("docs-markdown", null, `
                # Upgrading
            `)), core.h("div", { class: "docs-page-content" }, "Stuff.")));
    }
};

exports.docs_upgrading = DocsUpgrading;
