import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';
var DocsColors = /** @class */ (function () {
    function DocsColors(hostRef) {
        registerInstance(this, hostRef);
    }
    DocsColors.prototype.componentDidLoad = function () {
        initPage('Colors', false);
    };
    DocsColors.prototype.render = function () {
        return (h("docs-element", { title: "Colors" }, h("div", { slot: "overview" }, "Overview"), h("div", { slot: "design" }, "Design Guidelines Stuff"), h("div", { slot: "api" }, "API Stuff")));
    };
    return DocsColors;
}());
export { DocsColors as docs_colors };
