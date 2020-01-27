import { h } from "@stencil/core";
export class RaulPagingBar {
    constructor() {
        this.currentPage = 1;
        this.entities = [10, 20, 30, 40, 50];
        this.totalRows = 0;
        this.rowsPerPage = 10;
    }
    componentDidLoad() {
        this.totalPages = Math.ceil(this.totalRows / this.rowsPerPage);
        this.startRow = ((this.currentPage - 1) * this.rowsPerPage) + 1;
        this.endRow = (((this.currentPage - 1) * this.rowsPerPage) + this.rowsPerPage) > this.totalRows ? this.totalRows : ((this.currentPage - 1) * this.rowsPerPage) + this.rowsPerPage;
        this.validateRowsPerPage();
    }
    decrement() {
        this.currentPage = this.currentPage - 1;
        this.updateBar('previousPage');
    }
    entitesChanged(event) {
        this.rowsPerPage = Number(event.target.value);
        this.currentPage = 1;
        this.updateBar('entitiesChange');
    }
    increment() {
        this.currentPage = this.currentPage + 1;
        this.updateBar('nextPage');
    }
    pageValueChange(event) {
        let val = Number(event.target.value);
        if (val > this.totalPages || val < 1) {
            event.target.value = this.currentPage;
        }
        else {
            this.currentPage = val;
            this.updateBar('pageValueChange');
        }
    }
    updateBar(event) {
        this.totalPages = Math.ceil(this.totalRows / this.rowsPerPage);
        this.startRow = ((this.currentPage - 1) * this.rowsPerPage) + 1;
        let calcEndRow = ((this.currentPage - 1) * this.rowsPerPage) + this.rowsPerPage;
        this.endRow = calcEndRow > this.totalRows ? this.totalRows : calcEndRow;
        if (this.validateRowsPerPage()) {
            this.pagingChange.emit({
                event: event,
                currentPage: this.currentPage,
                rowsPerPage: this.rowsPerPage,
                totalPages: this.totalPages,
                startRow: this.startRow,
                endRow: this.endRow
            });
        }
    }
    validateRowsPerPage() {
        let entitiesArrayToString = this.entities.toString();
        if (!this.entities.includes(this.rowsPerPage)) {
            console.error(`RAUL Paging Bar Component ERROR: rowsPerPage does not match entities array. rowsPerPage is set to ${this.rowsPerPage} but available entities are: ${entitiesArrayToString}`);
        }
        return this.entities.includes(this.rowsPerPage);
    }
    render() {
        return (h("div", { class: "r-paging-bar" },
            h("div", { class: "r-paging-bar__col r-paging-bar__col--entries" },
                "Show",
                h("div", { class: "r-paging-bar__entry-select" },
                    h("select", { onChange: (event) => this.entitesChanged(event) }, this.entities && this.entities.map(entity => (h("option", { value: entity, selected: this.rowsPerPage === entity }, entity)))),
                    h("raul-icon", { class: "r-select__arrow", icon: "arrow-down-v" })),
                "entries"),
            h("div", { class: "r-paging-bar__col r-paging-bar__col--count" },
                this.startRow,
                " - ",
                this.endRow,
                " of ",
                this.totalRows),
            h("div", { class: "r-paging-bar__col r-paging-bar__col--nav" },
                h("button", { type: "button", "aria-label": "Previous Entries", onClick: () => this.decrement(), disabled: this.currentPage === 1 },
                    h("raul-icon", { icon: "arrow-left-v" })),
                h("div", { class: "r-paging-bar__nav" },
                    "Page",
                    h("div", { class: "r-paging-bar__input" },
                        h("input", { type: "number", min: "1", max: this.totalPages, value: this.currentPage, onChange: (event) => this.pageValueChange(event) })),
                    "of ",
                    this.totalPages),
                h("button", { type: "button", "aria-label": "Next Entries", onClick: () => this.increment(), disabled: this.currentPage === this.totalPages },
                    h("raul-icon", { icon: "arrow-right-v" })))));
    }
    static get is() { return "raul-paging-bar"; }
    static get originalStyleUrls() { return {
        "$": ["raul-paging-bar.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-paging-bar.css"]
    }; }
    static get properties() { return {
        "entities": {
            "type": "any",
            "mutable": false,
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "entities",
            "reflect": true,
            "defaultValue": "[10,20,30,40,50]"
        },
        "totalRows": {
            "type": "number",
            "mutable": false,
            "complexType": {
                "original": "number",
                "resolved": "number",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "total-rows",
            "reflect": true,
            "defaultValue": "0"
        },
        "rowsPerPage": {
            "type": "number",
            "mutable": false,
            "complexType": {
                "original": "number",
                "resolved": "number",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "rows-per-page",
            "reflect": true,
            "defaultValue": "10"
        }
    }; }
    static get states() { return {
        "currentPage": {},
        "totalPages": {},
        "startRow": {},
        "endRow": {}
    }; }
    static get events() { return [{
            "method": "pagingChange",
            "name": "pagingChange",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": ""
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }]; }
}
