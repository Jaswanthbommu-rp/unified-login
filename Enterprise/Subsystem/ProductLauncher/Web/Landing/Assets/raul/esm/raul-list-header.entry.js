import { r as registerInstance, h, H as Host } from './core-9263a98c.js';

const RaulListHeader = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h(Host, null, h("slot", null)));
    }
    static get style() { return "raul-list-header{display:block;border-bottom-width:3px;border-color:#ebedee;padding-top:1rem;padding-bottom:1rem}"; }
};

export { RaulListHeader as raul_list_header };
