import { r as registerInstance, h } from './core-9263a98c.js';

const RaulGridRow = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("slot", null));
    }
    static get style() { return "raul-grid-row{display:-ms-flexbox;display:flex;width:100%}"; }
};

export { RaulGridRow as raul_grid_row };
