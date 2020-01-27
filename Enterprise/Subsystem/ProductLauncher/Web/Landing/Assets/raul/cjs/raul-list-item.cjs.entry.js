'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulListItem = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h(core.Host, { role: "listitem" }, core.h("slot", null)));
    }
    static get style() { return "raul-list-item{display:block;border-bottom-width:1px;border-color:#ebedee;padding-top:1rem;padding-bottom:1rem;font-size:.875rem}"; }
};

exports.raul_list_item = RaulListItem;
