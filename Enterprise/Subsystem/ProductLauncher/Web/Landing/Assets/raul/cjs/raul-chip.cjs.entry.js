'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const index = require('./index-1cfd8f32.js');

const RaulChip = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.raulChipRemove = core.createEvent(this, "raulChipRemove", 7);
    }
    componentDidLoad() {
        this.title = this.chipTextEl.textContent;
    }
    handleChipRemove() {
        this.raulChipRemove.emit();
    }
    render() {
        return (core.h("div", { class: "r-chip" }, core.h("div", { class: "r-chip__text", title: this.title, ref: el => this.chipTextEl = el }, core.h("slot", null)), this.removable &&
            core.h("button", { class: "r-chip__remove-button", onClick: this.removable ? () => this.handleChipRemove() : null, onKeyDown: this.removable
                    ? (e) => index.DELETE_KEYS.includes(e.key) ? this.handleChipRemove() : null
                    : null }, core.h("raul-icon", { icon: "close", class: "r-chip__remove-icon" }))));
    }
    static get style() { return "raul-chip{display:inline-block;max-width:100%}raul-chip .r-chip{position:relative;display:-ms-flexbox;display:flex;-ms-flex-align:start;align-items:flex-start;background-color:#e5f4ff;font-size:.75rem;font-weight:500;color:#37474f;text-align:left;line-height:1.25;border-radius:.125rem;padding-left:.5rem;padding-right:.5rem;padding-top:.25rem;padding-bottom:.25rem;max-width:100%}raul-chip .r-chip__remove-button{margin-left:.5rem;margin-top:1px;font-size:.625rem}raul-chip .r-chip__remove-icon{margin:-.25rem;padding:.25rem}"; }
};

exports.raul_chip = RaulChip;
