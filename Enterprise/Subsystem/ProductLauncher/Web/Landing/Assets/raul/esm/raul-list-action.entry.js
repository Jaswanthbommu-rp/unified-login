import { r as registerInstance, h } from './core-9263a98c.js';

const RaulListAction = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("slot", null));
    }
    static get style() { return "raul-list-action,raul-list-action .r-list__action{display:-ms-inline-flexbox;display:inline-flex}raul-list-action .r-list__action{font-size:.875rem;color:#0076cc;padding:.5rem;margin:-.5rem}raul-list-action .r-list__action raul-icon{font-size:1rem}"; }
};

export { RaulListAction as raul_list_action };
