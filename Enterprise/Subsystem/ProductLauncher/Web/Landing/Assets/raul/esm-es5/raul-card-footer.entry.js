import { r as registerInstance, h } from './core-9263a98c.js';
var RaulCardFooter = /** @class */ (function () {
    function RaulCardFooter(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulCardFooter.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulCardFooter, "style", {
        get: function () { return "raul-card-footer{display:block;margin-top:1rem}"; },
        enumerable: true,
        configurable: true
    });
    return RaulCardFooter;
}());
export { RaulCardFooter as raul_card_footer };
