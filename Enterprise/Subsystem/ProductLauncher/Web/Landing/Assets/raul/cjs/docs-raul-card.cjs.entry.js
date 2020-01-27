'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsRaulCard = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
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
        return (core.h("docs-element", { title: "Card" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Basic"), core.h("div", { class: "-mx-5 p-5 bg-gray-lightest", innerHTML: this.basicCardTemplate }), core.h("docs-code", { class: "mt-5", code: this.basicCardTemplate })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Hoverable"), core.h("div", { class: "-mx-5 p-5 bg-gray-lightest", innerHTML: this.hoverableCardTemplate }), core.h("docs-code", { class: "mt-5", code: this.hoverableCardTemplate })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Composable"), core.h("div", { class: "-mx-5 p-5 bg-gray-lightest", innerHTML: this.composableCardTemplate }), core.h("docs-code", { class: "mt-5", code: this.composableCardTemplate })))), core.h("div", { slot: "design" }, "Design Guidelines"), core.h("div", { slot: "api" }, core.h("docs-preview", { component: "raul-card", content: "Card Content" }), core.h("docs-interface", { component: "raul-card" }))));
    }
};

exports.docs_raul_card = DocsRaulCard;
