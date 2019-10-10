/*jshint esversion: 6 */

class Page {
    constructor() { }

    createPageTab(id, title, childNodes, tabsContainer) {
        let page = $(`<li data-drilldown-item class="pages-tab w-100">
                        <a href="javascript:void(0);" data-tabname="page-${id}" data-toggle="pill" id="${id}">${title}</a>
                      </li>`);

        tabsContainer.append(page);

        /*Create subLists*/
        if (childNodes.length) {
            let subList = $(`<ul id="sublist-${id}" class="nav nav-pills nav-stacked pages-tabs" data-drilldown-sub></ul>`);
            let link = $(`a[data-tabname="page-${id}"`);

            /*Make menu item to drilldown button*/
            link.removeAttr('data-drilldown-item');
            link.attr('data-drilldown-button', '');
            link.after(subList);

            childNodes.forEach((item, i) => {
                this.createPageTab(item.id, item.name, item.childNodes, subList);
            });

        }

    }

    createPageContent(id, title, tabContentContainer) {
        let content = $(`<div class="tab-pane" id="page-${id}" data-tabname="${id}">
                        <div class="pages-head mb-4 mt-3 page-tab-head">
                            <i class="fa fa-angle-left d-lg-none page-leftnav-toggle" aria-hidden="true"></i> 
                            ${title}</div>
                        <form>
                            <div class="settings-section-container"></div>

                            <div class="pull-right i-p-action-btns mt-3">
                                <button class="disabled button button-primary button-outline" type="button">Cancel</button>
                                <button class="disabled button button-primary" type="submit">Save</button>
                            </div>
                        </form>
                    </div>`);

        tabContentContainer.append(content);
    }

}

class Section {
    constructor(id, title, description, isVisible) {
        this.id = id || 'wrap';
        this.title = title || '';
        this.description = description;
        this.isVisible = typeof isVisible === 'undefined' ? true : isVisible;
    }

    addTo(container) {
        let section = $(`<div id="section-${this.id}" class="mb-5 ${this.isVisible ? '' : 'hidden'}">
                            <div class="mt-2 mb-3">
                                <h3 class="mb-1">${this.title}</h3>
                                <div class="text-muted ${this.description ? '' : 'hidden'}">${this.description}</div>
                            </div>
                            <div class="raul-list-group">
                                <div class="raul-list-group-item">
                                    <ul class="controls-container"></ul>
                                </div>
                            </div>
                        </div>
                      `);

        container.append(section);
    }

}

class Control {
    constructor(control) {
        this.id = control.id;
        this.isVisible = control.isVisible;
        this.value = control.value;
        this.labelText = control.labelText;
        this.infoText = control.infoText;
        this.type = control.type.toLowerCase();
    }

    create() {
        let _this = this;

        return (function () {
            switch (_this.type) {
                case 'toggle':
                    return _this.createToggle();
            }
        })();
    }

    createToggle() {
        let tooltip = `<a data-tooltip="${this.infoText}" data-tooltip-pos="top" data-tooltip-size="200">
                            <img src="../Assets/build/images/settings-icons/settings-notifications.svg" width="20" class="info-icon">
                        </a>`;
        let checkedMark = '';
        if (this.value != null) {
            checkedMark = this.value ? 'checked' : '';
        } else {
            checkedMark = this.defaultValue ? 'checked' : '';
        }

        return $(`<li class="toggle-setting ${this.isVisible ? '' : 'hidden'}">
                        <label>
                            <div class="raul-list-item-first-line">
                                ${this.labelText}
                                ${this.infoText ? tooltip : ''}
                            </div>

                            <div class="raul-switch-wrapper">
                                <div class="raul-switch">
                                    <input type="checkbox" class="skipcontrolTrigger" id="${this.id}" ${checkedMark} >
                                    <span class="raul-switch-slider"></span>
                                </div>
                            </div>
                        </label>
                    </li>`);
    }

    addTo(container) {
        let control = this.create();

        container.append(control);
    }

}