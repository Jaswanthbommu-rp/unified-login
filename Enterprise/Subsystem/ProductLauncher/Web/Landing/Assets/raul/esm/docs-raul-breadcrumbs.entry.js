import { r as registerInstance, h } from './core-9263a98c.js';

const DocsRaulBreadcrumbs = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
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
        return (h("docs-element", { title: "Breadcrumbs" }, h("div", { slot: "overview" }, h("docs-showcase", null, h("div", null, h("div", { class: "heading-lg" }, "Basic"), h("div", { innerHTML: this.basicBreadcrumbs }), h("docs-code", { class: "mt-5", code: this.basicBreadcrumbs })), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Back Icon"), h("div", { innerHTML: this.backIconBreadcrumbs }), h("docs-code", { class: "mt-5", code: this.backIconBreadcrumbs })))), h("div", { slot: "design" }, "Design Guidelines"), h("div", { slot: "api" }, h("docs-interface", { component: "raul-breadcrumbs" }))));
    }
};

export { DocsRaulBreadcrumbs as docs_raul_breadcrumbs };
