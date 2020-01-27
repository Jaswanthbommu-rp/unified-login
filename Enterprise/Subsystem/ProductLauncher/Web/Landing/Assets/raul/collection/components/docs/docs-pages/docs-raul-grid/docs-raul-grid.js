import { h } from "@stencil/core";
export class DocsRaulGrid {
    constructor() {
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
        return (h("docs-element", { title: "Grid" },
            h("div", { slot: "overview" },
                h("docs-showcase", null,
                    h("div", { class: "mt-10" },
                        h("div", { class: "heading-lg" }, "Basic"),
                        h("div", { innerHTML: this.basicGridTemplate }),
                        h("docs-code", { class: "mt-5", code: this.basicGridTemplate })),
                    h("div", { class: "mt-10" },
                        h("div", { class: "heading-lg" }, "Striped and Hoverable"),
                        h("div", { innerHTML: this.stripedAndHoverableeGridTemplate }),
                        h("docs-code", { class: "mt-5", code: this.stripedAndHoverableeGridTemplate })))),
            h("div", { slot: "design" }),
            h("div", { slot: "api" },
                h("docs-interface", { component: "raul-grid" }))));
    }
    static get is() { return "docs-raul-grid"; }
}
