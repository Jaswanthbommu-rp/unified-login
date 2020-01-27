'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulGridCell = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("div", { class: "r-grid__cell" }, core.h("slot", null)));
    }
    static get style() { return "raul-grid-cell{display:-ms-flexbox;display:flex;-ms-flex-align:center;align-items:center;width:100%;padding-top:.75rem;padding-bottom:.75rem;padding-left:.5rem;padding-right:.5rem;border-bottom-width:1px;border-color:#ebedee}"; }
};

exports.raul_grid_cell = RaulGridCell;
