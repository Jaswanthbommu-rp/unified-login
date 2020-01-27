import { h } from "@stencil/core";
export class DocsShowcase {
    render() {
        return (h("div", { class: "r-docs-showcase page-section" },
            h("h2", { id: "examples", class: "text-lg font-bold mb-6" }, "Examples"),
            h("slot", null)));
    }
    static get is() { return "docs-showcase"; }
}
