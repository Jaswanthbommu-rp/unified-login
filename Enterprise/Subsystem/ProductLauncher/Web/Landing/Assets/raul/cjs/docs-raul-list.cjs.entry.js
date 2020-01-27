'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsRaulList = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.listBasic = `
    <raul-list>
      <raul-list-item>Item 1</raul-list-item>
      <raul-list-item>Item 2</raul-list-item>
      <raul-list-item>Item 3</raul-list-item>
    </raul-list>
  `;
        this.listWithTitle = `
    <raul-list>
      <raul-list-header>
        <raul-list-title>List  title</raul-list-title>
      </raul-list-header>
      <raul-list-item>
        <raul-list-item-title>Item 1 title</raul-list-item-title>
        Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum ullamcorper quis nisl at commodo. Nam condimentum risus purus, vitae semper ipsum volutpat in.
      </raul-list-item>
      <raul-list-item>
        <raul-list-item-title>Item 2 title</raul-list-item-title>
        Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum ullamcorper quis nisl at commodo. Nam condimentum risus purus, vitae semper ipsum volutpat in.
      </raul-list-item>
      <raul-list-item>
        <raul-list-item-title>Item 3 title</raul-list-item-title>
        Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum ullamcorper quis nisl at commodo. Nam condimentum risus purus, vitae semper ipsum volutpat in.
      </raul-list-item>
    </raul-list>
  `;
        this.listWithTitleAndSubtitle = `
    <raul-list>
      <raul-list-header>
        <raul-list-title>List  title</raul-list-title>
      </raul-list-header>
      <raul-list-item>
        <raul-list-item-title>Item 1 title</raul-list-item-title>
        <raul-list-item-subtitle>Item 1 subtitle</raul-list-item-subtitle>
      </raul-list-item>
      <raul-list-item>
        <raul-list-item-title>Item 2 title</raul-list-item-title>
        <raul-list-item-subtitle>Item 2 subtitle</raul-list-item-subtitle>
      </raul-list-item>
      <raul-list-item>
        <raul-list-item-title>Item 3 title</raul-list-item-title>
        <raul-list-item-subtitle>Item 3 subtitle</raul-list-item-subtitle>
      </raul-list-item>
    </raul-list>
  `;
        this.listWithCallToAction = `
    <raul-list>
      <raul-list-header>
        <div class="flex justify-between items-start">
          <raul-list-title>List  title</raul-list-title>
          <raul-list-action>
            <a href="#" class="r-list__action">Action</a>
          </raul-list-action>
        </div>
      </raul-list-header>
      <raul-list-item>
        <div class="flex justify-between items-start">
          <raul-list-item-title>Item 1 title</raul-list-item-title>
          <raul-list-item-action>
            <a href="#" class="r-list__item__action">
              <raul-icon icon="add-2"></raul-icon>
            </a>
          </raul-list-item-action>
        </div>
        <raul-list-item-subtitle>Item 1 subtitle</raul-list-item-subtitle>
      </raul-list-item>
      <raul-list-item>
        <div class="flex justify-between items-start">
          <raul-list-item-title>Item 2 title</raul-list-item-title>
          <raul-list-item-action>
            <button type="button" class="r-list__item__action">
              <raul-icon icon="add-2"></raul-icon>          
            </button>
          </raul-list-item-action>
        </div>
        <raul-list-item-subtitle>Item 2 subtitle</raul-list-item-subtitle>
      </raul-list-item>
      <raul-list-item>
        <raul-list-item-title>Item 3 title</raul-list-item-title>
        <raul-list-item-subtitle>Item 3 subtitle</raul-list-item-subtitle>
      </raul-list-item>
    </raul-list>
  `;
    }
    render() {
        return (core.h("docs-element", { title: "List " }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Basic"), core.h("div", { class: "md:w-6/12" }, core.h("div", { innerHTML: this.listBasic })), core.h("docs-code", { class: "mt-5", code: this.listBasic })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "With title"), core.h("div", { class: "md:w-6/12" }, core.h("div", { innerHTML: this.listWithTitle })), core.h("docs-code", { class: "mt-5", code: this.listWithTitle })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "With title and subtitle"), core.h("div", { class: "md:w-6/12" }, core.h("div", { innerHTML: this.listWithTitleAndSubtitle })), core.h("docs-code", { class: "mt-5", code: this.listWithTitleAndSubtitle })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "With call to action"), core.h("div", { class: "md:w-6/12" }, core.h("div", { innerHTML: this.listWithCallToAction })), core.h("docs-code", { class: "mt-5", code: this.listWithCallToAction })))), core.h("div", { slot: "design" }, core.h("docs-readme", { component: "raul-list" })), core.h("div", { slot: "api" }, core.h("docs-interface", { component: "raul-list" }))));
    }
};

exports.docs_raul_list = DocsRaulList;
