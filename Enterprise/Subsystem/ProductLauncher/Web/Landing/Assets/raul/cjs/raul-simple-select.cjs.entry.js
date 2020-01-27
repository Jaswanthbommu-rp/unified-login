'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulSimpleSelect = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.inputId = `raul-select-${selectIds++}`;
    }
    render() {
        return (core.h("div", { class: {
                'r-form-element': true,
                'r-form-element--invalid': !!this.error
            } }, this.label && core.h("label", { class: "r-form-element__label", htmlFor: this.inputId }, this.label), core.h("div", { class: "r-form-element__control" }, core.h("slot", null), core.h("raul-icon", { class: "r-form-element__control__arrow", icon: "arrow-down-v" })), this.hint && core.h("small", { class: "r-form-element__hint" }, this.hint), this.error &&
            core.h("div", { class: "r-form-element__error" }, core.h("raul-icon", { icon: "interface-alert-diamond", class: "r-form-element__error__icon" }), " ", this.error)));
    }
    get el() { return core.getElement(this); }
    static get style() { return "raul-simple-select{display:block}raul-simple-select .r-form-element__control{position:relative}raul-simple-select .r-form-element__control select{border-width:1px;border-color:#c6ccd0;display:block;font-size:.875rem;height:2.5rem;padding-left:.75rem;padding-right:2rem;width:100%;-webkit-appearance:none;-moz-appearance:none;appearance:none;background-color:var(--raul-input-bg,#fff);border-radius:var(--raul-input-border-radius,.125rem);color:var(--raul-input-color,#37474f)}raul-simple-select .r-form-element__control select::-webkit-input-placeholder{color:#9ba3a7;opacity:1}raul-simple-select .r-form-element__control select::-moz-placeholder{color:#9ba3a7;opacity:1}raul-simple-select .r-form-element__control select:-ms-input-placeholder{color:#9ba3a7;opacity:1}raul-simple-select .r-form-element__control select::-ms-input-placeholder{color:#9ba3a7;opacity:1}raul-simple-select .r-form-element__control select::placeholder{color:#9ba3a7;opacity:1}raul-simple-select .r-form-element__control select[disabled]{background-color:#ebedee;border-color:#ebedee;color:#9ba3a7;cursor:not-allowed}raul-simple-select .r-form-element__control select:focus{border-color:#0076cc;outline:0}raul-simple-select .r-form-element__control select:invalid{border-color:#d01a1f}raul-simple-select .r-form-element__control select::-ms-expand{display:none}raul-simple-select .r-form-element__control__arrow{position:absolute;font-size:1rem;color:#9ba3a7;right:.75rem;top:50%;-webkit-transform:translateY(-50%);transform:translateY(-50%)}raul-simple-select .r-form-element__label{display:inline-block;font-weight:500;font-size:.75rem;margin-bottom:.25rem}raul-simple-select .r-form-element__hint{display:block;font-size:.75rem;margin-top:.5rem}raul-simple-select .r-form-element__error{display:-ms-flexbox;display:flex;font-size:.75rem;-ms-flex-align:center;align-items:center;color:#d01a1f;margin-top:.5rem}raul-simple-select .r-form-element__error__icon{font-size:1rem;margin-right:.25rem}raul-simple-select .r-form-element--invalid select{border-color:#d01a1f}"; }
};
let selectIds = 0;

exports.raul_simple_select = RaulSimpleSelect;
