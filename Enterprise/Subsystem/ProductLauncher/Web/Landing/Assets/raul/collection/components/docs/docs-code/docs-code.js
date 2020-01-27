import { h } from "@stencil/core";
import Prism from 'prismjs';
import 'prismjs/plugins/normalize-whitespace/prism-normalize-whitespace';
export class DocsCode {
    constructor() {
        this.language = 'html';
    }
    componentDidLoad() {
        Prism.plugins.NormalizeWhitespace.setDefaults();
        Prism.highlightAll();
    }
    render() {
        return (h("pre", { class: "line-numbers" },
            h("code", { class: `language-${this.language}` }, `${this.code}`)));
    }
    static get is() { return "docs-code"; }
    static get originalStyleUrls() { return {
        "$": ["docs-code.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["docs-code.css"]
    }; }
    static get properties() { return {
        "code": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "code",
            "reflect": false
        },
        "language": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "language",
            "reflect": false,
            "defaultValue": "'html'"
        }
    }; }
}
