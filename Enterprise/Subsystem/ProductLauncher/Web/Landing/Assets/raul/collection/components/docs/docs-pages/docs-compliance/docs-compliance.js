import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsCompliance {
    componentDidLoad() {
        initPage('Compliance', false);
    }
    render() {
        return (h("div", { class: "docs-page-container" },
            h("div", { class: "docs-page-header" },
                h("docs-markdown", null, `
                # Compliance
            `)),
            h("div", { class: "docs-page-content" }, "Content here.")));
    }
    static get is() { return "docs-compliance"; }
}
