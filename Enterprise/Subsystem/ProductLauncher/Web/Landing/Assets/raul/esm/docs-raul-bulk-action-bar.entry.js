import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';

const DocsRaulBulkActionBar = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
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
        return (h("docs-element", { title: "Bulk Action Bar" }, h("div", { slot: "overview" }, h("docs-showcase", null, h("button", { onClick: () => this.toggleBulckActionBar(), class: "text-primary" }, "Open Bulk Actions"), h("raul-bulk-action-bar", { totalRecords: 55, selectedCount: 0, open: this.isOpen }, this.buttons && this.buttons.map(button => (h("raul-button", { variant: "reverse", size: "small", onClick: () => this.buttonClick() }, button)))))), h("div", { slot: "design" }, h("docs-readme", { component: "raul-bulk-action-bar" })), h("div", { slot: "api" }, h("docs-preview", { component: "raul-bulk-action-bar", content: "Bulk Action Bar" }), h("docs-interface", { component: "raul-bulk-action-bar" }))));
    }
};

export { DocsRaulBulkActionBar as docs_raul_bulk_action_bar };
