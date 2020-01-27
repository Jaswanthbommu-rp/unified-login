import { r as registerInstance, h } from './core-9263a98c.js';
var DocsRaulProgress = /** @class */ (function () {
    function DocsRaulProgress(hostRef) {
        registerInstance(this, hostRef);
        this.dynamicProgress = "\n    <raul-progress\n      label=\"Label Text\"\n      hint=\"Optional Text\"\n      value=\"25\"\n    ></raul-progress>\n  ";
        this.staticProgress = "\n    <raul-progress\n      label=\"Label Text\"\n      static\n      value=\"100\"\n    ></raul-progress>\n  ";
        this.warningProgress = "\n    <raul-progress\n      label=\"Label Text\"\n      hint=\"Optional Text\"\n      value=\"25\"\n      color=\"warning\"\n    ></raul-progress>\n    \n    <raul-progress\n      label=\"Label Text\"\n      static\n      value=\"100\"\n      color=\"warning\"\n      class=\"mt-5\"\n    ></raul-progress>\n  ";
        this.dangerProgress = "\n    <raul-progress\n      label=\"Label Text\"\n      hint=\"Optional Text\"\n      value=\"25\"\n      color=\"danger\"\n    ></raul-progress>\n    \n    <raul-progress\n      label=\"Label Text\"\n      static\n      value=\"100\"\n      color=\"danger\"\n      class=\"mt-5\"\n    ></raul-progress>\n  ";
        this.successProgress = "\n    <raul-progress\n      label=\"Label Text\"\n      hint=\"Optional Text\"\n      value=\"25\"\n      color=\"success\"\n    ></raul-progress>\n    \n    <raul-progress\n      label=\"Label Text\"\n      static\n      value=\"100\"\n      color=\"success\"\n      class=\"mt-5\"\n    ></raul-progress>\n  ";
    }
    DocsRaulProgress.prototype.render = function () {
        return (h("docs-element", { title: "Progress" }, h("div", { slot: "overview" }, h("docs-showcase", null, h("div", null, h("div", { class: "heading-lg" }, "Dynamic"), h("div", { class: "md:w-6/12", innerHTML: this.dynamicProgress }), h("docs-code", { class: "mt-5", code: this.dynamicProgress })), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Static"), h("div", { class: "md:w-6/12", innerHTML: this.staticProgress }), h("docs-code", { class: "mt-5", code: this.staticProgress })), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Warning"), h("div", { class: "md:w-6/12", innerHTML: this.warningProgress }), h("docs-code", { class: "mt-5", code: this.warningProgress })), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Danger"), h("div", { class: "md:w-6/12", innerHTML: this.dangerProgress }), h("docs-code", { class: "mt-5", code: this.dangerProgress })), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Success"), h("div", { class: "md:w-6/12", innerHTML: this.successProgress }), h("docs-code", { class: "mt-5", code: this.successProgress })))), h("div", { slot: "design" }, "Design Guidelines"), h("div", { slot: "api" }, h("docs-interface", { component: "raul-progress" }))));
    };
    return DocsRaulProgress;
}());
export { DocsRaulProgress as docs_raul_progress };
