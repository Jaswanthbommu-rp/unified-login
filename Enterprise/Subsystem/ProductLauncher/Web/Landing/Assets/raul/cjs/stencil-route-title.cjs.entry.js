'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const activeRouter = require('./active-router-058f7bb0.js');

const RouteTitle = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.titleSuffix = '';
        this.pageTitle = '';
    }
    updateDocumentTitle() {
        const el = this.el;
        if (el.ownerDocument) {
            el.ownerDocument.title = `${this.pageTitle}${this.titleSuffix || ''}`;
        }
    }
    componentWillLoad() {
        this.updateDocumentTitle();
    }
    get el() { return core.getElement(this); }
    static get watchers() { return {
        "pageTitle": ["updateDocumentTitle"]
    }; }
};
activeRouter.ActiveRouter.injectProps(RouteTitle, [
    'titleSuffix',
]);

exports.stencil_route_title = RouteTitle;
