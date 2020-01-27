import { h } from "@stencil/core";
export class RaulTabPane {
    render() {
        return (h("div", { class: "tabs", role: "tabpanel", id: this.name, "aria-labelledby": `${this.name}-tab` },
            h("slot", null)));
    }
    static get is() { return "raul-tab-pane"; }
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
                "text": "The name of the tab pane."
            },
            "attribute": "name",
            "reflect": true
        }
    }; }
}
