import { h } from "@stencil/core";
export class DocsMenuPage {
    handleClick(e) {
        if (this.el.contains(e.target)) {
            this.menuPageActivated.emit();
        }
    }
    render() {
        return (h("div", { class: "r-docs-menu__page" },
            h("stencil-route-link", { "anchor-class": "r-docs-menu__page__link", url: this.url }, this.name)));
    }
    static get is() { return "docs-menu-page"; }
    static get properties() { return {
        "name": {
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
            "attribute": "name",
            "reflect": false
        },
        "url": {
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
            "attribute": "url",
            "reflect": false
        }
    }; }
    static get events() { return [{
            "method": "menuPageActivated",
            "name": "menuPageActivated",
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
    static get elementRef() { return "el"; }
    static get listeners() { return [{
            "name": "click",
            "method": "handleClick",
            "target": "window",
            "capture": false,
            "passive": false
        }]; }
}
