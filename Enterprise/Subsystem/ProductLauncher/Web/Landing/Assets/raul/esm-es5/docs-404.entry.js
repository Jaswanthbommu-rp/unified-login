import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';
var DocsNotFound = /** @class */ (function () {
    function DocsNotFound(hostRef) {
        registerInstance(this, hostRef);
    }
    DocsNotFound.prototype.componentDidLoad = function () {
        initPage('Page not found', false);
    };
    DocsNotFound.prototype.render = function () {
        return (h("div", { class: "docs-page-container" }, h("div", { class: "docs-page-header" }, h("docs-markdown", null, "\n                # 404\n                These aren't the droids your looking for\n            "))));
    };
    Object.defineProperty(DocsNotFound, "style", {
        get: function () { return ""; },
        enumerable: true,
        configurable: true
    });
    return DocsNotFound;
}());
export { DocsNotFound as docs_404 };
