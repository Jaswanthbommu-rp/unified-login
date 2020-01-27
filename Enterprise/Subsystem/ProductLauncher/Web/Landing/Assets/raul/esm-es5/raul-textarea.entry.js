import { r as registerInstance, h, g as getElement } from './core-9263a98c.js';
var RaulTextarea = /** @class */ (function () {
    function RaulTextarea(hostRef) {
        registerInstance(this, hostRef);
        this.inputId = "raul-textarea-" + textareaIds++;
    }
    RaulTextarea.prototype.componentDidLoad = function () {
        var textareaEl = this.el.querySelector('textarea');
        if (textareaEl) {
            if (textareaEl.id) {
                this.inputId = textareaEl.id;
            }
            else {
                textareaEl.id = this.inputId;
            }
        }
    };
    RaulTextarea.prototype.render = function () {
        return (h("div", { class: {
                'r-form-element': true,
                'r-form-element--invalid': !!this.error
            } }, this.label && h("label", { class: "r-form-element__label", htmlFor: this.inputId }, this.label), h("div", { class: "r-form-element__control" }, h("slot", null)), this.hint && h("small", { class: "r-form-element__hint" }, this.hint), this.error &&
            h("div", { class: "r-form-element__error" }, h("raul-icon", { icon: "interface-alert-diamond", class: "r-form-element__error__icon" }), " ", this.error)));
    };
    Object.defineProperty(RaulTextarea.prototype, "el", {
        get: function () { return getElement(this); },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(RaulTextarea, "style", {
        get: function () { return "raul-textarea{display:block}raul-textarea .r-form-element__control{position:relative}raul-textarea .r-form-element__control textarea{border-width:1px;border-color:#c6ccd0;display:block;font-size:.875rem;height:5rem;padding-left:.75rem;padding-right:.75rem;padding-top:.5rem;padding-bottom:.5rem;width:100%;background-color:var(--raul-input-bg,#fff);border-radius:var(--raul-input-border-radius,.125rem);color:var(--raul-input-color,#37474f);min-height:2.5rem}raul-textarea .r-form-element__control textarea::-webkit-input-placeholder{color:#c6ccd0}raul-textarea .r-form-element__control textarea::-moz-placeholder{color:#c6ccd0}raul-textarea .r-form-element__control textarea:-ms-input-placeholder{color:#c6ccd0}raul-textarea .r-form-element__control textarea::-ms-input-placeholder{color:#c6ccd0}raul-textarea .r-form-element__control textarea::placeholder{color:#c6ccd0}raul-textarea .r-form-element__control textarea[disabled]{background-color:#ebedee;border-color:#ebedee;color:#9ba3a7;cursor:not-allowed}raul-textarea .r-form-element__control textarea:focus{border-color:#0076cc;outline:0}raul-textarea .r-form-element__control textarea:invalid{border-color:#d01a1f}raul-textarea .r-form-element__label{display:inline-block;font-weight:500;font-size:.75rem;margin-bottom:.25rem}raul-textarea .r-form-element__hint{display:block;font-size:.75rem;margin-top:.5rem}raul-textarea .r-form-element__error{display:-ms-flexbox;display:flex;font-size:.75rem;-ms-flex-align:center;align-items:center;color:#d01a1f;margin-top:.5rem}raul-textarea .r-form-element__error__icon{font-size:1rem;margin-right:.25rem}raul-textarea .r-form-element--invalid textarea{border-color:#d01a1f}"; },
        enumerable: true,
        configurable: true
    });
    return RaulTextarea;
}());
var textareaIds = 0;
export { RaulTextarea as raul_textarea };
