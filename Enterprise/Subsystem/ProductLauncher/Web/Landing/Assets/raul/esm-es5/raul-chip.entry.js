import { r as registerInstance, c as createEvent, h } from './core-9263a98c.js';
import { D as DELETE_KEYS } from './index-45f40cb4.js';
var RaulChip = /** @class */ (function () {
    function RaulChip(hostRef) {
        registerInstance(this, hostRef);
        this.raulChipRemove = createEvent(this, "raulChipRemove", 7);
    }
    RaulChip.prototype.componentDidLoad = function () {
        this.title = this.chipTextEl.textContent;
    };
    RaulChip.prototype.handleChipRemove = function () {
        this.raulChipRemove.emit();
    };
    RaulChip.prototype.render = function () {
        var _this = this;
        return (h("div", { class: "r-chip" }, h("div", { class: "r-chip__text", title: this.title, ref: function (el) { return _this.chipTextEl = el; } }, h("slot", null)), this.removable &&
            h("button", { class: "r-chip__remove-button", onClick: this.removable ? function () { return _this.handleChipRemove(); } : null, onKeyDown: this.removable
                    ? function (e) { return DELETE_KEYS.includes(e.key) ? _this.handleChipRemove() : null; }
                    : null }, h("raul-icon", { icon: "close", class: "r-chip__remove-icon" }))));
    };
    Object.defineProperty(RaulChip, "style", {
        get: function () { return "raul-chip{display:inline-block;max-width:100%}raul-chip .r-chip{position:relative;display:-ms-flexbox;display:flex;-ms-flex-align:start;align-items:flex-start;background-color:#e5f4ff;font-size:.75rem;font-weight:500;color:#37474f;text-align:left;line-height:1.25;border-radius:.125rem;padding-left:.5rem;padding-right:.5rem;padding-top:.25rem;padding-bottom:.25rem;max-width:100%}raul-chip .r-chip__remove-button{margin-left:.5rem;margin-top:1px;font-size:.625rem}raul-chip .r-chip__remove-icon{margin:-.25rem;padding:.25rem}"; },
        enumerable: true,
        configurable: true
    });
    return RaulChip;
}());
export { RaulChip as raul_chip };
