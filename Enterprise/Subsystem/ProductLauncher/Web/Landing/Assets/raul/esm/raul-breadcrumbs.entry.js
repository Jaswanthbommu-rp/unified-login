import { r as registerInstance, h, H as Host } from './core-9263a98c.js';

const RaulBreadcrumbs = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        this.overflow = false;
    }
    render() {
        return (h(Host, { role: "list" }, h("slot", null)));
    }
    static get style() { return "raul-breadcrumbs{display:-ms-flexbox;display:flex;-ms-flex-wrap:wrap;flex-wrap:wrap}"; }
};

export { RaulBreadcrumbs as raul_breadcrumbs };
