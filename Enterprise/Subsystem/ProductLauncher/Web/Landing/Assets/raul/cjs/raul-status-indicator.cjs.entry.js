'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulStatusIndicator = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.variant = 'default';
    }
    render() {
        return (core.h("div", { class: {
                'status-indicator': true,
                [`status-indicator--${this.variant}`]: this.variant !== 'default'
            } }));
    }
    static get style() { return "raul-status-indicator{display:inline-block}raul-status-indicator .status-indicator{display:inline-block;border-radius:9999px;width:.5rem;height:.5rem;background-color:#ebedee}raul-status-indicator .status-indicator--destructive{background-color:#d01a1f}raul-status-indicator .status-indicator--success{background-color:#139c3e}raul-status-indicator .status-indicator--warning{background-color:#fec12d}"; }
};

exports.raul_status_indicator = RaulStatusIndicator;
