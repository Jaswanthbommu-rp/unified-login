import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsUpgrading {
    componentDidLoad() {
        initPage('Upgrading', false);
    }
    render() {
        return (h("div", { class: "docs-page-container" },
            h("div", { class: "docs-page-header" },
                h("docs-markdown", null, `
                # Upgrading
            `)),
            h("div", { class: "docs-page-content" }, "Stuff.")));
    }
    static get is() { return "docs-upgrading"; }
}
