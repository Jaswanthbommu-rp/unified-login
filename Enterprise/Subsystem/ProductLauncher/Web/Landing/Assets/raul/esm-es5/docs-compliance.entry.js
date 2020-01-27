import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';
var DocsCompliance = /** @class */ (function () {
    function DocsCompliance(hostRef) {
        registerInstance(this, hostRef);
    }
    DocsCompliance.prototype.componentDidLoad = function () {
        initPage('Compliance', false);
    };
    DocsCompliance.prototype.render = function () {
        return (h("div", { class: "docs-page-container" }, h("div", { class: "docs-page-header" }, h("docs-markdown", null, "\n                # Compliance\n            ")), h("div", { class: "docs-page-content" }, "Content here.")));
    };
    return DocsCompliance;
}());
export { DocsCompliance as docs_compliance };
