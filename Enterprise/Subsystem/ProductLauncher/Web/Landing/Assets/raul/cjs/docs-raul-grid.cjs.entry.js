'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsRaulGrid = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.basicGridTemplate = `
    <raul-grid>
      <raul-grid-header>
        <raul-grid-row>
          <raul-grid-cell class="w-auto">
            <raul-checkbox small>
              <input type="checkbox" />
            </raul-checkbox>
          </raul-grid-cell>
          <raul-grid-cell class="w-2/3 md:w-1/4">Caption</raul-grid-cell>
          <raul-grid-cell class="hidden w-1/2 md:flex md:w-1/4">Caption</raul-grid-cell>
          <raul-grid-cell class="hidden w-1/2 md:flex md:w-1/4">Caption</raul-grid-cell>
          <raul-grid-cell class="w-1/3 md:w-1/4">Caption</raul-grid-cell>
          <raul-grid-cell class="w-6 justify-end"></raul-grid-cell>
        </raul-grid-row>
      </raul-grid-header>
      <raul-grid-body>
        <raul-grid-row>
          <raul-grid-cell class="w-auto">
            <raul-checkbox small>
              <input type="checkbox" />
            </raul-checkbox>
          </raul-grid-cell>
          <raul-grid-cell class="w-2/3 md:w-1/4">
            <a href="#" class="font-medium">Benson Smith</a>
            <div>273 Jefferson St. Richardson, TX 87549</div>
          </raul-grid-cell>
          <raul-grid-cell class="hidden w-1/2 md:flex md:w-1/4">
            <div class="text-base font-semibold">Primary Text</div>
            <div>Secondary Text</div>
          </raul-grid-cell>
          <raul-grid-cell class="hidden w-1/2 md:flex md:w-1/4">
            Secondary Text
          </raul-grid-cell>
          <raul-grid-cell class="w-1/3 md:w-1/4">
            <raul-status variant="success">Status</raul-status>
          </raul-grid-cell>
          <raul-grid-cell class="w-6 justify-end">
            <button type="button">
              <raul-icon icon="navigation-show-more-vertical" class="text-xl" />
            </button>
          </raul-grid-cell>
        </raul-grid-row>
        <raul-grid-row>
          <raul-grid-cell class="w-auto">
            <raul-checkbox small>
              <input type="checkbox" />
            </raul-checkbox>
          </raul-grid-cell>
          <raul-grid-cell class="w-2/3 md:w-1/4">
            <a href="#" class="font-medium">Benson Smith</a>
            <div>273 Jefferson St. Richardson, TX 87549</div>
          </raul-grid-cell>
          <raul-grid-cell class="hidden md:flex md:w-1/4">
            <div class="text-base font-semibold">Primary Text</div>
            <div>Secondary Text</div>
          </raul-grid-cell>
          <raul-grid-cell class="hidden md:flex md:w-1/4">
            Secondary Text
          </raul-grid-cell>
          <raul-grid-cell class="w-1/3 md:w-1/4">
            <raul-status variant="success">Status</raul-status>
          </raul-grid-cell>
          <raul-grid-cell class="w-6 justify-end">
            <button type="button">
              <raul-icon icon="navigation-show-more-vertical" class="text-xl" />
            </button>
          </raul-grid-cell>
        </raul-grid-row>
        <raul-grid-row>
          <raul-grid-cell class="w-auto">
            <raul-checkbox small>
              <input type="checkbox" />
            </raul-checkbox>
          </raul-grid-cell>
          <raul-grid-cell class="w-2/3 md:w-1/4">
            <a href="#" class="font-medium">Benson Smith</a>
            <div>273 Jefferson St. Richardson, TX 87549</div>
          </raul-grid-cell>
          <raul-grid-cell class="hidden w-1/2 md:flex md:w-1/4">
            <div class="text-base font-semibold">Primary Text</div>
            <div>Secondary Text</div>
          </raul-grid-cell>
          <raul-grid-cell class="hidden w-1/2 md:flex md:w-1/4">
            Secondary Text
          </raul-grid-cell>
          <raul-grid-cell class="w-1/3 md:w-1/4">
            <raul-status variant="success">Status</raul-status>
          </raul-grid-cell>
          <raul-grid-cell class="w-6 justify-end">
            <button type="button">
              <raul-icon icon="navigation-show-more-vertical" class="text-xl" />
            </button>
          </raul-grid-cell>
        </raul-grid-row>
      </raul-grid-body>
    </raul-grid>
  `;
        this.stripedAndHoverableeGridTemplate = `
    <raul-grid striped hoverable>
      <raul-grid-header>
        <raul-grid-row>
          <raul-grid-cell class="w-auto">
            <raul-checkbox small>
              <input type="checkbox" />
            </raul-checkbox>
          </raul-grid-cell>
          <raul-grid-cell class="w-2/3 md:w-1/4">
            Caption
            <raul-grid-sorter />
          </raul-grid-cell>
          <raul-grid-cell class="hidden w-1/2 md:flex md:w-1/4">
            Caption
            <raul-grid-sorter />
          </raul-grid-cell>
          <raul-grid-cell class="hidden w-1/2 md:flex md:w-1/4">
            Caption
            <raul-grid-sorter />
          </raul-grid-cell>
          <raul-grid-cell class="w-1/3 md:w-1/4">
            Caption
            <raul-grid-sorter />
          </raul-grid-cell>
          <raul-grid-cell class="w-6 justify-end"></raul-grid-cell>
        </raul-grid-row>
      </raul-grid-header>
      <raul-grid-body>
        <raul-grid-row>
          <raul-grid-cell class="w-auto">
            <raul-checkbox small>
              <input type="checkbox" />
            </raul-checkbox>
          </raul-grid-cell>
          <raul-grid-cell class="w-2/3 md:w-1/4">
            <a href="#" class="font-medium">Benson Smith</a>
            <div>273 Jefferson St. Richardson, TX 87549</div>
          </raul-grid-cell>
          <raul-grid-cell class="hidden w-1/2 md:flex md:w-1/4">
            <div class="text-base font-semibold">Primary Text</div>
            <div>Secondary Text</div>
          </raul-grid-cell>
          <raul-grid-cell class="hidden w-1/2 md:flex md:w-1/4">
            Secondary Text
          </raul-grid-cell>
          <raul-grid-cell class="w-1/3 md:w-1/4">
            <raul-status variant="success">Status</raul-status>
          </raul-grid-cell>
          <raul-grid-cell class="w-6 justify-end">
            <button type="button">
              <raul-icon icon="navigation-show-more-vertical" class="text-xl" />
            </button>
          </raul-grid-cell>
        </raul-grid-row>
        <raul-grid-row>
          <raul-grid-cell class="w-auto">
            <raul-checkbox small>
              <input type="checkbox" />
            </raul-checkbox>
          </raul-grid-cell>
          <raul-grid-cell class="w-2/3 md:w-1/4">
            <a href="#" class="font-medium">Benson Smith</a>
            <div>273 Jefferson St. Richardson, TX 87549</div>
          </raul-grid-cell>
          <raul-grid-cell class="hidden md:flex md:w-1/4">
            <div class="text-base font-semibold">Primary Text</div>
            <div>Secondary Text</div>
          </raul-grid-cell>
          <raul-grid-cell class="hidden md:flex md:w-1/4">
            Secondary Text
          </raul-grid-cell>
          <raul-grid-cell class="w-1/3 md:w-1/4">
            <raul-status variant="success">Status</raul-status>
          </raul-grid-cell>
          <raul-grid-cell class="w-6 justify-end">
            <button type="button">
              <raul-icon icon="navigation-show-more-vertical" class="text-xl" />
            </button>
          </raul-grid-cell>
        </raul-grid-row>
        <raul-grid-row>
          <raul-grid-cell class="w-auto">
            <raul-checkbox small>
              <input type="checkbox" />
            </raul-checkbox>
          </raul-grid-cell>
          <raul-grid-cell class="w-2/3 md:w-1/4">
            <a href="#" class="font-medium">Benson Smith</a>
            <div>273 Jefferson St. Richardson, TX 87549</div>
          </raul-grid-cell>
          <raul-grid-cell class="hidden w-1/2 md:flex md:w-1/4">
            <div class="text-base font-semibold">Primary Text</div>
            <div>Secondary Text</div>
          </raul-grid-cell>
          <raul-grid-cell class="hidden w-1/2 md:flex md:w-1/4">
            Secondary Text
          </raul-grid-cell>
          <raul-grid-cell class="w-1/3 md:w-1/4">
            <raul-status variant="success">Status</raul-status>
          </raul-grid-cell>
          <raul-grid-cell class="w-6 justify-end">
            <button type="button">
              <raul-icon icon="navigation-show-more-vertical" class="text-xl" />
            </button>
          </raul-grid-cell>
        </raul-grid-row>
      </raul-grid-body>
    </raul-grid>
  `;
    }
    render() {
        return (core.h("docs-element", { title: "Grid" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Basic"), core.h("div", { innerHTML: this.basicGridTemplate }), core.h("docs-code", { class: "mt-5", code: this.basicGridTemplate })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Striped and Hoverable"), core.h("div", { innerHTML: this.stripedAndHoverableeGridTemplate }), core.h("docs-code", { class: "mt-5", code: this.stripedAndHoverableeGridTemplate })))), core.h("div", { slot: "design" }), core.h("div", { slot: "api" }, core.h("docs-interface", { component: "raul-grid" }))));
    }
};

exports.docs_raul_grid = DocsRaulGrid;
