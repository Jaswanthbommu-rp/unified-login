'use strict';

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

/*jshint esversion: 6 */

var Page = function () {
    function Page() {
        _classCallCheck(this, Page);
    }

    _createClass(Page, [{
        key: 'createPageTab',
        value: function createPageTab(id, title, childNodes, tabsContainer) {
            var _this2 = this;

            var page = $('<li data-drilldown-item class="pages-tab w-100">\n                        <a href="javascript:void(0);" data-tabname="page-' + id + '" data-toggle="pill" id="' + id + '">' + title + '</a>\n                      </li>');

            tabsContainer.append(page);

            /*Create subLists*/
            if (childNodes.length) {
                var subList = $('<ul id="sublist-' + id + '" class="nav nav-pills nav-stacked pages-tabs" data-drilldown-sub></ul>');
                var link = $('a[data-tabname="page-' + id + '"');

                /*Make menu item to drilldown button*/
                link.removeAttr('data-drilldown-item');
                link.attr('data-drilldown-button', '');
                link.after(subList);

                childNodes.forEach(function (item, i) {
                    _this2.createPageTab(item.id, item.name, item.childNodes, subList);
                });
            }
        }
    }, {
        key: 'createPageContent',
        value: function createPageContent(id, title, tabContentContainer) {
            var content = $('<div class="tab-pane" id="page-' + id + '" data-tabname="' + id + '">\n                        <div class="pages-head mb-4 mt-3 page-tab-head">\n                            <i class="fa fa-angle-left d-lg-none page-leftnav-toggle" aria-hidden="true"></i> \n                            ' + title + '</div>\n                        <form>\n                            <div class="settings-section-container"></div>\n\n                            <div class="pull-right i-p-action-btns mt-3">\n                                <button class="disabled button button-primary button-outline" type="button">Cancel</button>\n                                <button class="disabled button button-primary" type="submit">Save</button>\n                            </div>\n                        </form>\n                    </div>');

            tabContentContainer.append(content);
        }
    }]);

    return Page;
}();

var Section = function () {
    function Section(id, title, description, isVisible) {
        _classCallCheck(this, Section);

        this.id = id || 'wrap';
        this.title = title || '';
        this.description = description;
        this.isVisible = typeof isVisible === 'undefined' ? true : isVisible;
    }

    _createClass(Section, [{
        key: 'addTo',
        value: function addTo(container) {
            var section = $('<div id="section-' + this.id + '" class="mb-5 ' + (this.isVisible ? '' : 'hidden') + '">\n                            <div class="mt-2 mb-3">\n                                <h3 class="mb-1">' + this.title + '</h3>\n                                <div class="text-muted ' + (this.description ? '' : 'hidden') + '">' + this.description + '</div>\n                            </div>\n                            <div class="raul-list-group">\n                                <div class="raul-list-group-item">\n                                    <ul class="controls-container"></ul>\n                                </div>\n                            </div>\n                        </div>\n                      ');

            container.append(section);
        }
    }]);

    return Section;
}();

var Control = function () {
    function Control(control) {
        _classCallCheck(this, Control);

        this.id = control.id;
        this.isVisible = control.isVisible;
        this.value = control.value;
        this.labelText = control.labelText;
        this.infoText = control.infoText;
        this.type = control.type.toLowerCase();
    }

    _createClass(Control, [{
        key: 'create',
        value: function create() {
            var _this = this;

            return function () {
                switch (_this.type) {
                    case 'toggle':
                        return _this.createToggle();
                }
            }();
        }
    }, {
        key: 'createToggle',
        value: function createToggle() {
            var tooltip = '<a data-tooltip="' + this.infoText + '" data-tooltip-pos="top" data-tooltip-size="200">\n                            <img src="../Assets/build/images/settings-icons/settings-notifications.svg" width="20" class="info-icon">\n                        </a>';
            var checkedMark = '';
            if (this.value != null) {
                checkedMark = this.value ? 'checked' : '';
            } else {
                checkedMark = this.defaultValue ? 'checked' : '';
            }

            return $('<li class="toggle-setting ' + (this.isVisible ? '' : 'hidden') + '">\n                        <label>\n                            <div class="raul-list-item-first-line">\n                                ' + this.labelText + '\n                                ' + (this.infoText ? tooltip : '') + '\n                            </div>\n\n                            <div class="raul-switch-wrapper">\n                                <div class="raul-switch">\n                                    <input type="checkbox" class="skipcontrolTrigger" id="' + this.id + '" ' + checkedMark + ' >\n                                    <span class="raul-switch-slider"></span>\n                                </div>\n                            </div>\n                        </label>\n                    </li>');
        }
    }, {
        key: 'addTo',
        value: function addTo(container) {
            var control = this.create();

            container.append(control);
        }
    }]);

    return Control;
}();