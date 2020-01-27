'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulChip = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        /**
         * Status variant.
         */
        this.variant = 'default';
    }
    componentDidLoad() {
        this.title = this.statusTextEl.textContent;
    }
    render() {
        return (core.h("div", { class: {
                'status': true,
                [`status--${this.variant}`]: this.variant !== 'default'
            } }, core.h("div", { class: "status__text", title: this.title, ref: el => this.statusTextEl = el }, core.h("slot", null))));
    }
    static get style() { return "raul-status{display:inline-block;max-width:100%}raul-status .status{position:relative;display:-ms-flexbox;display:flex;background-color:#ebedee;font-size:.75rem;font-weight:500;color:#37474f;text-align:left;line-height:1.25;border-radius:.125rem;padding-left:.5rem;padding-right:.5rem;padding-top:.25rem;padding-bottom:.25rem;max-width:100%}raul-status .status--destructive{background-color:#fae8e9;color:#d01a1f}raul-status .status--success{background-color:#e7f3eb;color:#139c3e}raul-status .status--warning{background-color:#fef8ea;color:#bc8701}"; }
};

exports.raul_status = RaulChip;
