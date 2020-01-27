import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';
var DocsUtils = /** @class */ (function () {
    function DocsUtils(hostRef) {
        registerInstance(this, hostRef);
    }
    DocsUtils.prototype.componentDidLoad = function () {
        initPage('Spacing', false);
    };
    DocsUtils.prototype.render = function () {
        return (h("docs-element", { title: "Spacing" }, h("div", { slot: "overview" }, "Overview"), h("div", { slot: "design" }, "Design Guidelines Stuff"), h("div", { slot: "api" }, "API Stuff")));
    };
    return DocsUtils;
}());
export { DocsUtils as docs_spacing };
