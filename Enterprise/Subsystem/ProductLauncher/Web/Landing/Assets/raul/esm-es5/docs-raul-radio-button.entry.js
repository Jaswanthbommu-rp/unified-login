import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';
var DocsRaulRadioButton = /** @class */ (function () {
    function DocsRaulRadioButton(hostRef) {
        registerInstance(this, hostRef);
    }
    DocsRaulRadioButton.prototype.componentDidLoad = function () {
        initPage('Radio Button');
    };
    DocsRaulRadioButton.prototype.render = function () {
        return (h("docs-element", { title: "Heading" }, h("div", { slot: "overview" }, h("docs-readme", { component: "raul-heading" }), h("docs-showcase", null)), h("div", { slot: "design" }, "Design Guidelines Stuff", h("docs-readme", { component: "raul-radio" })), h("div", { slot: "api" }, h("docs-preview", { component: "raul-radio", content: "<input type='radio' name='foo' value='bar' />" }), h("docs-interface", { component: "raul-radio" }))));
    };
    return DocsRaulRadioButton;
}());
export { DocsRaulRadioButton as docs_raul_radio_button };
