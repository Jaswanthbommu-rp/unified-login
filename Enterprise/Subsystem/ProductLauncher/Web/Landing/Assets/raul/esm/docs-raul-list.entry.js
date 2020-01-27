import { r as registerInstance, h } from './core-9263a98c.js';

const DocsRaulList = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
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
        return (h("docs-element", { title: "List " }, h("div", { slot: "overview" }, h("docs-showcase", null, h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Basic"), h("div", { class: "md:w-6/12" }, h("div", { innerHTML: this.listBasic })), h("docs-code", { class: "mt-5", code: this.listBasic })), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "With title"), h("div", { class: "md:w-6/12" }, h("div", { innerHTML: this.listWithTitle })), h("docs-code", { class: "mt-5", code: this.listWithTitle })), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "With title and subtitle"), h("div", { class: "md:w-6/12" }, h("div", { innerHTML: this.listWithTitleAndSubtitle })), h("docs-code", { class: "mt-5", code: this.listWithTitleAndSubtitle })), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "With call to action"), h("div", { class: "md:w-6/12" }, h("div", { innerHTML: this.listWithCallToAction })), h("docs-code", { class: "mt-5", code: this.listWithCallToAction })))), h("div", { slot: "design" }, h("docs-readme", { component: "raul-list" })), h("div", { slot: "api" }, h("docs-interface", { component: "raul-list" }))));
    }
};

export { DocsRaulList as docs_raul_list };
