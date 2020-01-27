import { r as registerInstance, h } from './core-9263a98c.js';

const RaulGridCell = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("div", { class: "r-grid__cell" }, h("slot", null)));
    }
    static get style() { return "raul-grid-cell{display:-ms-flexbox;display:flex;-ms-flex-align:center;align-items:center;width:100%;padding-top:.75rem;padding-bottom:.75rem;padding-left:.5rem;padding-right:.5rem;border-bottom-width:1px;border-color:#ebedee}"; }
};

export { RaulGridCell as raul_grid_cell };
