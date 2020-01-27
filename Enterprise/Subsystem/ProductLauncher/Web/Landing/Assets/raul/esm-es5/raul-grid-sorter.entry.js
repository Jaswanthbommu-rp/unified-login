import { r as registerInstance, c as createEvent, h } from './core-9263a98c.js';
var RaulGridSorter = /** @class */ (function () {
    function RaulGridSorter(hostRef) {
        registerInstance(this, hostRef);
        this.direction = null;
        this.raulSort = createEvent(this, "raulSort", 7);
    }
    RaulGridSorter.prototype.handleSort = function () {
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
    };
    RaulGridSorter.prototype.render = function () {
        var _this = this;
        return (h("button", { type: "button", class: "r-grid__sorter", onClick: function () { return _this.handleSort(); } }, h("span", { class: {
                'r-grid__sorter__icon': true,
                'r-grid__sorter__icon--ascending': true,
                'r-grid__sorter__icon--active': this.direction === 'ascending'
            } }), h("span", { class: {
                'r-grid__sorter__icon': true,
                'r-grid__sorter__icon--descending': true,
                'r-grid__sorter__icon--active': this.direction === 'descending'
            } })));
    };
    Object.defineProperty(RaulGridSorter, "style", {
        get: function () { return "raul-grid-sorter{display:inline-block;vertical-align:middle}raul-grid-sorter .r-grid__sorter{display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column;margin-left:.25rem;padding-left:.25rem;padding-right:.25rem}raul-grid-sorter .r-grid__sorter__icon{display:block;width:0;height:0;border-left-width:4px;border-right-width:4px;margin-top:1px;margin-bottom:1px;border-left-color:transparent;border-right-color:transparent}raul-grid-sorter .r-grid__sorter__icon--ascending{border-bottom-width:4px;border-bottom-color:#9ba3a7}raul-grid-sorter .r-grid__sorter__icon--ascending.r-grid__sorter__icon--active{border-bottom-color:#0076cc}raul-grid-sorter .r-grid__sorter__icon--descending{border-top-width:4px;border-top-color:#9ba3a7}raul-grid-sorter .r-grid__sorter__icon--descending.r-grid__sorter__icon--active{border-top-color:#0076cc}"; },
        enumerable: true,
        configurable: true
    });
    return RaulGridSorter;
}());
export { RaulGridSorter as raul_grid_sorter };
