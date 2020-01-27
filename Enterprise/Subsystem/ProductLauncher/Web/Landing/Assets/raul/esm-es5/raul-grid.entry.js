import { r as registerInstance, h } from './core-9263a98c.js';
var RaulGrid = /** @class */ (function () {
    function RaulGrid(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulGrid.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulGrid, "style", {
        get: function () { return "raul-grid{display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column;background-color:#fff;font-size:.875rem;color:#37474f}raul-grid raul-grid-header raul-grid-cell{background-color:#f7f8f9;font-size:.75rem;font-weight:700}raul-grid raul-grid-body raul-grid-row[active]{background-color:#e5f4ff}raul-grid[small] raul-grid-body raul-grid-cell{font-size:.75rem}raul-grid[striped] raul-grid-body raul-grid-row:nth-child(2n){background-color:#f7f8f9}raul-grid[hoverable] raul-grid-body raul-grid-row:hover{background-color:#e5f4ff}"; },
        enumerable: true,
        configurable: true
    });
    return RaulGrid;
}());
export { RaulGrid as raul_grid };
