import { Host, h } from "@stencil/core";
export class RaulOption {
    render() {
        return (h(Host, { class: {
                'r-option': true,
                'r-option--multiple': this.multiple,
                'r-option--active': this.focused,
                'r-option--disabled': this.disabled,
                'r-option--selected': this.selected,
                'r-option--group': this.variant === 'group',
                'r-option--heading': this.variant === 'heading',
                'r-option--sub-option': this.variant === 'sub-option',
                'r-option--with-description': !!this.description
            }, role: "option", tabindex: "-1", id: this.optionId, "aria-selected": this.selected ? 'true' : 'false', "aria-disabled": this.disabled ? 'true' : null },
            this.icon && h("raul-icon", { class: "r-option__icon", icon: this.icon, kind: this.iconKind }),
            h("div", { class: "r-option__text" },
                h("div", { class: "r-option__text__title", title: this.text }, this.text),
                this.description && h("div", { class: "r-option__text__description", title: this.description }, this.description)),
            this.multiple
                ? h("raul-checkbox", { small: true },
                    h("input", { type: "checkbox", tabindex: "-1", "aria-hidden": "true", checked: this.selected, disabled: this.disabled, value: this.value, ref: el => el && (el.indeterminate = this.indeterminate) }))
                : h("raul-icon", { icon: "check-mark-success", class: "r-option__check-mark" })));
    }
    static get is() { return "raul-option"; }
    static get properties() { return {
        "multiple": {
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
            "attribute": "multiple",
            "reflect": false
        },
        "selected": {
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
            "attribute": "selected",
            "reflect": false
        },
        "indeterminate": {
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
            "attribute": "indeterminate",
            "reflect": false
        },
        "focused": {
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
            "attribute": "focused",
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
                "text": ""
            },
            "attribute": "disabled",
            "reflect": false
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
                "text": ""
            },
            "attribute": "value",
            "reflect": false
        },
        "text": {
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
            "attribute": "text",
            "reflect": false
        },
        "description": {
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
            "attribute": "description",
            "reflect": false
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
                "text": ""
            },
            "attribute": "icon",
            "reflect": false
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
                "text": ""
            },
            "attribute": "icon-kind",
            "reflect": false
        },
        "optionId": {
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
            "attribute": "option-id",
            "reflect": false
        },
        "variant": {
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
            "attribute": "variant",
            "reflect": false
        }
    }; }
}
