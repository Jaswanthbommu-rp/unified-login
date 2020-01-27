import { r as registerInstance, h } from './core-9263a98c.js';
var DocsRaulBreadcrumbs = /** @class */ (function () {
    function DocsRaulBreadcrumbs(hostRef) {
        registerInstance(this, hostRef);
        this.basicBreadcrumbs = "\n    <raul-breadcrumbs>\n      <raul-breadcrumb>\n        <a href=\"#\">Previous</a>\n      </raul-breadcrumb>\n  \n      <raul-breadcrumb>\n        <a href=\"#\">Previous</a>\n      </raul-breadcrumb>\n  \n      <raul-breadcrumb>\n        Current\n      </raul-breadcrumb>\n    </raul-breadcrumbs>\n  ";
        this.backIconBreadcrumbs = "\n    <raul-breadcrumbs>\n      <raul-breadcrumb>\n        <a href=\"#\">\n          <raul-breadcrumb-back-icon></raul-breadcrumb-back-icon> Back\n        </a>\n      </raul-breadcrumb>\n    </raul-breadcrumbs>\n  ";
    }
    DocsRaulBreadcrumbs.prototype.render = function () {
        return (h("docs-element", { title: "Breadcrumbs" }, h("div", { slot: "overview" }, h("docs-showcase", null, h("div", null, h("div", { class: "heading-lg" }, "Basic"), h("div", { innerHTML: this.basicBreadcrumbs }), h("docs-code", { class: "mt-5", code: this.basicBreadcrumbs })), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Back Icon"), h("div", { innerHTML: this.backIconBreadcrumbs }), h("docs-code", { class: "mt-5", code: this.backIconBreadcrumbs })))), h("div", { slot: "design" }, "Design Guidelines"), h("div", { slot: "api" }, h("docs-interface", { component: "raul-breadcrumbs" }))));
    };
    return DocsRaulBreadcrumbs;
}());
export { DocsRaulBreadcrumbs as docs_raul_breadcrumbs };
