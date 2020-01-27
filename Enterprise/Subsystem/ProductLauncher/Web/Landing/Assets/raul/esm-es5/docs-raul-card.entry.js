import { r as registerInstance, h } from './core-9263a98c.js';
var DocsRaulCard = /** @class */ (function () {
    function DocsRaulCard(hostRef) {
        registerInstance(this, hostRef);
        this.basicCardTemplate = "\n    <raul-card>\n      Card Content\n    </raul-card>\n  ";
        this.hoverableCardTemplate = "\n    <a href=\"#\">\n      <raul-card hoverable>\n        Card Content\n      </raul-card>\n    </a>\n  ";
        this.composableCardTemplate = "\n    <raul-card>\n      <raul-card-header>\n        <raul-card-title>Card Title</raul-card-title>\n        <raul-card-subtitle>Card Subtitle</raul-card-subtitle>\n\n        <raul-card-header-actions slot=\"card-header-actions\">\n          <button>\n            <raul-icon icon=\"navigation-show-more-vertical\" />\n          </button>\n        </raul-card-header-actions>\n      </raul-card-header>\n\n      <raul-card-body>\n        Card Body Content\n      </raul-card-body>\n\n      <raul-card-footer>\n        Card Footer Content\n      </raul-card-footer>\n    </raul-card>\n  ";
    }
    DocsRaulCard.prototype.render = function () {
        return (h("docs-element", { title: "Card" }, h("div", { slot: "overview" }, h("docs-showcase", null, h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Basic"), h("div", { class: "-mx-5 p-5 bg-gray-lightest", innerHTML: this.basicCardTemplate }), h("docs-code", { class: "mt-5", code: this.basicCardTemplate })), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Hoverable"), h("div", { class: "-mx-5 p-5 bg-gray-lightest", innerHTML: this.hoverableCardTemplate }), h("docs-code", { class: "mt-5", code: this.hoverableCardTemplate })), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Composable"), h("div", { class: "-mx-5 p-5 bg-gray-lightest", innerHTML: this.composableCardTemplate }), h("docs-code", { class: "mt-5", code: this.composableCardTemplate })))), h("div", { slot: "design" }, "Design Guidelines"), h("div", { slot: "api" }, h("docs-preview", { component: "raul-card", content: "Card Content" }), h("docs-interface", { component: "raul-card" }))));
    };
    return DocsRaulCard;
}());
export { DocsRaulCard as docs_raul_card };
