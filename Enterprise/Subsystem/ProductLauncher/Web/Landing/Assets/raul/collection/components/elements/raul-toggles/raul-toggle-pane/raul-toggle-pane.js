import { h } from "@stencil/core";
export class RaulTogglePane {
    render() {
        return (h("div", { class: "r-toggles", role: "tabpanel", id: this.name, "aria-labelledby": `${this.name}-toggle` },
            h("slot", null)));
    }
    static get is() { return "raul-toggle-pane"; }
    static get properties() { return {
        "name": {
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
                "text": "The name of the toggle pane."
            },
            "attribute": "name",
            "reflect": true
        }
    }; }
}
