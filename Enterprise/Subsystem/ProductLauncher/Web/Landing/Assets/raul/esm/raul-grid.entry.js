import { r as registerInstance, h } from './core-9263a98c.js';

const RaulGrid = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("slot", null));
    }
    static get style() { return "raul-grid{display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column;background-color:#fff;font-size:.875rem;color:#37474f}raul-grid raul-grid-header raul-grid-cell{background-color:#f7f8f9;font-size:.75rem;font-weight:700}raul-grid raul-grid-body raul-grid-row[active]{background-color:#e5f4ff}raul-grid[small] raul-grid-body raul-grid-cell{font-size:.75rem}raul-grid[striped] raul-grid-body raul-grid-row:nth-child(2n){background-color:#f7f8f9}raul-grid[hoverable] raul-grid-body raul-grid-row:hover{background-color:#e5f4ff}"; }
};

export { RaulGrid as raul_grid };
