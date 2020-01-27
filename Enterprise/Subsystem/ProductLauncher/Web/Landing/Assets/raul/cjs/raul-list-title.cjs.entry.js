'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulListTitle = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("slot", null));
    }
    static get style() { return "raul-list-title{display:block;font-size:1rem;font-weight:600;line-height:1.25;margin-bottom:.5rem;margin-bottom:0}"; }
};

exports.raul_list_title = RaulListTitle;
