import { h } from "@stencil/core";
import Beautify from 'js-beautify';
const hasScalarValue = (type) => {
    return type === 'string' || type === 'boolean' || type.match(/.+|.+/);
};
export class DocsPreview {
    constructor() {
        this.content = null;
        this.componentHTML = null;
    }
    componentDidLoad() {
        this.refreshSnippet();
    }
    handlePropChange(e) {
        e.stopPropagation();
        const { name, attr, type, value } = e.detail;
        if (hasScalarValue(type)) {
            this.componentEl.setAttribute(attr, value);
        }
        else {
            this.componentEl[name] = value;
        }
        this.refreshSnippet();
    }
    refreshSnippet() {
        const componentElClone = this.componentEl.cloneNode();
        componentElClone.innerHTML = this.content;
        componentElClone.removeAttribute('class');
        componentElClone.removeAttribute('class');
        const snippetText = componentElClone
            .outerHTML
            .replace(/(<raul-.+?>)/, '$1\n')
            .replace(/(<\/raul-.+?>)/, '\n$1')
            .replace(/(aria-.+?=".*?")|(aria\-[^=\n\s]*)/, '');
        this.componentHTML = Beautify.html_beautify(snippetText, {
            wrap_attributes: 'force-expand-multiline',
            indent_size: 2,
        });
    }
    render() {
        const ComponentTag = this.component;
        return (h("div", { class: "r-docs-preview page-section" },
            h("h2", { id: "preview", class: "text-lg font-bold" }, "Preview"),
            h("div", { class: "r-docs-preview__widget" },
                h("div", { class: "r-docs-preview__component", style: { padding: '16px !important' } },
                    h(ComponentTag, { ref: (el) => this.componentEl = el, innerHTML: this.content })),
                h("div", { class: "r-docs-preview__controls" },
                    h("docs-preview-props", { component: this.component })),
                h("div", { class: "r-docs-preview__snippet" },
                    h("pre", null,
                        h("code", null, this.componentHTML))))));
    }
    static get is() { return "docs-preview"; }
    static get originalStyleUrls() { return {
        "$": ["docs-preview.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["docs-preview.css"]
    }; }
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
        },
        "content": {
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
            "attribute": "content",
            "reflect": false,
            "defaultValue": "null"
        }
    }; }
    static get states() { return {
        "componentHTML": {}
    }; }
    static get listeners() { return [{
            "name": "docsPropChange",
            "method": "handlePropChange",
            "target": undefined,
            "capture": false,
            "passive": false
        }]; }
}
