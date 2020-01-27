import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';
var DocsIntroduction = /** @class */ (function () {
    function DocsIntroduction(hostRef) {
        registerInstance(this, hostRef);
    }
    DocsIntroduction.prototype.componentDidLoad = function () {
        initPage('Introduction', false);
    };
    DocsIntroduction.prototype.render = function () {
        return (h("div", { class: "docs-page-container" }, h("div", { class: "docs-page-header" }, h("docs-markdown", null, "\n                # Introduction\n            ")), h("div", { class: "docs-page-content" }, "Content here.")));
    };
    return DocsIntroduction;
}());
export { DocsIntroduction as docs_introduction };
