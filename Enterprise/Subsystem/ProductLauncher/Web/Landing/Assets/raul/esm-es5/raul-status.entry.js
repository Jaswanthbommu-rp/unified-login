import { r as registerInstance, h } from './core-9263a98c.js';
var RaulChip = /** @class */ (function () {
    function RaulChip(hostRef) {
        registerInstance(this, hostRef);
        /**
         * Status variant.
         */
        this.variant = 'default';
    }
    RaulChip.prototype.componentDidLoad = function () {
        this.title = this.statusTextEl.textContent;
    };
    RaulChip.prototype.render = function () {
        var _a;
        var _this = this;
        return (h("div", { class: (_a = {
                    'status': true
                },
                _a["status--" + this.variant] = this.variant !== 'default',
                _a) }, h("div", { class: "status__text", title: this.title, ref: function (el) { return _this.statusTextEl = el; } }, h("slot", null))));
    };
    Object.defineProperty(RaulChip, "style", {
        get: function () { return "raul-status{display:inline-block;max-width:100%}raul-status .status{position:relative;display:-ms-flexbox;display:flex;background-color:#ebedee;font-size:.75rem;font-weight:500;color:#37474f;text-align:left;line-height:1.25;border-radius:.125rem;padding-left:.5rem;padding-right:.5rem;padding-top:.25rem;padding-bottom:.25rem;max-width:100%}raul-status .status--destructive{background-color:#fae8e9;color:#d01a1f}raul-status .status--success{background-color:#e7f3eb;color:#139c3e}raul-status .status--warning{background-color:#fef8ea;color:#bc8701}"; },
        enumerable: true,
        configurable: true
    });
    return RaulChip;
}());
export { RaulChip as raul_status };
