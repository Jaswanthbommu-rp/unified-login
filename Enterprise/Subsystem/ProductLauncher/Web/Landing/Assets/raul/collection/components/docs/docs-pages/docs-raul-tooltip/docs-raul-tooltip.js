import { h } from "@stencil/core";
export class DocsRaulTooltip {
    constructor() {
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
        return (h("docs-element", { title: "Tooltips" },
            h("div", { slot: "overview" },
                h("docs-showcase", null,
                    h("div", null,
                        h("div", { class: "heading-lg" }, "Basic"),
                        h("div", { innerHTML: this.basicTooltip }),
                        h("docs-code", { class: "mt-5", code: this.basicTooltip })),
                    h("div", { class: "mt-10" },
                        h("div", { class: "heading-lg" }, "Positioned"),
                        h("div", { class: "flex justify-center" },
                            h("div", { class: "md:w-6/12", innerHTML: this.positionedTooltips })),
                        h("docs-code", { class: "mt-5", code: this.positionedTooltips })),
                    h("div", { class: "mt-10" },
                        h("div", { class: "heading-lg" }, "Disabled Hover Listener"),
                        h("div", { innerHTML: this.disabledHoverTooltip }),
                        h("docs-code", { class: "mt-5", code: this.disabledHoverTooltip })),
                    h("div", { class: "mt-10" },
                        h("div", { class: "heading-lg" }, "Disabled Focus Listener"),
                        h("div", { innerHTML: this.disabledFocusTooltip }),
                        h("docs-code", { class: "mt-5", code: this.disabledFocusTooltip })))),
            h("div", { slot: "design" }, "Design Guidelines"),
            h("div", { slot: "api" },
                h("docs-interface", { component: "raul-tooltip" }))));
    }
    static get is() { return "docs-raul-tooltip"; }
}
