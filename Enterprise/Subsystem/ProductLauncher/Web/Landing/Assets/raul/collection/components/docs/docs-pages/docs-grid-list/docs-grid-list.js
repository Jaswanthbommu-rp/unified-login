import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsIndex {
    constructor() {
        this.data1 = false;
        this.data2 = false;
        this.data3 = false;
        this.data4 = false;
        this.selectAll = false;
        this.selectedItems = 0;
        this.totalRecords = 4;
    }
    componentDidLoad() {
        initPage('Grid List');
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
        return (h("div", { class: "docs-page-container" },
            h("div", { class: "docs-page-header" },
                h("docs-markdown", null, `
                 # Grid List
            `)),
            h("div", { class: "docs-page-content" },
                h("docs-markdown", null, `
              ## Simple Grid
          `),
                h("div", { class: "flex mb-4" },
                    h("div", { class: "w-2/3" },
                        h("div", { class: "r-grid-list r-grid-list--striped" },
                            h("div", { class: "r-grid-list__row" },
                                h("div", { class: "w-1/4 r-grid-list__cell " }, "First Name"),
                                h("div", { class: "r-grid-list__cell" },
                                    h("span", { class: "font-bold" }, "Robert"))),
                            h("div", { class: "r-grid-list__row" },
                                h("div", { class: "w-1/4 r-grid-list__cell" }, "Last Name"),
                                h("div", { class: "r-grid-list__cell" },
                                    h("span", { class: "font-bold" }, "Page"))),
                            h("div", { class: "r-grid-list__row" },
                                h("div", { class: "w-1/4 r-grid-list__cell" }, "Email"),
                                h("div", { class: "r-grid-list__cell" },
                                    h("span", { class: "font-bold" }, "r.page@realpage.com"))),
                            h("div", { class: "r-grid-list__row" },
                                h("div", { class: "w-1/4 r-grid-list__cell" }, "Phone"),
                                h("div", { class: "r-grid-list__cell" },
                                    h("span", { class: "font-bold" }, "972.000.1111"))),
                            h("div", { class: "r-grid-list__row" },
                                h("div", { class: "w-1/4 r-grid-list__cell" }, "Address"),
                                h("div", { class: "r-grid-list__cell" },
                                    h("span", { class: "font-bold" }, "1234 East Henderson Ave")))))),
                h("docs-markdown", null, `
              ## Grid with selection, striping and highlighting w/ Paging and Bulk Action Behaviors 
          `),
                h("raul-bulk-action-bar", { totalRecords: this.totalRecords, selectedCount: this.selectedItems },
                    h("raul-button", { variant: "reverse", size: "small" }, "Action 1"),
                    h("raul-button", { variant: "reverse", size: "small" }, "Action 2"),
                    h("raul-button", { variant: "reverse", size: "small" }, "Action 3"),
                    h("raul-button", { variant: "reverse", size: "small" }, "Action 4"),
                    h("raul-button", { variant: "reverse", size: "small" }, "Action 5")),
                h("div", { class: "r-grid-list r-grid-list--striped r-grid-list--highlight-hover" },
                    h("div", { class: "r-grid-list__header" },
                        h("div", { class: "flex-1 r-grid-list__selector" },
                            h("raul-checkbox", { small: true },
                                h("input", { type: "checkbox", name: "selectAll", value: "selectAll", checked: this.selectAll, onChange: () => this.onDataChange(event) }))),
                        h("div", { class: "flex-1" },
                            h("div", { class: "flex" },
                                h("div", { class: "w-1/4 r-grid-list__cell" },
                                    h("div", { class: "truncate" }, "Column")),
                                h("div", { class: "w-1/4 r-grid-list__cell" },
                                    h("div", { class: "truncate" }, "Column")),
                                h("div", { class: "w-1/4 r-grid-list__cell" },
                                    h("div", { class: "truncate" }, "Column")),
                                h("div", { class: "w-1/4 r-grid-list__cell" },
                                    h("div", { class: "truncate" }, "Column"))))),
                    h("div", { class: {
                            'r-grid-list__row': true,
                            'r-grid-list__row--highlighted': this.data1
                        } },
                        h("div", { class: "flex-1 r-grid-list__selector" },
                            h("raul-checkbox", { small: true },
                                h("input", { type: "checkbox", name: "data1", value: "selectAll", checked: this.data1, onChange: () => this.onDataChange(event) }))),
                        h("div", { class: "flex-1" },
                            h("div", { class: "flex" },
                                h("div", { class: "w-1/4 r-grid-list__cell" },
                                    h("a", { href: "#" }, "Data link")),
                                h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"),
                                h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"),
                                h("div", { class: "w-1/4 r-grid-list__cell" }, "Data")))),
                    h("div", { class: {
                            'r-grid-list__row': true,
                            'r-grid-list__row--highlighted': this.data2
                        } },
                        h("div", { class: "flex-1 r-grid-list__selector" },
                            h("raul-checkbox", { small: true },
                                h("input", { type: "checkbox", name: "data2", value: "selectAll", checked: this.data2, onChange: () => this.onDataChange(event) }))),
                        h("div", { class: "flex-1" },
                            h("div", { class: "flex" },
                                h("div", { class: "w-1/4 r-grid-list__cell" },
                                    h("a", { href: "#" }, "Data link")),
                                h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"),
                                h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"),
                                h("div", { class: "w-1/4 r-grid-list__cell" }, "Data")))),
                    h("div", { class: {
                            'r-grid-list__row': true,
                            'r-grid-list__row--highlighted': this.data3
                        } },
                        h("div", { class: "flex-1 r-grid-list__selector" },
                            h("raul-checkbox", { small: true },
                                h("input", { type: "checkbox", name: "data3", value: "selectAll", checked: this.data3, onChange: () => this.onDataChange(event) }))),
                        h("div", { class: "flex-1" },
                            h("div", { class: "flex" },
                                h("div", { class: "w-1/4 r-grid-list__cell" },
                                    h("a", { href: "#" }, "Data link")),
                                h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"),
                                h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"),
                                h("div", { class: "w-1/4 r-grid-list__cell" }, "Data")))),
                    h("div", { class: {
                            'r-grid-list__row': true,
                            'r-grid-list__row--highlighted': this.data4
                        } },
                        h("div", { class: "flex-1 r-grid-list__selector" },
                            h("raul-checkbox", { small: true },
                                h("input", { type: "checkbox", name: "data4", value: "selectAll", checked: this.data4, onChange: () => this.onDataChange(event) }))),
                        h("div", { class: "flex-1" },
                            h("div", { class: "flex" },
                                h("div", { class: "w-1/4 r-grid-list__cell" },
                                    h("a", { href: "#" }, "Data link")),
                                h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"),
                                h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"),
                                h("div", { class: "w-1/4 r-grid-list__cell" }, "Data"))))),
                h("raul-paging-bar", { totalRows: this.totalRecords }),
                h("div", { class: "mt-4" }),
                h("docs-markdown", null, `
              ## Grid with two lines of content and actions
          `),
                h("div", { class: "r-grid-list mb-4" },
                    h("div", { class: "r-grid-list__header" },
                        h("div", { class: "flex1 r-grid-list__selector" },
                            h("raul-checkbox", { small: true },
                                h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))),
                        h("div", { class: "flex-1" },
                            h("div", { class: "flex" },
                                h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" },
                                    h("div", { class: "r-grid-list__header-sorter r-grid-list__header-sorter--asc-sort" }, "Column")),
                                h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" },
                                    h("div", { class: "r-grid-list__header-sorter" }, "Column")),
                                h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" },
                                    h("div", { class: "r-grid-list__header-sorter" }, "Column")),
                                h("div", { class: "hidden md:flex md:w-1/4 r-grid-list__cell" }))),
                        h("div", { class: "flex-1 r-grid-list__action-menu" })),
                    h("div", { class: "r-grid-list__row" },
                        h("div", { class: "flex-1 r-grid-list__selector" },
                            h("raul-checkbox", { small: true },
                                h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))),
                        h("div", { class: "flex-1" },
                            h("div", { class: "flex" },
                                h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" },
                                    h("div", { class: "r-grid-list__data-block" },
                                        h("a", { class: "font-bold", href: "#" }, "Jeff Smith"),
                                        h("br", null),
                                        "273 Jefferson St. Richardson, TX 87549")),
                                h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" },
                                    h("div", { class: "r-grid-list__data-block" },
                                        h("span", { class: "font-bold" }, "First Line Text"),
                                        h("br", null),
                                        "Secondary Text")),
                                h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, "Data"),
                                h("div", { class: "hidden md:flex w-1/4 r-grid-list__cell r-grid-list__cell--actions" },
                                    h("raul-icon", { icon: "file-copy" }),
                                    h("raul-icon", { icon: "data-download-8" })))),
                        h("div", { class: "flex-1 r-grid-list__action-menu" },
                            h("raul-action-menu", { items: [{ title: 'Action 1', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }] }))),
                    h("div", { class: "r-grid-list__row" },
                        h("div", { class: "flex-1 r-grid-list__selector" },
                            h("raul-checkbox", { small: true },
                                h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))),
                        h("div", { class: "flex-1" },
                            h("div", { class: "flex" },
                                h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" },
                                    h("div", { class: "r-grid-list__data-block" },
                                        h("a", { class: "font-bold", href: "#" }, "Jeff Smith"),
                                        h("br", null),
                                        "273 Jefferson St. Richardson, TX 87549")),
                                h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" },
                                    h("div", { class: "r-grid-list__data-block" },
                                        h("span", { class: "font-bold" }, "First Line Text"),
                                        h("br", null),
                                        "Secondary Text")),
                                h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, "Data"),
                                h("div", { class: "hidden md:flex md:w-1/4 r-grid-list__cell r-grid-list__cell--actions" },
                                    h("raul-icon", { icon: "file-copy" }),
                                    h("raul-icon", { icon: "data-download-8" })))),
                        h("div", { class: "flex-1 r-grid-list__action-menu" },
                            h("raul-action-menu", { items: [{ title: 'Action 1', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }] }))),
                    h("div", { class: "r-grid-list__row" },
                        h("div", { class: "flex-1 r-grid-list__selector" },
                            h("raul-checkbox", { small: true },
                                h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))),
                        h("div", { class: "flex-1" },
                            h("div", { class: "flex" },
                                h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" },
                                    h("div", { class: "r-grid-list__data-block" },
                                        h("a", { class: "font-bold", href: "#" }, "Jeff Smith"),
                                        h("br", null),
                                        "273 Jefferson St. Richardson, TX 87549")),
                                h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" },
                                    h("div", { class: "r-grid-list__data-block" },
                                        h("span", { class: "font-bold" }, "First Line Text"),
                                        h("br", null),
                                        "Secondary Text")),
                                h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, "Data"),
                                h("div", { class: "hidden md:flex md:w-1/4 r-grid-list__cell r-grid-list__cell--actions" },
                                    h("raul-icon", { icon: "file-copy" }),
                                    h("raul-icon", { icon: "data-download-8" })))),
                        h("div", { class: "flex-1 r-grid-list__action-menu" },
                            h("raul-action-menu", { items: [{ title: 'Action 1', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }] }))),
                    h("div", { class: "r-grid-list__row" },
                        h("div", { class: "flex-1 r-grid-list__selector" },
                            h("raul-checkbox", { small: true },
                                h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))),
                        h("div", { class: "flex-1" },
                            h("div", { class: "flex" },
                                h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" },
                                    h("div", { class: "r-grid-list__data-block" },
                                        h("a", { class: "font-bold", href: "#" }, "Jeff Smith"),
                                        h("br", null),
                                        "273 Jefferson St. Richardson, TX 87549")),
                                h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" },
                                    h("div", { class: "r-grid-list__data-block" },
                                        h("span", { class: "font-bold" }, "First Line Text"),
                                        h("br", null),
                                        "Secondary Text")),
                                h("div", { class: "w-1/3 md:w-1/4 r-grid-list__cell" }, "Data"),
                                h("div", { class: "hidden md:flex md:w-1/4 r-grid-list__cell r-grid-list__cell--actions" },
                                    h("raul-icon", { icon: "file-copy" }),
                                    h("raul-icon", { icon: "data-download-8" })))),
                        h("div", { class: "flex-1 r-grid-list__action-menu" },
                            h("raul-action-menu", { items: [{ title: 'Action 1', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }] })))),
                h("docs-markdown", null, `
              ## HTML Table with grid styles
          `),
                h("table", { class: "r-grid-list r-grid-list--striped r-grid-list--highlight-hover mb-4" },
                    h("thead", null,
                        h("tr", { class: "r-grid-list__header" },
                            h("th", { class: "r-grid-list__cell" },
                                h("div", { class: "r-grid-list__header-sorter r-grid-list__header-sorter--asc-sort" }, "Column")),
                            h("th", { class: "r-grid-list__cell" }, "Column"),
                            h("th", { class: "r-grid-list__cell" }, "Column"),
                            h("th", { class: "r-grid-list__cell" }, "Column"))),
                    h("tbody", null,
                        h("tr", { class: "r-grid-list__row" },
                            h("td", { class: "r-grid-list__cell" }, "Data"),
                            h("td", { class: "r-grid-list__cell" }, "Data"),
                            h("td", { class: "r-grid-list__cell" }, "Data"),
                            h("td", { class: "r-grid-list__cell" }, "Data")),
                        h("tr", { class: "r-grid-list__row" },
                            h("td", { class: "r-grid-list__cell" }, "Data"),
                            h("td", { class: "r-grid-list__cell" }, "Data"),
                            h("td", { class: "r-grid-list__cell" }, "Data"),
                            h("td", { class: "r-grid-list__cell" }, "Data")),
                        h("tr", { class: "r-grid-list__row" },
                            h("td", { class: "r-grid-list__cell" }, "Data"),
                            h("td", { class: "r-grid-list__cell" }, "Data"),
                            h("td", { class: "r-grid-list__cell" }, "Data"),
                            h("td", { class: "r-grid-list__cell" }, "Data")))),
                h("docs-markdown", null, `
              ## Optional Responsive Card mode
          `),
                h("div", { class: "r-grid-list r-grid-list--responsive-card mb-4" },
                    h("div", { class: "r-grid-list__header" },
                        h("div", { class: "flex-1 r-grid-list__selector" },
                            h("raul-checkbox", { small: true },
                                h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))),
                        h("div", { class: "flex-1" },
                            h("div", { class: "flex" },
                                h("div", { class: "w-1/4 r-grid-list__cell" },
                                    h("div", { class: "r-grid-list__header-sorter r-grid-list__header-sorter--asc-sort" }, "Column")),
                                h("div", { class: "w-1/4 r-grid-list__cell" },
                                    h("div", { class: "r-grid-list__header-sorter" }, "Column")),
                                h("div", { class: "w-1/4 r-grid-list__cell" },
                                    h("div", { class: "r-grid-list__header-sorter" }, "Column")),
                                h("div", { class: "w-1/4 r-grid-list__cell" }, "Column"))),
                        h("div", { class: "flex-1 r-grid-list__action-menu" })),
                    h("div", { class: "r-grid-list__row" },
                        h("div", { class: "flex-1 r-grid-list__selector" },
                            h("raul-checkbox", { small: true },
                                h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))),
                        h("div", { class: "flex-1" },
                            h("div", { class: "flex" },
                                h("div", { class: "w-1/4 r-grid-list__cell" },
                                    h("a", { href: "#" }, "Data link")),
                                h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"),
                                h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"),
                                h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"))),
                        h("div", { class: "flex-1 r-grid-list__action-menu" },
                            h("raul-action-menu", { items: [{ title: 'Action 1', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }] }))),
                    h("div", { class: "r-grid-list__row" },
                        h("div", { class: "flex-1 r-grid-list__selector" },
                            h("raul-checkbox", { small: true },
                                h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))),
                        h("div", { class: "flex-1" },
                            h("div", { class: "flex" },
                                h("div", { class: "w-1/4 r-grid-list__cell" },
                                    h("a", { href: "#" }, "Data link")),
                                h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"),
                                h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"),
                                h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"))),
                        h("div", { class: "flex-1 r-grid-list__action-menu" },
                            h("raul-action-menu", { items: [{ title: 'Action 1', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }] }))),
                    h("div", { class: "r-grid-list__row" },
                        h("div", { class: "flex-1 r-grid-list__selector" },
                            h("raul-checkbox", { small: true },
                                h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))),
                        h("div", { class: "flex-1" },
                            h("div", { class: "flex" },
                                h("div", { class: "w-1/4 r-grid-list__cell" },
                                    h("a", { href: "#" }, "Data link")),
                                h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"),
                                h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"),
                                h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"))),
                        h("div", { class: "flex-1 r-grid-list__action-menu" },
                            h("raul-action-menu", { items: [{ title: 'Action 1', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }] }))),
                    h("div", { class: "r-grid-list__row" },
                        h("div", { class: "flex-1 r-grid-list__selector" },
                            h("raul-checkbox", { small: true },
                                h("input", { type: "checkbox", name: "selectAll", value: "selectAll" }))),
                        h("div", { class: "flex-1" },
                            h("div", { class: "flex" },
                                h("div", { class: "w-1/4 r-grid-list__cell" },
                                    h("a", { href: "#" }, "Data link")),
                                h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"),
                                h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"),
                                h("div", { class: "w-1/4 r-grid-list__cell", "data-column-name": "Column" }, "Data"))),
                        h("div", { class: "flex-1 r-grid-list__action-menu" },
                            h("raul-action-menu", { items: [{ title: 'Action 1', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }, { title: 'Action 3', url: '#', payload: '' }] })))))));
    }
    static get is() { return "docs-grid-list"; }
    static get states() { return {
        "data1": {},
        "data2": {},
        "data3": {},
        "data4": {},
        "selectAll": {},
        "selectedItems": {}
    }; }
    static get listeners() { return [{
            "name": "bulkActionsClose",
            "method": "handlebulkActionsClose",
            "target": undefined,
            "capture": false,
            "passive": false
        }, {
            "name": "bulkActionsSelectAll",
            "method": "bulkActionsSelectAll",
            "target": undefined,
            "capture": false,
            "passive": false
        }]; }
}
