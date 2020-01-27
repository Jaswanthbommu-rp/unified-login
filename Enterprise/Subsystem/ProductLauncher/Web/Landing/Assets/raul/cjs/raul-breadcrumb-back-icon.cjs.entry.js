'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulBreadcrumbBackIcon = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("raul-icon", { icon: "arrow-left-1" }));
    }
    static get style() { return "raul-breadcrumb-back-icon{display:-ms-inline-flexbox;display:inline-flex;font-size:1.25rem;padding-right:.5rem}"; }
};

exports.raul_breadcrumb_back_icon = RaulBreadcrumbBackIcon;
