import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsNotFound {
    componentDidLoad() {
        initPage('Page not found', false);
    }
    render() {
        return (h("div", { class: "docs-page-container" },
            h("div", { class: "docs-page-header" },
                h("docs-markdown", null, `
                # 404
                These aren't the droids your looking for
            `))));
    }
    static get is() { return "docs-404"; }
    static get originalStyleUrls() { return {
        "$": ["docs-404.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["docs-404.css"]
    }; }
}
