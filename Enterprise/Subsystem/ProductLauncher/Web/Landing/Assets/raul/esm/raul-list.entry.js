import { r as registerInstance, h } from './core-9263a98c.js';

const RaulList = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("slot", null));
    }
    static get style() { return "raul-list{display:block;background-color:#fff}"; }
};

export { RaulList as raul_list };
