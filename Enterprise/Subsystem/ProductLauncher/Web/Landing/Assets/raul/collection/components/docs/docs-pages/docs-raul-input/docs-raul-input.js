import { h } from "@stencil/core";
export class DocsRaulInput {
    render() {
        return (h("docs-element", { title: "Input" },
            h("div", { slot: "overview" },
                h("docs-readme", { component: "raul-input" }),
                h("docs-showcase", null,
                    h("div", { class: "mb-md" },
                        h("raul-input", null,
                            h("input", { type: "text", placeholder: "Placeholder text" }))),
                    h("div", { class: "mb-md" },
                        h("raul-input", { label: "Label text" },
                            h("input", { type: "text", placeholder: "Placeholder text" }))),
                    h("div", { class: "mb-md" },
                        h("raul-input", { label: "Label text", hint: "Optional hint" },
                            h("input", { type: "text", placeholder: "Placeholder text" }))),
                    h("div", { class: "mb-md" },
                        h("raul-input", { label: "Disabled" },
                            h("input", { type: "text", placeholder: "Placeholder text", disabled: true }))),
                    h("div", { class: "mb-md" },
                        h("raul-input", { label: "Invalid", error: "Error message" },
                            h("input", { type: "text", placeholder: "Placeholder text" }))),
                    h("div", { class: "mb-md" },
                        h("raul-input", { label: "Invalid", hint: "Optional hint", error: "Error message" },
                            h("input", { type: "text", placeholder: "Placeholder text" }))),
                    h("div", { class: "mb-md" },
                        h("raul-input", { label: "Type search", type: "search" },
                            h("input", { type: "text", placeholder: "Placeholder text" }))))),
            h("div", { slot: "design" }, "Design Guidelines"),
            h("div", { slot: "api" },
                h("docs-preview", { component: "raul-input", content: "\r\n              <input type='text' placeholder='Placeholder text'></input>\r\n            " }),
                h("docs-interface", { component: "raul-input" }))));
    }
    static get is() { return "docs-raul-input"; }
}
