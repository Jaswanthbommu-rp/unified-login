import { r as registerInstance, h } from './core-9263a98c.js';

const DocsRaulCard = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        this.basicCardTemplate = `
    <raul-card>
      Card Content
    </raul-card>
  `;
        this.hoverableCardTemplate = `
    <a href="#">
      <raul-card hoverable>
        Card Content
      </raul-card>
    </a>
  `;
        this.composableCardTemplate = `
    <raul-card>
      <raul-card-header>
        <raul-card-title>Card Title</raul-card-title>
        <raul-card-subtitle>Card Subtitle</raul-card-subtitle>

        <raul-card-header-actions slot="card-header-actions">
          <button>
            <raul-icon icon="navigation-show-more-vertical" />
          </button>
        </raul-card-header-actions>
      </raul-card-header>

      <raul-card-body>
        Card Body Content
      </raul-card-body>

      <raul-card-footer>
        Card Footer Content
      </raul-card-footer>
    </raul-card>
  `;
    }
    render() {
        return (h("docs-element", { title: "Card" }, h("div", { slot: "overview" }, h("docs-showcase", null, h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Basic"), h("div", { class: "-mx-5 p-5 bg-gray-lightest", innerHTML: this.basicCardTemplate }), h("docs-code", { class: "mt-5", code: this.basicCardTemplate })), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Hoverable"), h("div", { class: "-mx-5 p-5 bg-gray-lightest", innerHTML: this.hoverableCardTemplate }), h("docs-code", { class: "mt-5", code: this.hoverableCardTemplate })), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Composable"), h("div", { class: "-mx-5 p-5 bg-gray-lightest", innerHTML: this.composableCardTemplate }), h("docs-code", { class: "mt-5", code: this.composableCardTemplate })))), h("div", { slot: "design" }, "Design Guidelines"), h("div", { slot: "api" }, h("docs-preview", { component: "raul-card", content: "Card Content" }), h("docs-interface", { component: "raul-card" }))));
    }
};

export { DocsRaulCard as docs_raul_card };
