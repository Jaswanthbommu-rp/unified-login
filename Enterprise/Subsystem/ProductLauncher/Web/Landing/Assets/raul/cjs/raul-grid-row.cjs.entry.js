'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulGridRow = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("slot", null));
    }
    static get style() { return "raul-grid-row{display:-ms-flexbox;display:flex;width:100%}"; }
};

exports.raul_grid_row = RaulGridRow;
