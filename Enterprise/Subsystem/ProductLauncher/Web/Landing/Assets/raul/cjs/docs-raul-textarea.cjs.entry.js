'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsRaulTextarea = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("docs-element", { title: "Textarea" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("div", { class: "mb-md" }, core.h("raul-textarea", null, core.h("textarea", { placeholder: "Placeholder text" }))), core.h("div", { class: "mb-md" }, core.h("raul-textarea", { label: "Label text" }, core.h("textarea", { id: "textarea-0", placeholder: "Placeholder text" }))), core.h("div", { class: "mb-md" }, core.h("raul-textarea", { label: "Label text", hint: "Optional hint" }, core.h("textarea", { placeholder: "Placeholder text" }))), core.h("div", { class: "mb-md" }, core.h("raul-textarea", { label: "Disabled" }, core.h("textarea", { placeholder: "Placeholder text", disabled: true }))), core.h("div", { class: "mb-md" }, core.h("raul-textarea", { label: "Invalid", error: "Error message" }, core.h("textarea", { placeholder: "Placeholder text" }))), core.h("div", { class: "mb-md" }, core.h("raul-textarea", { label: "Invalid", hint: "Optional hint", error: "Error message" }, core.h("textarea", { placeholder: "Placeholder text" }))))), core.h("div", { slot: "design" }, "Design Guidelines"), core.h("div", { slot: "api" }, core.h("docs-preview", { component: "raul-textarea", content: "\r\n              <textarea placeholder='Placeholder text'></textarea>\r\n            " }), core.h("docs-interface", { component: "raul-textarea" }))));
    }
};

exports.docs_raul_textarea = DocsRaulTextarea;
