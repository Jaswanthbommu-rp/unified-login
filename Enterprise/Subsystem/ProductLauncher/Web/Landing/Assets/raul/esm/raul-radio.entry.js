import { r as registerInstance, h, g as getElement } from './core-9263a98c.js';

const RaulRadio = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        /**
         * If `true`, the radio border will become red. This can be useful for form validations.
         */
        this.invalid = false;
        /**
         * If `true`, the radio size will be small.
         */
        this.small = false;
    }
    render() {
        return (h("div", { class: {
                'r-radio': true,
                'r-radio--invalid': this.invalid,
                'r-radio--small': this.small
            } }, h("label", { class: "r-radio__label" }, h("slot", null), h("div", { class: "r-radio__label__icon" }), this.labelText &&
            h("div", { class: "r-radio__label__text" }, this.labelText))));
    }
    get el() { return getElement(this); }
    static get style() { return "raul-radio{display:inline-block}raul-radio .r-radio{position:relative}raul-radio .r-radio__label{display:-ms-flexbox;display:flex;cursor:pointer;line-height:20px}raul-radio .r-radio__label input[type=radio]{height:0;opacity:0;position:absolute;width:0;z-index:-1}raul-radio .r-radio__label input[type=radio]:checked~.r-radio__label__icon{background-color:var(--raul-radio-checked-bg-color,#0076cc);border-color:#0076cc}raul-radio .r-radio__label input[type=radio]:checked~.r-radio__label__icon:after{background-color:#fff;border-radius:50%;content:\"\";height:.3em;left:50%;position:absolute;top:50%;-webkit-transform:translate(-50%,-50%);transform:translate(-50%,-50%);width:.3em}raul-radio .r-radio__label input[type=radio]:disabled~.r-radio__label__icon{background-color:#ebedee;border-color:#ebedee}raul-radio .r-radio__label input[type=radio]:disabled:checked~.r-radio__label__icon:after{background-color:#9ba3a7}body[modality] raul-radio .r-radio__label input[type=radio]:focus~.r-radio__label__icon:before{border-radius:var(--raul-radio-border-radius,50%);bottom:-4px;-webkit-box-shadow:0 0 0 1px #0076cc;box-shadow:0 0 0 1px #0076cc;content:\"\";left:-4px;right:-4px;position:absolute;top:-4px}raul-radio .r-radio__label__icon{background-color:#fff;border:1px solid #b8bec1;border-radius:var(--raul-radio-border-radius,50%);font-size:20px;-ms-flex:none;flex:none;height:20px;position:relative;width:20px;-webkit-transition:background-color .15s linear,border-color .15s linear;transition:background-color .15s linear,border-color .15s linear}raul-radio .r-radio__label__text{font-size:14px;margin-left:8px;word-break:break-all}raul-radio .r-radio--small .r-radio__label{line-height:14px}raul-radio .r-radio--small .r-radio__label__icon{font-size:14px;height:14px;width:14px}raul-radio .r-radio--invalid .r-radio__label__icon{border-color:#d01a1f}"; }
};

export { RaulRadio as raul_radio };
