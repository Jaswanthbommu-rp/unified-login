import { r as registerInstance, h } from './core-9263a98c.js';
var RaulGridFooter = /** @class */ (function () {
    function RaulGridFooter(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulGridFooter.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulGridFooter, "style", {
        get: function () { return "raul-grid-footer{display:-ms-flexbox;display:flex;-ms-flex-wrap:wrap;flex-wrap:wrap;width:100%}"; },
        enumerable: true,
        configurable: true
    });
    return RaulGridFooter;
}());
export { RaulGridFooter as raul_grid_footer };
