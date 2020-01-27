import { r as registerInstance, h, H as Host } from './core-9263a98c.js';

const RaulListItem = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h(Host, { role: "listitem" }, h("slot", null)));
    }
    static get style() { return "raul-list-item{display:block;border-bottom-width:1px;border-color:#ebedee;padding-top:1rem;padding-bottom:1rem;font-size:.875rem}"; }
};

export { RaulListItem as raul_list_item };
