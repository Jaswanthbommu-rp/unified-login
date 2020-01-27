import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsRaulText {
    componentDidLoad() {
        initPage('Text');
    }
    render() {
        return (h("div", { class: "docs-page-container" },
            h("div", { class: "docs-page-header tabbed" },
                h("docs-markdown", null, `
                # Text
            `),
                h("raul-tabs", { tabs: [
                        { label: "OVERVIEW", name: "overview" },
                        { label: "DESIGN GUIDELINES", name: "design-guidelines" },
                        { label: "API", name: "api" },
                    ], "active-tab": "overview" })),
            h("div", { class: "docs-page-content" },
                h("docs-readme", { component: "raul-text" }),
                h("docs-preview", { component: "raul-text", content: "Lorem ipsum dolor sit amet" }),
                h("docs-showcase", null),
                h("docs-interface", { component: "raul-text" }))));
    }
    static get is() { return "docs-raul-text"; }
}
