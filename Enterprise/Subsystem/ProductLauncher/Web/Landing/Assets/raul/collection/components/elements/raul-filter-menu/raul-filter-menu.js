import { h } from "@stencil/core";
export class RaulDropdownMenu {
    constructor() {
        /**
        * Icon to be used in the menu toggle. Defaults to `list-bullets-3`
        */
        this.icon = 'list-bullets-3';
    }
    /**
    * Method to programatically close the menu
    */
    async closeMenu() {
        this.dropdownMenu.closeMenu();
    }
    render() {
        return (h("raul-dropdown-menu", { ref: el => this.dropdownMenu = el, items: this.items, color: 'primary', dividers: true, emphasizeFinal: false },
            h("div", { class: 'r-filter-menu__icon-button-container', slot: "toggle" },
                h("raul-icon", { icon: this.icon }),
                h("raul-icon", { icon: 'arrow-down-v' })),
            h("slot", null)));
    }
    static get is() { return "raul-filter-menu"; }
    static get originalStyleUrls() { return {
        "$": ["raul-filter-menu.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-filter-menu.css"]
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
                "text": "As an alternative to the <raul-filter-menu-item>, you can programatically provide an array of items to be shown in the dropdown. {title: `string`, icon: `string`, payload: `any`}.\r\nPayload will be the detail of the `optionSelected` event emitted when clicking an action"
            }
        },
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
                "text": "Icon to be used in the menu toggle. Defaults to `list-bullets-3`"
            },
            "attribute": "icon",
            "reflect": false,
            "defaultValue": "'list-bullets-3'"
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
