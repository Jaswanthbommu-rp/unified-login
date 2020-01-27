import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';
var DocsUpgrading = /** @class */ (function () {
    function DocsUpgrading(hostRef) {
        registerInstance(this, hostRef);
    }
    DocsUpgrading.prototype.componentDidLoad = function () {
        initPage('Upgrading', false);
    };
    DocsUpgrading.prototype.render = function () {
        return (h("div", { class: "docs-page-container" }, h("div", { class: "docs-page-header" }, h("docs-markdown", null, "\n                # Upgrading\n            ")), h("div", { class: "docs-page-content" }, "Stuff.")));
    };
    return DocsUpgrading;
}());
export { DocsUpgrading as docs_upgrading };
