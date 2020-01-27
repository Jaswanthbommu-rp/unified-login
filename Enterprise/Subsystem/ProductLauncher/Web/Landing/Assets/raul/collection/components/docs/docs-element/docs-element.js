import { h } from "@stencil/core";
export class DocsElement {
    constructor() {
        this.activeTab = "overview";
    }
    render() {
        return (h("div", { class: "docs-element" },
            h("div", { class: "docs-page-header tabbed" },
                h("h1", null, this.title),
                h("raul-tabs", { tabs: [
                        { label: "OVERVIEW", name: "overview" },
                        { label: "DESIGN GUIDELINES", name: "design" },
                        { label: "API", name: "api" },
                    ], "active-tab": this.activeTab, onRaulTabChange: (e) => this.activeTab = e.detail })),
            h("div", { class: "p-4 md:p-12" },
                h("div", { style: { display: this.activeTab === 'overview' ? 'block' : 'none' } },
                    h("slot", { name: "overview" })),
                h("div", { style: { display: this.activeTab === 'design' ? 'block' : 'none' } },
                    h("slot", { name: "design" })),
                h("div", { style: { display: this.activeTab === 'api' ? 'block' : 'none' } },
                    h("slot", { name: "api" })))));
    }
    static get is() { return "docs-element"; }
    static get properties() { return {
        "title": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": true,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "title",
            "reflect": false
        }
    }; }
    static get states() { return {
        "activeTab": {}
    }; }
}
