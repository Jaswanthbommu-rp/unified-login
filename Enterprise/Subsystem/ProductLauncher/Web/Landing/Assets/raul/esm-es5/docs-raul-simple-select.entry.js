import { r as registerInstance, h } from './core-9263a98c.js';
var DocsRaulSimpleSelect = /** @class */ (function () {
    function DocsRaulSimpleSelect(hostRef) {
        registerInstance(this, hostRef);
    }
    DocsRaulSimpleSelect.prototype.render = function () {
        return (h("docs-element", { title: "Simple Select" }, h("div", { slot: "overview" }, h("docs-showcase", null, h("div", { class: "mb-md" }, h("raul-simple-select", null, h("select", null, h("option", null, "Cat"), h("option", null, "Dog"), h("option", null, "Cat and Dog")))), h("div", { class: "mb-md" }, h("raul-simple-select", { label: "Label text" }, h("select", { id: "select-0" }, h("option", null, "Cat"), h("option", null, "Dog"), h("option", null, "Cat and Dog")))), h("div", { class: "mb-md" }, h("raul-simple-select", { label: "Label text", hint: "Optional hint" }, h("select", null, h("option", null, "Cat"), h("option", null, "Dog"), h("option", null, "Cat and Dog")))), h("div", { class: "mb-md" }, h("raul-simple-select", { label: "Disabled" }, h("select", { disabled: true }, h("option", null, "Cat"), h("option", null, "Dog"), h("option", null, "Cat and Dog")))), h("div", { class: "mb-md" }, h("raul-simple-select", { label: "Invalid", error: "Error message" }, h("select", null, h("option", null, "Cat"), h("option", null, "Dog"), h("option", null, "Cat and Dog")))), h("div", { class: "mb-md" }, h("raul-simple-select", { label: "Invalid", hint: "Optional hint", error: "Error message" }, h("select", null, h("option", null, "Cat"), h("option", null, "Dog"), h("option", null, "Cat and Dog")))))), h("div", { slot: "design" }, "Design Guidelines"), h("div", { slot: "api" }, h("docs-preview", { component: "raul-simple-select", content: "\r\n              <select>\r\n                <option>Cat</option>\r\n                <option>Dog</option>\r\n                <option>Cat and Dog</option>\r\n              </select>\r\n            " }), h("docs-interface", { component: "raul-simple-select" }))));
    };
    return DocsRaulSimpleSelect;
}());
export { DocsRaulSimpleSelect as docs_raul_simple_select };
