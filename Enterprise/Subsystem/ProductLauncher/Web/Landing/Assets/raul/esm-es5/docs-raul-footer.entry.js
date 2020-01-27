import { r as registerInstance, h } from './core-9263a98c.js';
var DocsRaulFooter = /** @class */ (function () {
    function DocsRaulFooter(hostRef) {
        registerInstance(this, hostRef);
        this.basicFooter = "\n    <raul-footer>\n      <raul-footer-contact>\n        RealPage, Inc. Richardson, TX 75082 <a href=\"tel:(877) 325-7243\">(877) 325-7243</a>\n      </raul-footer-contact>\n\n      <raul-footer-copyright>\n        \u00A9 Copyright 1998-2019 <a href=\"#\">All rights reserved</a>\n      </raul-footer-copyright>\n\n      <raul-footer-social-icons\n        facebook-url=\"#\"\n        twitter-url=\"#\"\n        linkedin-url=\"#\"\n        youtube-url=\"#\"></raul-footer-social-icons>\n\n      <raul-footer-navigation-links>\n        <raul-footer-navigation-link>\n          <a href=\"#\">Terms of Use</a>\n        </raul-footer-navigation-link>\n\n        <raul-footer-navigation-link>\n          <a href=\"#\">Privacy Policy</a>\n        </raul-footer-navigation-link>\n\n        <raul-footer-navigation-link>\n          <a href=\"#\">Privacy Shield</a>\n        </raul-footer-navigation-link>\n\n        <raul-footer-navigation-link>\n          <a href=\"#\">DMCA Notice</a>\n        </raul-footer-navigation-link>\n      </raul-footer-navigation-links>\n    </raul-footer>\n  ";
    }
    DocsRaulFooter.prototype.render = function () {
        return (h("docs-element", { title: "Footer" }, h("div", { slot: "overview" }, h("docs-showcase", null, h("div", null, h("div", { class: "heading-lg" }, "Basic"), h("div", { innerHTML: this.basicFooter }), h("docs-code", { class: "mt-5", code: this.basicFooter })))), h("div", { slot: "design" }, "Design Guidelines"), h("div", { slot: "api" }, h("docs-interface", { component: "raul-footer" }))));
    };
    return DocsRaulFooter;
}());
export { DocsRaulFooter as docs_raul_footer };
