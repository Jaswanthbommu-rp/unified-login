import { r as registerInstance, h } from './core-9263a98c.js';
var RaulStatusIndicator = /** @class */ (function () {
    function RaulStatusIndicator(hostRef) {
        registerInstance(this, hostRef);
        this.variant = 'default';
    }
    RaulStatusIndicator.prototype.render = function () {
        var _a;
        return (h("div", { class: (_a = {
                    'status-indicator': true
                },
                _a["status-indicator--" + this.variant] = this.variant !== 'default',
                _a) }));
    };
    Object.defineProperty(RaulStatusIndicator, "style", {
        get: function () { return "raul-status-indicator{display:inline-block}raul-status-indicator .status-indicator{display:inline-block;border-radius:9999px;width:.5rem;height:.5rem;background-color:#ebedee}raul-status-indicator .status-indicator--destructive{background-color:#d01a1f}raul-status-indicator .status-indicator--success{background-color:#139c3e}raul-status-indicator .status-indicator--warning{background-color:#fec12d}"; },
        enumerable: true,
        configurable: true
    });
    return RaulStatusIndicator;
}());
export { RaulStatusIndicator as raul_status_indicator };
