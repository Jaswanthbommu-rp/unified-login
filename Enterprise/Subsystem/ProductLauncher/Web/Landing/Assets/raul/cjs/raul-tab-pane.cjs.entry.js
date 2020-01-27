'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulTabPane = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("div", { class: "tabs", role: "tabpanel", id: this.name, "aria-labelledby": `${this.name}-tab` }, core.h("slot", null)));
    }
};

exports.raul_tab_pane = RaulTabPane;
