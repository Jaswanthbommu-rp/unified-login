import { r as registerInstance, h } from './core-9263a98c.js';
var RaulAlert = /** @class */ (function () {
    function RaulAlert(hostRef) {
        registerInstance(this, hostRef);
        /**
         * Determines the color of the left bar
         */
        this.variant = 'information';
        /**
         * Corners can be rounded or not
         */
        this.rounded = false;
    }
    RaulAlert.prototype.componentWillLoad = function () {
        if (!this.heading) {
            console.error('Heading text is required for Raul alert component');
        }
    };
    RaulAlert.prototype.render = function () {
        var _a;
        return (h("div", { class: (_a = {
                    'r-alert': true
                },
                _a["r-alert__" + this.variant] = true,
                _a['r-alert__rounded'] = this.rounded,
                _a) }, h("div", { class: 'r-alert__heading' }, this.heading), this.content &&
            h("div", { class: 'r-alert__content' }, this.content), this.ctaMessage && this.ctaUrl ?
            h("a", { href: this.ctaUrl }, h("div", { class: 'r-alert__cta' }, this.ctaMessage)) : null));
    };
    Object.defineProperty(RaulAlert, "style", {
        get: function () { return ".r-alert{width:100%;padding:1rem;font-size:.875rem;height:auto}.r-alert__rounded{border-radius:2px}.r-alert__information{background-color:#e5f4ff;border-left:12px solid #0076cc}.r-alert__information a{color:#0076cc}.r-alert__success{background-color:#e7f3eb;border-left:12px solid #139c3e}.r-alert__success a{color:#139c3e}.r-alert__warning{background-color:#fef8ea;border-left:12px solid #fec12d}.r-alert__warning a{color:#fec12d}.r-alert__danger{background-color:#fae8e9;border-left:12px solid #d01a1f}.r-alert__danger a{color:#d01a1f}.r-alert__warning a{color:#bc8701}.r-alert .r-alert__heading{font-weight:600}.r-alert .r-alert__content{padding-bottom:.25rem;color:rgba(32,39,55,.8)}.r-alert a{display:inline-block;text-decoration:none}.r-alert a:focus{outline-color:#0076cc}"; },
        enumerable: true,
        configurable: true
    });
    return RaulAlert;
}());
export { RaulAlert as raul_alert };
