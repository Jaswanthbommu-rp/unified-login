import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsIntroduction {
    componentDidLoad() {
        initPage('Introduction', false);
    }
    render() {
        return (h("div", { class: "docs-page-container" },
            h("div", { class: "docs-page-header" },
                h("docs-markdown", null, `
                # Introduction
            `)),
            h("div", { class: "docs-page-content" }, "Content here.")));
    }
    static get is() { return "docs-introduction"; }
}
