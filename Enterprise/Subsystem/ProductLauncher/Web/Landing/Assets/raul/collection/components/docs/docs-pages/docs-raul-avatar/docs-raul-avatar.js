import { h } from "@stencil/core";
export class DocsRaulAvatar {
    render() {
        return (h("docs-element", { title: "Avatar" },
            h("div", { slot: "overview" },
                h("docs-showcase", null,
                    h("div", null,
                        h("raul-avatar", { class: "mr-3" }),
                        h("raul-avatar", { variant: "property" })),
                    h("div", { class: "mt-3" },
                        h("raul-avatar", { primary: true, class: "mr-3" }),
                        h("raul-avatar", { primary: true, variant: "property" })),
                    h("div", { class: "mt-3" },
                        h("raul-avatar", { small: true, class: "mr-3" }),
                        h("raul-avatar", { small: true, variant: "property" })),
                    h("div", { class: "mt-3" },
                        h("raul-avatar", { primary: true, small: true, class: "mr-3" }),
                        h("raul-avatar", { primary: true, small: true, variant: "property" })))),
            h("div", { slot: "design" }, "Design Guidelines"),
            h("div", { slot: "api" },
                h("docs-preview", { component: "raul-avatar" }),
                h("docs-interface", { component: "raul-avatar" }))));
    }
    static get is() { return "docs-raul-avatar"; }
}
