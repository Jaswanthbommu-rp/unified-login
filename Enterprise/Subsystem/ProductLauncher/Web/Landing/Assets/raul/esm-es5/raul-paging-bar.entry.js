import { r as registerInstance, c as createEvent, h } from './core-9263a98c.js';
var RaulPagingBar = /** @class */ (function () {
    function RaulPagingBar(hostRef) {
        registerInstance(this, hostRef);
        this.currentPage = 1;
        this.entities = [10, 20, 30, 40, 50];
        this.totalRows = 0;
        this.rowsPerPage = 10;
        this.pagingChange = createEvent(this, "pagingChange", 7);
    }
    RaulPagingBar.prototype.componentDidLoad = function () {
        this.totalPages = Math.ceil(this.totalRows / this.rowsPerPage);
        this.startRow = ((this.currentPage - 1) * this.rowsPerPage) + 1;
        this.endRow = (((this.currentPage - 1) * this.rowsPerPage) + this.rowsPerPage) > this.totalRows ? this.totalRows : ((this.currentPage - 1) * this.rowsPerPage) + this.rowsPerPage;
        this.validateRowsPerPage();
    };
    RaulPagingBar.prototype.decrement = function () {
        this.currentPage = this.currentPage - 1;
        this.updateBar('previousPage');
    };
    RaulPagingBar.prototype.entitesChanged = function (event) {
        this.rowsPerPage = Number(event.target.value);
        this.currentPage = 1;
        this.updateBar('entitiesChange');
    };
    RaulPagingBar.prototype.increment = function () {
        this.currentPage = this.currentPage + 1;
        this.updateBar('nextPage');
    };
    RaulPagingBar.prototype.pageValueChange = function (event) {
        var val = Number(event.target.value);
        if (val > this.totalPages || val < 1) {
            event.target.value = this.currentPage;
        }
        else {
            this.currentPage = val;
            this.updateBar('pageValueChange');
        }
    };
    RaulPagingBar.prototype.updateBar = function (event) {
        this.totalPages = Math.ceil(this.totalRows / this.rowsPerPage);
        this.startRow = ((this.currentPage - 1) * this.rowsPerPage) + 1;
        var calcEndRow = ((this.currentPage - 1) * this.rowsPerPage) + this.rowsPerPage;
        this.endRow = calcEndRow > this.totalRows ? this.totalRows : calcEndRow;
        if (this.validateRowsPerPage()) {
            this.pagingChange.emit({
                event: event,
                currentPage: this.currentPage,
                rowsPerPage: this.rowsPerPage,
                totalPages: this.totalPages,
                startRow: this.startRow,
                endRow: this.endRow
            });
        }
    };
    RaulPagingBar.prototype.validateRowsPerPage = function () {
        var entitiesArrayToString = this.entities.toString();
        if (!this.entities.includes(this.rowsPerPage)) {
            console.error("RAUL Paging Bar Component ERROR: rowsPerPage does not match entities array. rowsPerPage is set to " + this.rowsPerPage + " but available entities are: " + entitiesArrayToString);
        }
        return this.entities.includes(this.rowsPerPage);
    };
    RaulPagingBar.prototype.render = function () {
        var _this = this;
        return (h("div", { class: "r-paging-bar" }, h("div", { class: "r-paging-bar__col r-paging-bar__col--entries" }, "Show", h("div", { class: "r-paging-bar__entry-select" }, h("select", { onChange: function (event) { return _this.entitesChanged(event); } }, this.entities && this.entities.map(function (entity) { return (h("option", { value: entity, selected: _this.rowsPerPage === entity }, entity)); })), h("raul-icon", { class: "r-select__arrow", icon: "arrow-down-v" })), "entries"), h("div", { class: "r-paging-bar__col r-paging-bar__col--count" }, this.startRow, " - ", this.endRow, " of ", this.totalRows), h("div", { class: "r-paging-bar__col r-paging-bar__col--nav" }, h("button", { type: "button", "aria-label": "Previous Entries", onClick: function () { return _this.decrement(); }, disabled: this.currentPage === 1 }, h("raul-icon", { icon: "arrow-left-v" })), h("div", { class: "r-paging-bar__nav" }, "Page", h("div", { class: "r-paging-bar__input" }, h("input", { type: "number", min: "1", max: this.totalPages, value: this.currentPage, onChange: function (event) { return _this.pageValueChange(event); } })), "of ", this.totalPages), h("button", { type: "button", "aria-label": "Next Entries", onClick: function () { return _this.increment(); }, disabled: this.currentPage === this.totalPages }, h("raul-icon", { icon: "arrow-right-v" })))));
    };
    Object.defineProperty(RaulPagingBar, "style", {
        get: function () { return "raul-paging-bar{display:block}raul-paging-bar .r-paging-bar{display:-ms-flexbox;display:flex;-ms-flex-align:center;align-items:center;-ms-flex-wrap:nowrap;flex-wrap:nowrap;font-size:.75rem;border-bottom:1px solid #ebedee}raul-paging-bar .r-paging-bar__col{max-width:100%;padding-top:.5rem;padding-bottom:.5rem;padding-right:.75rem;-ms-flex:0 0 100%;flex:0 0 100%}\@media (min-width:768px){raul-paging-bar .r-paging-bar__col{-ms-flex:0 0 33.3333333333%;flex:0 0 33.3333333333%;max-width:33.3333333333%}}raul-paging-bar .r-paging-bar__col--entries{display:none}\@media (min-width:768px){raul-paging-bar .r-paging-bar__col--entries{display:block;-ms-flex:0 0 33.3333333333%;flex:0 0 33.3333333333%;max-width:33.3333333333%}}raul-paging-bar .r-paging-bar__col--count{display:none;text-align:center}\@media (min-width:768px){raul-paging-bar .r-paging-bar__col--count{display:block;-ms-flex:0 0 33.3333333333%;flex:0 0 33.3333333333%;max-width:33.3333333333%}}raul-paging-bar .r-paging-bar__col--nav{display:-ms-flexbox;display:flex;-ms-flex-align:center;align-items:center;-ms-flex-pack:center;justify-content:center;text-align:center}\@media (min-width:768px){raul-paging-bar .r-paging-bar__col--nav{-ms-flex-pack:end;justify-content:flex-end;padding-right:0;text-align:right;-ms-flex:0 0 33.3333333333%;flex:0 0 33.3333333333%;max-width:33.3333333333%}}raul-paging-bar .r-paging-bar__col--nav button{position:relative;vertical-align:middle;padding-top:0;padding-bottom:0;padding-left:.75rem;padding-right:.75rem;margin-top:.25rem;font-size:.875rem;line-height:1}raul-paging-bar .r-paging-bar__col--nav button[disabled]{opacity:.3}body[modality=keyboard] raul-paging-bar .r-paging-bar__col--nav button:focus{outline:0}body[modality=keyboard] raul-paging-bar .r-paging-bar__col--nav button:focus:before{display:none;position:absolute;border:1px solid #0076cc;border-radius:3px;right:8px;content:\"\";height:24px;top:-3px;width:24px}raul-paging-bar .r-paging-bar__entry-select{display:inline-block;background-color:transparent;position:relative;vertical-align:baseline;margin-top:0;margin-bottom:0;margin-left:.25rem;margin-right:.25rem}raul-paging-bar .r-paging-bar__entry-select .r-select__arrow{position:absolute;color:#0076cc;right:2px;top:.5rem}raul-paging-bar .r-paging-bar__entry-select select{-webkit-appearance:none;-moz-appearance:none;appearance:none;background-color:transparent;position:relative;border-radius:.25rem;padding-left:.5rem;padding-right:.75rem;padding-top:.25rem;padding-bottom:.25rem;z-index:10;border-width:1px;border-color:transparent;color:#0076cc;width:42px}raul-paging-bar .r-paging-bar__entry-select select::-ms-expand{display:none}body[modality=keyboard] raul-paging-bar .r-paging-bar__entry-select select:focus{outline:0;border-radius:.25rem;border-color:#0076cc;border-radius:3px}raul-paging-bar .r-paging-bar select:focus~.r-paging-bar__entry-select{border-color:#0076cc}raul-paging-bar .r-paging-bar__nav{display:inline-block}raul-paging-bar .r-paging-bar__input{display:inline-block;margin:0 4px;width:40px}raul-paging-bar .r-paging-bar__input input[type=number]{text-align:center;width:100%;border:1px solid #c6ccd0;border-radius:2px;color:#0076cc;padding:2px 6px;-webkit-transition:all .35s;transition:all .35s}raul-paging-bar .r-paging-bar__input input[type=number]:focus{outline:0;border-color:#0076cc}raul-paging-bar .r-paging-bar__input input[type=number]::-webkit-inner-spin-button,raul-paging-bar .r-paging-bar__input input[type=number]::-webkit-outer-spin-button{-webkit-appearance:none;appearance:none;margin:0}"; },
        enumerable: true,
        configurable: true
    });
    return RaulPagingBar;
}());
export { RaulPagingBar as raul_paging_bar };
