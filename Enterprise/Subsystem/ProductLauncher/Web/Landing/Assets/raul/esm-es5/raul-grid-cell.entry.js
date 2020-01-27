import { r as registerInstance, h } from './core-9263a98c.js';
var RaulGridCell = /** @class */ (function () {
    function RaulGridCell(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulGridCell.prototype.render = function () {
        return (h("div", { class: "r-grid__cell" }, h("slot", null)));
    };
    Object.defineProperty(RaulGridCell, "style", {
        get: function () { return "raul-grid-cell{display:-ms-flexbox;display:flex;-ms-flex-align:center;align-items:center;width:100%;padding-top:.75rem;padding-bottom:.75rem;padding-left:.5rem;padding-right:.5rem;border-bottom-width:1px;border-color:#ebedee}"; },
        enumerable: true,
        configurable: true
    });
    return RaulGridCell;
}());
export { RaulGridCell as raul_grid_cell };
