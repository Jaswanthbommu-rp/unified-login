'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsColors = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Colors', false);
    }
    render() {
        return (core.h("docs-element", { title: "Colors" }, core.h("div", { slot: "overview" }, "Overview"), core.h("div", { slot: "design" }, "Design Guidelines Stuff"), core.h("div", { slot: "api" }, "API Stuff")));
    }
};

exports.docs_colors = DocsColors;
