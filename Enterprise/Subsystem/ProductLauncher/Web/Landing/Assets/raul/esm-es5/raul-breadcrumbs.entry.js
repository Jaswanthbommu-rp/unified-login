import { r as registerInstance, h, H as Host } from './core-9263a98c.js';
var RaulBreadcrumbs = /** @class */ (function () {
    function RaulBreadcrumbs(hostRef) {
        registerInstance(this, hostRef);
        this.overflow = false;
    }
    RaulBreadcrumbs.prototype.render = function () {
        return (h(Host, { role: "list" }, h("slot", null)));
    };
    Object.defineProperty(RaulBreadcrumbs, "style", {
        get: function () { return "raul-breadcrumbs{display:-ms-flexbox;display:flex;-ms-flex-wrap:wrap;flex-wrap:wrap}"; },
        enumerable: true,
        configurable: true
    });
    return RaulBreadcrumbs;
}());
export { RaulBreadcrumbs as raul_breadcrumbs };
