import { h } from "@stencil/core";
import { ripple } from '../../../utils/animation';
export class ButtonComponent {
    constructor() {
        this.iconOnly = false;
        /**
         * Determines the primary appearance of the button based on its purpose.
         */
        this.variant = "secondary";
        /**
         * Determines the primary appearance of the button based on its purpose.
         */
        this.size = "default";
        /**
         * Controls whether this button is disabled.
         */
        this.disabled = false;
        /**
         * Adds `add` icon.
         */
        this.add = false;
        /**
         * Adds `delete` icon.
         */
        this.delete = false;
    }
    componentDidLoad() {
        if (!this.btnTextEl.innerHTML) {
            this.iconOnly = true;
        }
    }
    render() {
        const renderAddOrDeleteIcon = () => {
            return this.add ? h("raul-icon", { icon: "add-2" }) : this.delete ? h("raul-icon", { icon: "remove-2" }) : null;
        };
        const renderIcon = () => {
            return this.icon && !(this.add || this.delete) ? h("raul-icon", { icon: this.icon, kind: this.iconKind }) : null;
        };
        const renderButton = () => {
            const ButtonTypeTag = this.href ? 'a' : this.type ? 'input' : 'button';
            return (h(ButtonTypeTag, { class: "r-button__element", type: this.type, href: this.href && !this.disabled ? this.href : null, "aria-disabled": this.disabled, disabled: this.disabled, value: this.value, onMouseDown: e => ripple(e, this.focusRingEl) }));
        };
        return (h("div", { class: {
                'r-button': true,
                [`r-button--${this.variant}`]: !!this.variant,
                [`r-button--${this.size}`]: !!this.size
            } },
            h("div", { class: "r-button__focus-ring", ref: el => this.focusRingEl = el }),
            renderButton(),
            h("div", { class: {
                    'r-button__content': true,
                    'r-button__content--icon-only': this.iconOnly
                } },
                renderAddOrDeleteIcon(),
                renderIcon(),
                h("span", { ref: el => this.btnTextEl = el },
                    h("slot", null)))));
    }
    static get is() { return "raul-button"; }
    static get originalStyleUrls() { return {
        "$": ["raul-button.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-button.css"]
    }; }
    static get properties() { return {
        "variant": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "\"primary\" | \"secondary\" | \"reverse\" | \"danger\" | \"text\" | \"control\"",
                "resolved": "\"control\" | \"danger\" | \"primary\" | \"reverse\" | \"secondary\" | \"text\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "Determines the primary appearance of the button based on its purpose."
            },
            "attribute": "variant",
            "reflect": true,
            "defaultValue": "\"secondary\""
        },
        "size": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "\"default\" | \"small\"",
                "resolved": "\"default\" | \"small\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "Determines the primary appearance of the button based on its purpose."
            },
            "attribute": "size",
            "reflect": true,
            "defaultValue": "\"default\""
        },
        "type": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "\"button\" | \"reset\" | \"submit\"",
                "resolved": "\"button\" | \"reset\" | \"submit\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "Controls the underlying markup based on the use case for the button."
            },
            "attribute": "type",
            "reflect": true
        },
        "value": {
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
                "text": "Only valid for input types (submit, reset)."
            },
            "attribute": "value",
            "reflect": true
        },
        "href": {
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
                "text": "Determines link behavior. Only valid for links."
            },
            "attribute": "href",
            "reflect": true
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
                "text": "Controls whether this button is disabled."
            },
            "attribute": "disabled",
            "reflect": true,
            "defaultValue": "false"
        },
        "add": {
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
                "text": "Adds `add` icon."
            },
            "attribute": "add",
            "reflect": true,
            "defaultValue": "false"
        },
        "delete": {
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
                "text": "Adds `delete` icon."
            },
            "attribute": "delete",
            "reflect": true,
            "defaultValue": "false"
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
                "text": "The button icon name."
            },
            "attribute": "icon",
            "reflect": true
        },
        "iconKind": {
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
                "text": "The button icon kind."
            },
            "attribute": "icon-kind",
            "reflect": true
        }
    }; }
    static get states() { return {
        "iconOnly": {}
    }; }
}
