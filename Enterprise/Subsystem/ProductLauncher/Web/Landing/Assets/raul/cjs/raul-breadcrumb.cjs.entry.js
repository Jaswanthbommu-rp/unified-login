'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulBreadcrumb = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h(core.Host, { role: "listitem" }, core.h("slot", null), core.h("raul-icon", { icon: "arrow-right-v", class: "r-breadcrumb__separator-icon" })));
    }
    static get style() { return "raul-breadcrumb{display:-ms-inline-flexbox;display:inline-flex;-ms-flex-align:center;align-items:center;font-size:.75rem;color:#37474f}raul-breadcrumb .r-breadcrumb__separator-icon{color:#37474f;padding-left:.25rem;padding-right:.25rem}raul-breadcrumb:last-of-type{font-weight:600}raul-breadcrumb:last-of-type .r-breadcrumb__separator-icon{display:none}raul-breadcrumb a{display:-ms-flexbox;display:flex;-ms-flex-align:center;align-items:center;color:#37474f}raul-breadcrumb a:hover{color:#0076cc;text-decoration:none}"; }
};

exports.raul_breadcrumb = RaulBreadcrumb;
