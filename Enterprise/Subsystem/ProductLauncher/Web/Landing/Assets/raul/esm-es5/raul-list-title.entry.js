import { r as registerInstance, h } from './core-9263a98c.js';
var RaulListTitle = /** @class */ (function () {
    function RaulListTitle(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulListTitle.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulListTitle, "style", {
        get: function () { return "raul-list-title{display:block;font-size:1rem;font-weight:600;line-height:1.25;margin-bottom:.5rem;margin-bottom:0}"; },
        enumerable: true,
        configurable: true
    });
    return RaulListTitle;
}());
export { RaulListTitle as raul_list_title };
