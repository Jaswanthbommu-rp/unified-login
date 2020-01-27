'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulCardSubtitle = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("slot", null));
    }
    static get style() { return "raul-card-subtitle{display:block;font-size:.875rem;line-height:1.25;margin-bottom:.5rem;color:rgba(55,71,79,.8);margin-bottom:0}"; }
};

exports.raul_card_subtitle = RaulCardSubtitle;
