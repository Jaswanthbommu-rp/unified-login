import { r as registerInstance, h } from './core-9263a98c.js';
var RaulGridHeader = /** @class */ (function () {
    function RaulGridHeader(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulGridHeader.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulGridHeader, "style", {
        get: function () { return "raul-grid-header{display:-ms-flexbox;display:flex;-ms-flex-wrap:wrap;flex-wrap:wrap;width:100%}"; },
        enumerable: true,
        configurable: true
    });
    return RaulGridHeader;
}());
export { RaulGridHeader as raul_grid_header };
