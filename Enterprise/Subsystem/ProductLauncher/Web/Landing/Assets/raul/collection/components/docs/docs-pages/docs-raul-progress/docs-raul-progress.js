import { h } from "@stencil/core";
export class DocsRaulProgress {
    constructor() {
        this.dynamicProgress = `
    <raul-progress
      label="Label Text"
      hint="Optional Text"
      value="25"
    ></raul-progress>
  `;
        this.staticProgress = `
    <raul-progress
      label="Label Text"
      static
      value="100"
    ></raul-progress>
  `;
        this.warningProgress = `
    <raul-progress
      label="Label Text"
      hint="Optional Text"
      value="25"
      color="warning"
    ></raul-progress>
    
    <raul-progress
      label="Label Text"
      static
      value="100"
      color="warning"
      class="mt-5"
    ></raul-progress>
  `;
        this.dangerProgress = `
    <raul-progress
      label="Label Text"
      hint="Optional Text"
      value="25"
      color="danger"
    ></raul-progress>
    
    <raul-progress
      label="Label Text"
      static
      value="100"
      color="danger"
      class="mt-5"
    ></raul-progress>
  `;
        this.successProgress = `
    <raul-progress
      label="Label Text"
      hint="Optional Text"
      value="25"
      color="success"
    ></raul-progress>
    
    <raul-progress
      label="Label Text"
      static
      value="100"
      color="success"
      class="mt-5"
    ></raul-progress>
  `;
    }
    render() {
        return (h("docs-element", { title: "Progress" },
            h("div", { slot: "overview" },
                h("docs-showcase", null,
                    h("div", null,
                        h("div", { class: "heading-lg" }, "Dynamic"),
                        h("div", { class: "md:w-6/12", innerHTML: this.dynamicProgress }),
                        h("docs-code", { class: "mt-5", code: this.dynamicProgress })),
                    h("div", { class: "mt-10" },
                        h("div", { class: "heading-lg" }, "Static"),
                        h("div", { class: "md:w-6/12", innerHTML: this.staticProgress }),
                        h("docs-code", { class: "mt-5", code: this.staticProgress })),
                    h("div", { class: "mt-10" },
                        h("div", { class: "heading-lg" }, "Warning"),
                        h("div", { class: "md:w-6/12", innerHTML: this.warningProgress }),
                        h("docs-code", { class: "mt-5", code: this.warningProgress })),
                    h("div", { class: "mt-10" },
                        h("div", { class: "heading-lg" }, "Danger"),
                        h("div", { class: "md:w-6/12", innerHTML: this.dangerProgress }),
                        h("docs-code", { class: "mt-5", code: this.dangerProgress })),
                    h("div", { class: "mt-10" },
                        h("div", { class: "heading-lg" }, "Success"),
                        h("div", { class: "md:w-6/12", innerHTML: this.successProgress }),
                        h("docs-code", { class: "mt-5", code: this.successProgress })))),
            h("div", { slot: "design" }, "Design Guidelines"),
            h("div", { slot: "api" },
                h("docs-interface", { component: "raul-progress" }))));
    }
    static get is() { return "docs-raul-progress"; }
}
