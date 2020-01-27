import { h } from "@stencil/core";
import { MenuItem } from './MenuItem';
export class RaulDropdownMenu {
    constructor() {
        this.open = false;
        this.top = false;
        this.right = false;
        this.openMutated = false;
        this.dividers = false;
        this.emphasizeFinal = false;
        this.color = 'primary';
        this.disabled = false;
        this.handleToggleClick = () => {
            this.open = !this.open;
        };
        this.handleKeyDown = e => {
            if (e.key === 'Enter')
                this.open = !this.open;
        };
        this.handleMenuItemClick = payload => {
            this.optionSelected.emit(payload);
        };
        this.handleMenuItemBlur = (e) => {
            requestAnimationFrame(() => {
                let length = this.dropdown.querySelectorAll('.r-dropdown-menu__menu-item').length;
                if ((e.target === this.dropdown.querySelectorAll('.r-dropdown-menu__menu-item')[length - 1])
                    &&
                        (document.activeElement !== this.dropdown.querySelectorAll('.r-dropdown-menu__menu-item')[length - 2])) {
                    this.open = false;
                }
            });
        };
        this.handleEscape = e => {
            if (['Escape', 'Esc'].includes(e.key)) {
                this.open = false;
                this.toggle.focus();
            }
        };
    }
    componentDidLoad() {
        window.addEventListener('click', (e) => {
            // @ts-ignore
            if (this.open && !this.dropdown.contains(e.target) && !this.toggle.contains(e.target)) {
                this.open = false;
            }
        }, true); // third argument is capture
    }
    handleOpenChange(newValue, oldValue) {
        if (newValue && !oldValue) {
            this.openMutated = true;
        }
        if (!newValue) {
            this.top = false;
            this.right = false;
        }
    }
    componentDidRender() {
        if (this.open && this.openMutated) {
            this.checkViewportCollision();
        }
        this.openMutated = false;
    }
    checkViewportCollision() {
        const rect = this.dropdown.getBoundingClientRect();
        this.top = rect.bottom > window.innerHeight;
        this.right = rect.right < rect.width;
    }
    async closeMenu() {
        this.open = false;
    }
    render() {
        return (h("div", { class: {
                'r-dropdown-menu': true,
                'r-dropdown-menu--dividers': this.dividers,
                'r-dropdown-menu--emphasize-final': this.emphasizeFinal,
                'r-dropdown-menu--color-active': this.color === 'active',
                'r-dropdown-menu--show': this.open
            }, onKeyDown: this.handleEscape },
            h("div", { onClick: this.handleToggleClick, class: 'r-dropdown-menu__toggle', role: "button", tabIndex: 0, onKeyDown: this.handleKeyDown, ref: el => this.toggle = el },
                h("div", { class: 'r-dropdown-menu__toggle__focus-utility', tabindex: -1 },
                    h("slot", { name: "toggle" }))),
            h("div", { ref: el => this.dropdown = el, class: {
                    'r-dropdown-menu__dropdown': true,
                    'r-dropdown-menu__dropdown--show': this.open,
                    'r-dropdown-menu__dropdown--default': !this.top && !this.right,
                    'r-dropdown-menu__dropdown--top': this.top,
                    'r-dropdown-menu__dropdown--right': this.right
                } },
                this.items && this.items.map(item => h(MenuItem, Object.assign({}, item, { onBlurCallback: this.handleMenuItemBlur, onClickCallback: this.handleMenuItemClick, disabled: this.disabled }))),
                h("slot", null))));
    }
    static get is() { return "raul-dropdown-menu"; }
    static get originalStyleUrls() { return {
        "$": ["raul-dropdown-menu.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-dropdown-menu.css"]
    }; }
    static get properties() { return {
        "dividers": {
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
                "text": ""
            },
            "attribute": "dividers",
            "reflect": false,
            "defaultValue": "false"
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
                "text": ""
            },
            "attribute": "emphasize-final",
            "reflect": false,
            "defaultValue": "false"
        },
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
            "optional": true,
            "docs": {
                "tags": [],
                "text": ""
            }
        },
        "color": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "'active' | 'primary'",
                "resolved": "\"active\" | \"primary\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "color",
            "reflect": false,
            "defaultValue": "'primary'"
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
                "text": ""
            },
            "attribute": "disabled",
            "reflect": false,
            "defaultValue": "false"
        }
    }; }
    static get states() { return {
        "open": {},
        "top": {},
        "right": {}
    }; }
    static get events() { return [{
            "method": "optionSelected",
            "name": "optionSelected",
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
                "text": "",
                "tags": []
            }
        }
    }; }
    static get watchers() { return [{
            "propName": "open",
            "methodName": "handleOpenChange"
        }]; }
}
