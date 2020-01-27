import { r as registerInstance, h } from './core-9263a98c.js';
import { r as ripple } from './index-ac60d12e.js';
var ButtonComponent = /** @class */ (function () {
    function ButtonComponent(hostRef) {
        registerInstance(this, hostRef);
        this.iconOnly = false;
        /**
         * Determines the primary appearance of the button based on its purpose.
         */
        this.variant = "secondary";
        /**
         * Determines the primary appearance of the button based on its purpose.
         */
        this.size = "default";
        /**
         * Controls whether this button is disabled.
         */
        this.disabled = false;
        /**
         * Adds `add` icon.
         */
        this.add = false;
        /**
         * Adds `delete` icon.
         */
        this.delete = false;
    }
    ButtonComponent.prototype.componentDidLoad = function () {
        if (!this.btnTextEl.innerHTML) {
            this.iconOnly = true;
        }
    };
    ButtonComponent.prototype.render = function () {
        var _a;
        var _this = this;
        var renderAddOrDeleteIcon = function () {
            return _this.add ? h("raul-icon", { icon: "add-2" }) : _this.delete ? h("raul-icon", { icon: "remove-2" }) : null;
        };
        var renderIcon = function () {
            return _this.icon && !(_this.add || _this.delete) ? h("raul-icon", { icon: _this.icon, kind: _this.iconKind }) : null;
        };
        var renderButton = function () {
            var ButtonTypeTag = _this.href ? 'a' : _this.type ? 'input' : 'button';
            return (h(ButtonTypeTag, { class: "r-button__element", type: _this.type, href: _this.href && !_this.disabled ? _this.href : null, "aria-disabled": _this.disabled, disabled: _this.disabled, value: _this.value, onMouseDown: function (e) { return ripple(e, _this.focusRingEl); } }));
        };
        return (h("div", { class: (_a = {
                    'r-button': true
                },
                _a["r-button--" + this.variant] = !!this.variant,
                _a["r-button--" + this.size] = !!this.size,
                _a) }, h("div", { class: "r-button__focus-ring", ref: function (el) { return _this.focusRingEl = el; } }), renderButton(), h("div", { class: {
                'r-button__content': true,
                'r-button__content--icon-only': this.iconOnly
            } }, renderAddOrDeleteIcon(), renderIcon(), h("span", { ref: function (el) { return _this.btnTextEl = el; } }, h("slot", null)))));
    };
    Object.defineProperty(ButtonComponent, "style", {
        get: function () { return "raul-button{display:inline-block}raul-button[type=button],raul-button[type=reset],raul-button[type=submit]{-webkit-appearance:none;-moz-appearance:none;appearance:none}raul-button .r-button{position:relative}raul-button .r-button__content{position:relative;border-width:1px;border-color:transparent;font-size:.875rem;font-weight:500;display:-ms-flexbox;display:flex;-ms-flex-align:center;align-items:center;-ms-flex-pack:center;justify-content:center;height:2.5rem;padding-left:1rem;padding-right:1rem;width:100%;border-radius:9999px;white-space:nowrap;min-width:5rem;-webkit-transition:background-color .15s ease-in-out,border-color .15s ease-in-out,color .15s ease-in-out;transition:background-color .15s ease-in-out,border-color .15s ease-in-out,color .15s ease-in-out}raul-button .r-button__content raul-icon{margin-right:.5rem;margin-top:-1px;font-size:1.25rem}raul-button .r-button__content--icon-only{padding:0;min-width:40px}raul-button .r-button__content--icon-only raul-icon{margin-right:0}raul-button .r-button__element{-webkit-appearance:none;-moz-appearance:none;appearance:none;position:absolute;top:0;right:0;bottom:0;left:0;cursor:pointer;opacity:0;height:100%;width:100%;z-index:10;border-radius:9999px}raul-button .r-button__element[aria-disabled],raul-button .r-button__element[disabled]{pointer-events:none}body[modality=keyboard] raul-button .r-button__element:focus~.r-button__content:before{position:absolute;border-width:1px;border-color:transparent;border-radius:9999px;content:\"\";bottom:-4px;left:-4px;right:-4px;top:-4px}raul-button .r-button__focus-ring{position:absolute;top:0;right:0;bottom:0;left:0;overflow:hidden;height:100%;width:100%;z-index:10;border-radius:9999px}raul-button .r-button--small .r-button__content{font-size:.75rem;height:2rem}raul-button .r-button--small .r-button__content--icon-only{min-width:2rem}raul-button .r-button--small .r-button__content raul-icon{font-size:1rem}body[modality=keyboard] raul-button .r-button--control .r-button__element:focus~.r-button__content:before,raul-button .r-button--control .r-button__content,raul-button .r-button--control .r-button__element,raul-button .r-button--control .r-button__focus-ring{border-radius:.125rem}raul-button .r-button--primary .r-button__content{color:#fff;background-color:#0076cc;border-color:#0076cc}raul-button .r-button--primary .r-button__focus-ring{color:#fff}raul-button .r-button--primary .r-button__element:hover~.r-button__content{background-color:#0069b7;border-color:#0069b7}raul-button .r-button--primary .r-button__element[aria-disabled]~.r-button__content,raul-button .r-button--primary .r-button__element[disabled]~.r-button__content{background-color:#ebedee;border-color:#ebedee;color:#9ba3a7}body[modality=keyboard] raul-button .r-button--primary .r-button__element:focus~.r-button__content:before{border-color:#0076cc}raul-button .r-button--secondary .r-button__content{color:#0076cc;background-color:#fff;border-color:#0076cc}raul-button .r-button--secondary .r-button__focus-ring{color:#0076cc}raul-button .r-button--secondary .r-button__element:hover~.r-button__content{border-color:#0069b7;color:#0069b7}raul-button .r-button--secondary .r-button__element[aria-disabled]~.r-button__content,raul-button .r-button--secondary .r-button__element[disabled]~.r-button__content{border-color:#c6ccd0;color:#9ba3a7}body[modality=keyboard] raul-button .r-button--secondary .r-button__element:focus~.r-button__content:before{border-color:#0076cc}raul-button .r-button--danger .r-button__content{color:#fff;background-color:#d01a1f;border-color:#d01a1f}raul-button .r-button--danger .r-button__focus-ring{color:#fff}raul-button .r-button--danger .r-button__element:hover~.r-button__content{background-color:#ba171b;border-color:#ba171b}raul-button .r-button--danger .r-button__element[aria-disabled]~.r-button__content,raul-button .r-button--danger .r-button__element[disabled]~.r-button__content{background-color:#ebedee;border-color:#ebedee;color:#9ba3a7}body[modality=keyboard] raul-button .r-button--danger .r-button__element:focus~.r-button__content:before{border-color:#d01a1f}raul-button .r-button--text .r-button__content,raul-button .r-button--text .r-button__focus-ring{color:#0076cc}raul-button .r-button--text .r-button__element:hover~.r-button__content{background-color:#e5f4ff}raul-button .r-button--text .r-button__element[aria-disabled]~.r-button__content,raul-button .r-button--text .r-button__element[disabled]~.r-button__content{color:#9ba3a7}body[modality=keyboard] raul-button .r-button--text .r-button__element:focus~.r-button__content:before{border-color:#0076cc}raul-button .r-button--reverse .r-button__content{color:#fff;background-color:rgba(55,71,79,.2)}raul-button .r-button--reverse .r-button__focus-ring{color:#fff}raul-button .r-button--reverse .r-button__element:hover~.r-button__content{background-color:rgba(55,71,79,.4)}raul-button .r-button--reverse .r-button__element[aria-disabled]~.r-button__content,raul-button .r-button--reverse .r-button__element[disabled]~.r-button__content{color:#9ba3a7}body[modality=keyboard] raul-button .r-button--reverse .r-button__element:focus~.r-button__content:before{border-color:#fff}raul-button .r-button--control .r-button__content{color:#202737;background-color:#fff;border-color:#c6ccd0}raul-button .r-button--control .r-button__focus-ring{color:#202737}raul-button .r-button--control .r-button__element:hover~.r-button__content{border-color:#c6ccd0;color:#0076cc}raul-button .r-button--control .r-button__element[aria-disabled]~.r-button__content,raul-button .r-button--control .r-button__element[disabled]~.r-button__content{border-color:#c6ccd0;color:#9ba3a7}body[modality=keyboard] raul-button .r-button--control .r-button__element:focus~.r-button__content:before{border-color:#0076cc}"; },
        enumerable: true,
        configurable: true
    });
    return ButtonComponent;
}());
export { ButtonComponent as raul_button };
