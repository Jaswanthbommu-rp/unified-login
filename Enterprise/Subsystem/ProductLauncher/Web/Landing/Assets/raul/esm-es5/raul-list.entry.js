import { r as registerInstance, h } from './core-9263a98c.js';
var RaulList = /** @class */ (function () {
    function RaulList(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulList.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulList, "style", {
        get: function () { return "raul-list{display:block;background-color:#fff}"; },
        enumerable: true,
        configurable: true
    });
    return RaulList;
}());
export { RaulList as raul_list };
