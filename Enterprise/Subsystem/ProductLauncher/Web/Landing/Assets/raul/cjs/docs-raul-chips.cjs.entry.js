'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsRaulChips = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    handleRaulChipRemove(e) {
        const chipEl = e.target;
        chipEl.parentNode.removeChild(chipEl);
    }
    render() {
        return (core.h("docs-element", { title: "Chips" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Basic"), core.h("div", null, core.h("raul-chip", null, "Chip text")), core.h("div", { class: "w-40 mt-3" }, core.h("raul-chip", null, "Really long text to test the component"))), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Removable"), core.h("div", null, core.h("raul-chip", { removable: true, onRaulChipRemove: (e) => this.handleRaulChipRemove(e) }, "Chip text")), core.h("div", { class: "w-40 mt-3" }, core.h("raul-chip", { removable: true, onRaulChipRemove: (e) => this.handleRaulChipRemove(e) }, "Really long text to test the component"))))), core.h("div", { slot: "design" }, core.h("docs-readme", { component: "raul-chip" })), core.h("div", { slot: "api" }, core.h("docs-preview", { component: "raul-chip", content: "Chip" }), core.h("docs-interface", { component: "raul-chip" }))));
    }
};

exports.docs_raul_chips = DocsRaulChips;
