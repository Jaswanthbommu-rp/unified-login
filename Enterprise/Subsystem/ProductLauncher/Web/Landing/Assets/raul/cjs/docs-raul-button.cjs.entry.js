'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsRaulButton = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Button');
    }
    render() {
        let darkBG = {
            display: 'inline-block',
            verticalAlign: 'middle',
            padding: '10px',
            background: '#0076cc'
        };
        return (core.h("docs-element", { title: "Buttons" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("h3", null, "Button"), core.h("div", { class: "raul-button-group flex items-center mb-md" }, core.h("raul-button", { variant: "primary" }, "Primary"), core.h("raul-button", { variant: "secondary" }, "Secondary"), core.h("raul-button", { variant: "danger" }, "Danger"), core.h("div", { class: "mr-sm", style: darkBG }, core.h("raul-button", { variant: "reverse" }, "Reverse")), core.h("raul-button", { variant: "text" }, "Text"), core.h("raul-button", { variant: "control" }, "Control"), core.h("raul-button", { variant: "control", icon: "content-filter" }, "Control"), core.h("raul-button", { variant: "control", icon: "arrow-left-v" })), core.h("h3", null, "Button Link"), core.h("div", { class: "raul-button-group flex items-center mb-md" }, core.h("raul-button", { variant: "primary", href: "http://www.realpage.com" }, "Primary"), core.h("raul-button", { variant: "secondary", href: "http://www.realpage.com" }, "Secondary"), core.h("raul-button", { variant: "danger", href: "http://www.realpage.com" }, "Danger"), core.h("div", { class: "mr-sm", style: darkBG }, core.h("raul-button", { variant: "reverse", href: "http://www.realpage.com" }, "Reverse")), core.h("raul-button", { variant: "text", href: "http://www.realpage.com" }, "Text"), core.h("raul-button", { variant: "control", href: "http://www.realpage.com" }, "Control"), core.h("raul-button", { variant: "control", href: "http://www.realpage.com", icon: "content-filter" }, "Control"), core.h("raul-button", { variant: "control", href: "http://www.realpage.com", icon: "arrow-left-v" })), core.h("h3", null, "Input Type Submit/Reset/Button"), core.h("div", { class: "raul-button-group flex items-center mb-md" }, core.h("raul-button", { variant: "primary", type: "submit", value: "Submit" }, "Submit"), core.h("raul-button", { variant: "secondary", type: "reset", value: "Reset" }, "Reset"), core.h("raul-button", { variant: "danger", type: "button" }, "Button"), core.h("div", { class: "mr-sm", style: darkBG }, core.h("raul-button", { variant: "reverse", type: "reset", value: "Reset" }, "Reset")), core.h("raul-button", { variant: "text", type: "button", value: "Button" }, "Button"), core.h("raul-button", { variant: "control", type: "button", value: "Button" }, "Control"), core.h("raul-button", { variant: "control", type: "button", value: "Control", icon: "content-filter" }, "Control"), core.h("raul-button", { variant: "control", type: "button", value: "", icon: "arrow-left-v" })), core.h("h3", null, "Disabled"), core.h("div", { class: "raul-button-group flex items-center mb-md" }, core.h("raul-button", { variant: "primary", disabled: true }, "Primary"), core.h("raul-button", { variant: "secondary", disabled: true }, "Secondary"), core.h("raul-button", { variant: "danger", disabled: true }, "Danger"), core.h("div", { class: "mr-sm", style: darkBG }, core.h("raul-button", { variant: "reverse", disabled: true }, "Reverse")), core.h("raul-button", { variant: "text", disabled: true }, "Text"), core.h("raul-button", { variant: "control", disabled: true }, "Control"), core.h("raul-button", { variant: "control", icon: "content-filter", disabled: true }, "Control"), core.h("raul-button", { variant: "control", icon: "arrow-left-v", disabled: true })), core.h("h3", null, "Small"), core.h("div", { class: "raul-button-group flex items-center mb-md" }, core.h("raul-button", { variant: "primary", size: "small" }, "Primary"), core.h("raul-button", { variant: "secondary", size: "small" }, "Secondary"), core.h("raul-button", { variant: "danger", size: "small" }, "Danger"), core.h("div", { class: "mr-sm", style: darkBG }, core.h("raul-button", { variant: "reverse", size: "small" }, "Reverse")), core.h("raul-button", { variant: "text", size: "small" }, "Text"), core.h("raul-button", { variant: "control", size: "small" }, "Control"), core.h("raul-button", { variant: "control", icon: "content-filter", size: "small" }, "Control"), core.h("raul-button", { variant: "control", icon: "arrow-left-v", size: "small" })))), core.h("div", { slot: "design" }, core.h("docs-readme", { component: "raul-button" })), core.h("div", { slot: "api" }, core.h("docs-preview", { component: "raul-button", content: "My Button" }), core.h("docs-interface", { component: "raul-button" }))));
    }
};

exports.docs_raul_button = DocsRaulButton;
