import { r as registerInstance, h, H as Host } from './core-9263a98c.js';
var RaulBreadcrumb = /** @class */ (function () {
    function RaulBreadcrumb(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulBreadcrumb.prototype.render = function () {
        return (h(Host, { role: "listitem" }, h("slot", null), h("raul-icon", { icon: "arrow-right-v", class: "r-breadcrumb__separator-icon" })));
    };
    Object.defineProperty(RaulBreadcrumb, "style", {
        get: function () { return "raul-breadcrumb{display:-ms-inline-flexbox;display:inline-flex;-ms-flex-align:center;align-items:center;font-size:.75rem;color:#37474f}raul-breadcrumb .r-breadcrumb__separator-icon{color:#37474f;padding-left:.25rem;padding-right:.25rem}raul-breadcrumb:last-of-type{font-weight:600}raul-breadcrumb:last-of-type .r-breadcrumb__separator-icon{display:none}raul-breadcrumb a{display:-ms-flexbox;display:flex;-ms-flex-align:center;align-items:center;color:#37474f}raul-breadcrumb a:hover{color:#0076cc;text-decoration:none}"; },
        enumerable: true,
        configurable: true
    });
    return RaulBreadcrumb;
}());
export { RaulBreadcrumb as raul_breadcrumb };
