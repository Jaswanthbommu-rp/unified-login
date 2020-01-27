'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsRaulRadioButton = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Radio Button');
    }
    render() {
        return (core.h("docs-element", { title: "Heading" }, core.h("div", { slot: "overview" }, core.h("docs-readme", { component: "raul-heading" }), core.h("docs-showcase", null)), core.h("div", { slot: "design" }, "Design Guidelines Stuff", core.h("docs-readme", { component: "raul-radio" })), core.h("div", { slot: "api" }, core.h("docs-preview", { component: "raul-radio", content: "<input type='radio' name='foo' value='bar' />" }), core.h("docs-interface", { component: "raul-radio" }))));
    }
};

exports.docs_raul_radio_button = DocsRaulRadioButton;
