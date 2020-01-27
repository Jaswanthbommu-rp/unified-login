import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsRaulBadge {
    componentDidLoad() {
        initPage('Badge');
    }
    render() {
        return (h("docs-element", { title: "Badges" },
            h("div", { slot: "overview", class: "r-content" },
                h("docs-showcase", null,
                    h("p", { class: "font-md" },
                        h("raul-badge", { icon: "add-circle-1", content: "88" }),
                        " ",
                        h("code", null, "variant=\"primary\"")),
                    h("p", { class: "font-md" },
                        h("raul-badge", { icon: "add-circle-1", content: "88", variant: "error" }),
                        " ",
                        h("code", null, "variant=\"error\"")),
                    h("p", { class: "font-md" },
                        h("raul-badge", { icon: "add-circle-1", content: "88", variant: "warning" }),
                        " ",
                        h("code", null, "variant=\"warning\"")),
                    h("p", { class: "font-md" },
                        h("raul-badge", { icon: "add-circle-1", content: "88", variant: "success" }),
                        " ",
                        h("code", null, "variant=\"success\"")))),
            h("div", { slot: "design" }, "Design Guidelines Stuff"),
            h("div", { slot: "api" },
                h("docs-readme", { component: "raul-badge" }),
                h("docs-preview", { component: "raul-badge" }),
                h("docs-interface", { component: "raul-badge" }))));
    }
    static get is() { return "docs-raul-badge"; }
}
