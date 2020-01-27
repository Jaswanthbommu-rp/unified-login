import { r as registerInstance, h } from './core-9263a98c.js';

const RaulGridBody = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("slot", null));
    }
    static get style() { return "raul-grid-body{display:-ms-flexbox;display:flex;-ms-flex-wrap:wrap;flex-wrap:wrap;width:100%}"; }
};

export { RaulGridBody as raul_grid_body };
