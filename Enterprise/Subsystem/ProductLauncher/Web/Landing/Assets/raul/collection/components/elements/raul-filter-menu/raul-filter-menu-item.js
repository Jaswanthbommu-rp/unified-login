import { h } from "@stencil/core";
import { MenuItem } from './../raul-dropdown-menu/MenuItem';
// title, icon, payload, url, disabled, onClickCallback, onBlurCallback 
export class RaulFilterMenuItem {
    render() {
        return (h(MenuItem
        // url={this.url}
        , { 
            // url={this.url}
            icon: this.icon, disabled: this.disabled, payload: this.payload, onClickCallback: this.onClickCallback, onBlurCallback: this.onBlurCallback, event: this.optionSelected },
            h("slot", null)));
    }
    static get is() { return "raul-filter-menu-item"; }
    static get originalStyleUrls() { return {
        "$": ["raul-filter-menu.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-filter-menu.css"]
    }; }
    static get properties() { return {
        "icon": {
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
                "text": "An icon to be rendered before the item"
            },
            "attribute": "icon",
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
        "onClickCallback": {
            "type": "unknown",
            "mutable": false,
            "complexType": {
                "original": "Function",
                "resolved": "Function",
                "references": {
                    "Function": {
                        "location": "global"
                    }
                }
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "The raul-filter-menu-item will pass the click event and the payload in the callback"
            }
        },
        "onBlurCallback": {
            "type": "unknown",
            "mutable": false,
            "complexType": {
                "original": "Function",
                "resolved": "Function",
                "references": {
                    "Function": {
                        "location": "global"
                    }
                }
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "The raul-filter-menu-item will pass the blur event and the payload in the callback"
            }
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
