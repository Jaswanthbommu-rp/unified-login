'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsAccessibility = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Accessibility', false);
    }
    render() {
        return (core.h("div", { class: "docs-page-container" }, core.h("div", { class: "docs-page-header" }, core.h("docs-markdown", null, `
                # Accessibility
            `)), core.h("div", { class: "docs-page-content" }, core.h("docs-markdown", null, `
            # Accessibility

            Accessibility insures that websites, tools and technologies are designed and developed so that people with disabilities are able to use them. It also enhances the experience of users who may have temporary disabilities or limitations, as well as benefits general users alike.


            ::: tip RealPage follows WCAG 2.0 “AA” level compliance
            * WCAG 2.0 (Web Content Accessibility Guidelines) defines how to make web content more accessible to people with disabilities.
            * WCAG 2.0 is created in cooperation of individuals and organizations with guidelines that are organized under 4 principles; perceivable, operable, understandable, and robust. For each guideline, there are testable success criteria, which are at three levels: A, AA, and AAA.
            * For more information visit: [http://www.w3.org/TR/WCAG20/](http://www.w3.org/TR/WCAG20/)
            :::

            Making RealPage products accessible is a joint effort between the RAUL team and product teams. The RAUL team strives to create components that are accessible, but many of the guidelines below cover the content created with the components.

            ## Implementing Accessibility

            ### Getting Started
            Establish an accessibility baseline by auditing your product using browser extensions and add accessibility linters in your build process. Add accessibility tickets to your sprints and share your enhancements with customers.

            ### Maintaining Fitness
            Incorporate browser audit in your QA testing program and monitor your builds for accesibility.

            ## WCAG Guidelines

            #### Perceivable
            *Insure the user interface and content is easy for the user to understand.*

            * Provide text alternatives for any non-text content.
            * Provide alternatives for time-based media.
            * Create content that can be presented in different ways.
            * Make it easier for users to see and hear content.

            #### Operable
            *Insure that all navigation and content is fully functionable at all times.*

            * Make all functionality available from a keyboard.
            * Provide users enough time to read and use content.
            * Do not design content in a way that is know to cause seizures.
            * Provide ways to help users navigate and find content.

            #### Understandable
            *Insure operation of the user interface and content in ways they would predict.*

            * Make text content readable and understandable.
            * Make web pages appear and operate in predictable ways.
            * Help users avoid and correct mistakes without input assistance.
            * Do not indicate change through color alone.

            #### Robust
            *Insure content can support other varieties of tools like assistive technologies.*

            * Maximize compatibility with current and future user agents.
            * Code is written in a way that facilitates accessibility.
            * Important status messages not focused on must be announced to screen readers.
            * Significant HTML/XHTML validation/parsing errors are avoided.

          `))));
    }
};

exports.docs_accessibility = DocsAccessibility;
