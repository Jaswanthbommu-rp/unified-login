'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsRaulSimpleSelect = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("docs-element", { title: "Simple Select" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("div", { class: "mb-md" }, core.h("raul-simple-select", null, core.h("select", null, core.h("option", null, "Cat"), core.h("option", null, "Dog"), core.h("option", null, "Cat and Dog")))), core.h("div", { class: "mb-md" }, core.h("raul-simple-select", { label: "Label text" }, core.h("select", { id: "select-0" }, core.h("option", null, "Cat"), core.h("option", null, "Dog"), core.h("option", null, "Cat and Dog")))), core.h("div", { class: "mb-md" }, core.h("raul-simple-select", { label: "Label text", hint: "Optional hint" }, core.h("select", null, core.h("option", null, "Cat"), core.h("option", null, "Dog"), core.h("option", null, "Cat and Dog")))), core.h("div", { class: "mb-md" }, core.h("raul-simple-select", { label: "Disabled" }, core.h("select", { disabled: true }, core.h("option", null, "Cat"), core.h("option", null, "Dog"), core.h("option", null, "Cat and Dog")))), core.h("div", { class: "mb-md" }, core.h("raul-simple-select", { label: "Invalid", error: "Error message" }, core.h("select", null, core.h("option", null, "Cat"), core.h("option", null, "Dog"), core.h("option", null, "Cat and Dog")))), core.h("div", { class: "mb-md" }, core.h("raul-simple-select", { label: "Invalid", hint: "Optional hint", error: "Error message" }, core.h("select", null, core.h("option", null, "Cat"), core.h("option", null, "Dog"), core.h("option", null, "Cat and Dog")))))), core.h("div", { slot: "design" }, "Design Guidelines"), core.h("div", { slot: "api" }, core.h("docs-preview", { component: "raul-simple-select", content: "\r\n              <select>\r\n                <option>Cat</option>\r\n                <option>Dog</option>\r\n                <option>Cat and Dog</option>\r\n              </select>\r\n            " }), core.h("docs-interface", { component: "raul-simple-select" }))));
    }
};

exports.docs_raul_simple_select = DocsRaulSimpleSelect;
