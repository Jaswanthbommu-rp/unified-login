import { h } from "@stencil/core";
import getDocs from '../get-docs';
export class DocsReadme {
    componentWillLoad() {
        this.readme = getDocs(this.component).readme;
    }
    render() {
        return (h("div", { class: "r-docs-readme page-section" },
            h("docs-markdown", null, this.readme)));
    }
    static get is() { return "docs-readme"; }
    static get properties() { return {
        "component": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": true,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "component",
            "reflect": true
        }
    }; }
}
