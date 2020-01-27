import { r as registerInstance, h } from './core-9263a98c.js';

const RaulTabPane = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("div", { class: "tabs", role: "tabpanel", id: this.name, "aria-labelledby": `${this.name}-tab` }, h("slot", null)));
    }
};

export { RaulTabPane as raul_tab_pane };
