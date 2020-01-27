'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsRaulFooter = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.basicFooter = `
    <raul-footer>
      <raul-footer-contact>
        RealPage, Inc. Richardson, TX 75082 <a href="tel:(877) 325-7243">(877) 325-7243</a>
      </raul-footer-contact>

      <raul-footer-copyright>
        © Copyright 1998-2019 <a href="#">All rights reserved</a>
      </raul-footer-copyright>

      <raul-footer-social-icons
        facebook-url="#"
        twitter-url="#"
        linkedin-url="#"
        youtube-url="#"></raul-footer-social-icons>

      <raul-footer-navigation-links>
        <raul-footer-navigation-link>
          <a href="#">Terms of Use</a>
        </raul-footer-navigation-link>

        <raul-footer-navigation-link>
          <a href="#">Privacy Policy</a>
        </raul-footer-navigation-link>

        <raul-footer-navigation-link>
          <a href="#">Privacy Shield</a>
        </raul-footer-navigation-link>

        <raul-footer-navigation-link>
          <a href="#">DMCA Notice</a>
        </raul-footer-navigation-link>
      </raul-footer-navigation-links>
    </raul-footer>
  `;
    }
    render() {
        return (core.h("docs-element", { title: "Footer" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("div", null, core.h("div", { class: "heading-lg" }, "Basic"), core.h("div", { innerHTML: this.basicFooter }), core.h("docs-code", { class: "mt-5", code: this.basicFooter })))), core.h("div", { slot: "design" }, "Design Guidelines"), core.h("div", { slot: "api" }, core.h("docs-interface", { component: "raul-footer" }))));
    }
};

exports.docs_raul_footer = DocsRaulFooter;
