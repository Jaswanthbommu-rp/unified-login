import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsRaulPagingBar {
    componentDidLoad() {
        initPage('Paging Bar');
    }
    render() {
        return (h("docs-element", { title: "Paging Bar" },
            h("div", { slot: "overview" },
                h("docs-showcase", null,
                    h("raul-paging-bar", { "total-rows": "55" }))),
            h("div", { slot: "design" },
                h("docs-readme", { component: "raul-paging-bar" })),
            h("div", { slot: "api" },
                h("docs-preview", { component: "raul-paging-bar", content: "Paging Bar" }),
                h("docs-interface", { component: "raul-paging-bar" }))));
    }
    static get is() { return "docs-raul-paging-bar"; }
}
