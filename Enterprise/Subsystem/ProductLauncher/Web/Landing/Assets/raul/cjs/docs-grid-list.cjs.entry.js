'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsIndex = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.data1 = false;
        this.data2 = false;
        this.data3 = false;
        this.data4 = false;
        this.selectAll = false;
        this.selectedItems = 0;
        this.totalRecords = 4;
    }
    componentDidLoad() {
        initPage.initPage('Grid List');
    }
    handlebulkActionsClose() {
        this.selectAll = this.data1 = this.data2 = this.data3 = this.data4 = false;
        this.selectedItems = 0;
    }
    bulkActionsSelectAll() {
        this.selectAll = this.data1 = this.data2 = this.data3 = this.data4 = true;
        this.selectedItems = this.totalRecords;
    }
    onDataChange(e) {
        let target = e.target.name;
        switch (target) {
            case 'data1': {
                if (this.data1) {
                    this.data1 = false;
                    this.selectedItems = this.selectedItems - 1;
                }
                else {
                    this.data1 = true;
                    this.selectedItems = this.selectedItems + 1;
                }
                break;
            }
            case 'data2': {
                if (this.data2) {
                    this.data2 = false;
                    this.selectedItems = this.selectedItems - 1;
                }
                else {
                    this.data2 = true;
                    this.selectedItems = this.selectedItems + 1;
                }
                break;
            }
            case 'data3': {
                if (this.data3) {
                    this.data3 = false;
                    this.selectedItems = this.selectedItems - 1;
                }
                else {
                    this.data3 = true;
                    this.selectedItems = this.selectedItems + 1;
                }
                break;
            }
            case 'data4': {
                if (this.data4) {
                    this.data4 = false;
                    this.selectedItems = this.selectedItems - 1;
                }
                else {
                    this.data4 = true;
                    this.selectedItems = this.selectedItems + 1;
                }
                break;
            }
            case 'selectAll': {
                if (this.selectAll) {
                    this.data1 = this.data2 = this.data3 = this.data4 = false;
                    this.selectedItems = 0;
                }
                else {
                    this.data1 = this.data2 = this.data3 = this.data4 = true;
                    this.selectedItems = 4;
                }
                break;
            }
        }
        let bulkActionBar = document.querySelector('raul-bulk-action-bar');
        bulkActionBar.open = (this.data1 || this.data2 || this.data3 || this.data4) ? true : false;
        this.selectAll = this.data1 && this.data2 && this.data3 && this.data4 ? true : false;
    }
    render() {
        return (core.h("div", { class: "docs-page-container" }, core.h("div", { class: "docs-page-header" }, core.h("docs-markdown", null, `
                 # Grid List
            `)), core.h("div", { class: "docs-page-content" }, core.h("docs-markdown", null, `
              ## Simple Grid
          `), core.h("div", { class: "flex mb-4" }, core.h("div", { class: "w-2/3" }, core.h("div", { class: "r-grid-list r-grid-list--striped" }, core.h("div", { class: "r-grid-list__row" }, core.h("div", { class: "w-1/4 r-grid-list__cell " }, "First Name"), core.h("div", { class: "r-grid-list__cell" }, core.h("span", { class: "font-bold" }, "Robert"))), core.h("div", { class: "r-grid-list__row" }, core.h("div", { class: "w-1/4 r-grid-list__cell" }, "Last Name"), core.h("div", { class: "r-grid-list__cell" }, core.h("span", { class: "font-bold" }, "Page"))), core.h("div", { class: "r-grid-list__row" }, core.h("div", { class: "w-1/4 r-grid-list__cell" }, "Email"), core.h("div", { class: "r-grid-list__cell" }, core.h("span", { class: "font-bold" }, "r.page@realpage.com"))), core.h("div", { class: "r-grid-list__row" }, core.h("div", { class: "w-1/4 r-grid-list__cell" }, "Phone"), core.h("div", { class: "r-grid-list__cell" }, core.h("span", { class: "font-bold" }, "972.000.1111"))), core.h("div", { class: "r-grid-list__row" }, core.h("div", { class: "w-1/4 r-grid-list__cell" }, "Address"), core.h("div", { class: "r-grid-list__cell" }, core.h("span", { class: "font-bold" }, "1234 East Henderson Ave")))))), core.h("docs-markdown", null, `
              ## Grid with selection, striping and highlighting w/ Paging and Bulk Action Behaviors 
          `), core.h("raul-bulk-action-bar", { totalRecords: this.totalRecords, selectedCount: this.selectedItems }, core.h("raul-button", { variant: "reverse", size: "small" }, "Action 1"), core.h("raul-button", { variant: "reverse", size: "small" }, "Action 2"), core.h("raul-button", { variant: "reverse", size: "small" }, "Action 3"), core.h("raul-button", { variant: "reverse", size: "small" }, "Action 4"), core.h("raul-button", { variant: "reverse", size: "small" }, "Action 5")), core.h("div", { class: "r-grid-list r-grid-list--striped r-grid-list--highlight-hover" }, core.h("div", { class: "r-grid-list__header" }, core.h("div", { class: "flex-1 r-grid-list__selector" }, core.h("raul-checkbox", { small: true }, core.h("input", { type: "checkbox", name: "selectAll", value: "selectAll", checked: this.selectAll, onChange: () => this.onDataChange(event) }))), core.h("div", { class: "flex-1" }, core.h("div", { class: "flex" }, core.h("div", { class: "w-1/4 r-grid-list__cell" }, core.h("div", { class: "truncate" }, "Column")), core.h("div", { class: "w-1/4 r-grid-list__cell" }, core.h("div", { class: "truncate" }, "Column")), core.h("div", { class: "w-1/4 r-grid-list__cell" }, core.h("div", { class: "truncate" }, "Column")), core.h("div", { class: "w-1/4 r-grid-list__cell" }, core.h("div", { class: "truncate" }, "Column"))))), core.h("div", { class: {
                'r-grid-list__row': true,
                'r-grid-list__row--highlighted': this.data1
            } }, core.h("div", { class: "flex-1 r-grid-list__selector" }, core.h("raul-checkbox", { small: true }, core.h("input", { type: "checkbox", name: "data1", value: "selectAll", checked: this.data1, onChange: () => this.onDataChange(event) }))), core.h("div", { class: "flex-1" }, core.h("div", { class: "flex" }, core.h("div", { class: "w-1/4 r-grid-list__cell" }, core.h("a", { href: "#" }, "Data link")), core.h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"), core.h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"), core.h("div", { class: "w-1/4 r-grid-list__cell" }, "Data")))), core.h("div", { class: {
                'r-grid-list__row': true,
                'r-grid-list__row--highlighted': this.data2
            } }, core.h("div", { class: "flex-1 r-grid-list__selector" }, core.h("raul-checkbox", { small: true }, core.h("input", { type: "checkbox", name: "data2", value: "selectAll", checked: this.data2, onChange: () => this.onDataChange(event) }))), core.h("div", { class: "flex-1" }, core.h("div", { class: "flex" }, core.h("div", { class: "w-1/4 r-grid-list__cell" }, core.h("a", { href: "#" }, "Data link")), core.h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"), core.h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"), core.h("div", { class: "w-1/4 r-grid-list__cell" }, "Data")))), core.h("div", { class: {
                'r-grid-list__row': true,
                'r-grid-list__row--highlighted': this.data3
            } }, core.h("div", { class: "flex-1 r-grid-list__selector" }, core.h("raul-checkbox", { small: true }, core.h("input", { type: "checkbox", name: "data3", value: "selectAll", checked: this.data3, onChange: () => this.onDataChange(event) }))), core.h("div", { class: "flex-1" }, core.h("div", { class: "flex" }, core.h("div", { class: "w-1/4 r-grid-list__cell" }, core.h("a", { href: "#" }, "Data link")), core.h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"), core.h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"), core.h("div", { class: "w-1/4 r-grid-list__cell" }, "Data")))), core.h("div", { class: {
                'r-grid-list__row': true,
                'r-grid-list__row--highlighted': this.data4
            } }, core.h("div", { class: "flex-1 r-grid-list__selector" }, core.h("raul-checkbox", { small: true }, core.h("input", { type: "checkbox", name: "data4", value: "selectAll", checked: this.data4, onChange: () => this.onDataChange(event) }))), core.h("div", { class: "flex-1" }, core.h("div", { class: "flex" }, core.h("div", { class: "w-1/4 r-grid-list__cell" }, core.h("a", { href: "#" }, "Data link")), core.h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"), core.h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"), core.h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"))))), core.h("raul-paging-bar", { totalRows: this.totalRecords }), core.h("div", { class: "mt-4" }), core.h("docs-markdown", null, `
              ## Grid with two lines of content and actions
          `), core.h("div", { class: "r-grid-list mb-4" }, core.h("div", { class: "r-grid-list__header" }, core.h("div", { class: "flex1 r-grid-list__selector" }, core.h("raul-checkbox", { small: true }, core.h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))), core.h("div", { class: "flex-1" }, core.h("div", { class: "flex" }, core.h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, core.h("div", { class: "r-grid-list__header-sorter r-grid-list__header-sorter--asc-sort" }, "Column")), core.h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, core.h("div", { class: "r-grid-list__header-sorter" }, "Column")), core.h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, core.h("div", { class: "r-grid-list__header-sorter" }, "Column")), core.h("div", { class: "hidden md:flex md:w-1/4 r-grid-list__cell" }))), core.h("div", { class: "flex-1 r-grid-list__action-menu" })), core.h("div", { class: "r-grid-list__row" }, core.h("div", { class: "flex-1 r-grid-list__selector" }, core.h("raul-checkbox", { small: true }, core.h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))), core.h("div", { class: "flex-1" }, core.h("div", { class: "flex" }, core.h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, core.h("div", { class: "r-grid-list__data-block" }, core.h("a", { class: "font-bold", href: "#" }, "Jeff Smith"), core.h("br", null), "273 Jefferson St. Richardson, TX 87549")), core.h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, core.h("div", { class: "r-grid-list__data-block" }, core.h("span", { class: "font-bold" }, "First Line Text"), core.h("br", null), "Secondary Text")), core.h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, "Data"), core.h("div", { class: "hidden md:flex w-1/4 r-grid-list__cell r-grid-list__cell--actions" }, core.h("raul-icon", { icon: "file-copy" }), core.h("raul-icon", { icon: "data-download-8" })))), core.h("div", { class: "flex-1 r-grid-list__action-menu" }, core.h("raul-action-menu", { items: [{ title: 'Action 1', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }] }))), core.h("div", { class: "r-grid-list__row" }, core.h("div", { class: "flex-1 r-grid-list__selector" }, core.h("raul-checkbox", { small: true }, core.h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))), core.h("div", { class: "flex-1" }, core.h("div", { class: "flex" }, core.h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, core.h("div", { class: "r-grid-list__data-block" }, core.h("a", { class: "font-bold", href: "#" }, "Jeff Smith"), core.h("br", null), "273 Jefferson St. Richardson, TX 87549")), core.h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, core.h("div", { class: "r-grid-list__data-block" }, core.h("span", { class: "font-bold" }, "First Line Text"), core.h("br", null), "Secondary Text")), core.h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, "Data"), core.h("div", { class: "hidden md:flex md:w-1/4 r-grid-list__cell r-grid-list__cell--actions" }, core.h("raul-icon", { icon: "file-copy" }), core.h("raul-icon", { icon: "data-download-8" })))), core.h("div", { class: "flex-1 r-grid-list__action-menu" }, core.h("raul-action-menu", { items: [{ title: 'Action 1', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }] }))), core.h("div", { class: "r-grid-list__row" }, core.h("div", { class: "flex-1 r-grid-list__selector" }, core.h("raul-checkbox", { small: true }, core.h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))), core.h("div", { class: "flex-1" }, core.h("div", { class: "flex" }, core.h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, core.h("div", { class: "r-grid-list__data-block" }, core.h("a", { class: "font-bold", href: "#" }, "Jeff Smith"), core.h("br", null), "273 Jefferson St. Richardson, TX 87549")), core.h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, core.h("div", { class: "r-grid-list__data-block" }, core.h("span", { class: "font-bold" }, "First Line Text"), core.h("br", null), "Secondary Text")), core.h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, "Data"), core.h("div", { class: "hidden md:flex md:w-1/4 r-grid-list__cell r-grid-list__cell--actions" }, core.h("raul-icon", { icon: "file-copy" }), core.h("raul-icon", { icon: "data-download-8" })))), core.h("div", { class: "flex-1 r-grid-list__action-menu" }, core.h("raul-action-menu", { items: [{ title: 'Action 1', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }] }))), core.h("div", { class: "r-grid-list__row" }, core.h("div", { class: "flex-1 r-grid-list__selector" }, core.h("raul-checkbox", { small: true }, core.h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))), core.h("div", { class: "flex-1" }, core.h("div", { class: "flex" }, core.h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, core.h("div", { class: "r-grid-list__data-block" }, core.h("a", { class: "font-bold", href: "#" }, "Jeff Smith"), core.h("br", null), "273 Jefferson St. Richardson, TX 87549")), core.h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, core.h("div", { class: "r-grid-list__data-block" }, core.h("span", { class: "font-bold" }, "First Line Text"), core.h("br", null), "Secondary Text")), core.h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, "Data"), core.h("div", { class: "hidden md:flex md:w-1/4 r-grid-list__cell r-grid-list__cell--actions" }, core.h("raul-icon", { icon: "file-copy" }), core.h("raul-icon", { icon: "data-download-8" })))), core.h("div", { class: "flex-1 r-grid-list__action-menu" }, core.h("raul-action-menu", { items: [{ title: 'Action 1', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }] })))), core.h("docs-markdown", null, `
              ## HTML Table with grid styles
          `), core.h("table", { class: "r-grid-list r-grid-list--striped r-grid-list--highlight-hover mb-4" }, core.h("thead", null, core.h("tr", { class: "r-grid-list__header" }, core.h("th", { class: "r-grid-list__cell" }, core.h("div", { class: "r-grid-list__header-sorter r-grid-list__header-sorter--asc-sort" }, "Column")), core.h("th", { class: "r-grid-list__cell" }, "Column"), core.h("th", { class: "r-grid-list__cell" }, "Column"), core.h("th", { class: "r-grid-list__cell" }, "Column"))), core.h("tbody", null, core.h("tr", { class: "r-grid-list__row" }, core.h("td", { class: "r-grid-list__cell" }, "Data"), core.h("td", { class: "r-grid-list__cell" }, "Data"), core.h("td", { class: "r-grid-list__cell" }, "Data"), core.h("td", { class: "r-grid-list__cell" }, "Data")), core.h("tr", { class: "r-grid-list__row" }, core.h("td", { class: "r-grid-list__cell" }, "Data"), core.h("td", { class: "r-grid-list__cell" }, "Data"), core.h("td", { class: "r-grid-list__cell" }, "Data"), core.h("td", { class: "r-grid-list__cell" }, "Data")), core.h("tr", { class: "r-grid-list__row" }, core.h("td", { class: "r-grid-list__cell" }, "Data"), core.h("td", { class: "r-grid-list__cell" }, "Data"), core.h("td", { class: "r-grid-list__cell" }, "Data"), core.h("td", { class: "r-grid-list__cell" }, "Data")))), core.h("docs-markdown", null, `
              ## Optional Responsive Card mode
          `), core.h("div", { class: "r-grid-list r-grid-list--responsive-card mb-4" }, core.h("div", { class: "r-grid-list__header" }, core.h("div", { class: "flex-1 r-grid-list__selector" }, core.h("raul-checkbox", { small: true }, core.h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))), core.h("div", { class: "flex-1" }, core.h("div", { class: "flex" }, core.h("div", { class: "w-1/4 r-grid-list__cell" }, core.h("div", { class: "r-grid-list__header-sorter r-grid-list__header-sorter--asc-sort" }, "Column")), core.h("div", { class: "w-1/4 r-grid-list__cell" }, core.h("div", { class: "r-grid-list__header-sorter" }, "Column")), core.h("div", { class: "w-1/4 r-grid-list__cell" }, core.h("div", { class: "r-grid-list__header-sorter" }, "Column")), core.h("div", { class: "w-1/4 r-grid-list__cell" }, "Column"))), core.h("div", { class: "flex-1 r-grid-list__action-menu" })), core.h("div", { class: "r-grid-list__row" }, core.h("div", { class: "flex-1 r-grid-list__selector" }, core.h("raul-checkbox", { small: true }, core.h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))), core.h("div", { class: "flex-1" }, core.h("div", { class: "flex" }, core.h("div", { class: "w-1/4 r-grid-list__cell" }, core.h("a", { href: "#" }, "Data link")), core.h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"), core.h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"), core.h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"))), core.h("div", { class: "flex-1 r-grid-list__action-menu" }, core.h("raul-action-menu", { items: [{ title: 'Action 1', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }] }))), core.h("div", { class: "r-grid-list__row" }, core.h("div", { class: "flex-1 r-grid-list__selector" }, core.h("raul-checkbox", { small: true }, core.h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))), core.h("div", { class: "flex-1" }, core.h("div", { class: "flex" }, core.h("div", { class: "w-1/4 r-grid-list__cell" }, core.h("a", { href: "#" }, "Data link")), core.h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"), core.h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"), core.h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"))), core.h("div", { class: "flex-1 r-grid-list__action-menu" }, core.h("raul-action-menu", { items: [{ title: 'Action 1', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }] }))), core.h("div", { class: "r-grid-list__row" }, core.h("div", { class: "flex-1 r-grid-list__selector" }, core.h("raul-checkbox", { small: true }, core.h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))), core.h("div", { class: "flex-1" }, core.h("div", { class: "flex" }, core.h("div", { class: "w-1/4 r-grid-list__cell" }, core.h("a", { href: "#" }, "Data link")), core.h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"), core.h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"), core.h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"))), core.h("div", { class: "flex-1 r-grid-list__action-menu" }, core.h("raul-action-menu", { items: [{ title: 'Action 1', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }] }))), core.h("div", { class: "r-grid-list__row" }, core.h("div", { class: "flex-1 r-grid-list__selector" }, core.h("raul-checkbox", { small: true }, core.h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))), core.h("div", { class: "flex-1" }, core.h("div", { class: "flex" }, core.h("div", { class: "w-1/4 r-grid-list__cell" }, core.h("a", { href: "#" }, "Data link")), core.h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"), core.h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"), core.h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"))), core.h("div", { class: "flex-1 r-grid-list__action-menu" }, core.h("raul-action-menu", { items: [{ title: 'Action 1', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }] })))))));
    }
};

exports.docs_grid_list = DocsIndex;
