import { h } from "@stencil/core";
import { DELETE_KEYS } from '../../../utils/constants';
export class RaulChip {
    componentDidLoad() {
        this.title = this.chipTextEl.textContent;
    }
    handleChipRemove() {
        this.raulChipRemove.emit();
    }
    render() {
        return (h("div", { class: "r-chip" },
            h("div", { class: "r-chip__text", title: this.title, ref: el => this.chipTextEl = el },
                h("slot", null)),
            this.removable &&
                h("button", { class: "r-chip__remove-button", onClick: this.removable ? () => this.handleChipRemove() : null, onKeyDown: this.removable
                        ? (e) => DELETE_KEYS.includes(e.key) ? this.handleChipRemove() : null
                        : null },
                    h("raul-icon", { icon: "close", class: "r-chip__remove-icon" }))));
    }
    static get is() { return "raul-chip"; }
    static get originalStyleUrls() { return {
        "$": ["raul-chip.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-chip.css"]
    }; }
    static get properties() { return {
        "removable": {
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
                "text": "Makes the chip removable."
            },
            "attribute": "removable",
            "reflect": false
        }
    }; }
    static get states() { return {
        "title": {}
    }; }
    static get events() { return [{
            "method": "raulChipRemove",
            "name": "raulChipRemove",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": "Emitted when the removable chip is clicked or delete/backspace keys are pressed."
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }]; }
}
