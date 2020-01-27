import { h } from "@stencil/core";
export class DocsRaulChips {
    handleRaulChipRemove(e) {
        const chipEl = e.target;
        chipEl.parentNode.removeChild(chipEl);
    }
    render() {
        return (h("docs-element", { title: "Chips" },
            h("div", { slot: "overview" },
                h("docs-showcase", null,
                    h("div", { class: "mt-10" },
                        h("div", { class: "heading-lg" }, "Basic"),
                        h("div", null,
                            h("raul-chip", null, "Chip text")),
                        h("div", { class: "w-40 mt-3" },
                            h("raul-chip", null, "Really long text to test the component"))),
                    h("div", { class: "mt-10" },
                        h("div", { class: "heading-lg" }, "Removable"),
                        h("div", null,
                            h("raul-chip", { removable: true, onRaulChipRemove: (e) => this.handleRaulChipRemove(e) }, "Chip text")),
                        h("div", { class: "w-40 mt-3" },
                            h("raul-chip", { removable: true, onRaulChipRemove: (e) => this.handleRaulChipRemove(e) }, "Really long text to test the component"))))),
            h("div", { slot: "design" },
                h("docs-readme", { component: "raul-chip" })),
            h("div", { slot: "api" },
                h("docs-preview", { component: "raul-chip", content: "Chip" }),
                h("docs-interface", { component: "raul-chip" }))));
    }
    static get is() { return "docs-raul-chips"; }
}
