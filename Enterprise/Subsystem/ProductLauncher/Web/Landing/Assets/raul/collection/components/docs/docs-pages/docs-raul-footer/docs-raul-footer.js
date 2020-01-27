import { h } from "@stencil/core";
export class DocsRaulFooter {
    constructor() {
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
        return (h("docs-element", { title: "Footer" },
            h("div", { slot: "overview" },
                h("docs-showcase", null,
                    h("div", null,
                        h("div", { class: "heading-lg" }, "Basic"),
                        h("div", { innerHTML: this.basicFooter }),
                        h("docs-code", { class: "mt-5", code: this.basicFooter })))),
            h("div", { slot: "design" }, "Design Guidelines"),
            h("div", { slot: "api" },
                h("docs-interface", { component: "raul-footer" }))));
    }
    static get is() { return "docs-raul-footer"; }
}
