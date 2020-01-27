import { r as registerInstance, h } from './core-9263a98c.js';
var RaulCardSubtitle = /** @class */ (function () {
    function RaulCardSubtitle(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulCardSubtitle.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulCardSubtitle, "style", {
        get: function () { return "raul-card-subtitle{display:block;font-size:.875rem;line-height:1.25;margin-bottom:.5rem;color:rgba(55,71,79,.8);margin-bottom:0}"; },
        enumerable: true,
        configurable: true
    });
    return RaulCardSubtitle;
}());
export { RaulCardSubtitle as raul_card_subtitle };
