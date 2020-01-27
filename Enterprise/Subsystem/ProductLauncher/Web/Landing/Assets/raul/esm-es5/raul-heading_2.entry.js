import { r as registerInstance, h } from './core-9263a98c.js';
import { r as randomUID } from './index-b2673790.js';
var RaulHeading = /** @class */ (function () {
    function RaulHeading(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulHeading.prototype.render = function () {
        var size = this.variant === 'page' ? 'hero' :
            this.variant === 'section' ? 'extra-large' :
                this.variant === 'content' ? 'large'
                    : 'large';
        return (h("raul-text", { paragraph: true, size: size, key: randomUID() }, h("slot", null)));
    };
    return RaulHeading;
}());
var RaulText = /** @class */ (function () {
    function RaulText(hostRef) {
        registerInstance(this, hostRef);
        this.align = 'left';
        this.inline = false;
        this.strong = false;
        this.emphasis = false;
        this.underline = false;
        this.lineThrough = false;
        this.ellipsis = false;
        this.capitalize = false;
        this.uppercase = false;
        this.paragraph = false;
    }
    RaulText.prototype.render = function () {
        var _a;
        var Tag = this.inline ? 'span' :
            this.size === 'small' ? 'p' :
                this.size === 'medium' ? 'p' :
                    this.size === 'large' ? 'h3' :
                        this.size === 'extra-large' ? 'h2' :
                            this.size === 'hero' ? 'h1'
                                : 'span';
        return (h(Tag, { class: (_a = {},
                _a['r-text'] = true,
                _a["r-text--" + this.size] = !!this.size,
                _a["r-text--" + this.color] = !!this.color,
                _a["r-text--align-" + this.align] = true,
                _a['r-text--inline'] = this.inline,
                _a['r-text--strong'] = this.strong,
                _a['r-text--underline'] = this.underline,
                _a['r-text--line-through'] = this.lineThrough,
                _a['r-text--emphasis'] = this.emphasis,
                _a['r-text--ellipsis'] = this.ellipsis,
                _a['r-text--capitalize'] = this.capitalize,
                _a['r-text--uppercase'] = this.uppercase,
                _a['r-text--paragraph'] = this.paragraph,
                _a), key: randomUID() }, h("slot", null)));
    };
    Object.defineProperty(RaulText, "style", {
        get: function () { return "raul-text .r-text{font-family:Roboto,sans-serif;color:#37474f!important;margin:0!important;border:none!important}raul-text .r-text--small{font-size:.75rem;line-height:18px!important;color:rgba(55,71,79,.8)!important}raul-text .r-text--medium{font-size:.875rem;line-height:20px!important;color:rgba(55,71,79,.8)!important}raul-text .r-text--large{font-size:1rem;font-weight:700!important;line-height:20px!important}raul-text .r-text--extra-large{font-size:1.25rem;font-weight:700!important;line-height:26px!important}raul-text .r-text--hero{font-size:2.25rem;font-weight:700!important;line-height:40px!important}raul-text .r-text--strong{opacity:1!important;font-weight:600!important}raul-text .r-text--primary{color:#0076cc!important}raul-text .r-text--danger{color:#d31612!important}raul-text .r-text--success{color:#1cb94e!important}raul-text .r-text--white{color:#fff!important}raul-text .r-text--align-left{text-align:left!important}raul-text .r-text--align-center{text-align:center!important}raul-text .r-text--align-right{text-align:right!important}raul-text .r-text--align-justify{text-align:justify!important}raul-text .r-text--emphasis{font-style:italic}raul-text .r-text--underline{text-decoration:underline}raul-text .r-text--line-through{text-decoration:line-through}raul-text .r-text--ellipsis{white-space:nowrap;overflow:hidden;text-overflow:ellipsis}raul-text .r-text--capitalize{text-transform:capitalize}raul-text .r-text--uppercase{text-transform:uppercase}raul-text .r-text--paragraph{margin-bottom:24px!important}"; },
        enumerable: true,
        configurable: true
    });
    return RaulText;
}());
export { RaulHeading as raul_heading, RaulText as raul_text };
