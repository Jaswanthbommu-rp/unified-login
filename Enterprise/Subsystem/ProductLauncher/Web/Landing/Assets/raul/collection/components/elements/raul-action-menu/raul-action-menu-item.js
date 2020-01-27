import { h } from "@stencil/core";
import { MenuItem } from './../raul-dropdown-menu/MenuItem';
export class RaulDropdownMenu {
    render() {
        return (h(MenuItem, { url: this.url, disabled: this.disabled, payload: this.payload, onClickCallback: this.clickCallback, onBlurCallback: this.blurCallback, event: this.optionSelected },
            h("slot", null)));
    }
    static get is() { return "raul-action-menu-item"; }
    static get originalStyleUrls() { return {
        "$": ["raul-action-menu.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-action-menu.css"]
    }; }
    static get properties() { return {
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
                "text": "If you provide an url, the raul-action-menu-item will render an `a` tag"
            },
            "attribute": "url",
            "reflect": false
        },
        "disabled": {
            "type": "boolean",
            "mutable": false,
            "complexType": {
                "original": "boolean",
                "resolved": "boolean",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "If true, the option will be disabled"
            },
            "attribute": "disabled",
            "reflect": false
        },
        "payload": {
            "type": "unknown",
            "mutable": false,
            "complexType": {
                "original": "unknown",
                "resolved": "unknown",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "Any sort of data that should be passed in the optionSelected event.detail and the callback functions"
            }
        }
    }; }
    static get events() { return [{
            "method": "clickCallback",
            "name": "clickCallback",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": "The raul-action-menu-item will pass the click event and the payload in the callback"
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }, {
            "method": "blurCallback",
            "name": "blurCallback",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": "The raul-action-menu-item will pass the blur event and the payload in the callback"
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }, {
            "method": "optionSelected",
            "name": "optionSelected",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": "Event emitted when an option is selected."
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }]; }
}
