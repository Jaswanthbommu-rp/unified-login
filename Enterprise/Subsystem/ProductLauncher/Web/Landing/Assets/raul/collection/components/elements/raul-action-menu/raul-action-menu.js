import { h } from "@stencil/core";
export class RaulActionMenu {
    constructor() {
        /**
         * If set to true, the last action will be separated with a divider
         */
        this.emphasizeFinal = false;
        /**
         * Disables actions
         */
        this.disabled = false;
    }
    /**
     * Method to programatically close the menu
     */
    async closeMenu() {
        this.dropdownMenu.closeMenu();
    }
    render() {
        return (h("raul-dropdown-menu", { items: this.items, color: 'active', dividers: false, emphasizeFinal: this.emphasizeFinal, disabled: this.disabled, ref: el => this.dropdownMenu = el },
            h("raul-icon", { icon: 'navigation-show-more-vertical', slot: "toggle" }),
            h("slot", null)));
    }
    static get is() { return "raul-action-menu"; }
    static get originalStyleUrls() { return {
        "$": ["raul-action-menu.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-action-menu.css"]
    }; }
    static get properties() { return {
        "items": {
            "type": "unknown",
            "mutable": false,
            "complexType": {
                "original": "DropdownMenuItem[]",
                "resolved": "DropdownMenuItem[]",
                "references": {
                    "DropdownMenuItem": {
                        "location": "import",
                        "path": "../../../utils/types"
                    }
                }
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "As an alternative to the <raul-action-menu-item>, you can programatically provide an array of items to be shown in the dropdown. {title: `string`, url?: `string`, payload: `any`}.\r\nPayload will be the detail of the `optionSelected` event emitted when clicking an action that doesn't have an url"
            }
        },
        "emphasizeFinal": {
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
                "text": "If set to true, the last action will be separated with a divider"
            },
            "attribute": "emphasize-final",
            "reflect": false,
            "defaultValue": "false"
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
                "text": "Disables actions"
            },
            "attribute": "disabled",
            "reflect": false,
            "defaultValue": "false"
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
    static get methods() { return {
        "closeMenu": {
            "complexType": {
                "signature": "() => Promise<void>",
                "parameters": [],
                "references": {
                    "Promise": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "Method to programatically close the menu",
                "tags": []
            }
        }
    }; }
}
