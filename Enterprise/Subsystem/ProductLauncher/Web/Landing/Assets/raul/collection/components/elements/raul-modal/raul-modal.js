import { h } from "@stencil/core";
export class RaulModal {
    constructor() {
        /**
        * A `normal` modal will have a the header and body centered and no close button. A `media` modal will have the header content aligned to the left and a close button.
        */
        this.variant = 'normal';
        /**
        * Determines wether the modal can be closed via clicking away or the `Escape` key
        */
        this.dismissable = true;
    }
    componentDidLoad() {
        this.overlay.focus();
        if (this.dismissable) {
            this.overlay.addEventListener('click', (e) => {
                // @ts-ignore
                if (!this.modal.contains(e.target))
                    this.modalClose.emit();
            }, true);
            window.addEventListener('keydown', e => {
                if (['Esc', 'Escape'].includes(e.key))
                    this.modalClose.emit();
            });
        }
    }
    render() {
        return (h("div", { class: {
                "r-modal__overlay": true,
                "r-modal__overlay--media": this.variant === 'media'
            }, tabindex: "-1", ref: el => this.overlay = el },
            h("div", { ref: el => this.modal = el, class: {
                    "r-modal__container": true,
                    "r-modal__container--normal": this.variant === 'normal',
                    "r-modal__container--media": this.variant === 'media'
                }, role: "dialog", "aria-modal": "true" },
                h("slot", null))));
    }
    static get is() { return "raul-modal"; }
    static get originalStyleUrls() { return {
        "$": ["raul-modal.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-modal.css"]
    }; }
    static get properties() { return {
        "variant": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "'normal' | 'media'",
                "resolved": "\"media\" | \"normal\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "A `normal` modal will have a the header and body centered and no close button. A `media` modal will have the header content aligned to the left and a close button."
            },
            "attribute": "variant",
            "reflect": false,
            "defaultValue": "'normal'"
        },
        "dismissable": {
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
                "text": "Determines wether the modal can be closed via clicking away or the `Escape` key"
            },
            "attribute": "dismissable",
            "reflect": false,
            "defaultValue": "true"
        }
    }; }
    static get events() { return [{
            "method": "modalClose",
            "name": "modalClose",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": "Event emitted when the modal should be closed (because user clicked the close button, clicked away, pressed `Escape` or chose an option)"
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }]; }
}
