'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulListItemSubtitle = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("slot", null));
    }
    static get style() { return "raul-list-item-subtitle{display:block}"; }
};

exports.raul_list_item_subtitle = RaulListItemSubtitle;
