'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulAsideActions = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("slot", null));
    }
    static get style() { return "raul-aside-actions{display:-ms-flexbox;display:flex;-ms-flex-wrap:wrap;flex-wrap:wrap;-ms-flex-pack:end;justify-content:flex-end}"; }
};

const RaulAsideSubtitle = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("slot", null));
    }
    static get style() { return "raul-aside-subtitle{display:block;font-size:.875rem;line-height:1.25;margin-bottom:.5rem;color:rgba(55,71,79,.8);margin-bottom:0;margin-top:.25rem}"; }
};

const RaulAsideTitle = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("slot", null));
    }
    static get style() { return "raul-aside-title{display:block;font-size:1.25rem;font-weight:600;line-height:1.25;margin-bottom:.5rem;margin-bottom:0}"; }
};

exports.raul_aside_actions = RaulAsideActions;
exports.raul_aside_subtitle = RaulAsideSubtitle;
exports.raul_aside_title = RaulAsideTitle;
