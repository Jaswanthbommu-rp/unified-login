'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulCardHeaderActions = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("slot", null));
    }
    static get style() { return "raul-card-header-actions{display:block;-ms-flex:0 1 auto;flex:0 1 auto}raul-card-header-actions a,raul-card-header-actions button{display:-ms-inline-flexbox;display:inline-flex;font-size:1.25rem;padding:.25rem}"; }
};

exports.raul_card_header_actions = RaulCardHeaderActions;
