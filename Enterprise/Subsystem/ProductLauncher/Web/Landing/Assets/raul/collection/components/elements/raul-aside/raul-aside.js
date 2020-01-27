import { Host, h } from "@stencil/core";
export class RaulAside {
    constructor() {
        this.visible = false;
        this.expanded = false;
        this.focused = false;
        this.size = 'medium';
    }
    componentDidLoad() {
        this.dialogTransitionDuration = parseFloat(window.getComputedStyle(this.dialogEl).transitionDuration) * 1000;
    }
    handleRaulAsideOpen(e) {
        if (this.el !== e.target) {
            requestAnimationFrame(() => {
                this.dialogWidth = this.dialogEl.offsetWidth;
                this.secondaryDialogWidth = e.target.querySelector('.r-aside__dialog').offsetWidth;
            });
            this.blur();
        }
    }
    handleRaulAsideClose(e) {
        if (this.el !== e.target) {
            this.secondaryDialogWidth = 0;
            this.focus();
        }
    }
    /**
     * Opens the aside.
     * @returns {Promise<void>}
     */
    async open() {
        this.asideTrigger = document.activeElement;
        this.visible = true;
        requestAnimationFrame(() => this.expanded = true);
        this.focus();
        this.raulAsideOpen.emit();
        document.body.classList.add('no-scroll');
    }
    /**
     * Closes the aside.
     * @returns {Promise<void>}
     */
    async close() {
        this.expanded = false;
        setTimeout(() => this.visible = false, this.dialogTransitionDuration);
        this.blur();
        if (this.asideTrigger) {
            this.asideTrigger.focus();
        }
        this.raulAsideClose.emit();
        document.body.classList.remove('no-scroll');
    }
    dialogOffsetX() {
        return -(this.secondaryDialogWidth - this.dialogWidth + 40);
    }
    focus() {
        requestAnimationFrame(() => {
            this.focused = true;
            this.asideEl.focus();
        });
    }
    blur() {
        requestAnimationFrame(() => {
            this.focused = false;
            this.asideEl.blur();
        });
    }
    render() {
        return (h(Host, { visible: this.visible, expanded: this.expanded },
            h("div", { class: "r-aside", role: "dialog", tabindex: "-1", "aria-hidden": !this.visible, onKeyDown: e => e.key === 'Escape' && this.focused ? this.close() : null, ref: (el) => this.asideEl = el },
                h("slot", { name: "secondary-aside" }),
                h("div", { class: "r-aside__backdrop", onClick: () => this.close() }),
                h("div", { class: "r-aside__dialog", role: "document", style: { transform: this.secondaryDialogWidth ? `translateX(${this.dialogOffsetX()}px)` : null }, ref: el => this.dialogEl = el },
                    h("slot", null)))));
    }
    static get is() { return "raul-aside"; }
    static get originalStyleUrls() { return {
        "$": ["raul-aside.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-aside.css"]
    }; }
    static get properties() { return {
        "size": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "'small' | 'medium' | 'large'",
                "resolved": "\"large\" | \"medium\" | \"small\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "size",
            "reflect": true,
            "defaultValue": "'medium'"
        }
    }; }
    static get states() { return {
        "visible": {},
        "expanded": {},
        "focused": {},
        "dialogWidth": {},
        "secondaryDialogWidth": {}
    }; }
    static get events() { return [{
            "method": "raulAsideOpen",
            "name": "raulAsideOpen",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": "Emitted when the aside opens."
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }, {
            "method": "raulAsideClose",
            "name": "raulAsideClose",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": "Emitted when the aside closes."
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }]; }
    static get methods() { return {
        "open": {
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
                "text": "Opens the aside.",
                "tags": [{
                        "name": "returns",
                        "text": undefined
                    }]
            }
        },
        "close": {
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
                "text": "Closes the aside.",
                "tags": [{
                        "name": "returns",
                        "text": undefined
                    }]
            }
        }
    }; }
    static get elementRef() { return "el"; }
    static get listeners() { return [{
            "name": "raulAsideOpen",
            "method": "handleRaulAsideOpen",
            "target": undefined,
            "capture": false,
            "passive": false
        }, {
            "name": "raulAsideClose",
            "method": "handleRaulAsideClose",
            "target": undefined,
            "capture": false,
            "passive": false
        }]; }
}
