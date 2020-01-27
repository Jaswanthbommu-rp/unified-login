import { r as registerInstance, h } from './core-9263a98c.js';

const RaulListTitle = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("slot", null));
    }
    static get style() { return "raul-list-title{display:block;font-size:1rem;font-weight:600;line-height:1.25;margin-bottom:.5rem;margin-bottom:0}"; }
};

export { RaulListTitle as raul_list_title };
