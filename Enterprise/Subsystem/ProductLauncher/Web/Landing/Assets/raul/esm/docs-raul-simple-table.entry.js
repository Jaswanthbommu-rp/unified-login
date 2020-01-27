import { r as registerInstance, h } from './core-9263a98c.js';

const DocsRaulSimpleTable = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
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
        return (h("docs-element", { title: "Simple Table" }, h("div", { slot: "overview" }, h("docs-showcase", null, h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Basic"), h("div", { innerHTML: this.basicTableTemplate }), h("docs-code", { class: "mt-5", code: this.basicTableTemplate })), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Striped and Hoverable"), h("div", { innerHTML: this.stripedAndHoverableTableTemplate }), h("docs-code", { class: "mt-5", code: this.stripedAndHoverableTableTemplate })))), h("div", { slot: "design" }), h("div", { slot: "api" }, h("docs-interface", { component: "raul-table" }))));
    }
};

export { DocsRaulSimpleTable as docs_raul_simple_table };
