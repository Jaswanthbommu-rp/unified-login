'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsRaulSimpleTable = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.basicTableTemplate = `
    <raul-simple-table>
      <table>
        <thead>
          <th class="w-1">
            <raul-checkbox small>
              <input type="checkbox" />
            </raul-checkbox>
          </th>
          <th>Caption</th>
          <th>Caption</th>
          <th>Caption</th>
          <th>Caption</th>
          <th></th>
        </thead>
        <tbody>
          <tr>
            <td>
              <raul-checkbox small>
                <input type="checkbox" />
              </raul-checkbox>
            </td>
            <td>
              <a href="#" class="font-medium">Benson Smith</a>
              <div>273 Jefferson St. Richardson, TX 87549</div>
            </td>
            <td>
              <div class="text-base font-semibold">Primary Text</div>
              <div>Secondary Text</div>
            </td>
            <td>
              Secondary Text
            </td>
            <td>
              <raul-status variant="success">Status</raul-status>
            </td>
            <td class="text-right">
              <button type="button">
                <raul-icon icon="navigation-show-more-vertical" class="text-xl" />
              </button>
            </td>
          </tr>
          <tr>
            <td>
              <raul-checkbox small>
                <input type="checkbox" />
              </raul-checkbox>
            </td>
            <td>
              <a href="#" class="font-medium">Benson Smith</a>
              <div>273 Jefferson St. Richardson, TX 87549</div>
            </td>
            <td>
              <div class="text-base font-semibold">Primary Text</div>
              <div>Secondary Text</div>
            </td>
            <td>
              Secondary Text
            </td>
            <td>
              <raul-status variant="success">Status</raul-status>
            </td>
            <td class="text-right">
              <button type="button">
                <raul-icon icon="navigation-show-more-vertical" class="text-xl" />
              </button>
            </td>
          </tr>
          <tr>
            <td>
              <raul-checkbox small>
                <input type="checkbox" />
              </raul-checkbox>
            </td>
            <td>
              <a href="#" class="font-medium">Benson Smith</a>
              <div>273 Jefferson St. Richardson, TX 87549</div>
            </td>
            <td>
              <div class="text-base font-semibold">Primary Text</div>
              <div>Secondary Text</div>
            </td>
            <td>
              Secondary Text
            </td>
            <td>
              <raul-status variant="success">Status</raul-status>
            </td>
            <td class="text-right">
              <button type="button">
                <raul-icon icon="navigation-show-more-vertical" class="text-xl" />
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </raul-simple-table>
  `;
        this.stripedAndHoverableTableTemplate = `
    <raul-simple-table striped hoverable>
      <table>
        <thead>
          <th class="w-1">
            <raul-checkbox small>
              <input type="checkbox" />
            </raul-checkbox>
          </th>
          <th>
            Caption
            <raul-simple-table-sorter />
          </th>
          <th>
            Caption
            <raul-simple-table-sorter />
          </th>
          <th>
            Caption
            <raul-simple-table-sorter />
          </th>
          <th>
            Caption
            <raul-simple-table-sorter />
          </th>
          <th></th>
        </thead>
        <tbody>
          <tr>
            <td>
              <raul-checkbox small>
                <input type="checkbox" />
              </raul-checkbox>
            </td>
            <td>
              <a href="#" class="font-medium">Benson Smith</a>
              <div>273 Jefferson St. Richardson, TX 87549</div>
            </td>
            <td>
              <div class="text-base font-semibold">Primary Text</div>
              <div>Secondary Text</div>
            </td>
            <td>
              Secondary Text
            </td>
            <td>
              <raul-status variant="success">Status</raul-status>
            </td>
            <td class="text-right">
              <button type="button">
                <raul-icon icon="navigation-show-more-vertical" class="text-xl" />
              </button>
            </td>
          </tr>
          <tr>
            <td>
              <raul-checkbox small>
                <input type="checkbox" />
              </raul-checkbox>
            </td>
            <td>
              <a href="#" class="font-medium">Benson Smith</a>
              <div>273 Jefferson St. Richardson, TX 87549</div>
            </td>
            <td>
              <div class="text-base font-semibold">Primary Text</div>
              <div>Secondary Text</div>
            </td>
            <td>
              Secondary Text
            </td>
            <td>
              <raul-status variant="success">Status</raul-status>
            </td>
            <td class="text-right">
              <button type="button">
                <raul-icon icon="navigation-show-more-vertical" class="text-xl" />
              </button>
            </td>
          </tr>
          <tr>
            <td>
              <raul-checkbox small>
                <input type="checkbox" />
              </raul-checkbox>
            </td>
            <td>
              <a href="#" class="font-medium">Benson Smith</a>
              <div>273 Jefferson St. Richardson, TX 87549</div>
            </td>
            <td>
              <div class="text-base font-semibold">Primary Text</div>
              <div>Secondary Text</div>
            </td>
            <td>
              Secondary Text
            </td>
            <td>
              <raul-status variant="success">Status</raul-status>
            </td>
            <td class="text-right">
              <button type="button">
                <raul-icon icon="navigation-show-more-vertical" class="text-xl" />
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </raul-simple-table>
  `;
    }
    render() {
        return (core.h("docs-element", { title: "Simple Table" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Basic"), core.h("div", { innerHTML: this.basicTableTemplate }), core.h("docs-code", { class: "mt-5", code: this.basicTableTemplate })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Striped and Hoverable"), core.h("div", { innerHTML: this.stripedAndHoverableTableTemplate }), core.h("docs-code", { class: "mt-5", code: this.stripedAndHoverableTableTemplate })))), core.h("div", { slot: "design" }), core.h("div", { slot: "api" }, core.h("docs-interface", { component: "raul-table" }))));
    }
};

exports.docs_raul_simple_table = DocsRaulSimpleTable;
