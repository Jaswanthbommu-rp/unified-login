import { h } from "@stencil/core";
import getDocs from '../../get-docs';
export class DocsPreviewProps {
    componentWillLoad() {
        this.propDocs = getDocs(this.component).props;
    }
    renderPropControl(doc) {
        const { name, attr, type, required } = doc;
        const fallback = doc.default; // default is a Typescript reserved word
        const labelText = name + (required ? '*' : '');
        const emitChange = (value) => {
            this.docsPropChange.emit({ name, attr, type, value });
        };
        if (type === 'string') {
            return (h("raul-input", { label: labelText },
                h("input", { class: "mb-4", type: "text", value: fallback, onInput: e => emitChange(e.target.value) })));
        }
        if (type === 'boolean') {
            return (h("raul-switch", { "label-text": labelText },
                h("input", { type: 'checkbox', checked: false, onChange: e => emitChange(e.target.checked) })));
        }
        const selectOptions = type.replace(/"/g, '').split(' | ').map(opt => { return { text: opt, value: opt }; });
        if (selectOptions.length > 0) {
            return (h("raul-select", { class: "display-block mb-4", label: labelText, options: selectOptions, value: fallback, onRaulChange: e => emitChange(e.detail.value) }));
        }
    }
    render() {
        return (h("div", { class: "r-docs-preview-props" }, this.propDocs.map((propDoc) => {
            return this.renderPropControl(propDoc);
        })));
    }
    static get is() { return "docs-preview-props"; }
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
    static get events() { return [{
            "method": "docsPropChange",
            "name": "docsPropChange",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": ""
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }]; }
}
