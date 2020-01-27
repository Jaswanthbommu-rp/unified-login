'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulBreadcrumbs = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.overflow = false;
    }
    render() {
        return (core.h(core.Host, { role: "list" }, core.h("slot", null)));
    }
    static get style() { return "raul-breadcrumbs{display:-ms-flexbox;display:flex;-ms-flex-wrap:wrap;flex-wrap:wrap}"; }
};

exports.raul_breadcrumbs = RaulBreadcrumbs;
