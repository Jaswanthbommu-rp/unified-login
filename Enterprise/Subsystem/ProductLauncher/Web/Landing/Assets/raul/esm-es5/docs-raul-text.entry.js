import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';
var DocsRaulText = /** @class */ (function () {
    function DocsRaulText(hostRef) {
        registerInstance(this, hostRef);
    }
    DocsRaulText.prototype.componentDidLoad = function () {
        initPage('Text');
    };
    DocsRaulText.prototype.render = function () {
        return (h("div", { class: "docs-page-container" }, h("div", { class: "docs-page-header tabbed" }, h("docs-markdown", null, "\n                # Text\n            "), h("raul-tabs", { tabs: [
                { label: "OVERVIEW", name: "overview" },
                { label: "DESIGN GUIDELINES", name: "design-guidelines" },
                { label: "API", name: "api" },
            ], "active-tab": "overview" })), h("div", { class: "docs-page-content" }, h("docs-readme", { component: "raul-text" }), h("docs-preview", { component: "raul-text", content: "Lorem ipsum dolor sit amet" }), h("docs-showcase", null), h("docs-interface", { component: "raul-text" }))));
    };
    return DocsRaulText;
}());
export { DocsRaulText as docs_raul_text };
