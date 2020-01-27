'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsRaulProgress = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.dynamicProgress = `
    <raul-progress
      label="Label Text"
      hint="Optional Text"
      value="25"
    ></raul-progress>
  `;
        this.staticProgress = `
    <raul-progress
      label="Label Text"
      static
      value="100"
    ></raul-progress>
  `;
        this.warningProgress = `
    <raul-progress
      label="Label Text"
      hint="Optional Text"
      value="25"
      color="warning"
    ></raul-progress>
    
    <raul-progress
      label="Label Text"
      static
      value="100"
      color="warning"
      class="mt-5"
    ></raul-progress>
  `;
        this.dangerProgress = `
    <raul-progress
      label="Label Text"
      hint="Optional Text"
      value="25"
      color="danger"
    ></raul-progress>
    
    <raul-progress
      label="Label Text"
      static
      value="100"
      color="danger"
      class="mt-5"
    ></raul-progress>
  `;
        this.successProgress = `
    <raul-progress
      label="Label Text"
      hint="Optional Text"
      value="25"
      color="success"
    ></raul-progress>
    
    <raul-progress
      label="Label Text"
      static
      value="100"
      color="success"
      class="mt-5"
    ></raul-progress>
  `;
    }
    render() {
        return (core.h("docs-element", { title: "Progress" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("div", null, core.h("div", { class: "heading-lg" }, "Dynamic"), core.h("div", { class: "md:w-6/12", innerHTML: this.dynamicProgress }), core.h("docs-code", { class: "mt-5", code: this.dynamicProgress })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Static"), core.h("div", { class: "md:w-6/12", innerHTML: this.staticProgress }), core.h("docs-code", { class: "mt-5", code: this.staticProgress })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Warning"), core.h("div", { class: "md:w-6/12", innerHTML: this.warningProgress }), core.h("docs-code", { class: "mt-5", code: this.warningProgress })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Danger"), core.h("div", { class: "md:w-6/12", innerHTML: this.dangerProgress }), core.h("docs-code", { class: "mt-5", code: this.dangerProgress })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Success"), core.h("div", { class: "md:w-6/12", innerHTML: this.successProgress }), core.h("docs-code", { class: "mt-5", code: this.successProgress })))), core.h("div", { slot: "design" }, "Design Guidelines"), core.h("div", { slot: "api" }, core.h("docs-interface", { component: "raul-progress" }))));
    }
};

exports.docs_raul_progress = DocsRaulProgress;
