'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulListHeader = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h(core.Host, null, core.h("slot", null)));
    }
    static get style() { return "raul-list-header{display:block;border-bottom-width:3px;border-color:#ebedee;padding-top:1rem;padding-bottom:1rem}"; }
};

exports.raul_list_header = RaulListHeader;
