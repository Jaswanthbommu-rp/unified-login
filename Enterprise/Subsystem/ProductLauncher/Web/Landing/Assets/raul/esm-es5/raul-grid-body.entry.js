import { r as registerInstance, h } from './core-9263a98c.js';
var RaulGridBody = /** @class */ (function () {
    function RaulGridBody(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulGridBody.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulGridBody, "style", {
        get: function () { return "raul-grid-body{display:-ms-flexbox;display:flex;-ms-flex-wrap:wrap;flex-wrap:wrap;width:100%}"; },
        enumerable: true,
        configurable: true
    });
    return RaulGridBody;
}());
export { RaulGridBody as raul_grid_body };
