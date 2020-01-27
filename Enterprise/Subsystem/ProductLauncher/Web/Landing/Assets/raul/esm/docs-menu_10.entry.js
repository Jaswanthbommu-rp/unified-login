import { r as registerInstance, h, H as Host, c as createEvent, g as getElement, d as getContext } from './core-9263a98c.js';
import { e as expandHeight, c as collapseHeight } from './index-4f8d920e.js';
import { A as ActiveRouter } from './active-router-13b658d3.js';

const DocsMenu = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        this.open = false;
    }
    toggleMenu() {
        this.open = !this.open;
    }
    render() {
        return (h(Host, { class: this.open ? 'open' : '' }, h("div", { class: "r-docs-menu" }, h("button", { class: "docs-menu-toggle", onClick: () => this.toggleMenu() }, h("span", null), h("span", null), h("span", null), h("span", null)), h("docs-menu-section", { name: "Home", url: "/" }), h("docs-menu-section", { name: "Overview" }, h("docs-menu-page", { name: "Introduction", url: "/overview/introduction" }), h("docs-menu-page", { name: "Upgrading", url: "/overview/upgrading" }), h("docs-menu-page", { name: "Compliance", url: "/overview/compliance" }), h("docs-menu-page", { name: "Accessibility", url: "/overview/accessibility" })), h("docs-menu-section", { name: "Theme" }, h("docs-menu-page", { name: "Typography", url: "/theme/typography" }), h("docs-menu-page", { name: "Colors", url: "/theme/colors" }), h("docs-menu-page", { name: "Spacing", url: "/theme/spacing" })), h("docs-menu-section", { name: "Components" }, h("docs-menu-page", { name: "Overview", url: "/elements/index" }), h("docs-menu-elem-page", { name: "Accordion", url: "/elements/accordion" }), h("docs-menu-elem-page", { name: "Action menu", url: "/elements/action-menu" }), h("docs-menu-elem-page", { name: "Alert", url: "/elements/alert" }), h("docs-menu-elem-page", { name: "Aside", url: "/elements/aside" }), h("docs-menu-elem-page", { name: "Avatar", url: "/elements/avatar" }), h("docs-menu-elem-page", { name: "Badge", url: "/elements/badge" }), h("docs-menu-elem-page", { name: "Breadcrumbs", url: "/elements/breadcrumbs" }), h("docs-menu-elem-page", { name: "Bulk Action Bar", url: "/elements/bulk-action-bar" }), h("docs-menu-elem-page", { name: "Button", url: "/elements/button" }), h("docs-menu-elem-page", { name: "Card", url: "/elements/card" }), h("docs-menu-elem-page", { name: "Checkbox", url: "/elements/checkbox" }), h("docs-menu-elem-page", { name: "Chips", url: "/elements/chips" }), h("docs-menu-elem-page", { name: "Date picker", url: "/elements/date-picker" }), h("docs-menu-elem-page", { name: "Filter Bar", url: "/elements/filter-bar" }), h("docs-menu-elem-page", { name: "Filter menu", url: "/elements/filter-menu" }), h("docs-menu-elem-page", { name: "Footer", url: "/elements/footer" }), h("docs-menu-elem-page", { name: "Grid", url: "/elements/grid" }), h("docs-menu-elem-page", { name: "Input", url: "/elements/input" }), h("docs-menu-elem-page", { name: "List", url: "/elements/list" }), h("docs-menu-elem-page", { name: "Loaders", url: "/elements/loaders" }), h("docs-menu-elem-page", { name: "Modal", url: "/elements/modal" }), h("docs-menu-elem-page", { name: "Paging Bar", url: "/elements/paging-bar" }), h("docs-menu-elem-page", { name: "Progress", url: "/elements/progress" }), h("docs-menu-elem-page", { name: "Radio Button", url: "/elements/radio" }), h("docs-menu-elem-page", { name: "Select", url: "/elements/select" }), h("docs-menu-elem-page", { name: "Simple Select", url: "/elements/simple-select" }), h("docs-menu-elem-page", { name: "Simple Table", url: "/elements/simple-table" }), h("docs-menu-elem-page", { name: "Snackbar", url: "/elements/snackbar" }), h("docs-menu-elem-page", { name: "Sortables", url: "/elements/sortables" }), h("docs-menu-elem-page", { name: "Status", url: "/elements/status" }), h("docs-menu-elem-page", { name: "Status Indicator", url: "/elements/status-indicator" }), h("docs-menu-elem-page", { name: "Switch", url: "/elements/switch" }), h("docs-menu-elem-page", { name: "Tabs", url: "/elements/tabs" }), h("docs-menu-elem-page", { name: "Textarea", url: "/elements/textarea" }), h("docs-menu-elem-page", { name: "Toggles", url: "/elements/toggles" }), h("docs-menu-elem-page", { name: "Tooltip", url: "/elements/tooltip" })))));
    }
    static get style() { return "main{padding-top:3rem}docs-menu{position:fixed;height:3rem;display:block;min-width:100vw;overflow-y:hidden;z-index:50;background-color:#fff;border-bottom:1px solid #e4e6e7}docs-menu.open{min-height:100vh}docs-menu.open .r-docs-menu{overflow:auto}.docs-menu-toggle{width:24px;height:24px;margin:0;-webkit-transition:.5s ease-in-out;transition:.5s ease-in-out;cursor:pointer;top:16px;right:16px}.docs-menu-toggle,.docs-menu-toggle span{position:absolute;-webkit-transform:rotate(0deg);transform:rotate(0deg)}.docs-menu-toggle span{background-color:#9ba3a7;display:block;height:3px;width:100%;border-radius:9px;opacity:1;left:0;-webkit-transition:.25s ease-in-out;transition:.25s ease-in-out}.docs-menu-toggle span:first-child{top:0}.docs-menu-toggle span:nth-child(2),.docs-menu-toggle span:nth-child(3){top:30%}.docs-menu-toggle span:nth-child(4){top:60%}.open .docs-menu-toggle span:first-child,.open .docs-menu-toggle span:nth-child(4){opacity:0}.open .docs-menu-toggle span:nth-child(2){-webkit-transform:rotate(45deg);transform:rotate(45deg)}.open .docs-menu-toggle span:nth-child(3){-webkit-transform:rotate(-45deg);transform:rotate(-45deg)}.r-docs-menu{font-size:.875rem;width:100%;overflow:hidden;padding:16px;padding-top:72px;padding-right:24px;list-style-type:none;overflow:auto;height:100vh}.r-docs-menu:before{background:#fff url(https://cdn.realpage.com/images/rp-logo.svg) 15px no-repeat;background-size:50%;content:\"\";position:fixed;width:199px;height:47px;z-index:1;top:0;left:0}\@media (min-width:1024px){main{padding-top:0}docs-menu{height:auto;position:relative;border-right:1px solid #e4e6e7;border-bottom:0;min-width:200px}.docs-menu-toggle{display:none}.r-docs-menu{position:fixed;overflow:auto;width:200px}.r-docs-menu:after{background:transparent;content:\"\";position:fixed;width:199px;height:20px;background:-webkit-gradient(linear,left top,left bottom,from(#fff),to(hsla(0,0%,100%,0)));background:linear-gradient(180deg,#fff,hsla(0,0%,100%,0));z-index:1;top:60px;left:0;pointer-events:none}.r-docs-menu:before{height:60px;top:0}}.r-docs-menu__section__name{font-weight:600;cursor:pointer;margin:0;color:#37474f;height:48px}.r-docs-menu__section__name .link-active{color:#f06000}.r-docs-menu__section__name raul-icon{float:right;-webkit-transition:all .3s ease;transition:all .3s ease}.r-docs-menu__section__content{height:0;overflow:hidden;-webkit-transition:height .3s ease-in-out;transition:height .3s ease-in-out}.r-docs-menu__page{display:block;margin-left:16px}.r-docs-menu__page__link{display:block;font-weight:400;color:#303436;text-decoration:none;height:48px}.r-docs-menu__page__link.link-active,.r-docs-menu__page__link:hover{color:#f06000}.r-docs-menu__page__link.link-active{font-weight:500}.r-docs-menu docs-menu-section.open raul-icon{-webkit-transform:rotate(180deg);transform:rotate(180deg);-webkit-transition:all .3s ease;transition:all .3s ease}"; }
};

const DocsMenuElemPage = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("docs-menu-page", { name: this.name, url: this.url }));
    }
};

const DocsMenuPage = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        this.menuPageActivated = createEvent(this, "menuPageActivated", 7);
    }
    handleClick(e) {
        if (this.el.contains(e.target)) {
            this.menuPageActivated.emit();
        }
    }
    render() {
        return (h("div", { class: "r-docs-menu__page" }, h("stencil-route-link", { "anchor-class": "r-docs-menu__page__link", url: this.url }, this.name)));
    }
    get el() { return getElement(this); }
};

const DocsMenuSection = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        this.expanded = false;
        this.expandSection = createEvent(this, "expandSection", 7);
    }
    componentDidLoad() {
        this.setInitialToggleState();
    }
    setInitialToggleState() {
        if (this.el.querySelector('.link-active')) {
            this.expanded = true;
            expandHeight(this.contentEl, false);
        }
    }
    handleClick(e) {
        if (this.el.contains(e.target)) {
            if (this.expanded) {
                this.expanded = false;
            }
            else {
                this.expanded = true;
                expandHeight(this.contentEl);
                this.expandSection.emit(this.el);
                this.el.classList.add("open");
            }
        }
    }
    maybeCollapse(e) {
        if (e.detail !== this.el) {
            this.expanded = false;
            collapseHeight(this.contentEl);
            this.el.classList.remove("open");
        }
    }
    renderSectionTitle() {
        if (this.url) {
            return (h("div", { class: "r-docs-menu__section" }, h("h4", { onClick: (e) => this.handleClick(e), class: "r-docs-menu__section__name" }, h("stencil-route-link", { "anchor-class": "r-docs-menu__header__link", url: this.url }, this.name)), h("div", { class: "r-docs-menu__section__content", ref: (el) => this.contentEl = el }, h("slot", null))));
        }
        else {
            return (h("div", { class: "r-docs-menu__section" }, h("h4", { onClick: (e) => this.handleClick(e), class: "r-docs-menu__section__name" }, this.name, h("raul-icon", { icon: "arrow-down-v", class: "text-gray-dark" })), h("div", { class: "r-docs-menu__section__content", ref: (el) => this.contentEl = el }, h("slot", null))));
        }
    }
    render() {
        return (h("span", null, this.renderSectionTitle()));
    }
    get el() { return getElement(this); }
};

const DocsShell = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("div", { class: "docs-container" }, h("docs-menu", { slot: "navigation" }), h("main", { slot: "page" }, h("stencil-router", null, h("stencil-route-switch", { scrollTopOffset: 0 }, h("stencil-route", { url: "/", component: "docs-home", exact: true }), h("stencil-route", { url: "/overview/introduction", component: "docs-introduction" }), h("stencil-route", { url: "/overview/upgrading", component: "docs-upgrading" }), h("stencil-route", { url: "/overview/compliance", component: "docs-compliance" }), h("stencil-route", { url: "/overview/accessibility", component: "docs-accessibility" }), h("stencil-route", { url: "/theme/typography", component: "docs-typography" }), h("stencil-route", { url: "/theme/colors", component: "docs-colors" }), h("stencil-route", { url: "/theme/spacing", component: "docs-spacing" }), h("stencil-route", { url: "/elements/index", component: "docs-index" }), h("stencil-route", { url: "/elements/heading", component: "docs-raul-heading" }), h("stencil-route", { url: "/elements/text", component: "docs-raul-text" }), h("stencil-route", { url: "/elements/alert", component: "docs-raul-alert" }), h("stencil-route", { url: "/elements/button", component: "docs-raul-button" }), h("stencil-route", { url: "/elements/checkbox", component: "docs-raul-checkbox" }), h("stencil-route", { url: "/elements/radio", component: "docs-raul-radio-button" }), h("stencil-route", { url: "/elements/badge", component: "docs-raul-badge" }), h("stencil-route", { url: "/elements/modal", component: "docs-raul-modal" }), h("stencil-route", { url: "/elements/simple-select", component: "docs-raul-simple-select" }), h("stencil-route", { url: "/elements/select", component: "docs-raul-select" }), h("stencil-route", { url: "/elements/action-menu", component: "docs-raul-action-menu" }), h("stencil-route", { url: "/elements/filter-menu", component: "docs-raul-filter-menu" }), h("stencil-route", { url: "/elements/sortables", component: "docs-raul-sortable-list" }), h("stencil-route", { url: "/elements/loaders", component: "docs-raul-loaders" }), h("stencil-route", { url: "/elements/input", component: "docs-raul-input" }), h("stencil-route", { url: "/elements/textarea", component: "docs-raul-textarea" }), h("stencil-route", { url: "/elements/tabs", component: "docs-raul-tabs" }), h("stencil-route", { url: "/elements/date-picker", component: "docs-raul-date-picker" }), h("stencil-route", { url: "/elements/paging-bar", component: "docs-raul-paging-bar" }), h("stencil-route", { url: "/elements/bulk-action-bar", component: "docs-raul-bulk-action-bar" }), h("stencil-route", { url: "/elements/snackbar", component: "docs-raul-snackbar" }), h("stencil-route", { url: "/elements/switch", component: "docs-raul-switch" }), h("stencil-route", { url: "/elements/chips", component: "docs-raul-chips" }), h("stencil-route", { url: "/elements/status", component: "docs-raul-status" }), h("stencil-route", { url: "/elements/status-indicator", component: "docs-raul-status-indicator" }), h("stencil-route", { url: "/elements/aside", component: "docs-raul-aside" }), h("stencil-route", { url: "/elements/list", component: "docs-raul-list" }), h("stencil-route", { url: "/elements/simple-table", component: "docs-raul-simple-table" }), h("stencil-route", { url: "/elements/grid", component: "docs-raul-grid" }), h("stencil-route", { url: "/elements/filter-bar", component: "docs-raul-filter-bar" }), h("stencil-route", { url: "/elements/accordion", component: "docs-raul-accordion" }), h("stencil-route", { url: "/elements/avatar", component: "docs-raul-avatar" }), h("stencil-route", { url: "/elements/breadcrumbs", component: "docs-raul-breadcrumbs" }), h("stencil-route", { url: "/elements/toggles", component: "docs-raul-toggles" }), h("stencil-route", { url: "/elements/card", component: "docs-raul-card" }), h("stencil-route", { url: "/elements/progress", component: "docs-raul-progress" }), h("stencil-route", { url: "/elements/footer", component: "docs-raul-footer" }), h("stencil-route", { url: "/elements/tooltip", component: "docs-raul-tooltip" }), h("stencil-route", { component: "docs-404" }))))));
    }
    static get style() { return ".docs-container,body,html{min-height:100%}.docs-container{display:-ms-flexbox;display:flex;-ms-flex-align:stretch;align-items:stretch;min-height:100vh}.docs-page-container,main{-ms-flex:1 1 0%;flex:1 1 0%;-ms-flex-direction:column;flex-direction:column}.docs-page-container{background-color:#f7f8f9;display:-ms-flexbox;display:flex}.docs-page-header{background-color:#f7f8f9;padding:1.5rem}\@media (min-width:1024px){.docs-page-header{padding:3rem}}.docs-page-header .r-content h1,.docs-page-header h1{font-size:2.25rem;font-weight:700;letter-spacing:-.27px;margin-top:12px;padding-top:26px;position:relative}.docs-page-header h1:before{background-color:#f06000;width:19px}.docs-page-header h1:after,.docs-page-header h1:before{border-radius:4px;content:\"\";display:block;height:8px;position:absolute;top:0}.docs-page-header h1:after{background-color:#9ba3a7;width:37px;left:27px}.docs-page-header.tabbed{padding-bottom:0;border-bottom-width:1px;border-color:#ebedee}.docs-page-header.tabbed raul-tabs .tabs{margin:0 0 -1px -15px}.docs-page-header.tabbed raul-tabs .tabs .tabs__item--active{border-bottom-width:2px;border-color:#f06000}.docs-page-content{-ms-flex:1 1 0%;flex:1 1 0%;background-color:#fff;padding:1.5rem}\@media (min-width:1024px){.docs-page-content{padding:3rem}}.docs-cards-container{display:-ms-flexbox;display:flex;-ms-flex-wrap:wrap;flex-wrap:wrap;-ms-flex-align:stretch;align-items:stretch;margin-right:-24px}.docs-cards-container .docs-card-wrapper{padding-right:1.5rem;margin-bottom:1.5rem}.docs-cards-container .docs-card-wrapper .docs-card{display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column;padding:2rem;text-align:center;border-width:1px;border-color:#ebedee;background-color:#fff;height:100%;-webkit-box-shadow:none;box-shadow:none;z-index:0;-webkit-transition:all .3s ease;transition:all .3s ease}.docs-cards-container .docs-card-wrapper .docs-card .docs-card-illustration{display:-ms-flexbox;display:flex;-ms-flex-positive:1;flex-grow:1;-ms-flex-align:center;align-items:center;-ms-flex-item-align:stretch;align-self:stretch;-ms-flex-pack:center;justify-content:center;padding-top:1.5rem;padding-bottom:1.5rem;min-height:140px}.docs-cards-container .docs-card-wrapper:focus>.docs-card,.docs-cards-container .docs-card-wrapper:hover>.docs-card{margin-top:-8px;margin-bottom:8px;-webkit-box-shadow:0 8px 16px rgba(0,0,0,.2);box-shadow:0 8px 16px rgba(0,0,0,.2);z-index:1;-webkit-transition:all .3s ease;transition:all .3s ease}"; }
};

const kindDirectory = {
    family: 'families',
    icon: 'icons',
    product: 'products',
    resource: 'resources'
};
const iconCache = {};
const RaulIcon = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        this.icon = null;
        this.kind = 'icon';
    }
    iconChanged() {
        this.loadSVG();
    }
    componentWillLoad() {
        this.loadSVG();
    }
    async fetchSVG() {
        try {
            const resp = await fetch(`//cdn.realpage.com/images/${kindDirectory[this.kind]}/${this.icon}.svg`, { cache: 'no-store' });
            if (resp.ok) {
                return resp.text();
            }
            else {
                throw new Error(`[RAUL] Failed to fetch icon ${this.kind}/${this.icon}`);
            }
        }
        catch (e) {
            console.error(e);
        }
    }
    async loadSVG() {
        if (this.icon) {
            const key = `${this.kind}-${this.icon}`;
            if (!iconCache[key]) {
                const svg = await this.fetchSVG();
                if (svg) {
                    //this is because svg's can be focused in IE with the tab key and this breaks keyboard/tab navigation
                    iconCache[key] = svg.replace('<svg', '<svg aria-hidden="true" focusable="false" ');
                }
            }
            this.svgContent = iconCache[key];
        }
    }
    render() {
        return (h("i", { class: "r-icon", innerHTML: this.svgContent }));
    }
    static get watchers() { return {
        "icon": ["iconChanged"]
    }; }
    static get style() { return "raul-icon{display:inline-block;vertical-align:middle}raul-icon svg{display:block;height:1em;width:1em}raul-icon svg g[fill=none] circle,raul-icon svg g[fill=none] ellipse,raul-icon svg g[fill=none] path,raul-icon svg g[fill=none] polygon,raul-icon svg g[fill=none] polyline,raul-icon svg g[fill=none] rect,raul-icon svg g[style*=\"fill:none\"] circle,raul-icon svg g[style*=\"fill:none\"] ellipse,raul-icon svg g[style*=\"fill:none\"] path,raul-icon svg g[style*=\"fill:none\"] polygon,raul-icon svg g[style*=\"fill:none\"] polyline,raul-icon svg g[style*=\"fill:none\"] rect{fill:inherit!important}raul-icon svg circle:not([fill=none]):not([style*=\"fill:none\"]),raul-icon svg ellipse:not([fill=none]):not([style*=\"fill:none\"]),raul-icon svg path:not([fill=none]):not([style*=\"fill:none\"]),raul-icon svg polygon:not([fill=none]):not([style*=\"fill:none\"]),raul-icon svg polyline:not([fill=none]):not([style*=\"fill:none\"]),raul-icon svg rect:not([fill=none]):not([style*=\"fill:none\"]){fill:currentColor!important}"; }
};

/**
 * TS adaption of https://github.com/pillarjs/path-to-regexp/blob/master/index.js
 */
/**
 * Default configs.
 */
const DEFAULT_DELIMITER = '/';
const DEFAULT_DELIMITERS = './';
/**
 * The main path matching regexp utility.
 */
const PATH_REGEXP = new RegExp([
    // Match escaped characters that would otherwise appear in future matches.
    // This allows the user to escape special characters that won't transform.
    '(\\\\.)',
    // Match Express-style parameters and un-named parameters with a prefix
    // and optional suffixes. Matches appear as:
    //
    // "/:test(\\d+)?" => ["/", "test", "\d+", undefined, "?"]
    // "/route(\\d+)"  => [undefined, undefined, undefined, "\d+", undefined]
    '(?:\\:(\\w+)(?:\\(((?:\\\\.|[^\\\\()])+)\\))?|\\(((?:\\\\.|[^\\\\()])+)\\))([+*?])?'
].join('|'), 'g');
/**
 * Parse a string for the raw tokens.
 */
const parse = (str, options) => {
    var tokens = [];
    var key = 0;
    var index = 0;
    var path = '';
    var defaultDelimiter = (options && options.delimiter) || DEFAULT_DELIMITER;
    var delimiters = (options && options.delimiters) || DEFAULT_DELIMITERS;
    var pathEscaped = false;
    var res;
    while ((res = PATH_REGEXP.exec(str)) !== null) {
        var m = res[0];
        var escaped = res[1];
        var offset = res.index;
        path += str.slice(index, offset);
        index = offset + m.length;
        // Ignore already escaped sequences.
        if (escaped) {
            path += escaped[1];
            pathEscaped = true;
            continue;
        }
        var prev = '';
        var next = str[index];
        var name = res[2];
        var capture = res[3];
        var group = res[4];
        var modifier = res[5];
        if (!pathEscaped && path.length) {
            var k = path.length - 1;
            if (delimiters.indexOf(path[k]) > -1) {
                prev = path[k];
                path = path.slice(0, k);
            }
        }
        // Push the current path onto the tokens.
        if (path) {
            tokens.push(path);
            path = '';
            pathEscaped = false;
        }
        var partial = prev !== '' && next !== undefined && next !== prev;
        var repeat = modifier === '+' || modifier === '*';
        var optional = modifier === '?' || modifier === '*';
        var delimiter = prev || defaultDelimiter;
        var pattern = capture || group;
        tokens.push({
            name: name || key++,
            prefix: prev,
            delimiter: delimiter,
            optional: optional,
            repeat: repeat,
            partial: partial,
            pattern: pattern ? escapeGroup(pattern) : '[^' + escapeString(delimiter) + ']+?'
        });
    }
    // Push any remaining characters.
    if (path || index < str.length) {
        tokens.push(path + str.substr(index));
    }
    return tokens;
};
/**
 * Escape a regular expression string.
 */
const escapeString = (str) => {
    return str.replace(/([.+*?=^!:${}()[\]|/\\])/g, '\\$1');
};
/**
 * Escape the capturing group by escaping special characters and meaning.
 */
const escapeGroup = (group) => {
    return group.replace(/([=!:$/()])/g, '\\$1');
};
/**
 * Get the flags for a regexp from the options.
 */
const flags = (options) => {
    return options && options.sensitive ? '' : 'i';
};
/**
 * Pull out keys from a regexp.
 */
const regexpToRegexp = (path, keys) => {
    if (!keys)
        return path;
    // Use a negative lookahead to match only capturing groups.
    var groups = path.source.match(/\((?!\?)/g);
    if (groups) {
        for (var i = 0; i < groups.length; i++) {
            keys.push({
                name: i,
                prefix: null,
                delimiter: null,
                optional: false,
                repeat: false,
                partial: false,
                pattern: null
            });
        }
    }
    return path;
};
/**
 * Transform an array into a regexp.
 */
const arrayToRegexp = (path, keys, options) => {
    var parts = [];
    for (var i = 0; i < path.length; i++) {
        parts.push(pathToRegexp(path[i], keys, options).source);
    }
    return new RegExp('(?:' + parts.join('|') + ')', flags(options));
};
/**
 * Create a path regexp from string input.
 */
const stringToRegexp = (path, keys, options) => {
    return tokensToRegExp(parse(path, options), keys, options);
};
/**
 * Expose a function for taking tokens and returning a RegExp.
 */
const tokensToRegExp = (tokens, keys, options) => {
    options = options || {};
    var strict = options.strict;
    var end = options.end !== false;
    var delimiter = escapeString(options.delimiter || DEFAULT_DELIMITER);
    var delimiters = options.delimiters || DEFAULT_DELIMITERS;
    var endsWith = [].concat(options.endsWith || []).map(escapeString).concat('$').join('|');
    var route = '';
    var isEndDelimited = false;
    // Iterate over the tokens and create our regexp string.
    for (var i = 0; i < tokens.length; i++) {
        var token = tokens[i];
        if (typeof token === 'string') {
            route += escapeString(token);
            isEndDelimited = i === tokens.length - 1 && delimiters.indexOf(token[token.length - 1]) > -1;
        }
        else {
            var prefix = escapeString(token.prefix || '');
            var capture = token.repeat
                ? '(?:' + token.pattern + ')(?:' + prefix + '(?:' + token.pattern + '))*'
                : token.pattern;
            if (keys)
                keys.push(token);
            if (token.optional) {
                if (token.partial) {
                    route += prefix + '(' + capture + ')?';
                }
                else {
                    route += '(?:' + prefix + '(' + capture + '))?';
                }
            }
            else {
                route += prefix + '(' + capture + ')';
            }
        }
    }
    if (end) {
        if (!strict)
            route += '(?:' + delimiter + ')?';
        route += endsWith === '$' ? '$' : '(?=' + endsWith + ')';
    }
    else {
        if (!strict)
            route += '(?:' + delimiter + '(?=' + endsWith + '))?';
        if (!isEndDelimited)
            route += '(?=' + delimiter + '|' + endsWith + ')';
    }
    return new RegExp('^' + route, flags(options));
};
/**
 * Normalize the given path string, returning a regular expression.
 *
 * An empty array can be passed in for the keys, which will hold the
 * placeholder key descriptions. For example, using `/user/:id`, `keys` will
 * contain `[{ name: 'id', delimiter: '/', optional: false, repeat: false }]`.
 */
const pathToRegexp = (path, keys, options) => {
    if (path instanceof RegExp) {
        return regexpToRegexp(path, keys);
    }
    if (Array.isArray(path)) {
        return arrayToRegexp(path, keys, options);
    }
    return stringToRegexp(path, keys, options);
};

const hasBasename = (path, prefix) => {
    return (new RegExp('^' + prefix + '(\\/|\\?|#|$)', 'i')).test(path);
};
const stripBasename = (path, prefix) => {
    return hasBasename(path, prefix) ? path.substr(prefix.length) : path;
};
const stripTrailingSlash = (path) => {
    return path.charAt(path.length - 1) === '/' ? path.slice(0, -1) : path;
};
const addLeadingSlash = (path) => {
    return path.charAt(0) === '/' ? path : '/' + path;
};
const stripLeadingSlash = (path) => {
    return path.charAt(0) === '/' ? path.substr(1) : path;
};
const parsePath = (path) => {
    let pathname = path || '/';
    let search = '';
    let hash = '';
    const hashIndex = pathname.indexOf('#');
    if (hashIndex !== -1) {
        hash = pathname.substr(hashIndex);
        pathname = pathname.substr(0, hashIndex);
    }
    const searchIndex = pathname.indexOf('?');
    if (searchIndex !== -1) {
        search = pathname.substr(searchIndex);
        pathname = pathname.substr(0, searchIndex);
    }
    return {
        pathname,
        search: search === '?' ? '' : search,
        hash: hash === '#' ? '' : hash,
        query: {},
        key: ''
    };
};
const createPath = (location) => {
    const { pathname, search, hash } = location;
    let path = pathname || '/';
    if (search && search !== '?') {
        path += (search.charAt(0) === '?' ? search : `?${search}`);
    }
    if (hash && hash !== '#') {
        path += (hash.charAt(0) === '#' ? hash : `#${hash}`);
    }
    return path;
};
const parseQueryString = (query) => {
    if (!query) {
        return {};
    }
    return (/^[?#]/.test(query) ? query.slice(1) : query)
        .split('&')
        .reduce((params, param) => {
        let [key, value] = param.split('=');
        params[key] = value ? decodeURIComponent(value.replace(/\+/g, ' ')) : '';
        return params;
    }, {});
};

const isAbsolute = (pathname) => {
    return pathname.charAt(0) === '/';
};
const createKey = (keyLength) => {
    return Math.random().toString(36).substr(2, keyLength);
};
// About 1.5x faster than the two-arg version of Array#splice()
const spliceOne = (list, index) => {
    for (let i = index, k = i + 1, n = list.length; k < n; i += 1, k += 1) {
        list[i] = list[k];
    }
    list.pop();
};
// This implementation is based heavily on node's url.parse
const resolvePathname = (to, from = '') => {
    let fromParts = from && from.split('/') || [];
    let hasTrailingSlash;
    let up = 0;
    const toParts = to && to.split('/') || [];
    const isToAbs = to && isAbsolute(to);
    const isFromAbs = from && isAbsolute(from);
    const mustEndAbs = isToAbs || isFromAbs;
    if (to && isAbsolute(to)) {
        // to is absolute
        fromParts = toParts;
    }
    else if (toParts.length) {
        // to is relative, drop the filename
        fromParts.pop();
        fromParts = fromParts.concat(toParts);
    }
    if (!fromParts.length) {
        return '/';
    }
    if (fromParts.length) {
        const last = fromParts[fromParts.length - 1];
        hasTrailingSlash = (last === '.' || last === '..' || last === '');
    }
    else {
        hasTrailingSlash = false;
    }
    for (let i = fromParts.length; i >= 0; i--) {
        const part = fromParts[i];
        if (part === '.') {
            spliceOne(fromParts, i);
        }
        else if (part === '..') {
            spliceOne(fromParts, i);
            up++;
        }
        else if (up) {
            spliceOne(fromParts, i);
            up--;
        }
    }
    if (!mustEndAbs) {
        for (; up--; up) {
            fromParts.unshift('..');
        }
    }
    if (mustEndAbs && fromParts[0] !== '' && (!fromParts[0] || !isAbsolute(fromParts[0]))) {
        fromParts.unshift('');
    }
    let result = fromParts.join('/');
    if (hasTrailingSlash && result.substr(-1) !== '/') {
        result += '/';
    }
    return result;
};
const valueEqual = (a, b) => {
    if (a === b) {
        return true;
    }
    if (a == null || b == null) {
        return false;
    }
    if (Array.isArray(a)) {
        return Array.isArray(b) && a.length === b.length && a.every((item, index) => {
            return valueEqual(item, b[index]);
        });
    }
    const aType = typeof a;
    const bType = typeof b;
    if (aType !== bType) {
        return false;
    }
    if (aType === 'object') {
        const aValue = a.valueOf();
        const bValue = b.valueOf();
        if (aValue !== a || bValue !== b) {
            return valueEqual(aValue, bValue);
        }
        const aKeys = Object.keys(a);
        const bKeys = Object.keys(b);
        if (aKeys.length !== bKeys.length) {
            return false;
        }
        return aKeys.every((key) => {
            return valueEqual(a[key], b[key]);
        });
    }
    return false;
};
const locationsAreEqual = (a, b) => {
    return a.pathname === b.pathname &&
        a.search === b.search &&
        a.hash === b.hash &&
        a.key === b.key &&
        valueEqual(a.state, b.state);
};
const createLocation = (path, state, key, currentLocation) => {
    let location;
    if (typeof path === 'string') {
        // Two-arg form: push(path, state)
        location = parsePath(path);
        if (state !== undefined) {
            location.state = state;
        }
    }
    else {
        // One-arg form: push(location)
        location = Object.assign({ pathname: '' }, path);
        if (location.search && location.search.charAt(0) !== '?') {
            location.search = '?' + location.search;
        }
        if (location.hash && location.hash.charAt(0) !== '#') {
            location.hash = '#' + location.hash;
        }
        if (state !== undefined && location.state === undefined) {
            location.state = state;
        }
    }
    try {
        location.pathname = decodeURI(location.pathname);
    }
    catch (e) {
        if (e instanceof URIError) {
            throw new URIError('Pathname "' + location.pathname + '" could not be decoded. ' +
                'This is likely caused by an invalid percent-encoding.');
        }
        else {
            throw e;
        }
    }
    location.key = key;
    if (currentLocation) {
        // Resolve incomplete/relative pathname relative to current location.
        if (!location.pathname) {
            location.pathname = currentLocation.pathname;
        }
        else if (location.pathname.charAt(0) !== '/') {
            location.pathname = resolvePathname(location.pathname, currentLocation.pathname);
        }
    }
    else {
        // When there is no prior location and pathname is empty, set it to /
        if (!location.pathname) {
            location.pathname = '/';
        }
    }
    location.query = parseQueryString(location.search || '');
    return location;
};

let cacheCount = 0;
const patternCache = {};
const cacheLimit = 10000;
// Memoized function for creating the path match regex
const compilePath = (pattern, options) => {
    const cacheKey = `${options.end}${options.strict}`;
    const cache = patternCache[cacheKey] || (patternCache[cacheKey] = {});
    const cachePattern = JSON.stringify(pattern);
    if (cache[cachePattern]) {
        return cache[cachePattern];
    }
    const keys = [];
    const re = pathToRegexp(pattern, keys, options);
    const compiledPattern = { re, keys };
    if (cacheCount < cacheLimit) {
        cache[cachePattern] = compiledPattern;
        cacheCount += 1;
    }
    return compiledPattern;
};
/**
 * Public API for matching a URL pathname to a path pattern.
 */
const matchPath = (pathname, options = {}) => {
    if (typeof options === 'string') {
        options = { path: options };
    }
    const { path = '/', exact = false, strict = false } = options;
    const { re, keys } = compilePath(path, { end: exact, strict });
    const match = re.exec(pathname);
    if (!match) {
        return null;
    }
    const [url, ...values] = match;
    const isExact = pathname === url;
    if (exact && !isExact) {
        return null;
    }
    return {
        path,
        url: path === '/' && url === '' ? '/' : url,
        isExact,
        params: keys.reduce((memo, key, index) => {
            memo[key.name] = values[index];
            return memo;
        }, {})
    };
};
const matchesAreEqual = (a, b) => {
    if (a == null && b == null) {
        return true;
    }
    if (b == null) {
        return false;
    }
    return a && b &&
        a.path === b.path &&
        a.url === b.url &&
        valueEqual(a.params, b.params);
};

const Route = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        this.group = null;
        this.match = null;
        this.componentProps = {};
        this.exact = false;
        this.scrollOnNextRender = false;
        this.previousMatch = null;
    }
    // Identify if the current route is a match.
    computeMatch(newLocation) {
        const isGrouped = this.group != null || (this.el.parentElement != null && this.el.parentElement.tagName.toLowerCase() === 'stencil-route-switch');
        if (!newLocation || isGrouped) {
            return;
        }
        this.previousMatch = this.match;
        return this.match = matchPath(newLocation.pathname, {
            path: this.url,
            exact: this.exact,
            strict: true
        });
    }
    async loadCompleted() {
        let routeViewOptions = {};
        if (this.history && this.history.location.hash) {
            routeViewOptions = {
                scrollToId: this.history.location.hash.substr(1)
            };
        }
        else if (this.scrollTopOffset) {
            routeViewOptions = {
                scrollTopOffset: this.scrollTopOffset
            };
        }
        // After all children have completed then tell switch
        // the provided callback will get executed after this route is in view
        if (typeof this.componentUpdated === 'function') {
            this.componentUpdated(routeViewOptions);
            // If this is an independent route and it matches then routes have updated.
            // If the only change to location is a hash change then do not scroll.
        }
        else if (this.match && !matchesAreEqual(this.match, this.previousMatch) && this.routeViewsUpdated) {
            this.routeViewsUpdated(routeViewOptions);
        }
    }
    async componentDidUpdate() {
        await this.loadCompleted();
    }
    async componentDidLoad() {
        await this.loadCompleted();
    }
    render() {
        // If there is no activeRouter then do not render
        // Check if this route is in the matching URL (for example, a parent route)
        if (!this.match || !this.history) {
            return null;
        }
        // component props defined in route
        // the history api
        // current match data including params
        const childProps = Object.assign({}, this.componentProps, { history: this.history, match: this.match });
        // If there is a routerRender defined then use
        // that and pass the component and component props with it.
        if (this.routeRender) {
            return this.routeRender(Object.assign({}, childProps, { component: this.component }));
        }
        if (this.component) {
            const ChildComponent = this.component;
            return (h(ChildComponent, Object.assign({}, childProps)));
        }
    }
    get el() { return getElement(this); }
    static get watchers() { return {
        "location": ["computeMatch"]
    }; }
    static get style() { return "stencil-route.inactive{display:none}"; }
};
ActiveRouter.injectProps(Route, [
    'location',
    'history',
    'historyType',
    'routeViewsUpdated'
]);

const getConfirmation = (win, message, callback) => (callback(win.confirm(message)));
const isModifiedEvent = (ev) => (ev.metaKey || ev.altKey || ev.ctrlKey || ev.shiftKey);
/**
 * Returns true if the HTML5 history API is supported. Taken from Modernizr.
 *
 * https://github.com/Modernizr/Modernizr/blob/master/LICENSE
 * https://github.com/Modernizr/Modernizr/blob/master/feature-detects/history.js
 * changed to avoid false negatives for Windows Phones: https://github.com/reactjs/react-router/issues/586
 */
const supportsHistory = (win) => {
    const ua = win.navigator.userAgent;
    if ((ua.indexOf('Android 2.') !== -1 || ua.indexOf('Android 4.0') !== -1) &&
        ua.indexOf('Mobile Safari') !== -1 &&
        ua.indexOf('Chrome') === -1 &&
        ua.indexOf('Windows Phone') === -1) {
        return false;
    }
    return win.history && 'pushState' in win.history;
};
/**
 * Returns true if browser fires popstate on hash change.
 * IE10 and IE11 do not.
 */
const supportsPopStateOnHashChange = (nav) => (nav.userAgent.indexOf('Trident') === -1);
/**
 * Returns false if using go(n) with hash history causes a full page reload.
 */
const supportsGoWithoutReloadUsingHash = (nav) => (nav.userAgent.indexOf('Firefox') === -1);
const isExtraneousPopstateEvent = (nav, event) => (event.state === undefined &&
    nav.userAgent.indexOf('CriOS') === -1);
const storageAvailable = (win, type) => {
    const storage = win[type];
    const x = '__storage_test__';
    try {
        storage.setItem(x, x);
        storage.removeItem(x);
        return true;
    }
    catch (e) {
        return e instanceof DOMException && (
        // everything except Firefox
        e.code === 22 ||
            // Firefox
            e.code === 1014 ||
            // test name field too, because code might not be present
            // everything except Firefox
            e.name === 'QuotaExceededError' ||
            // Firefox
            e.name === 'NS_ERROR_DOM_QUOTA_REACHED') &&
            // acknowledge QuotaExceededError only if there's something already stored
            storage.length !== 0;
    }
};

const getUrl = (url, root) => {
    // Don't allow double slashes
    if (url.charAt(0) == '/' && root.charAt(root.length - 1) == '/') {
        return root.slice(0, root.length - 1) + url;
    }
    return root + url;
};
const RouteLink = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        this.unsubscribe = () => { return; };
        this.activeClass = 'link-active';
        this.exact = false;
        this.strict = true;
        /**
          *  Custom tag to use instead of an anchor
          */
        this.custom = 'a';
        this.match = null;
    }
    componentWillLoad() {
        this.computeMatch();
    }
    // Identify if the current route is a match.
    computeMatch() {
        if (this.location) {
            this.match = matchPath(this.location.pathname, {
                path: this.urlMatch || this.url,
                exact: this.exact,
                strict: this.strict
            });
        }
    }
    handleClick(e) {
        if (isModifiedEvent(e) || !this.history || !this.url || !this.root) {
            return;
        }
        e.preventDefault();
        return this.history.push(getUrl(this.url, this.root));
    }
    // Get the URL for this route link without the root from the router
    render() {
        let anchorAttributes = {
            class: {
                [this.activeClass]: this.match !== null,
            },
            onClick: this.handleClick.bind(this)
        };
        if (this.anchorClass) {
            anchorAttributes.class[this.anchorClass] = true;
        }
        if (this.custom === 'a') {
            anchorAttributes = Object.assign({}, anchorAttributes, { href: this.url, title: this.anchorTitle, role: this.anchorRole, tabindex: this.anchorTabIndex, 'aria-haspopup': this.ariaHaspopup, id: this.anchorId, 'aria-posinset': this.ariaPosinset, 'aria-setsize': this.ariaSetsize, 'aria-label': this.ariaLabel });
        }
        return (h(this.custom, Object.assign({}, anchorAttributes), h("slot", null)));
    }
    get el() { return getElement(this); }
    static get watchers() { return {
        "location": ["computeMatch"]
    }; }
};
ActiveRouter.injectProps(RouteLink, [
    'history',
    'location',
    'root'
]);

const getUniqueId = () => {
    return ((Math.random() * 10e16).toString().match(/.{4}/g) || []).join('-');
};
const getMatch = (pathname, url, exact) => {
    return matchPath(pathname, {
        path: url,
        exact: exact,
        strict: true
    });
};
const isHTMLStencilRouteElement = (elm) => {
    return elm.tagName === 'STENCIL-ROUTE';
};
const RouteSwitch = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        this.group = getUniqueId();
        this.subscribers = [];
        this.queue = getContext(this, "queue");
    }
    componentWillLoad() {
        if (this.location != null) {
            this.regenerateSubscribers(this.location);
        }
    }
    async regenerateSubscribers(newLocation) {
        if (newLocation == null) {
            return;
        }
        let newActiveIndex = -1;
        this.subscribers = Array.prototype.slice.call(this.el.children)
            .filter(isHTMLStencilRouteElement)
            .map((childElement, index) => {
            const match = getMatch(newLocation.pathname, childElement.url, childElement.exact);
            if (match && newActiveIndex === -1) {
                newActiveIndex = index;
            }
            return {
                el: childElement,
                match: match
            };
        });
        if (newActiveIndex === -1) {
            return;
        }
        // Check if this actually changes which child is active
        // then just pass the new match down if the active route isn't changing.
        if (this.activeIndex === newActiveIndex) {
            this.subscribers[newActiveIndex].el.match = this.subscribers[newActiveIndex].match;
            return;
        }
        this.activeIndex = newActiveIndex;
        // Set all props on the new active route then wait until it says that it
        // is completed
        const activeChild = this.subscribers[this.activeIndex];
        if (this.scrollTopOffset) {
            activeChild.el.scrollTopOffset = this.scrollTopOffset;
        }
        activeChild.el.group = this.group;
        activeChild.el.match = activeChild.match;
        activeChild.el.componentUpdated = (routeViewUpdatedOptions) => {
            // After the new active route has completed then update visibility of routes
            this.queue.write(() => {
                this.subscribers.forEach((child, index) => {
                    child.el.componentUpdated = undefined;
                    if (index === this.activeIndex) {
                        return child.el.style.display = '';
                    }
                    if (this.scrollTopOffset) {
                        child.el.scrollTopOffset = this.scrollTopOffset;
                    }
                    child.el.group = this.group;
                    child.el.match = null;
                    child.el.style.display = 'none';
                });
            });
            if (this.routeViewsUpdated) {
                this.routeViewsUpdated(Object.assign({ scrollTopOffset: this.scrollTopOffset }, routeViewUpdatedOptions));
            }
        };
    }
    render() {
        return (h("slot", null));
    }
    get el() { return getElement(this); }
    static get watchers() { return {
        "location": ["regenerateSubscribers"]
    }; }
};
ActiveRouter.injectProps(RouteSwitch, [
    'location',
    'routeViewsUpdated'
]);

const warning = (value, ...args) => {
    if (!value) {
        console.warn(...args);
    }
};

// Adapted from the https://github.com/ReactTraining/history and converted to TypeScript
const createTransitionManager = () => {
    let prompt;
    let listeners = [];
    const setPrompt = (nextPrompt) => {
        warning(prompt == null, 'A history supports only one prompt at a time');
        prompt = nextPrompt;
        return () => {
            if (prompt === nextPrompt) {
                prompt = null;
            }
        };
    };
    const confirmTransitionTo = (location, action, getUserConfirmation, callback) => {
        // TODO: If another transition starts while we're still confirming
        // the previous one, we may end up in a weird state. Figure out the
        // best way to handle this.
        if (prompt != null) {
            const result = typeof prompt === 'function' ? prompt(location, action) : prompt;
            if (typeof result === 'string') {
                if (typeof getUserConfirmation === 'function') {
                    getUserConfirmation(result, callback);
                }
                else {
                    warning(false, 'A history needs a getUserConfirmation function in order to use a prompt message');
                    callback(true);
                }
            }
            else {
                // Return false from a transition hook to cancel the transition.
                callback(result !== false);
            }
        }
        else {
            callback(true);
        }
    };
    const appendListener = (fn) => {
        let isActive = true;
        const listener = (...args) => {
            if (isActive) {
                fn(...args);
            }
        };
        listeners.push(listener);
        return () => {
            isActive = false;
            listeners = listeners.filter(item => item !== listener);
        };
    };
    const notifyListeners = (...args) => {
        listeners.forEach(listener => listener(...args));
    };
    return {
        setPrompt,
        confirmTransitionTo,
        appendListener,
        notifyListeners
    };
};

const createScrollHistory = (win, applicationScrollKey = 'scrollPositions') => {
    let scrollPositions = new Map();
    const set = (key, value) => {
        scrollPositions.set(key, value);
        if (storageAvailable(win, 'sessionStorage')) {
            const arrayData = [];
            scrollPositions.forEach((value, key) => {
                arrayData.push([key, value]);
            });
            win.sessionStorage.setItem('scrollPositions', JSON.stringify(arrayData));
        }
    };
    const get = (key) => {
        return scrollPositions.get(key);
    };
    const has = (key) => {
        return scrollPositions.has(key);
    };
    const capture = (key) => {
        set(key, [win.scrollX, win.scrollY]);
    };
    if (storageAvailable(win, 'sessionStorage')) {
        const scrollData = win.sessionStorage.getItem(applicationScrollKey);
        scrollPositions = scrollData ?
            new Map(JSON.parse(scrollData)) :
            scrollPositions;
    }
    if ('scrollRestoration' in win.history) {
        history.scrollRestoration = 'manual';
    }
    return {
        set,
        get,
        has,
        capture
    };
};

// Adapted from the https://github.com/ReactTraining/history and converted to TypeScript
const PopStateEvent = 'popstate';
const HashChangeEvent = 'hashchange';
/**
 * Creates a history object that uses the HTML5 history API including
 * pushState, replaceState, and the popstate event.
 */
const createBrowserHistory = (win, props = {}) => {
    let forceNextPop = false;
    const globalHistory = win.history;
    const globalLocation = win.location;
    const globalNavigator = win.navigator;
    const canUseHistory = supportsHistory(win);
    const needsHashChangeListener = !supportsPopStateOnHashChange(globalNavigator);
    const scrollHistory = createScrollHistory(win);
    const forceRefresh = (props.forceRefresh != null) ? props.forceRefresh : false;
    const getUserConfirmation = (props.getUserConfirmation != null) ? props.getUserConfirmation : getConfirmation;
    const keyLength = (props.keyLength != null) ? props.keyLength : 6;
    const basename = props.basename ? stripTrailingSlash(addLeadingSlash(props.basename)) : '';
    const getHistoryState = () => {
        try {
            return win.history.state || {};
        }
        catch (e) {
            // IE 11 sometimes throws when accessing window.history.state
            // See https://github.com/ReactTraining/history/pull/289
            return {};
        }
    };
    const getDOMLocation = (historyState) => {
        historyState = historyState || {};
        const { key, state } = historyState;
        const { pathname, search, hash } = globalLocation;
        let path = pathname + search + hash;
        warning((!basename || hasBasename(path, basename)), 'You are attempting to use a basename on a page whose URL path does not begin ' +
            'with the basename. Expected path "' + path + '" to begin with "' + basename + '".');
        if (basename) {
            path = stripBasename(path, basename);
        }
        return createLocation(path, state, key || createKey(keyLength));
    };
    const transitionManager = createTransitionManager();
    const setState = (nextState) => {
        // Capture location for the view before changing history.
        scrollHistory.capture(history.location.key);
        Object.assign(history, nextState);
        // Set scroll position based on its previous storage value
        history.location.scrollPosition = scrollHistory.get(history.location.key);
        history.length = globalHistory.length;
        transitionManager.notifyListeners(history.location, history.action);
    };
    const handlePopState = (event) => {
        // Ignore extraneous popstate events in WebKit.
        if (!isExtraneousPopstateEvent(globalNavigator, event)) {
            handlePop(getDOMLocation(event.state));
        }
    };
    const handleHashChange = () => {
        handlePop(getDOMLocation(getHistoryState()));
    };
    const handlePop = (location) => {
        if (forceNextPop) {
            forceNextPop = false;
            setState();
        }
        else {
            const action = 'POP';
            transitionManager.confirmTransitionTo(location, action, getUserConfirmation, (ok) => {
                if (ok) {
                    setState({ action, location });
                }
                else {
                    revertPop(location);
                }
            });
        }
    };
    const revertPop = (fromLocation) => {
        const toLocation = history.location;
        // TODO: We could probably make this more reliable by
        // keeping a list of keys we've seen in sessionStorage.
        // Instead, we just default to 0 for keys we don't know.
        let toIndex = allKeys.indexOf(toLocation.key);
        let fromIndex = allKeys.indexOf(fromLocation.key);
        if (toIndex === -1) {
            toIndex = 0;
        }
        if (fromIndex === -1) {
            fromIndex = 0;
        }
        const delta = toIndex - fromIndex;
        if (delta) {
            forceNextPop = true;
            go(delta);
        }
    };
    const initialLocation = getDOMLocation(getHistoryState());
    let allKeys = [initialLocation.key];
    let listenerCount = 0;
    let isBlocked = false;
    // Public interface
    const createHref = (location) => {
        return basename + createPath(location);
    };
    const push = (path, state) => {
        warning(!(typeof path === 'object' && path.state !== undefined && state !== undefined), 'You should avoid providing a 2nd state argument to push when the 1st ' +
            'argument is a location-like object that already has state; it is ignored');
        const action = 'PUSH';
        const location = createLocation(path, state, createKey(keyLength), history.location);
        transitionManager.confirmTransitionTo(location, action, getUserConfirmation, (ok) => {
            if (!ok) {
                return;
            }
            const href = createHref(location);
            const { key, state } = location;
            if (canUseHistory) {
                globalHistory.pushState({ key, state }, '', href);
                if (forceRefresh) {
                    globalLocation.href = href;
                }
                else {
                    const prevIndex = allKeys.indexOf(history.location.key);
                    const nextKeys = allKeys.slice(0, prevIndex === -1 ? 0 : prevIndex + 1);
                    nextKeys.push(location.key);
                    allKeys = nextKeys;
                    setState({ action, location });
                }
            }
            else {
                warning(state === undefined, 'Browser history cannot push state in browsers that do not support HTML5 history');
                globalLocation.href = href;
            }
        });
    };
    const replace = (path, state) => {
        warning(!(typeof path === 'object' && path.state !== undefined && state !== undefined), 'You should avoid providing a 2nd state argument to replace when the 1st ' +
            'argument is a location-like object that already has state; it is ignored');
        const action = 'REPLACE';
        const location = createLocation(path, state, createKey(keyLength), history.location);
        transitionManager.confirmTransitionTo(location, action, getUserConfirmation, (ok) => {
            if (!ok) {
                return;
            }
            const href = createHref(location);
            const { key, state } = location;
            if (canUseHistory) {
                globalHistory.replaceState({ key, state }, '', href);
                if (forceRefresh) {
                    globalLocation.replace(href);
                }
                else {
                    const prevIndex = allKeys.indexOf(history.location.key);
                    if (prevIndex !== -1) {
                        allKeys[prevIndex] = location.key;
                    }
                    setState({ action, location });
                }
            }
            else {
                warning(state === undefined, 'Browser history cannot replace state in browsers that do not support HTML5 history');
                globalLocation.replace(href);
            }
        });
    };
    const go = (n) => {
        globalHistory.go(n);
    };
    const goBack = () => go(-1);
    const goForward = () => go(1);
    const checkDOMListeners = (delta) => {
        listenerCount += delta;
        if (listenerCount === 1) {
            win.addEventListener(PopStateEvent, handlePopState);
            if (needsHashChangeListener) {
                win.addEventListener(HashChangeEvent, handleHashChange);
            }
        }
        else if (listenerCount === 0) {
            win.removeEventListener(PopStateEvent, handlePopState);
            if (needsHashChangeListener) {
                win.removeEventListener(HashChangeEvent, handleHashChange);
            }
        }
    };
    const block = (prompt = '') => {
        const unblock = transitionManager.setPrompt(prompt);
        if (!isBlocked) {
            checkDOMListeners(1);
            isBlocked = true;
        }
        return () => {
            if (isBlocked) {
                isBlocked = false;
                checkDOMListeners(-1);
            }
            return unblock();
        };
    };
    const listen = (listener) => {
        const unlisten = transitionManager.appendListener(listener);
        checkDOMListeners(1);
        return () => {
            checkDOMListeners(-1);
            unlisten();
        };
    };
    const history = {
        length: globalHistory.length,
        action: 'POP',
        location: initialLocation,
        createHref,
        push,
        replace,
        go,
        goBack,
        goForward,
        block,
        listen,
        win: win
    };
    return history;
};

// Adapted from the https://github.com/ReactTraining/history and converted to TypeScript
const HashChangeEvent$1 = 'hashchange';
const HashPathCoders = {
    hashbang: {
        encodePath: (path) => path.charAt(0) === '!' ? path : '!/' + stripLeadingSlash(path),
        decodePath: (path) => path.charAt(0) === '!' ? path.substr(1) : path
    },
    noslash: {
        encodePath: stripLeadingSlash,
        decodePath: addLeadingSlash
    },
    slash: {
        encodePath: addLeadingSlash,
        decodePath: addLeadingSlash
    }
};
const createHashHistory = (win, props = {}) => {
    let forceNextPop = false;
    let ignorePath = null;
    let listenerCount = 0;
    let isBlocked = false;
    const globalLocation = win.location;
    const globalHistory = win.history;
    const canGoWithoutReload = supportsGoWithoutReloadUsingHash(win.navigator);
    const keyLength = (props.keyLength != null) ? props.keyLength : 6;
    const { getUserConfirmation = getConfirmation, hashType = 'slash' } = props;
    const basename = props.basename ? stripTrailingSlash(addLeadingSlash(props.basename)) : '';
    const { encodePath, decodePath } = HashPathCoders[hashType];
    const getHashPath = () => {
        // We can't use window.location.hash here because it's not
        // consistent across browsers - Firefox will pre-decode it!
        const href = globalLocation.href;
        const hashIndex = href.indexOf('#');
        return hashIndex === -1 ? '' : href.substring(hashIndex + 1);
    };
    const pushHashPath = (path) => (globalLocation.hash = path);
    const replaceHashPath = (path) => {
        const hashIndex = globalLocation.href.indexOf('#');
        globalLocation.replace(globalLocation.href.slice(0, hashIndex >= 0 ? hashIndex : 0) + '#' + path);
    };
    const getDOMLocation = () => {
        let path = decodePath(getHashPath());
        warning((!basename || hasBasename(path, basename)), 'You are attempting to use a basename on a page whose URL path does not begin ' +
            'with the basename. Expected path "' + path + '" to begin with "' + basename + '".');
        if (basename) {
            path = stripBasename(path, basename);
        }
        return createLocation(path, undefined, createKey(keyLength));
    };
    const transitionManager = createTransitionManager();
    const setState = (nextState) => {
        Object.assign(history, nextState);
        history.length = globalHistory.length;
        transitionManager.notifyListeners(history.location, history.action);
    };
    const handleHashChange = () => {
        const path = getHashPath();
        const encodedPath = encodePath(path);
        if (path !== encodedPath) {
            // Ensure we always have a properly-encoded hash.
            replaceHashPath(encodedPath);
        }
        else {
            const location = getDOMLocation();
            const prevLocation = history.location;
            if (!forceNextPop && locationsAreEqual(prevLocation, location)) {
                return; // A hashchange doesn't always == location change.
            }
            if (ignorePath === createPath(location)) {
                return; // Ignore this change; we already setState in push/replace.
            }
            ignorePath = null;
            handlePop(location);
        }
    };
    const handlePop = (location) => {
        if (forceNextPop) {
            forceNextPop = false;
            setState();
        }
        else {
            const action = 'POP';
            transitionManager.confirmTransitionTo(location, action, getUserConfirmation, (ok) => {
                if (ok) {
                    setState({ action, location });
                }
                else {
                    revertPop(location);
                }
            });
        }
    };
    const revertPop = (fromLocation) => {
        const toLocation = history.location;
        // TODO: We could probably make this more reliable by
        // keeping a list of paths we've seen in sessionStorage.
        // Instead, we just default to 0 for paths we don't know.
        let toIndex = allPaths.lastIndexOf(createPath(toLocation));
        let fromIndex = allPaths.lastIndexOf(createPath(fromLocation));
        if (toIndex === -1) {
            toIndex = 0;
        }
        if (fromIndex === -1) {
            fromIndex = 0;
        }
        const delta = toIndex - fromIndex;
        if (delta) {
            forceNextPop = true;
            go(delta);
        }
    };
    // Ensure the hash is encoded properly before doing anything else.
    const path = getHashPath();
    const encodedPath = encodePath(path);
    if (path !== encodedPath) {
        replaceHashPath(encodedPath);
    }
    const initialLocation = getDOMLocation();
    let allPaths = [createPath(initialLocation)];
    // Public interface
    const createHref = (location) => ('#' + encodePath(basename + createPath(location)));
    const push = (path, state) => {
        warning(state === undefined, 'Hash history cannot push state; it is ignored');
        const action = 'PUSH';
        const location = createLocation(path, undefined, createKey(keyLength), history.location);
        transitionManager.confirmTransitionTo(location, action, getUserConfirmation, (ok) => {
            if (!ok) {
                return;
            }
            const path = createPath(location);
            const encodedPath = encodePath(basename + path);
            const hashChanged = getHashPath() !== encodedPath;
            if (hashChanged) {
                // We cannot tell if a hashchange was caused by a PUSH, so we'd
                // rather setState here and ignore the hashchange. The caveat here
                // is that other hash histories in the page will consider it a POP.
                ignorePath = path;
                pushHashPath(encodedPath);
                const prevIndex = allPaths.lastIndexOf(createPath(history.location));
                const nextPaths = allPaths.slice(0, prevIndex === -1 ? 0 : prevIndex + 1);
                nextPaths.push(path);
                allPaths = nextPaths;
                setState({ action, location });
            }
            else {
                warning(false, 'Hash history cannot PUSH the same path; a new entry will not be added to the history stack');
                setState();
            }
        });
    };
    const replace = (path, state) => {
        warning(state === undefined, 'Hash history cannot replace state; it is ignored');
        const action = 'REPLACE';
        const location = createLocation(path, undefined, createKey(keyLength), history.location);
        transitionManager.confirmTransitionTo(location, action, getUserConfirmation, (ok) => {
            if (!ok) {
                return;
            }
            const path = createPath(location);
            const encodedPath = encodePath(basename + path);
            const hashChanged = getHashPath() !== encodedPath;
            if (hashChanged) {
                // We cannot tell if a hashchange was caused by a REPLACE, so we'd
                // rather setState here and ignore the hashchange. The caveat here
                // is that other hash histories in the page will consider it a POP.
                ignorePath = path;
                replaceHashPath(encodedPath);
            }
            const prevIndex = allPaths.indexOf(createPath(history.location));
            if (prevIndex !== -1) {
                allPaths[prevIndex] = path;
            }
            setState({ action, location });
        });
    };
    const go = (n) => {
        warning(canGoWithoutReload, 'Hash history go(n) causes a full page reload in this browser');
        globalHistory.go(n);
    };
    const goBack = () => go(-1);
    const goForward = () => go(1);
    const checkDOMListeners = (win, delta) => {
        listenerCount += delta;
        if (listenerCount === 1) {
            win.addEventListener(HashChangeEvent$1, handleHashChange);
        }
        else if (listenerCount === 0) {
            win.removeEventListener(HashChangeEvent$1, handleHashChange);
        }
    };
    const block = (prompt = '') => {
        const unblock = transitionManager.setPrompt(prompt);
        if (!isBlocked) {
            checkDOMListeners(win, 1);
            isBlocked = true;
        }
        return () => {
            if (isBlocked) {
                isBlocked = false;
                checkDOMListeners(win, -1);
            }
            return unblock();
        };
    };
    const listen = (listener) => {
        const unlisten = transitionManager.appendListener(listener);
        checkDOMListeners(win, 1);
        return () => {
            checkDOMListeners(win, -1);
            unlisten();
        };
    };
    const history = {
        length: globalHistory.length,
        action: 'POP',
        location: initialLocation,
        createHref,
        push,
        replace,
        go,
        goBack,
        goForward,
        block,
        listen,
        win: win
    };
    return history;
};

const getLocation = (location, root) => {
    // Remove the root URL if found at beginning of string
    const pathname = location.pathname.indexOf(root) == 0 ?
        '/' + location.pathname.slice(root.length) :
        location.pathname;
    return Object.assign({}, location, { pathname });
};
const HISTORIES = {
    'browser': createBrowserHistory,
    'hash': createHashHistory
};
const Router = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        this.root = '/';
        this.historyType = 'browser';
        // A suffix to append to the page title whenever
        // it's updated through RouteTitle
        this.titleSuffix = '';
        this.routeViewsUpdated = (options = {}) => {
            if (this.history && options.scrollToId && this.historyType === 'browser') {
                const elm = this.history.win.document.getElementById(options.scrollToId);
                if (elm) {
                    return elm.scrollIntoView();
                }
            }
            this.scrollTo(options.scrollTopOffset || this.scrollTopOffset);
        };
        this.isServer = getContext(this, "isServer");
        this.queue = getContext(this, "queue");
    }
    componentWillLoad() {
        this.history = HISTORIES[this.historyType](this.el.ownerDocument.defaultView);
        this.history.listen((location) => {
            location = getLocation(location, this.root);
            this.location = location;
        });
        this.location = getLocation(this.history.location, this.root);
    }
    scrollTo(scrollToLocation) {
        const history = this.history;
        if (scrollToLocation == null || this.isServer || !history) {
            return;
        }
        if (history.action === 'POP' && Array.isArray(history.location.scrollPosition)) {
            return this.queue.write(() => {
                if (history && history.location && Array.isArray(history.location.scrollPosition)) {
                    history.win.scrollTo(history.location.scrollPosition[0], history.location.scrollPosition[1]);
                }
            });
        }
        // okay, the frame has passed. Go ahead and render now
        return this.queue.write(() => {
            history.win.scrollTo(0, scrollToLocation);
        });
    }
    render() {
        if (!this.location || !this.history) {
            return;
        }
        const state = {
            historyType: this.historyType,
            location: this.location,
            titleSuffix: this.titleSuffix,
            root: this.root,
            history: this.history,
            routeViewsUpdated: this.routeViewsUpdated
        };
        return (h(ActiveRouter.Provider, { state: state }, h("slot", null)));
    }
    get el() { return getElement(this); }
};

export { DocsMenu as docs_menu, DocsMenuElemPage as docs_menu_elem_page, DocsMenuPage as docs_menu_page, DocsMenuSection as docs_menu_section, DocsShell as docs_shell, RaulIcon as raul_icon, Route as stencil_route, RouteLink as stencil_route_link, RouteSwitch as stencil_route_switch, Router as stencil_router };
