'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulAvatar = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.static = false;
        this.label = null;
        this.hint = null;
        this.value = '0';
        this.color = 'primary';
        this.raulProgressRemove = core.createEvent(this, "raulProgressRemove", 7);
    }
    handleProgressRemove() {
        this.raulProgressRemove.emit();
    }
    render() {
        return (core.h(core.Host, null, this.label &&
            core.h("div", { class: "r-progress__label" }, this.label, !this.static &&
                core.h("button", { type: "button", class: "r-progress__remove-btn", onClick: () => this.handleProgressRemove() }, core.h("raul-icon", { icon: "remove-2" }))), core.h("div", { class: "r-progress__bar" }, core.h("div", { class: "r-progress__bar__value", style: { width: `${this.value}%` } })), core.h("div", { class: "r-progress__text" }, `${this.value}%`, " ", !this.static && this.hint)));
    }
    static get style() { return "raul-progress{display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column}raul-progress .r-progress__label{display:-ms-flexbox;display:flex;-ms-flex-align:end;align-items:flex-end;font-size:.75rem;font-weight:600;margin-bottom:.25rem}raul-progress .r-progress__remove-btn{margin-left:auto}raul-progress .r-progress__bar{background-color:#ebedee;height:.25rem;border-radius:9999px;overflow:hidden;position:relative}raul-progress .r-progress__bar__value{background-color:#0076cc;border-radius:9999px;position:absolute;top:0;left:0;height:100%}raul-progress .r-progress__text{font-size:.75rem;margin-top:.25rem}raul-progress[static]{-ms-flex-direction:row;flex-direction:row;-ms-flex-align:center;align-items:center}raul-progress[static] .r-progress__label{font-weight:400;margin-bottom:0;margin-right:.5rem}raul-progress[static] .r-progress__bar{-ms-flex:0 0 5rem;flex:0 0 5rem}raul-progress[static] .r-progress__text{margin-top:0;margin-left:.5rem}raul-progress[color=warning] .r-progress__bar__value{background-color:#fec12d}raul-progress[color=danger] .r-progress__bar__value{background-color:#d01a1f}raul-progress[color=success] .r-progress__bar__value{background-color:#139c3e}"; }
};

exports.raul_progress = RaulAvatar;
