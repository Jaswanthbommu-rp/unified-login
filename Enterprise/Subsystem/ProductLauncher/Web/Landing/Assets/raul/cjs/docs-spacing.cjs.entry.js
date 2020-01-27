'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsUtils = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Spacing', false);
    }
    render() {
        return (core.h("docs-element", { title: "Spacing" }, core.h("div", { slot: "overview" }, "Overview"), core.h("div", { slot: "design" }, "Design Guidelines Stuff"), core.h("div", { slot: "api" }, "API Stuff")));
    }
};

exports.docs_spacing = DocsUtils;
