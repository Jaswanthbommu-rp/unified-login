'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulGridBody = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("slot", null));
    }
    static get style() { return "raul-grid-body{display:-ms-flexbox;display:flex;-ms-flex-wrap:wrap;flex-wrap:wrap;width:100%}"; }
};

exports.raul_grid_body = RaulGridBody;
