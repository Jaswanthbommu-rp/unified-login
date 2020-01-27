import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsRaulBulkActionBar {
    constructor() {
        this.buttons = [
            'Action 1',
            'Action 2',
            'Action 3',
            'Action 4',
            'Action 5',
        ];
        this.isOpen = false;
    }
    componentDidLoad() {
        initPage('Bulk Action Bar');
    }
    handlebulkActionsClose() {
        this.isOpen = this.isOpen ? false : true;
    }
    toggleBulckActionBar() {
        this.isOpen = this.isOpen ? false : true;
    }
    buttonClick() {
        console.log('Do a bulk action!');
    }
    render() {
        return (h("docs-element", { title: "Bulk Action Bar" },
            h("div", { slot: "overview" },
                h("docs-showcase", null,
                    h("button", { onClick: () => this.toggleBulckActionBar(), class: "text-primary" }, "Open Bulk Actions"),
                    h("raul-bulk-action-bar", { totalRecords: 55, selectedCount: 0, open: this.isOpen }, this.buttons && this.buttons.map(button => (h("raul-button", { variant: "reverse", size: "small", onClick: () => this.buttonClick() }, button)))))),
            h("div", { slot: "design" },
                h("docs-readme", { component: "raul-bulk-action-bar" })),
            h("div", { slot: "api" },
                h("docs-preview", { component: "raul-bulk-action-bar", content: "Bulk Action Bar" }),
                h("docs-interface", { component: "raul-bulk-action-bar" }))));
    }
    static get is() { return "docs-raul-bulk-action-bar"; }
    static get states() { return {
        "isOpen": {}
    }; }
    static get listeners() { return [{
            "name": "bulkActionsClose",
            "method": "handlebulkActionsClose",
            "target": undefined,
            "capture": false,
            "passive": false
        }]; }
}
