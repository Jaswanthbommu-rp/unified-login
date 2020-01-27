'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsRaulBreadcrumbs = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.basicBreadcrumbs = `
    <raul-breadcrumbs>
      <raul-breadcrumb>
        <a href="#">Previous</a>
      </raul-breadcrumb>
  
      <raul-breadcrumb>
        <a href="#">Previous</a>
      </raul-breadcrumb>
  
      <raul-breadcrumb>
        Current
      </raul-breadcrumb>
    </raul-breadcrumbs>
  `;
        this.backIconBreadcrumbs = `
    <raul-breadcrumbs>
      <raul-breadcrumb>
        <a href="#">
          <raul-breadcrumb-back-icon></raul-breadcrumb-back-icon> Back
        </a>
      </raul-breadcrumb>
    </raul-breadcrumbs>
  `;
    }
    render() {
        return (core.h("docs-element", { title: "Breadcrumbs" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("div", null, core.h("div", { class: "heading-lg" }, "Basic"), core.h("div", { innerHTML: this.basicBreadcrumbs }), core.h("docs-code", { class: "mt-5", code: this.basicBreadcrumbs })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Back Icon"), core.h("div", { innerHTML: this.backIconBreadcrumbs }), core.h("docs-code", { class: "mt-5", code: this.backIconBreadcrumbs })))), core.h("div", { slot: "design" }, "Design Guidelines"), core.h("div", { slot: "api" }, core.h("docs-interface", { component: "raul-breadcrumbs" }))));
    }
};

exports.docs_raul_breadcrumbs = DocsRaulBreadcrumbs;
