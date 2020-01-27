import { h } from "@stencil/core";
import { stripBaseIndent } from '../../../utils/string';
import marked from 'marked';
export class DocsMarkdown {
    componentDidLoad() {
        this.renderedHTML = marked(stripBaseIndent(this.elRaw.textContent));
    }
    render() {
        return (h("div", { class: "r-docs-markdown" },
            h("raul-content", null,
                h("div", { class: "r-docs-markdown__html", innerHTML: this.renderedHTML }),
                h("div", { ref: el => this.elRaw = el, class: "r-docs-markdown__raw" },
                    h("slot", null)))));
    }
    static get is() { return "docs-markdown"; }
    static get originalStyleUrls() { return {
        "$": ["docs-markdown.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["docs-markdown.css"]
    }; }
    static get states() { return {
        "renderedHTML": {}
    }; }
}
