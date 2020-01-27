'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsRaulInput = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("docs-element", { title: "Input" }, core.h("div", { slot: "overview" }, core.h("docs-readme", { component: "raul-input" }), core.h("docs-showcase", null, core.h("div", { class: "mb-md" }, core.h("raul-input", null, core.h("input", { type: "text", placeholder: "Placeholder text" }))), core.h("div", { class: "mb-md" }, core.h("raul-input", { label: "Label text" }, core.h("input", { type: "text", placeholder: "Placeholder text" }))), core.h("div", { class: "mb-md" }, core.h("raul-input", { label: "Label text", hint: "Optional hint" }, core.h("input", { type: "text", placeholder: "Placeholder text" }))), core.h("div", { class: "mb-md" }, core.h("raul-input", { label: "Disabled" }, core.h("input", { type: "text", placeholder: "Placeholder text", disabled: true }))), core.h("div", { class: "mb-md" }, core.h("raul-input", { label: "Invalid", error: "Error message" }, core.h("input", { type: "text", placeholder: "Placeholder text" }))), core.h("div", { class: "mb-md" }, core.h("raul-input", { label: "Invalid", hint: "Optional hint", error: "Error message" }, core.h("input", { type: "text", placeholder: "Placeholder text" }))), core.h("div", { class: "mb-md" }, core.h("raul-input", { label: "Type search", type: "search" }, core.h("input", { type: "text", placeholder: "Placeholder text" }))))), core.h("div", { slot: "design" }, "Design Guidelines"), core.h("div", { slot: "api" }, core.h("docs-preview", { component: "raul-input", content: "\r\n              <input type='text' placeholder='Placeholder text'></input>\r\n            " }), core.h("docs-interface", { component: "raul-input" }))));
    }
};

exports.docs_raul_input = DocsRaulInput;
