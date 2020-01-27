import { r as registerInstance, h } from './core-9263a98c.js';
var RaulGridRow = /** @class */ (function () {
    function RaulGridRow(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulGridRow.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulGridRow, "style", {
        get: function () { return "raul-grid-row{display:-ms-flexbox;display:flex;width:100%}"; },
        enumerable: true,
        configurable: true
    });
    return RaulGridRow;
}());
export { RaulGridRow as raul_grid_row };
