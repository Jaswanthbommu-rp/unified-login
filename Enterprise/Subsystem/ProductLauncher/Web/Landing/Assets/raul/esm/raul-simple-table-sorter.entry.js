import { r as registerInstance, c as createEvent, h } from './core-9263a98c.js';

const RaulSimpleTableSorter = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        this.direction = null;
        this.raulSort = createEvent(this, "raulSort", 7);
    }
    handleSort() {
        if (this.direction === null) {
            this.direction = 'ascending';
        }
        else if (this.direction === 'ascending') {
            this.direction = 'descending';
        }
        else if (this.direction === 'descending') {
            this.direction = null;
        }
        this.raulSort.emit({ direction: this.direction, field: this.field });
    }
    render() {
        return (h("button", { type: "button", class: "r-simple-table__sorter", onClick: () => this.handleSort() }, h("span", { class: {
                'r-simple-table__sorter__icon': true,
                'r-simple-table__sorter__icon--ascending': true,
                'r-simple-table__sorter__icon--active': this.direction === 'ascending'
            } }), h("span", { class: {
                'r-simple-table__sorter__icon': true,
                'r-simple-table__sorter__icon--descending': true,
                'r-simple-table__sorter__icon--active': this.direction === 'descending'
            } })));
    }
    static get style() { return "raul-simple-table-sorter{display:inline-block;vertical-align:middle}raul-simple-table-sorter .r-simple-table__sorter{display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column;margin-left:.25rem;padding-left:.25rem;padding-right:.25rem}raul-simple-table-sorter .r-simple-table__sorter__icon{display:block;width:0;height:0;border-left-width:4px;border-right-width:4px;margin-top:1px;margin-bottom:1px;border-left-color:transparent;border-right-color:transparent}raul-simple-table-sorter .r-simple-table__sorter__icon--ascending{border-bottom-width:4px;border-bottom-color:#9ba3a7}raul-simple-table-sorter .r-simple-table__sorter__icon--ascending.r-simple-table__sorter__icon--active{border-bottom-color:#0076cc}raul-simple-table-sorter .r-simple-table__sorter__icon--descending{border-top-width:4px;border-top-color:#9ba3a7}raul-simple-table-sorter .r-simple-table__sorter__icon--descending.r-simple-table__sorter__icon--active{border-top-color:#0076cc}"; }
};

export { RaulSimpleTableSorter as raul_simple_table_sorter };
