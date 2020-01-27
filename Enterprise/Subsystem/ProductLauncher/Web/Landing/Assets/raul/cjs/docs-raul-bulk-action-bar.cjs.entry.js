'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsRaulBulkActionBar = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
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
        initPage.initPage('Bulk Action Bar');
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
        return (core.h("docs-element", { title: "Bulk Action Bar" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("button", { onClick: () => this.toggleBulckActionBar(), class: "text-primary" }, "Open Bulk Actions"), core.h("raul-bulk-action-bar", { totalRecords: 55, selectedCount: 0, open: this.isOpen }, this.buttons && this.buttons.map(button => (core.h("raul-button", { variant: "reverse", size: "small", onClick: () => this.buttonClick() }, button)))))), core.h("div", { slot: "design" }, core.h("docs-readme", { component: "raul-bulk-action-bar" })), core.h("div", { slot: "api" }, core.h("docs-preview", { component: "raul-bulk-action-bar", content: "Bulk Action Bar" }), core.h("docs-interface", { component: "raul-bulk-action-bar" }))));
    }
};

exports.docs_raul_bulk_action_bar = DocsRaulBulkActionBar;
