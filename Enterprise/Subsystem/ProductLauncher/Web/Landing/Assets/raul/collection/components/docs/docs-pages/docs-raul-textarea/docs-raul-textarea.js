import { h } from "@stencil/core";
export class DocsRaulTextarea {
    render() {
        return (h("docs-element", { title: "Textarea" },
            h("div", { slot: "overview" },
                h("docs-showcase", null,
                    h("div", { class: "mb-md" },
                        h("raul-textarea", null,
                            h("textarea", { placeholder: "Placeholder text" }))),
                    h("div", { class: "mb-md" },
                        h("raul-textarea", { label: "Label text" },
                            h("textarea", { id: "textarea-0", placeholder: "Placeholder text" }))),
                    h("div", { class: "mb-md" },
                        h("raul-textarea", { label: "Label text", hint: "Optional hint" },
                            h("textarea", { placeholder: "Placeholder text" }))),
                    h("div", { class: "mb-md" },
                        h("raul-textarea", { label: "Disabled" },
                            h("textarea", { placeholder: "Placeholder text", disabled: true }))),
                    h("div", { class: "mb-md" },
                        h("raul-textarea", { label: "Invalid", error: "Error message" },
                            h("textarea", { placeholder: "Placeholder text" }))),
                    h("div", { class: "mb-md" },
                        h("raul-textarea", { label: "Invalid", hint: "Optional hint", error: "Error message" },
                            h("textarea", { placeholder: "Placeholder text" }))))),
            h("div", { slot: "design" }, "Design Guidelines"),
            h("div", { slot: "api" },
                h("docs-preview", { component: "raul-textarea", content: "\r\n              <textarea placeholder='Placeholder text'></textarea>\r\n            " }),
                h("docs-interface", { component: "raul-textarea" }))));
    }
    static get is() { return "docs-raul-textarea"; }
}
