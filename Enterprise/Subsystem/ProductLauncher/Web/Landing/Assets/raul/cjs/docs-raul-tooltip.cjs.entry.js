'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsRaulTooltip = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.basicTooltip = `
    <raul-tooltip text="Tooltip Text">
      <raul-button variant="text">Basic</raul-button>
    </raul-tooltip>
  `;
        this.positionedTooltips = `
    <div class="flex justify-center">
      <raul-tooltip text="Tooltip Text" placement="top-start">
        <raul-button variant="text">Top Start</raul-button>
      </raul-tooltip>
      
      <raul-tooltip text="Tooltip Text" placement="top">
        <raul-button variant="text">Top</raul-button>
      </raul-tooltip>
      
      <raul-tooltip text="Tooltip Text" placement="top-end">
        <raul-button variant="text">Top End</raul-button>
      </raul-tooltip>
    </div>
    
    <div class="flex">
      <div class="flex-1">
        <raul-tooltip text="Tooltip Text" placement="left-start">
          <raul-button variant="text">Left Start</raul-button>
        </raul-tooltip>
      </div>
      
      <div class="flex-1 text-right">
        <raul-tooltip text="Tooltip Text" placement="right-start">
          <raul-button variant="text">Right Start</raul-button>
        </raul-tooltip>
      </div>
    </div>
    
    <div class="flex">
      <div class="flex-1">
        <raul-tooltip text="Tooltip Text" placement="left">
          <raul-button variant="text">Left</raul-button>
        </raul-tooltip>
      </div>
      
      <div class="flex-1 text-right">
        <raul-tooltip text="Tooltip Text" placement="right">
          <raul-button variant="text">Right</raul-button>
        </raul-tooltip>
      </div>
    </div>
    
    <div class="flex">
      <div class="flex-1">
        <raul-tooltip text="Tooltip Text" placement="left-end">
          <raul-button variant="text">Left End</raul-button>
        </raul-tooltip>
      </div>
      
      <div class="flex-1 text-right">
        <raul-tooltip text="Tooltip Text" placement="right-end">
          <raul-button variant="text">Right End</raul-button>
        </raul-tooltip>
      </div>
    </div>
    
    <div class="flex justify-center">
      <raul-tooltip text="Tooltip Text" placement="bottom-start">
        <raul-button variant="text">Bottom Start</raul-button>
      </raul-tooltip>
      
      <raul-tooltip text="Tooltip Text" placement="bottom">
        <raul-button variant="text">Bottom</raul-button>
      </raul-tooltip>
      
      <raul-tooltip text="Tooltip Text" placement="bottom-end">
        <raul-button variant="text">Bottom End</raul-button>
      </raul-tooltip>
    </div>
  `;
        this.disabledHoverTooltip = `
    <raul-tooltip text="Tooltip Text" disabled-hover-listener>
      <raul-button variant="text">Basic</raul-button>
    </raul-tooltip>
  `;
        this.disabledFocusTooltip = `
    <raul-tooltip text="Tooltip Text" disabled-focus-listener>
      <raul-button variant="text">Basic</raul-button>
    </raul-tooltip>
  `;
    }
    render() {
        return (core.h("docs-element", { title: "Tooltips" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("div", null, core.h("div", { class: "heading-lg" }, "Basic"), core.h("div", { innerHTML: this.basicTooltip }), core.h("docs-code", { class: "mt-5", code: this.basicTooltip })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Positioned"), core.h("div", { class: "flex justify-center" }, core.h("div", { class: "md:w-6/12", innerHTML: this.positionedTooltips })), core.h("docs-code", { class: "mt-5", code: this.positionedTooltips })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Disabled Hover Listener"), core.h("div", { innerHTML: this.disabledHoverTooltip }), core.h("docs-code", { class: "mt-5", code: this.disabledHoverTooltip })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Disabled Focus Listener"), core.h("div", { innerHTML: this.disabledFocusTooltip }), core.h("docs-code", { class: "mt-5", code: this.disabledFocusTooltip })))), core.h("div", { slot: "design" }, "Design Guidelines"), core.h("div", { slot: "api" }, core.h("docs-interface", { component: "raul-tooltip" }))));
    }
};

exports.docs_raul_tooltip = DocsRaulTooltip;
