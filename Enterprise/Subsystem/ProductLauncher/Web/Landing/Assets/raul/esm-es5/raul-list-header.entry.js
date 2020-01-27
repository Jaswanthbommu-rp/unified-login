import { r as registerInstance, h, H as Host } from './core-9263a98c.js';
var RaulListHeader = /** @class */ (function () {
    function RaulListHeader(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulListHeader.prototype.render = function () {
        return (h(Host, null, h("slot", null)));
    };
    Object.defineProperty(RaulListHeader, "style", {
        get: function () { return "raul-list-header{display:block;border-bottom-width:3px;border-color:#ebedee;padding-top:1rem;padding-bottom:1rem}"; },
        enumerable: true,
        configurable: true
    });
    return RaulListHeader;
}());
export { RaulListHeader as raul_list_header };
