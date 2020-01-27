'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulListItemAction = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("slot", null));
    }
    static get style() { return "raul-list-item-action,raul-list-item-action .r-list__item__action{display:-ms-inline-flexbox;display:inline-flex}raul-list-item-action .r-list__item__action{font-size:.875rem;color:#0076cc;padding:.5rem;margin:-.5rem}raul-list-item-action .r-list__item__action raul-icon{font-size:1rem}"; }
};

exports.raul_list_item_action = RaulListItemAction;
