import { r as registerInstance, h } from './core-9263a98c.js';

const RaulGridHeader = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("slot", null));
    }
    static get style() { return "raul-grid-header{display:-ms-flexbox;display:flex;-ms-flex-wrap:wrap;flex-wrap:wrap;width:100%}"; }
};

export { RaulGridHeader as raul_grid_header };
