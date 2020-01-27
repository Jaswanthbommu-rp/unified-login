'use strict';

var timestamp = "2020-01-17T15:25:56";
var compiler = {
	name: "@stencil/core",
	version: "1.8.5",
	typescriptVersion: "3.7.2"
};
var components = [
	{
		filePath: "src/components/docs/docs-pages/docs-404/docs-404.tsx",
		encapsulation: "none",
		tag: "docs-404",
		readme: "# docs-404\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-markdown"
		],
		dependencyGraph: {
			"docs-404": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-accessibility/docs-accessibility.tsx",
		encapsulation: "none",
		tag: "docs-accessibility",
		readme: "# docs-accessibility\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-markdown"
		],
		dependencyGraph: {
			"docs-accessibility": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-code/docs-code.tsx",
		encapsulation: "none",
		tag: "docs-code",
		readme: "# docs-code\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "code",
				type: "string",
				mutable: false,
				attr: "code",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "language",
				type: "string",
				mutable: false,
				attr: "language",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "'html'",
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-breadcrumbs",
			"docs-raul-card",
			"docs-raul-footer",
			"docs-raul-grid",
			"docs-raul-list",
			"docs-raul-progress",
			"docs-raul-simple-table",
			"docs-raul-tooltip"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-breadcrumbs": [
				"docs-code"
			],
			"docs-raul-card": [
				"docs-code"
			],
			"docs-raul-footer": [
				"docs-code"
			],
			"docs-raul-grid": [
				"docs-code"
			],
			"docs-raul-list": [
				"docs-code"
			],
			"docs-raul-progress": [
				"docs-code"
			],
			"docs-raul-simple-table": [
				"docs-code"
			],
			"docs-raul-tooltip": [
				"docs-code"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-colors/docs-colors.tsx",
		encapsulation: "none",
		tag: "docs-colors",
		readme: "# docs-colors\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element"
		],
		dependencyGraph: {
			"docs-colors": [
				"docs-element"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-compliance/docs-compliance.tsx",
		encapsulation: "none",
		tag: "docs-compliance",
		readme: "# docs-compliance\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-markdown"
		],
		dependencyGraph: {
			"docs-compliance": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-device/docs-device.tsx",
		encapsulation: "none",
		tag: "docs-device",
		readme: "# docs-device\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-filter-bar",
			"docs-raul-tabs"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-filter-bar": [
				"docs-device"
			],
			"docs-raul-tabs": [
				"docs-device"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-element/docs-element.tsx",
		encapsulation: "none",
		tag: "docs-element",
		readme: "# docs-element\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "title",
				type: "string",
				mutable: false,
				attr: "title",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: true
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-colors",
			"docs-raul-accordion",
			"docs-raul-action-menu",
			"docs-raul-alert",
			"docs-raul-aside",
			"docs-raul-avatar",
			"docs-raul-badge",
			"docs-raul-breadcrumbs",
			"docs-raul-bulk-action-bar",
			"docs-raul-button",
			"docs-raul-card",
			"docs-raul-checkbox",
			"docs-raul-chips",
			"docs-raul-date-picker",
			"docs-raul-filter-bar",
			"docs-raul-filter-menu",
			"docs-raul-footer",
			"docs-raul-grid",
			"docs-raul-heading",
			"docs-raul-input",
			"docs-raul-list",
			"docs-raul-loaders",
			"docs-raul-modal",
			"docs-raul-paging-bar",
			"docs-raul-progress",
			"docs-raul-radio-button",
			"docs-raul-select",
			"docs-raul-simple-select",
			"docs-raul-simple-table",
			"docs-raul-snackbar",
			"docs-raul-sortable-list",
			"docs-raul-status",
			"docs-raul-status-indicator",
			"docs-raul-switch",
			"docs-raul-tabs",
			"docs-raul-textarea",
			"docs-raul-toggles",
			"docs-raul-tooltip",
			"docs-spacing",
			"docs-typography"
		],
		dependencies: [
			"raul-tabs"
		],
		dependencyGraph: {
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-colors": [
				"docs-element"
			],
			"docs-raul-accordion": [
				"docs-element"
			],
			"docs-raul-action-menu": [
				"docs-element"
			],
			"docs-raul-alert": [
				"docs-element"
			],
			"docs-raul-aside": [
				"docs-element"
			],
			"docs-raul-avatar": [
				"docs-element"
			],
			"docs-raul-badge": [
				"docs-element"
			],
			"docs-raul-breadcrumbs": [
				"docs-element"
			],
			"docs-raul-bulk-action-bar": [
				"docs-element"
			],
			"docs-raul-button": [
				"docs-element"
			],
			"docs-raul-card": [
				"docs-element"
			],
			"docs-raul-checkbox": [
				"docs-element"
			],
			"docs-raul-chips": [
				"docs-element"
			],
			"docs-raul-date-picker": [
				"docs-element"
			],
			"docs-raul-filter-bar": [
				"docs-element"
			],
			"docs-raul-filter-menu": [
				"docs-element"
			],
			"docs-raul-footer": [
				"docs-element"
			],
			"docs-raul-grid": [
				"docs-element"
			],
			"docs-raul-heading": [
				"docs-element"
			],
			"docs-raul-input": [
				"docs-element"
			],
			"docs-raul-list": [
				"docs-element"
			],
			"docs-raul-loaders": [
				"docs-element"
			],
			"docs-raul-modal": [
				"docs-element"
			],
			"docs-raul-paging-bar": [
				"docs-element"
			],
			"docs-raul-progress": [
				"docs-element"
			],
			"docs-raul-radio-button": [
				"docs-element"
			],
			"docs-raul-select": [
				"docs-element"
			],
			"docs-raul-simple-select": [
				"docs-element"
			],
			"docs-raul-simple-table": [
				"docs-element"
			],
			"docs-raul-snackbar": [
				"docs-element"
			],
			"docs-raul-sortable-list": [
				"docs-element"
			],
			"docs-raul-status": [
				"docs-element"
			],
			"docs-raul-status-indicator": [
				"docs-element"
			],
			"docs-raul-switch": [
				"docs-element"
			],
			"docs-raul-tabs": [
				"docs-element"
			],
			"docs-raul-textarea": [
				"docs-element"
			],
			"docs-raul-toggles": [
				"docs-element"
			],
			"docs-raul-tooltip": [
				"docs-element"
			],
			"docs-spacing": [
				"docs-element"
			],
			"docs-typography": [
				"docs-element"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-grid-list/docs-grid-list.tsx",
		encapsulation: "none",
		tag: "docs-grid-list",
		readme: "# docs-grid-list\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-markdown",
			"raul-bulk-action-bar",
			"raul-button",
			"raul-checkbox",
			"raul-paging-bar",
			"raul-icon",
			"raul-action-menu"
		],
		dependencyGraph: {
			"docs-grid-list": [
				"docs-markdown",
				"raul-bulk-action-bar",
				"raul-button",
				"raul-checkbox",
				"raul-paging-bar",
				"raul-icon",
				"raul-action-menu"
			],
			"docs-markdown": [
				"raul-content"
			],
			"raul-bulk-action-bar": [
				"raul-button",
				"raul-icon"
			],
			"raul-button": [
				"raul-icon"
			],
			"raul-paging-bar": [
				"raul-icon"
			],
			"raul-action-menu": [
				"raul-dropdown-menu",
				"raul-icon"
			],
			"raul-dropdown-menu": [
				"raul-icon"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-home/docs-home.tsx",
		encapsulation: "none",
		tag: "docs-home",
		readme: "# docs-home\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"raul-button"
		],
		dependencyGraph: {
			"docs-home": [
				"raul-button"
			],
			"raul-button": [
				"raul-icon"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-index/docs-index.tsx",
		encapsulation: "none",
		tag: "docs-index",
		readme: "# docs-roadmap\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-markdown",
			"raul-tabs",
			"raul-status-indicator",
			"raul-simple-table"
		],
		dependencyGraph: {
			"docs-index": [
				"docs-markdown",
				"raul-tabs",
				"raul-status-indicator",
				"raul-simple-table"
			],
			"docs-markdown": [
				"raul-content"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-interface/docs-interface.tsx",
		encapsulation: "none",
		tag: "docs-interface",
		readme: "# docs-interface\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "component",
				type: "string",
				mutable: false,
				attr: "component",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: true
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-accordion",
			"docs-raul-action-menu",
			"docs-raul-aside",
			"docs-raul-avatar",
			"docs-raul-badge",
			"docs-raul-breadcrumbs",
			"docs-raul-bulk-action-bar",
			"docs-raul-button",
			"docs-raul-card",
			"docs-raul-checkbox",
			"docs-raul-chips",
			"docs-raul-date-picker",
			"docs-raul-filter-bar",
			"docs-raul-filter-menu",
			"docs-raul-footer",
			"docs-raul-grid",
			"docs-raul-heading",
			"docs-raul-input",
			"docs-raul-list",
			"docs-raul-loaders",
			"docs-raul-modal",
			"docs-raul-paging-bar",
			"docs-raul-progress",
			"docs-raul-radio-button",
			"docs-raul-select",
			"docs-raul-simple-select",
			"docs-raul-simple-table",
			"docs-raul-snackbar",
			"docs-raul-sortable-list",
			"docs-raul-status",
			"docs-raul-status-indicator",
			"docs-raul-switch",
			"docs-raul-tabs",
			"docs-raul-text",
			"docs-raul-textarea",
			"docs-raul-toggles",
			"docs-raul-tooltip"
		],
		dependencies: [
			"raul-simple-table"
		],
		dependencyGraph: {
			"docs-interface": [
				"raul-simple-table"
			],
			"docs-raul-accordion": [
				"docs-interface"
			],
			"docs-raul-action-menu": [
				"docs-interface"
			],
			"docs-raul-aside": [
				"docs-interface"
			],
			"docs-raul-avatar": [
				"docs-interface"
			],
			"docs-raul-badge": [
				"docs-interface"
			],
			"docs-raul-breadcrumbs": [
				"docs-interface"
			],
			"docs-raul-bulk-action-bar": [
				"docs-interface"
			],
			"docs-raul-button": [
				"docs-interface"
			],
			"docs-raul-card": [
				"docs-interface"
			],
			"docs-raul-checkbox": [
				"docs-interface"
			],
			"docs-raul-chips": [
				"docs-interface"
			],
			"docs-raul-date-picker": [
				"docs-interface"
			],
			"docs-raul-filter-bar": [
				"docs-interface"
			],
			"docs-raul-filter-menu": [
				"docs-interface"
			],
			"docs-raul-footer": [
				"docs-interface"
			],
			"docs-raul-grid": [
				"docs-interface"
			],
			"docs-raul-heading": [
				"docs-interface"
			],
			"docs-raul-input": [
				"docs-interface"
			],
			"docs-raul-list": [
				"docs-interface"
			],
			"docs-raul-loaders": [
				"docs-interface"
			],
			"docs-raul-modal": [
				"docs-interface"
			],
			"docs-raul-paging-bar": [
				"docs-interface"
			],
			"docs-raul-progress": [
				"docs-interface"
			],
			"docs-raul-radio-button": [
				"docs-interface"
			],
			"docs-raul-select": [
				"docs-interface"
			],
			"docs-raul-simple-select": [
				"docs-interface"
			],
			"docs-raul-simple-table": [
				"docs-interface"
			],
			"docs-raul-snackbar": [
				"docs-interface"
			],
			"docs-raul-sortable-list": [
				"docs-interface"
			],
			"docs-raul-status": [
				"docs-interface"
			],
			"docs-raul-status-indicator": [
				"docs-interface"
			],
			"docs-raul-switch": [
				"docs-interface"
			],
			"docs-raul-tabs": [
				"docs-interface"
			],
			"docs-raul-text": [
				"docs-interface"
			],
			"docs-raul-textarea": [
				"docs-interface"
			],
			"docs-raul-toggles": [
				"docs-interface"
			],
			"docs-raul-tooltip": [
				"docs-interface"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-introduction/docs-introduction.tsx",
		encapsulation: "none",
		tag: "docs-introduction",
		readme: "# docs-introduction\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-markdown"
		],
		dependencyGraph: {
			"docs-introduction": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-markdown/docs-markdown.tsx",
		encapsulation: "none",
		tag: "docs-markdown",
		readme: "# docs-markdown\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-404",
			"docs-accessibility",
			"docs-compliance",
			"docs-grid-list",
			"docs-index",
			"docs-introduction",
			"docs-raul-text",
			"docs-readme",
			"docs-typography",
			"docs-upgrading"
		],
		dependencies: [
			"raul-content"
		],
		dependencyGraph: {
			"docs-markdown": [
				"raul-content"
			],
			"docs-404": [
				"docs-markdown"
			],
			"docs-accessibility": [
				"docs-markdown"
			],
			"docs-compliance": [
				"docs-markdown"
			],
			"docs-grid-list": [
				"docs-markdown"
			],
			"docs-index": [
				"docs-markdown"
			],
			"docs-introduction": [
				"docs-markdown"
			],
			"docs-raul-text": [
				"docs-markdown"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-typography": [
				"docs-markdown"
			],
			"docs-upgrading": [
				"docs-markdown"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-menu/docs-menu.tsx",
		encapsulation: "none",
		tag: "docs-menu",
		readme: "# docs-menu\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-shell"
		],
		dependencies: [
			"docs-menu-section",
			"docs-menu-page",
			"docs-menu-elem-page"
		],
		dependencyGraph: {
			"docs-menu": [
				"docs-menu-section",
				"docs-menu-page",
				"docs-menu-elem-page"
			],
			"docs-menu-section": [
				"stencil-route-link",
				"raul-icon"
			],
			"docs-menu-page": [
				"stencil-route-link"
			],
			"docs-menu-elem-page": [
				"docs-menu-page"
			],
			"docs-shell": [
				"docs-menu"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-menu/docs-menu-elem-page/docs-menu-elem-page.tsx",
		encapsulation: "none",
		tag: "docs-menu-elem-page",
		readme: "# docs-menu-elem-page\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "name",
				type: "string",
				mutable: false,
				attr: "name",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "url",
				type: "string",
				mutable: false,
				attr: "url",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-menu"
		],
		dependencies: [
			"docs-menu-page"
		],
		dependencyGraph: {
			"docs-menu-elem-page": [
				"docs-menu-page"
			],
			"docs-menu-page": [
				"stencil-route-link"
			],
			"docs-menu": [
				"docs-menu-elem-page"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-menu/docs-menu-page/docs-menu-page.tsx",
		encapsulation: "none",
		tag: "docs-menu-page",
		readme: "# docs-menu-page\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "name",
				type: "string",
				mutable: false,
				attr: "name",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "url",
				type: "string",
				mutable: false,
				attr: "url",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
			{
				event: "menuPageActivated",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-menu",
			"docs-menu-elem-page"
		],
		dependencies: [
			"stencil-route-link"
		],
		dependencyGraph: {
			"docs-menu-page": [
				"stencil-route-link"
			],
			"docs-menu": [
				"docs-menu-page"
			],
			"docs-menu-elem-page": [
				"docs-menu-page"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-menu/docs-menu-section/docs-menu-section.tsx",
		encapsulation: "none",
		tag: "docs-menu-section",
		readme: "# docs-menu-section\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "name",
				type: "String",
				mutable: false,
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "String"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "url",
				type: "string",
				mutable: false,
				attr: "url",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
			{
				event: "expandSection",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-menu"
		],
		dependencies: [
			"stencil-route-link",
			"raul-icon"
		],
		dependencyGraph: {
			"docs-menu-section": [
				"stencil-route-link",
				"raul-icon"
			],
			"docs-menu": [
				"docs-menu-section"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-preview/docs-preview.tsx",
		encapsulation: "none",
		tag: "docs-preview",
		readme: "# docs-preview\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "component",
				type: "string",
				mutable: false,
				attr: "component",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: true
			},
			{
				name: "content",
				type: "string",
				mutable: false,
				attr: "content",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "null",
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-action-menu",
			"docs-raul-avatar",
			"docs-raul-badge",
			"docs-raul-bulk-action-bar",
			"docs-raul-button",
			"docs-raul-card",
			"docs-raul-checkbox",
			"docs-raul-chips",
			"docs-raul-date-picker",
			"docs-raul-filter-bar",
			"docs-raul-filter-menu",
			"docs-raul-heading",
			"docs-raul-input",
			"docs-raul-paging-bar",
			"docs-raul-radio-button",
			"docs-raul-simple-select",
			"docs-raul-snackbar",
			"docs-raul-status",
			"docs-raul-status-indicator",
			"docs-raul-switch",
			"docs-raul-text",
			"docs-raul-textarea"
		],
		dependencies: [
			"docs-preview-props"
		],
		dependencyGraph: {
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-raul-action-menu": [
				"docs-preview"
			],
			"docs-raul-avatar": [
				"docs-preview"
			],
			"docs-raul-badge": [
				"docs-preview"
			],
			"docs-raul-bulk-action-bar": [
				"docs-preview"
			],
			"docs-raul-button": [
				"docs-preview"
			],
			"docs-raul-card": [
				"docs-preview"
			],
			"docs-raul-checkbox": [
				"docs-preview"
			],
			"docs-raul-chips": [
				"docs-preview"
			],
			"docs-raul-date-picker": [
				"docs-preview"
			],
			"docs-raul-filter-bar": [
				"docs-preview"
			],
			"docs-raul-filter-menu": [
				"docs-preview"
			],
			"docs-raul-heading": [
				"docs-preview"
			],
			"docs-raul-input": [
				"docs-preview"
			],
			"docs-raul-paging-bar": [
				"docs-preview"
			],
			"docs-raul-radio-button": [
				"docs-preview"
			],
			"docs-raul-simple-select": [
				"docs-preview"
			],
			"docs-raul-snackbar": [
				"docs-preview"
			],
			"docs-raul-status": [
				"docs-preview"
			],
			"docs-raul-status-indicator": [
				"docs-preview"
			],
			"docs-raul-switch": [
				"docs-preview"
			],
			"docs-raul-text": [
				"docs-preview"
			],
			"docs-raul-textarea": [
				"docs-preview"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-preview/docs-preview-props/docs-preview-props.tsx",
		encapsulation: "none",
		tag: "docs-preview-props",
		readme: "# docs-preview-props\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "component",
				type: "string",
				mutable: false,
				attr: "component",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: true
			}
		],
		methods: [
		],
		events: [
			{
				event: "docsPropChange",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-preview"
		],
		dependencies: [
			"raul-input",
			"raul-switch",
			"raul-select"
		],
		dependencyGraph: {
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-preview": [
				"docs-preview-props"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-accordion/docs-raul-accordion.tsx",
		encapsulation: "none",
		tag: "docs-raul-accordion",
		readme: "# docs-raul-accordion\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"raul-accordion",
			"raul-accordion-item",
			"raul-accordion-item-header",
			"raul-accordion-item-title",
			"raul-accordion-item-panel",
			"raul-status",
			"raul-action-menu",
			"raul-action-menu-item",
			"docs-readme",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-accordion": [
				"docs-element",
				"docs-showcase",
				"raul-accordion",
				"raul-accordion-item",
				"raul-accordion-item-header",
				"raul-accordion-item-title",
				"raul-accordion-item-panel",
				"raul-status",
				"raul-action-menu",
				"raul-action-menu-item",
				"docs-readme",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"raul-accordion-item-header": [
				"raul-icon"
			],
			"raul-action-menu": [
				"raul-dropdown-menu",
				"raul-icon"
			],
			"raul-dropdown-menu": [
				"raul-icon"
			],
			"raul-action-menu-item": [
				"raul-icon"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-action-menu/docs-raul-action-menu.tsx",
		encapsulation: "none",
		tag: "docs-raul-action-menu",
		readme: "# docs-raul-action-menu\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-readme",
			"docs-showcase",
			"raul-action-menu",
			"raul-action-menu-item",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-action-menu": [
				"docs-element",
				"docs-readme",
				"docs-showcase",
				"raul-action-menu",
				"raul-action-menu-item",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"raul-action-menu": [
				"raul-dropdown-menu",
				"raul-icon"
			],
			"raul-dropdown-menu": [
				"raul-icon"
			],
			"raul-action-menu-item": [
				"raul-icon"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-alert/docs-raul-alert.tsx",
		encapsulation: "none",
		tag: "docs-raul-alert",
		readme: "# docs-raul-alert\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-readme",
			"docs-showcase",
			"raul-alert"
		],
		dependencyGraph: {
			"docs-raul-alert": [
				"docs-element",
				"docs-readme",
				"docs-showcase",
				"raul-alert"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-aside/docs-raul-aside.tsx",
		encapsulation: "none",
		tag: "docs-raul-aside",
		readme: "# docs-raul-aside\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"raul-aside-title",
			"raul-aside-subtitle",
			"raul-aside-actions",
			"raul-button",
			"raul-aside",
			"raul-aside-header",
			"raul-aside-body",
			"raul-aside-footer",
			"docs-element",
			"docs-showcase",
			"docs-readme",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-aside": [
				"raul-aside-title",
				"raul-aside-subtitle",
				"raul-aside-actions",
				"raul-button",
				"raul-aside",
				"raul-aside-header",
				"raul-aside-body",
				"raul-aside-footer",
				"docs-element",
				"docs-showcase",
				"docs-readme",
				"docs-interface"
			],
			"raul-button": [
				"raul-icon"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-avatar/docs-raul-avatar.tsx",
		encapsulation: "none",
		tag: "docs-raul-avatar",
		readme: "# docs-raul-avatar\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"raul-avatar",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-avatar": [
				"docs-element",
				"docs-showcase",
				"raul-avatar",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"raul-avatar": [
				"raul-icon"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-badge/docs-raul-badge.tsx",
		encapsulation: "none",
		tag: "docs-raul-badge",
		readme: "# docs-raul-badge\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"raul-badge",
			"docs-readme",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-badge": [
				"docs-element",
				"docs-showcase",
				"raul-badge",
				"docs-readme",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"raul-badge": [
				"raul-icon"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-breadcrumbs/docs-raul-breadcrumbs.tsx",
		encapsulation: "none",
		tag: "docs-raul-breadcrumbs",
		readme: "# docs-raul-breadcrumbs\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"docs-code",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-breadcrumbs": [
				"docs-element",
				"docs-showcase",
				"docs-code",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-bulk-action-bar/docs-raul-bulk-action-bar.tsx",
		encapsulation: "none",
		tag: "docs-raul-bulk-action-bar",
		readme: "# docs-raul-bulk-action-bar\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"raul-bulk-action-bar",
			"raul-button",
			"docs-readme",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-bulk-action-bar": [
				"docs-element",
				"docs-showcase",
				"raul-bulk-action-bar",
				"raul-button",
				"docs-readme",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"raul-bulk-action-bar": [
				"raul-button",
				"raul-icon"
			],
			"raul-button": [
				"raul-icon"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-button/docs-raul-button.tsx",
		encapsulation: "none",
		tag: "docs-raul-button",
		readme: "# docs-raul-button\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"raul-button",
			"docs-readme",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-button": [
				"docs-element",
				"docs-showcase",
				"raul-button",
				"docs-readme",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"raul-button": [
				"raul-icon"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-card/docs-raul-card.tsx",
		encapsulation: "none",
		tag: "docs-raul-card",
		readme: "# docs-raul-card\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"docs-code",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-card": [
				"docs-element",
				"docs-showcase",
				"docs-code",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-checkbox/docs-raul-checkobx.tsx",
		encapsulation: "none",
		tag: "docs-raul-checkbox",
		readme: "# docs-raul-checkbox\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"docs-readme",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-checkbox": [
				"docs-element",
				"docs-showcase",
				"docs-readme",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-chips/docs-raul-chips.tsx",
		encapsulation: "none",
		tag: "docs-raul-chips",
		readme: "# docs-raul-chips\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"raul-chip",
			"docs-readme",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-chips": [
				"docs-element",
				"docs-showcase",
				"raul-chip",
				"docs-readme",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"raul-chip": [
				"raul-icon"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-date-picker/docs-raul-date-picker.tsx",
		encapsulation: "none",
		tag: "docs-raul-date-picker",
		readme: "# docs-raul-date-picker\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-readme",
			"docs-showcase",
			"raul-date-picker",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-date-picker": [
				"docs-element",
				"docs-readme",
				"docs-showcase",
				"raul-date-picker",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"raul-date-picker": [
				"raul-icon"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-filter-bar/docs-raul-filter-bar.tsx",
		encapsulation: "none",
		tag: "docs-raul-filter-bar",
		readme: "# docs-raul-filter-bar\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"raul-content",
			"raul-filter-bar",
			"raul-input",
			"raul-select",
			"raul-filter-bar-button",
			"docs-device",
			"raul-filter-menu",
			"raul-filter-menu-item",
			"raul-aside",
			"raul-aside-header",
			"raul-aside-body",
			"raul-aside-footer",
			"raul-button",
			"docs-readme",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-filter-bar": [
				"docs-element",
				"raul-content",
				"raul-filter-bar",
				"raul-input",
				"raul-select",
				"raul-filter-bar-button",
				"docs-device",
				"raul-filter-menu",
				"raul-filter-menu-item",
				"raul-aside",
				"raul-aside-header",
				"raul-aside-body",
				"raul-aside-footer",
				"raul-button",
				"docs-readme",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-filter-bar-button": [
				"raul-icon"
			],
			"raul-filter-menu": [
				"raul-dropdown-menu",
				"raul-icon"
			],
			"raul-dropdown-menu": [
				"raul-icon"
			],
			"raul-filter-menu-item": [
				"raul-icon"
			],
			"raul-button": [
				"raul-icon"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-filter-menu/docs-raul-filter-menu.tsx",
		encapsulation: "none",
		tag: "docs-raul-filter-menu",
		readme: "# docs-raul-filter-menu\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-readme",
			"docs-showcase",
			"raul-filter-menu",
			"raul-filter-menu-item",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-filter-menu": [
				"docs-element",
				"docs-readme",
				"docs-showcase",
				"raul-filter-menu",
				"raul-filter-menu-item",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"raul-filter-menu": [
				"raul-dropdown-menu",
				"raul-icon"
			],
			"raul-dropdown-menu": [
				"raul-icon"
			],
			"raul-filter-menu-item": [
				"raul-icon"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-footer/docs-raul-footer.tsx",
		encapsulation: "none",
		tag: "docs-raul-footer",
		readme: "# docs-raul-footer\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"docs-code",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-footer": [
				"docs-element",
				"docs-showcase",
				"docs-code",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-grid/docs-raul-grid.tsx",
		encapsulation: "none",
		tag: "docs-raul-grid",
		readme: "# docs-raul-grid\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"docs-code",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-grid": [
				"docs-element",
				"docs-showcase",
				"docs-code",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-heading/docs-raul-heading.tsx",
		encapsulation: "none",
		tag: "docs-raul-heading",
		readme: "# docs-raul-heading\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-readme",
			"docs-preview",
			"docs-showcase",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-heading": [
				"docs-element",
				"docs-readme",
				"docs-preview",
				"docs-showcase",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-input/docs-raul-input.tsx",
		encapsulation: "none",
		tag: "docs-raul-input",
		readme: "# docs-raul-input\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-readme",
			"docs-showcase",
			"raul-input",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-input": [
				"docs-element",
				"docs-readme",
				"docs-showcase",
				"raul-input",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"raul-input": [
				"raul-icon"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-list/docs-raul-list.tsx",
		encapsulation: "none",
		tag: "docs-raul-list",
		readme: "# docs-raul-list\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"docs-code",
			"docs-readme",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-list": [
				"docs-element",
				"docs-showcase",
				"docs-code",
				"docs-readme",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-loaders/docs-raul-loaders.tsx",
		encapsulation: "none",
		tag: "docs-raul-loaders",
		readme: "# docs-raul-loaders\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"raul-heading",
			"raul-content",
			"raul-button",
			"raul-page-loader",
			"raul-container-loader",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-loaders": [
				"docs-element",
				"raul-heading",
				"raul-content",
				"raul-button",
				"raul-page-loader",
				"raul-container-loader",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"raul-heading": [
				"raul-text"
			],
			"raul-button": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-modal/docs-raul-modal.tsx",
		encapsulation: "none",
		tag: "docs-raul-modal",
		readme: "# docs-raul-modal\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-readme",
			"raul-heading",
			"raul-button",
			"raul-modal",
			"raul-modal-header",
			"raul-modal-footer",
			"raul-modal-body",
			"raul-content",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-modal": [
				"docs-element",
				"docs-readme",
				"raul-heading",
				"raul-button",
				"raul-modal",
				"raul-modal-header",
				"raul-modal-footer",
				"raul-modal-body",
				"raul-content",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"raul-heading": [
				"raul-text"
			],
			"raul-button": [
				"raul-icon"
			],
			"raul-modal-header": [
				"raul-content",
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-paging-bar/docs-raul-paging-bar.tsx",
		encapsulation: "none",
		tag: "docs-raul-paging-bar",
		readme: "# docs-raul-paging-bar\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"raul-paging-bar",
			"docs-readme",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-paging-bar": [
				"docs-element",
				"docs-showcase",
				"raul-paging-bar",
				"docs-readme",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"raul-paging-bar": [
				"raul-icon"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-progress/docs-raul-progress.tsx",
		encapsulation: "none",
		tag: "docs-raul-progress",
		readme: "# docs-raul-progress\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"docs-code",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-progress": [
				"docs-element",
				"docs-showcase",
				"docs-code",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-radio-button/docs-raul-radio-button.tsx",
		encapsulation: "none",
		tag: "docs-raul-radio-button",
		readme: "# docs-raul-radio-button\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-readme",
			"docs-showcase",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-radio-button": [
				"docs-element",
				"docs-readme",
				"docs-showcase",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-select/docs-raul-select.tsx",
		encapsulation: "none",
		tag: "docs-raul-select",
		readme: "# docs-raul-select\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"raul-select",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-select": [
				"docs-element",
				"docs-showcase",
				"raul-select",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-simple-select/docs-raul-simple-select.tsx",
		encapsulation: "none",
		tag: "docs-raul-simple-select",
		readme: "# docs-raul-simple-select\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"raul-simple-select",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-simple-select": [
				"docs-element",
				"docs-showcase",
				"raul-simple-select",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"raul-simple-select": [
				"raul-icon"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-simple-table/docs-raul-simple-table.tsx",
		encapsulation: "none",
		tag: "docs-raul-simple-table",
		readme: "# docs-raul-simple-table\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"docs-code",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-simple-table": [
				"docs-element",
				"docs-showcase",
				"docs-code",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-snackbar/docs-raul-snackbar.tsx",
		encapsulation: "none",
		tag: "docs-raul-snackbar",
		readme: "# docs-raul-snackbar\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-readme",
			"docs-showcase",
			"raul-snackbar",
			"raul-button",
			"docs-preview",
			"docs-interface",
			"raul-toaster",
			"raul-snackbar-group"
		],
		dependencyGraph: {
			"docs-raul-snackbar": [
				"docs-element",
				"docs-readme",
				"docs-showcase",
				"raul-snackbar",
				"raul-button",
				"docs-preview",
				"docs-interface",
				"raul-toaster",
				"raul-snackbar-group"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"raul-snackbar": [
				"raul-icon"
			],
			"raul-button": [
				"raul-icon"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-sortable-list/docs-raul-sortable-list.tsx",
		encapsulation: "none",
		tag: "docs-raul-sortable-list",
		readme: "# docs-raul-sortable-list\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"raul-heading",
			"raul-sortable-list",
			"raul-card",
			"raul-card-header",
			"raul-card-title",
			"raul-card-body",
			"docs-readme",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-sortable-list": [
				"docs-element",
				"docs-showcase",
				"raul-heading",
				"raul-sortable-list",
				"raul-card",
				"raul-card-header",
				"raul-card-title",
				"raul-card-body",
				"docs-readme",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"raul-heading": [
				"raul-text"
			],
			"raul-sortable-list": [
				"raul-heading"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-status/docs-raul-status.tsx",
		encapsulation: "none",
		tag: "docs-raul-status",
		readme: "# docs-raul-status\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"raul-status",
			"docs-readme",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-status": [
				"docs-element",
				"docs-showcase",
				"raul-status",
				"docs-readme",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-status-indicator/docs-raul-status-indicator.tsx",
		encapsulation: "none",
		tag: "docs-raul-status-indicator",
		readme: "# docs-raul-status\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"raul-status-indicator",
			"docs-readme",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-status-indicator": [
				"docs-element",
				"docs-showcase",
				"raul-status-indicator",
				"docs-readme",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-switch/docs-raul-switch.tsx",
		encapsulation: "none",
		tag: "docs-raul-switch",
		readme: "# docs-raul-switch\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"raul-switch",
			"docs-readme",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-switch": [
				"docs-element",
				"docs-showcase",
				"raul-switch",
				"docs-readme",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-tabs/docs-raul-tabs.tsx",
		encapsulation: "none",
		tag: "docs-raul-tabs",
		readme: "# docs-raul-tabs\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"raul-content",
			"raul-tabs",
			"raul-tab-pane",
			"docs-device",
			"raul-status",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-tabs": [
				"docs-element",
				"raul-content",
				"raul-tabs",
				"raul-tab-pane",
				"docs-device",
				"raul-status",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-text/docs-raul-text.tsx",
		encapsulation: "none",
		tag: "docs-raul-text",
		readme: "# docs-raul-text\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-markdown",
			"raul-tabs",
			"docs-readme",
			"docs-preview",
			"docs-showcase",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-text": [
				"docs-markdown",
				"raul-tabs",
				"docs-readme",
				"docs-preview",
				"docs-showcase",
				"docs-interface"
			],
			"docs-markdown": [
				"raul-content"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-readme": [
				"docs-markdown"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-textarea/docs-raul-textarea.tsx",
		encapsulation: "none",
		tag: "docs-raul-textarea",
		readme: "# docs-raul-textarea\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"raul-textarea",
			"docs-preview",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-textarea": [
				"docs-element",
				"docs-showcase",
				"raul-textarea",
				"docs-preview",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"raul-textarea": [
				"raul-icon"
			],
			"docs-preview": [
				"docs-preview-props"
			],
			"docs-preview-props": [
				"raul-input",
				"raul-switch",
				"raul-select"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-toggles/docs-raul-toggles.tsx",
		encapsulation: "none",
		tag: "docs-raul-toggles",
		readme: "# docs-raul-tabs\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"raul-toggles",
			"raul-toggle-pane",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-toggles": [
				"docs-element",
				"raul-toggles",
				"raul-toggle-pane",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"raul-toggles": [
				"raul-select"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-raul-tooltip/docs-raul-tooltip.tsx",
		encapsulation: "none",
		tag: "docs-raul-tooltip",
		readme: "# docs-raul-tooltip\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-showcase",
			"docs-code",
			"docs-interface"
		],
		dependencyGraph: {
			"docs-raul-tooltip": [
				"docs-element",
				"docs-showcase",
				"docs-code",
				"docs-interface"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-readme/docs-readme.tsx",
		encapsulation: "none",
		tag: "docs-readme",
		readme: "# docs-readme\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "component",
				type: "string",
				mutable: false,
				attr: "component",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: true
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-accordion",
			"docs-raul-action-menu",
			"docs-raul-alert",
			"docs-raul-aside",
			"docs-raul-badge",
			"docs-raul-bulk-action-bar",
			"docs-raul-button",
			"docs-raul-checkbox",
			"docs-raul-chips",
			"docs-raul-date-picker",
			"docs-raul-filter-bar",
			"docs-raul-filter-menu",
			"docs-raul-heading",
			"docs-raul-input",
			"docs-raul-list",
			"docs-raul-modal",
			"docs-raul-paging-bar",
			"docs-raul-radio-button",
			"docs-raul-snackbar",
			"docs-raul-sortable-list",
			"docs-raul-status",
			"docs-raul-status-indicator",
			"docs-raul-switch",
			"docs-raul-text"
		],
		dependencies: [
			"docs-markdown"
		],
		dependencyGraph: {
			"docs-readme": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			],
			"docs-raul-accordion": [
				"docs-readme"
			],
			"docs-raul-action-menu": [
				"docs-readme"
			],
			"docs-raul-alert": [
				"docs-readme"
			],
			"docs-raul-aside": [
				"docs-readme"
			],
			"docs-raul-badge": [
				"docs-readme"
			],
			"docs-raul-bulk-action-bar": [
				"docs-readme"
			],
			"docs-raul-button": [
				"docs-readme"
			],
			"docs-raul-checkbox": [
				"docs-readme"
			],
			"docs-raul-chips": [
				"docs-readme"
			],
			"docs-raul-date-picker": [
				"docs-readme"
			],
			"docs-raul-filter-bar": [
				"docs-readme"
			],
			"docs-raul-filter-menu": [
				"docs-readme"
			],
			"docs-raul-heading": [
				"docs-readme"
			],
			"docs-raul-input": [
				"docs-readme"
			],
			"docs-raul-list": [
				"docs-readme"
			],
			"docs-raul-modal": [
				"docs-readme"
			],
			"docs-raul-paging-bar": [
				"docs-readme"
			],
			"docs-raul-radio-button": [
				"docs-readme"
			],
			"docs-raul-snackbar": [
				"docs-readme"
			],
			"docs-raul-sortable-list": [
				"docs-readme"
			],
			"docs-raul-status": [
				"docs-readme"
			],
			"docs-raul-status-indicator": [
				"docs-readme"
			],
			"docs-raul-switch": [
				"docs-readme"
			],
			"docs-raul-text": [
				"docs-readme"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-shell/docs-shell.tsx",
		encapsulation: "none",
		tag: "docs-shell",
		readme: "# docs-shell\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-menu",
			"stencil-router",
			"stencil-route-switch",
			"stencil-route"
		],
		dependencyGraph: {
			"docs-shell": [
				"docs-menu",
				"stencil-router",
				"stencil-route-switch",
				"stencil-route"
			],
			"docs-menu": [
				"docs-menu-section",
				"docs-menu-page",
				"docs-menu-elem-page"
			],
			"docs-menu-section": [
				"stencil-route-link",
				"raul-icon"
			],
			"docs-menu-page": [
				"stencil-route-link"
			],
			"docs-menu-elem-page": [
				"docs-menu-page"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-showcase/docs-shadowcase.tsx",
		encapsulation: "none",
		tag: "docs-showcase",
		readme: "# docs-showcase\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-accordion",
			"docs-raul-action-menu",
			"docs-raul-alert",
			"docs-raul-aside",
			"docs-raul-avatar",
			"docs-raul-badge",
			"docs-raul-breadcrumbs",
			"docs-raul-bulk-action-bar",
			"docs-raul-button",
			"docs-raul-card",
			"docs-raul-checkbox",
			"docs-raul-chips",
			"docs-raul-date-picker",
			"docs-raul-filter-menu",
			"docs-raul-footer",
			"docs-raul-grid",
			"docs-raul-heading",
			"docs-raul-input",
			"docs-raul-list",
			"docs-raul-paging-bar",
			"docs-raul-progress",
			"docs-raul-radio-button",
			"docs-raul-select",
			"docs-raul-simple-select",
			"docs-raul-simple-table",
			"docs-raul-snackbar",
			"docs-raul-sortable-list",
			"docs-raul-status",
			"docs-raul-status-indicator",
			"docs-raul-switch",
			"docs-raul-text",
			"docs-raul-textarea",
			"docs-raul-tooltip"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-accordion": [
				"docs-showcase"
			],
			"docs-raul-action-menu": [
				"docs-showcase"
			],
			"docs-raul-alert": [
				"docs-showcase"
			],
			"docs-raul-aside": [
				"docs-showcase"
			],
			"docs-raul-avatar": [
				"docs-showcase"
			],
			"docs-raul-badge": [
				"docs-showcase"
			],
			"docs-raul-breadcrumbs": [
				"docs-showcase"
			],
			"docs-raul-bulk-action-bar": [
				"docs-showcase"
			],
			"docs-raul-button": [
				"docs-showcase"
			],
			"docs-raul-card": [
				"docs-showcase"
			],
			"docs-raul-checkbox": [
				"docs-showcase"
			],
			"docs-raul-chips": [
				"docs-showcase"
			],
			"docs-raul-date-picker": [
				"docs-showcase"
			],
			"docs-raul-filter-menu": [
				"docs-showcase"
			],
			"docs-raul-footer": [
				"docs-showcase"
			],
			"docs-raul-grid": [
				"docs-showcase"
			],
			"docs-raul-heading": [
				"docs-showcase"
			],
			"docs-raul-input": [
				"docs-showcase"
			],
			"docs-raul-list": [
				"docs-showcase"
			],
			"docs-raul-paging-bar": [
				"docs-showcase"
			],
			"docs-raul-progress": [
				"docs-showcase"
			],
			"docs-raul-radio-button": [
				"docs-showcase"
			],
			"docs-raul-select": [
				"docs-showcase"
			],
			"docs-raul-simple-select": [
				"docs-showcase"
			],
			"docs-raul-simple-table": [
				"docs-showcase"
			],
			"docs-raul-snackbar": [
				"docs-showcase"
			],
			"docs-raul-sortable-list": [
				"docs-showcase"
			],
			"docs-raul-status": [
				"docs-showcase"
			],
			"docs-raul-status-indicator": [
				"docs-showcase"
			],
			"docs-raul-switch": [
				"docs-showcase"
			],
			"docs-raul-text": [
				"docs-showcase"
			],
			"docs-raul-textarea": [
				"docs-showcase"
			],
			"docs-raul-tooltip": [
				"docs-showcase"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-spacing/docs-spacing.tsx",
		encapsulation: "none",
		tag: "docs-spacing",
		readme: "# docs-spacing\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element"
		],
		dependencyGraph: {
			"docs-spacing": [
				"docs-element"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-typography/docs-typography.tsx",
		encapsulation: "none",
		tag: "docs-typography",
		readme: "# docs-typography\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-element",
			"docs-markdown"
		],
		dependencyGraph: {
			"docs-typography": [
				"docs-element",
				"docs-markdown"
			],
			"docs-element": [
				"raul-tabs"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-markdown": [
				"raul-content"
			]
		}
	},
	{
		filePath: "src/components/docs/docs-pages/docs-upgrading/docs-upgrading.tsx",
		encapsulation: "none",
		tag: "docs-upgrading",
		readme: "# docs-upgrading\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"docs-markdown"
		],
		dependencyGraph: {
			"docs-upgrading": [
				"docs-markdown"
			],
			"docs-markdown": [
				"raul-content"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-accordion/raul-accordion.tsx",
		encapsulation: "none",
		tag: "raul-accordion",
		readme: "# raul-accordion\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-accordion"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-accordion": [
				"raul-accordion"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-accordion/raul-accordion-item/raul-accordion-item.tsx",
		encapsulation: "none",
		tag: "raul-accordion-item",
		readme: "# raul-accordion-item\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "disabled",
				type: "boolean",
				mutable: false,
				attr: "disabled",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "expanded",
				type: "boolean",
				mutable: false,
				attr: "expanded",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "null",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "name",
				type: "string",
				mutable: false,
				attr: "name",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "`accordion-${this.accordionId}`",
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
			{
				event: "raulChange",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-accordion"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-accordion": [
				"raul-accordion-item"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-accordion/raul-accordion-item-header/raul-accordion-item-header.tsx",
		encapsulation: "none",
		tag: "raul-accordion-item-header",
		readme: "# raul-accordion-item-header\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "disabled",
				type: "boolean",
				mutable: false,
				attr: "disabled",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "expanded",
				type: "boolean",
				mutable: false,
				attr: "expanded",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "name",
				type: "string",
				mutable: false,
				attr: "name",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
			{
				event: "accordionItemHeaderClick",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-accordion"
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-accordion-item-header": [
				"raul-icon"
			],
			"docs-raul-accordion": [
				"raul-accordion-item-header"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-accordion/raul-accordion-item-panel/raul-accordion-item-panel.tsx",
		encapsulation: "none",
		tag: "raul-accordion-item-panel",
		readme: "# raul-accordion-item-panel\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "expanded",
				type: "boolean",
				mutable: false,
				attr: "expanded",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "name",
				type: "string",
				mutable: false,
				attr: "name",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-accordion"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-accordion": [
				"raul-accordion-item-panel"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-accordion/raul-accordion-item-title/raul-accordion-item-title.tsx",
		encapsulation: "none",
		tag: "raul-accordion-item-title",
		readme: "# raul-accordion-item-title\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-accordion"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-accordion": [
				"raul-accordion-item-title"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-action-menu/raul-action-menu.tsx",
		encapsulation: "none",
		tag: "raul-action-menu",
		readme: "# raul-action-menu\n\n## Usage\n\n```html\n<raul-action-menu>\n  <raul-action-menu-item payload=\"first-action\">\n    First action\n  </raul-action-menu-item>\n  <raul-action-menu-item payload=\"second-action\">\n    Second action\n  </raul-action-menu-item>\n  <raul-action-menu-item url=\"https://google.com\">\n    Go to google\n  </raul-action-menu-item>\n<raul-action-menu>\n```\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "disabled",
				type: "boolean",
				mutable: false,
				attr: "disabled",
				reflectToAttr: false,
				docs: "Disables actions",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "emphasizeFinal",
				type: "boolean",
				mutable: false,
				attr: "emphasize-final",
				reflectToAttr: false,
				docs: "If set to true, the last action will be separated with a divider",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "items",
				type: "DropdownMenuItem[]",
				mutable: false,
				reflectToAttr: false,
				docs: "As an alternative to the <raul-action-menu-item>, you can programatically provide an array of items to be shown in the dropdown. {title: `string`, url?: `string`, payload: `any`}.\nPayload will be the detail of the `optionSelected` event emitted when clicking an action that doesn't have an url",
				docsTags: [
				],
				values: [
					{
						type: "DropdownMenuItem[]"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
			{
				name: "closeMenu",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "closeMenu() => Promise<void>",
				parameters: [
				],
				docs: "Method to programatically close the menu",
				docsTags: [
				]
			}
		],
		events: [
			{
				event: "optionSelected",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "Event emitted when an option is selected.",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-grid-list",
			"docs-raul-accordion",
			"docs-raul-action-menu"
		],
		dependencies: [
			"raul-dropdown-menu",
			"raul-icon"
		],
		dependencyGraph: {
			"raul-action-menu": [
				"raul-dropdown-menu",
				"raul-icon"
			],
			"raul-dropdown-menu": [
				"raul-icon"
			],
			"docs-grid-list": [
				"raul-action-menu"
			],
			"docs-raul-accordion": [
				"raul-action-menu"
			],
			"docs-raul-action-menu": [
				"raul-action-menu"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-action-menu/raul-action-menu-item.tsx",
		encapsulation: "none",
		tag: "raul-action-menu-item",
		readme: "# raul-action-menu\n\n## Usage\n\n```html\n<raul-action-menu>\n  <raul-action-menu-item payload=\"first-action\">\n    First action\n  </raul-action-menu-item>\n  <raul-action-menu-item payload=\"second-action\">\n    Second action\n  </raul-action-menu-item>\n  <raul-action-menu-item url=\"https://google.com\">\n    Go to google\n  </raul-action-menu-item>\n<raul-action-menu>\n```\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "disabled",
				type: "boolean",
				mutable: false,
				attr: "disabled",
				reflectToAttr: false,
				docs: "If true, the option will be disabled",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "payload",
				type: "unknown",
				mutable: false,
				reflectToAttr: false,
				docs: "Any sort of data that should be passed in the optionSelected event.detail and the callback functions",
				docsTags: [
				],
				values: [
					{
						type: "unknown"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "url",
				type: "string",
				mutable: false,
				attr: "url",
				reflectToAttr: false,
				docs: "If you provide an url, the raul-action-menu-item will render an `a` tag",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
			{
				event: "blurCallback",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "The raul-action-menu-item will pass the blur event and the payload in the callback",
				docsTags: [
				]
			},
			{
				event: "clickCallback",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "The raul-action-menu-item will pass the click event and the payload in the callback",
				docsTags: [
				]
			},
			{
				event: "optionSelected",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "Event emitted when an option is selected.",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-accordion",
			"docs-raul-action-menu"
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-action-menu-item": [
				"raul-icon"
			],
			"docs-raul-accordion": [
				"raul-action-menu-item"
			],
			"docs-raul-action-menu": [
				"raul-action-menu-item"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-alert/raul-alert.tsx",
		encapsulation: "none",
		tag: "raul-alert",
		readme: "# raul-alert\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "content",
				type: "string",
				mutable: false,
				attr: "content",
				reflectToAttr: false,
				docs: "Alert text",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: true,
				required: false
			},
			{
				name: "ctaMessage",
				type: "string",
				mutable: false,
				attr: "cta-message",
				reflectToAttr: false,
				docs: "Action link text",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: true,
				required: false
			},
			{
				name: "ctaUrl",
				type: "string",
				mutable: false,
				attr: "cta-url",
				reflectToAttr: false,
				docs: "An action url",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: true,
				required: false
			},
			{
				name: "heading",
				type: "string",
				mutable: false,
				attr: "heading",
				reflectToAttr: false,
				docs: "A header",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "rounded",
				type: "boolean",
				mutable: false,
				attr: "rounded",
				reflectToAttr: false,
				docs: "Corners can be rounded or not",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: true,
				required: false
			},
			{
				name: "variant",
				type: "\"danger\" | \"information\" | \"success\" | \"warning\"",
				mutable: false,
				attr: "variant",
				reflectToAttr: false,
				docs: "Determines the color of the left bar",
				docsTags: [
				],
				"default": "'information'",
				values: [
					{
						value: "danger",
						type: "string"
					},
					{
						value: "information",
						type: "string"
					},
					{
						value: "success",
						type: "string"
					},
					{
						value: "warning",
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-alert"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-alert": [
				"raul-alert"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-aside/raul-aside.tsx",
		encapsulation: "none",
		tag: "raul-aside",
		readme: "# raul-aside\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "size",
				type: "\"large\" | \"medium\" | \"small\"",
				mutable: false,
				attr: "size",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "'medium'",
				values: [
					{
						value: "large",
						type: "string"
					},
					{
						value: "medium",
						type: "string"
					},
					{
						value: "small",
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
			{
				name: "close",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "close() => Promise<void>",
				parameters: [
				],
				docs: "Closes the aside.",
				docsTags: [
					{
						name: "returns"
					}
				]
			},
			{
				name: "open",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "open() => Promise<void>",
				parameters: [
				],
				docs: "Opens the aside.",
				docsTags: [
					{
						name: "returns"
					}
				]
			}
		],
		events: [
			{
				event: "raulAsideClose",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "Emitted when the aside closes.",
				docsTags: [
				]
			},
			{
				event: "raulAsideOpen",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "Emitted when the aside opens.",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-aside",
			"docs-raul-filter-bar"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-aside": [
				"raul-aside"
			],
			"docs-raul-filter-bar": [
				"raul-aside"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-aside/raul-aside-actions/raul-aside-actions.tsx",
		encapsulation: "none",
		tag: "raul-aside-actions",
		readme: "# raul-aside-actions\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-aside"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-aside": [
				"raul-aside-actions"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-aside/raul-aside-body/raul-aside-body.tsx",
		encapsulation: "none",
		tag: "raul-aside-body",
		readme: "# raul-aside-body\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-aside",
			"docs-raul-filter-bar"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-aside": [
				"raul-aside-body"
			],
			"docs-raul-filter-bar": [
				"raul-aside-body"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-aside/raul-aside-footer/raul-aside-footer.tsx",
		encapsulation: "none",
		tag: "raul-aside-footer",
		readme: "# raul-aside-footer\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-aside",
			"docs-raul-filter-bar"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-aside": [
				"raul-aside-footer"
			],
			"docs-raul-filter-bar": [
				"raul-aside-footer"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-aside/raul-aside-header/raul-aside-header.tsx",
		encapsulation: "none",
		tag: "raul-aside-header",
		readme: "# raul-aside-header\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-aside",
			"docs-raul-filter-bar"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-aside": [
				"raul-aside-header"
			],
			"docs-raul-filter-bar": [
				"raul-aside-header"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-aside/raul-aside-subtitle/raul-aside-subtitle.tsx",
		encapsulation: "none",
		tag: "raul-aside-subtitle",
		readme: "# raul-aside-subtitle\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-aside"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-aside": [
				"raul-aside-subtitle"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-aside/raul-aside-title/raul-aside-title.tsx",
		encapsulation: "none",
		tag: "raul-aside-title",
		readme: "# raul-aside-title\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-aside"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-aside": [
				"raul-aside-title"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-avatar/raul-avatar.tsx",
		encapsulation: "none",
		tag: "raul-avatar",
		readme: "# raul-avatar\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "primary",
				type: "boolean",
				mutable: false,
				attr: "primary",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "small",
				type: "boolean",
				mutable: false,
				attr: "small",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "variant",
				type: "\"profile\" | \"property\"",
				mutable: false,
				attr: "variant",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "'profile'",
				values: [
					{
						value: "profile",
						type: "string"
					},
					{
						value: "property",
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-avatar"
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-avatar": [
				"raul-icon"
			],
			"docs-raul-avatar": [
				"raul-avatar"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-badge/raul-badge.tsx",
		encapsulation: "none",
		tag: "raul-badge",
		readme: "# raul-badge\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "content",
				type: "string",
				mutable: false,
				attr: "content",
				reflectToAttr: false,
				docs: "The text or number to display in the badge.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "icon",
				type: "string",
				mutable: false,
				attr: "icon",
				reflectToAttr: false,
				docs: "The icon to display to the left of the content.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "variant",
				type: "\"error\" | \"primary\" | \"success\" | \"warning\"",
				mutable: false,
				attr: "variant",
				reflectToAttr: false,
				docs: "Determines the primary appearance of the badge based on its purpose.",
				docsTags: [
				],
				"default": "'primary'",
				values: [
					{
						value: "error",
						type: "string"
					},
					{
						value: "primary",
						type: "string"
					},
					{
						value: "success",
						type: "string"
					},
					{
						value: "warning",
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-badge"
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-badge": [
				"raul-icon"
			],
			"docs-raul-badge": [
				"raul-badge"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-breadcrumbs/raul-breadcrumb/raul-breadcrumb.tsx",
		encapsulation: "none",
		tag: "raul-breadcrumb",
		readme: "# raul-breadcrumb\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-breadcrumb": [
				"raul-icon"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-breadcrumbs/raul-breadcrumb-back-icon/raul-breadcrumb-back-icon.tsx",
		encapsulation: "none",
		tag: "raul-breadcrumb-back-icon",
		readme: "# raul-breadcrumb-back-icon\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-breadcrumb-back-icon": [
				"raul-icon"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-breadcrumbs/raul-breadcrumbs.tsx",
		encapsulation: "none",
		tag: "raul-breadcrumbs",
		readme: "# raul-breadcrumbs\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "overflow",
				type: "boolean",
				mutable: false,
				attr: "overflow",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-bulk-action-bar/raul-bulk-action-bar.tsx",
		encapsulation: "none",
		tag: "raul-bulk-action-bar",
		readme: "# raul-bulk-action-bar\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "open",
				type: "boolean",
				mutable: false,
				attr: "open",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "selectedCount",
				type: "number",
				mutable: false,
				attr: "selected-count",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "0",
				values: [
					{
						type: "number"
					}
				],
				optional: true,
				required: false
			},
			{
				name: "totalRecords",
				type: "number",
				mutable: false,
				attr: "total-records",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "0",
				values: [
					{
						type: "number"
					}
				],
				optional: true,
				required: false
			}
		],
		methods: [
		],
		events: [
			{
				event: "bulkActionsClose",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			},
			{
				event: "bulkActionsSelectAll",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-grid-list",
			"docs-raul-bulk-action-bar"
		],
		dependencies: [
			"raul-button",
			"raul-icon"
		],
		dependencyGraph: {
			"raul-bulk-action-bar": [
				"raul-button",
				"raul-icon"
			],
			"raul-button": [
				"raul-icon"
			],
			"docs-grid-list": [
				"raul-bulk-action-bar"
			],
			"docs-raul-bulk-action-bar": [
				"raul-bulk-action-bar"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-button/raul-button.tsx",
		encapsulation: "none",
		tag: "raul-button",
		readme: "# Button\n\nSomething about how to use buttons\n",
		docs: "Something about how to use buttons",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "add",
				type: "boolean",
				mutable: false,
				attr: "add",
				reflectToAttr: true,
				docs: "Adds `add` icon.",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "delete",
				type: "boolean",
				mutable: false,
				attr: "delete",
				reflectToAttr: true,
				docs: "Adds `delete` icon.",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "disabled",
				type: "boolean",
				mutable: false,
				attr: "disabled",
				reflectToAttr: true,
				docs: "Controls whether this button is disabled.",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "href",
				type: "string",
				mutable: false,
				attr: "href",
				reflectToAttr: true,
				docs: "Determines link behavior. Only valid for links.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "icon",
				type: "string",
				mutable: false,
				attr: "icon",
				reflectToAttr: true,
				docs: "The button icon name.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "iconKind",
				type: "string",
				mutable: false,
				attr: "icon-kind",
				reflectToAttr: true,
				docs: "The button icon kind.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "size",
				type: "\"default\" | \"small\"",
				mutable: false,
				attr: "size",
				reflectToAttr: true,
				docs: "Determines the primary appearance of the button based on its purpose.",
				docsTags: [
				],
				"default": "\"default\"",
				values: [
					{
						value: "default",
						type: "string"
					},
					{
						value: "small",
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "type",
				type: "\"button\" | \"reset\" | \"submit\"",
				mutable: false,
				attr: "type",
				reflectToAttr: true,
				docs: "Controls the underlying markup based on the use case for the button.",
				docsTags: [
				],
				values: [
					{
						value: "button",
						type: "string"
					},
					{
						value: "reset",
						type: "string"
					},
					{
						value: "submit",
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "value",
				type: "string",
				mutable: false,
				attr: "value",
				reflectToAttr: true,
				docs: "Only valid for input types (submit, reset).",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "variant",
				type: "\"control\" | \"danger\" | \"primary\" | \"reverse\" | \"secondary\" | \"text\"",
				mutable: false,
				attr: "variant",
				reflectToAttr: true,
				docs: "Determines the primary appearance of the button based on its purpose.",
				docsTags: [
				],
				"default": "\"secondary\"",
				values: [
					{
						value: "control",
						type: "string"
					},
					{
						value: "danger",
						type: "string"
					},
					{
						value: "primary",
						type: "string"
					},
					{
						value: "reverse",
						type: "string"
					},
					{
						value: "secondary",
						type: "string"
					},
					{
						value: "text",
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-grid-list",
			"docs-home",
			"docs-raul-aside",
			"docs-raul-bulk-action-bar",
			"docs-raul-button",
			"docs-raul-filter-bar",
			"docs-raul-loaders",
			"docs-raul-modal",
			"docs-raul-snackbar",
			"raul-bulk-action-bar"
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-button": [
				"raul-icon"
			],
			"docs-grid-list": [
				"raul-button"
			],
			"docs-home": [
				"raul-button"
			],
			"docs-raul-aside": [
				"raul-button"
			],
			"docs-raul-bulk-action-bar": [
				"raul-button"
			],
			"docs-raul-button": [
				"raul-button"
			],
			"docs-raul-filter-bar": [
				"raul-button"
			],
			"docs-raul-loaders": [
				"raul-button"
			],
			"docs-raul-modal": [
				"raul-button"
			],
			"docs-raul-snackbar": [
				"raul-button"
			],
			"raul-bulk-action-bar": [
				"raul-button"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-card/raul-card.tsx",
		encapsulation: "none",
		tag: "raul-card",
		readme: "# raul-card\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "hoverable",
				type: "boolean",
				mutable: false,
				attr: "hoverable",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-sortable-list"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-sortable-list": [
				"raul-card"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-card/raul-card-body/raul-card-body.tsx",
		encapsulation: "none",
		tag: "raul-card-body",
		readme: "# raul-card-body\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-sortable-list"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-sortable-list": [
				"raul-card-body"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-card/raul-card-footer/raul-card-footer.tsx",
		encapsulation: "none",
		tag: "raul-card-footer",
		readme: "# raul-card-footer\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-card/raul-card-header/raul-card-header.tsx",
		encapsulation: "none",
		tag: "raul-card-header",
		readme: "# raul-card-header\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-sortable-list"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-sortable-list": [
				"raul-card-header"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-card/raul-card-header-actions/raul-card-header-actions.tsx",
		encapsulation: "none",
		tag: "raul-card-header-actions",
		readme: "# raul-card-header-actions\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-card/raul-card-subtitle/raul-card-subtitle.tsx",
		encapsulation: "none",
		tag: "raul-card-subtitle",
		readme: "# raul-card-subtitle\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-card/raul-card-title/raul-card-title.tsx",
		encapsulation: "none",
		tag: "raul-card-title",
		readme: "# raul-card-title\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-sortable-list"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-sortable-list": [
				"raul-card-title"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-checkbox/raul-checkbox.tsx",
		encapsulation: "none",
		tag: "raul-checkbox",
		readme: "# raul-checkbox\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "invalid",
				type: "boolean",
				mutable: false,
				attr: "invalid",
				reflectToAttr: false,
				docs: "If `true`, the checkbox border will become red. This can be useful for form validations.",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "labelText",
				type: "string",
				mutable: false,
				attr: "label-text",
				reflectToAttr: false,
				docs: "The text label.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "small",
				type: "boolean",
				mutable: false,
				attr: "small",
				reflectToAttr: true,
				docs: "If `true`, the checkbox size will be small.",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-grid-list",
			"raul-option"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-grid-list": [
				"raul-checkbox"
			],
			"raul-option": [
				"raul-checkbox"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-chip/raul-chip.tsx",
		encapsulation: "none",
		tag: "raul-chip",
		readme: "# raul-chips\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "removable",
				type: "boolean",
				mutable: false,
				attr: "removable",
				reflectToAttr: false,
				docs: "Makes the chip removable.",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
			{
				event: "raulChipRemove",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "Emitted when the removable chip is clicked or delete/backspace keys are pressed.",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-chips"
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-chip": [
				"raul-icon"
			],
			"docs-raul-chips": [
				"raul-chip"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-loaders/raul-container-loder.tsx",
		encapsulation: "none",
		tag: "raul-container-loader",
		readme: "# raul-page-loader\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-loaders"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-loaders": [
				"raul-container-loader"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-content/raul-content.tsx",
		encapsulation: "none",
		tag: "raul-content",
		readme: "# raul-content\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "fullWidth",
				type: "boolean",
				mutable: false,
				attr: "full-width",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-markdown",
			"docs-raul-filter-bar",
			"docs-raul-loaders",
			"docs-raul-modal",
			"docs-raul-tabs",
			"raul-modal-header",
			"raul-range-picker"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-markdown": [
				"raul-content"
			],
			"docs-raul-filter-bar": [
				"raul-content"
			],
			"docs-raul-loaders": [
				"raul-content"
			],
			"docs-raul-modal": [
				"raul-content"
			],
			"docs-raul-tabs": [
				"raul-content"
			],
			"raul-modal-header": [
				"raul-content"
			],
			"raul-range-picker": [
				"raul-content"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-date-picker/raul-date-picker.tsx",
		encapsulation: "none",
		tag: "raul-date-picker",
		readme: "# raul-date-picker\n\n## Usage\n\n```html\n<raul-date-picker>\n</raul-date-picker>\n```\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "disabled",
				type: "boolean",
				mutable: false,
				attr: "disabled",
				reflectToAttr: false,
				docs: "If `true`, the date picker is disabled",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "error",
				type: "string",
				mutable: false,
				attr: "error",
				reflectToAttr: false,
				docs: "Makes the datepicker visually invalid and shows the feedback message.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "hint",
				type: "string",
				mutable: false,
				attr: "hint",
				reflectToAttr: false,
				docs: "Optional hint text.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "initialDate",
				type: "Date",
				mutable: false,
				reflectToAttr: false,
				docs: "A javascript Date object that represents the initial date of the date picker",
				docsTags: [
				],
				"default": "null",
				values: [
					{
						type: "Date"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "initialEndDate",
				type: "Date",
				mutable: false,
				reflectToAttr: false,
				docs: "A javascript Date object that represents the initial end date of the range picker",
				docsTags: [
				],
				"default": "null",
				values: [
					{
						type: "Date"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "initialStartDate",
				type: "Date",
				mutable: false,
				reflectToAttr: false,
				docs: "A javascript Date object that represents the initial start date of the range picker",
				docsTags: [
				],
				"default": "null",
				values: [
					{
						type: "Date"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "isInRangePicker",
				type: "boolean",
				mutable: false,
				attr: "is-in-range-picker",
				reflectToAttr: false,
				docs: "This is used internally by `raul-range-picker`",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "label",
				type: "string",
				mutable: false,
				attr: "label",
				reflectToAttr: false,
				docs: "A string that will be shown above the date picker",
				docsTags: [
				],
				"default": "''",
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "variant",
				type: "\"date\" | \"inline\" | \"range\"",
				mutable: false,
				attr: "variant",
				reflectToAttr: false,
				docs: "Can be `date` or `range`. Defaults to `date`",
				docsTags: [
				],
				"default": "'date'",
				values: [
					{
						value: "date",
						type: "string"
					},
					{
						value: "inline",
						type: "string"
					},
					{
						value: "range",
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
			{
				name: "clearDisplay",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "clearDisplay() => Promise<void>",
				parameters: [
				],
				docs: "This is used internally by `raul-range-picker`",
				docsTags: [
				]
			},
			{
				name: "clearPicker",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "clearPicker() => Promise<void>",
				parameters: [
				],
				docs: "Method used to programatically clear the picker",
				docsTags: [
				]
			},
			{
				name: "closePicker",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "closePicker() => Promise<void>",
				parameters: [
				],
				docs: "A method to programatically close the picker",
				docsTags: [
				]
			},
			{
				name: "openPicker",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "openPicker() => Promise<void>",
				parameters: [
				],
				docs: "A method to programatically open the picker",
				docsTags: [
				]
			},
			{
				name: "revertPicker",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "revertPicker() => Promise<void>",
				parameters: [
				],
				docs: "This is used internally by `raul-range-picker`",
				docsTags: [
				]
			},
			{
				name: "setDisplay",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "setDisplay(text: string) => Promise<void>",
				parameters: [
				],
				docs: "This is used internally by `raul-range-picker`",
				docsTags: [
				]
			},
			{
				name: "setPickerDate",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "setPickerDate(date: Date) => Promise<void>",
				parameters: [
				],
				docs: "This is used internally by `raul-range-picker`",
				docsTags: [
				]
			},
			{
				name: "setPickerEndDate",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "setPickerEndDate(date: Date) => Promise<void>",
				parameters: [
				],
				docs: "This is used internally by `raul-range-picker`",
				docsTags: [
				]
			},
			{
				name: "setPickerStartDate",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "setPickerStartDate(date: Date) => Promise<void>",
				parameters: [
				],
				docs: "This is used internally by `raul-range-picker`",
				docsTags: [
				]
			},
			{
				name: "setStateDate",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "setStateDate(date: Date) => Promise<void>",
				parameters: [
				],
				docs: "This is used internally by `raul-range-picker`",
				docsTags: [
				]
			},
			{
				name: "setStateEndDate",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "setStateEndDate(date: Date) => Promise<void>",
				parameters: [
				],
				docs: "This is used internally by `raul-range-picker`",
				docsTags: [
				]
			},
			{
				name: "setStateStartDate",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "setStateStartDate(date: Date) => Promise<void>",
				parameters: [
				],
				docs: "This is used internally by `raul-range-picker`",
				docsTags: [
				]
			}
		],
		events: [
			{
				event: "dateSelected",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "Event emitted when a date is selected. Event.detail will be a javascript Date object",
				docsTags: [
				]
			},
			{
				event: "rangeSelected",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "Event emitted when a range is selected. Event.detail will be {startDate: Date, endDate: Date}, where `Date` will be a javascript Date object",
				docsTags: [
				]
			},
			{
				event: "UNSAFE_dateSelected",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "To be used internally by `raul-range-picker`",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-date-picker",
			"raul-range-picker"
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-date-picker": [
				"raul-icon"
			],
			"docs-raul-date-picker": [
				"raul-date-picker"
			],
			"raul-range-picker": [
				"raul-date-picker"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-dropdown-menu/raul-dropdown-menu.tsx",
		encapsulation: "none",
		tag: "raul-dropdown-menu",
		readme: "# raul-dropdown-menu\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "color",
				type: "\"active\" | \"primary\"",
				mutable: false,
				attr: "color",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "'primary'",
				values: [
					{
						value: "active",
						type: "string"
					},
					{
						value: "primary",
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "disabled",
				type: "boolean",
				mutable: false,
				attr: "disabled",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "dividers",
				type: "boolean",
				mutable: false,
				attr: "dividers",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "emphasizeFinal",
				type: "boolean",
				mutable: false,
				attr: "emphasize-final",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "items",
				type: "DropdownMenuItem[]",
				mutable: false,
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "DropdownMenuItem[]"
					}
				],
				optional: true,
				required: false
			}
		],
		methods: [
			{
				name: "closeMenu",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "closeMenu() => Promise<void>",
				parameters: [
				],
				docs: "",
				docsTags: [
				]
			}
		],
		events: [
			{
				event: "optionSelected",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"raul-action-menu",
			"raul-filter-menu"
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-dropdown-menu": [
				"raul-icon"
			],
			"raul-action-menu": [
				"raul-dropdown-menu"
			],
			"raul-filter-menu": [
				"raul-dropdown-menu"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-filter-bar/raul-filter-bar.tsx",
		encapsulation: "none",
		tag: "raul-filter-bar",
		readme: "# raul-filter-bar-button\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-filter-bar"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-filter-bar": [
				"raul-filter-bar"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-filter-bar/raul-filter-bar-button.tsx",
		encapsulation: "none",
		tag: "raul-filter-bar-button",
		readme: "# raul-filter-bar-button\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "filterCount",
				type: "number",
				mutable: false,
				attr: "filter-count",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "0",
				values: [
					{
						type: "number"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-filter-bar"
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-filter-bar-button": [
				"raul-icon"
			],
			"docs-raul-filter-bar": [
				"raul-filter-bar-button"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-filter-menu/raul-filter-menu.tsx",
		encapsulation: "none",
		tag: "raul-filter-menu",
		readme: "# raul-filter-menu\n\n## Usage\n\n```html\n  <raul-filter-menu>\n    <raul-filter-menu-item icon='alien-head' payload=\"first-item\">\n      First item\n    </raul-filter-menu-item>\n    <raul-filter-menu-item icon='artist' payload=\"second-item\">\n      Second item\n    </raul-filter-menu-item>\n    <raul-filter-menu-item icon='astronaut' payload=\"third-item\">\n      Third item\n    </raul-filter-menu-item>\n  </raul-filter-menu>\n```\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "icon",
				type: "string",
				mutable: false,
				attr: "icon",
				reflectToAttr: false,
				docs: "Icon to be used in the menu toggle. Defaults to `list-bullets-3`",
				docsTags: [
				],
				"default": "'list-bullets-3'",
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "items",
				type: "DropdownMenuItem[]",
				mutable: false,
				reflectToAttr: false,
				docs: "As an alternative to the <raul-filter-menu-item>, you can programatically provide an array of items to be shown in the dropdown. {title: `string`, icon: `string`, payload: `any`}.\nPayload will be the detail of the `optionSelected` event emitted when clicking an action",
				docsTags: [
				],
				values: [
					{
						type: "DropdownMenuItem[]"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
			{
				name: "closeMenu",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "closeMenu() => Promise<void>",
				parameters: [
				],
				docs: "Method to programatically close the menu",
				docsTags: [
				]
			}
		],
		events: [
			{
				event: "optionSelected",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "Event emitted when an option is selected.",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-filter-bar",
			"docs-raul-filter-menu"
		],
		dependencies: [
			"raul-dropdown-menu",
			"raul-icon"
		],
		dependencyGraph: {
			"raul-filter-menu": [
				"raul-dropdown-menu",
				"raul-icon"
			],
			"raul-dropdown-menu": [
				"raul-icon"
			],
			"docs-raul-filter-bar": [
				"raul-filter-menu"
			],
			"docs-raul-filter-menu": [
				"raul-filter-menu"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-filter-menu/raul-filter-menu-item.tsx",
		encapsulation: "none",
		tag: "raul-filter-menu-item",
		readme: "# raul-filter-menu\n\n## Usage\n\n```html\n  <raul-filter-menu>\n    <raul-filter-menu-item icon='alien-head' payload=\"first-item\">\n      First item\n    </raul-filter-menu-item>\n    <raul-filter-menu-item icon='artist' payload=\"second-item\">\n      Second item\n    </raul-filter-menu-item>\n    <raul-filter-menu-item icon='astronaut' payload=\"third-item\">\n      Third item\n    </raul-filter-menu-item>\n  </raul-filter-menu>\n```\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "disabled",
				type: "boolean",
				mutable: false,
				attr: "disabled",
				reflectToAttr: false,
				docs: "If true, the option will be disabled",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "icon",
				type: "string",
				mutable: false,
				attr: "icon",
				reflectToAttr: false,
				docs: "An icon to be rendered before the item",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "onBlurCallback",
				type: "Function",
				mutable: false,
				reflectToAttr: false,
				docs: "The raul-filter-menu-item will pass the blur event and the payload in the callback",
				docsTags: [
				],
				values: [
					{
						type: "Function"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "onClickCallback",
				type: "Function",
				mutable: false,
				reflectToAttr: false,
				docs: "The raul-filter-menu-item will pass the click event and the payload in the callback",
				docsTags: [
				],
				values: [
					{
						type: "Function"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "payload",
				type: "unknown",
				mutable: false,
				reflectToAttr: false,
				docs: "Any sort of data that should be passed in the optionSelected event.detail and the callback functions",
				docsTags: [
				],
				values: [
					{
						type: "unknown"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
			{
				event: "optionSelected",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "Event emitted when an option is selected.",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-filter-bar",
			"docs-raul-filter-menu"
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-filter-menu-item": [
				"raul-icon"
			],
			"docs-raul-filter-bar": [
				"raul-filter-menu-item"
			],
			"docs-raul-filter-menu": [
				"raul-filter-menu-item"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-footer/raul-footer.tsx",
		encapsulation: "none",
		tag: "raul-footer",
		readme: "# raul-footer\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-footer/raul-footer-contact/raul-footer-contact.tsx",
		encapsulation: "none",
		tag: "raul-footer-contact",
		readme: "# raul-footer-contact\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-footer/raul-footer-copyright/raul-footer-copyright.tsx",
		encapsulation: "none",
		tag: "raul-footer-copyright",
		readme: "# raul-footer-copyright\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-footer/raul-footer-navigation-link/raul-footer-navigation-link.tsx",
		encapsulation: "none",
		tag: "raul-footer-navigation-link",
		readme: "# raul-footer-navigation-link\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-footer/raul-footer-navigation-links/raul-footer-navigation-links.tsx",
		encapsulation: "none",
		tag: "raul-footer-navigation-links",
		readme: "# raul-footer-navigation-links\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-footer/raul-footer-social-icon/raul-footer-social-icon.tsx",
		encapsulation: "none",
		tag: "raul-footer-social-icon",
		readme: "# raul-footer-social-icon\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"raul-footer-social-icons"
		],
		dependencies: [
		],
		dependencyGraph: {
			"raul-footer-social-icons": [
				"raul-footer-social-icon"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-footer/raul-footer-social-icons/raul-footer-social-icons.tsx",
		encapsulation: "none",
		tag: "raul-footer-social-icons",
		readme: "# raul-footer-social-icons\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "facebookUrl",
				type: "any",
				mutable: false,
				attr: "facebook-url",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "any"
					}
				],
				optional: true,
				required: false
			},
			{
				name: "linkedinUrl",
				type: "any",
				mutable: false,
				attr: "linkedin-url",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "any"
					}
				],
				optional: true,
				required: false
			},
			{
				name: "twitterUrl",
				type: "any",
				mutable: false,
				attr: "twitter-url",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "any"
					}
				],
				optional: true,
				required: false
			},
			{
				name: "youtubeUrl",
				type: "any",
				mutable: false,
				attr: "youtube-url",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "any"
					}
				],
				optional: true,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"raul-footer-social-icon"
		],
		dependencyGraph: {
			"raul-footer-social-icons": [
				"raul-footer-social-icon"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-grid/raul-grid.tsx",
		encapsulation: "none",
		tag: "raul-grid",
		readme: "# raul-grid\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "hoverable",
				type: "boolean",
				mutable: false,
				attr: "hoverable",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "small",
				type: "boolean",
				mutable: false,
				attr: "small",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "striped",
				type: "boolean",
				mutable: false,
				attr: "striped",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-grid/raul-grid-body/raul-grid-body.tsx",
		encapsulation: "none",
		tag: "raul-grid-body",
		readme: "# raul-grid-body\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-grid/raul-grid-cell/raul-grid-cell.tsx",
		encapsulation: "none",
		tag: "raul-grid-cell",
		readme: "# raul-grid-cell\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-grid/raul-grid-footer/raul-grid-footer.tsx",
		encapsulation: "none",
		tag: "raul-grid-footer",
		readme: "# raul-grid-footer\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-grid/raul-grid-header/raul-grid-header.tsx",
		encapsulation: "none",
		tag: "raul-grid-header",
		readme: "# raul-grid-header\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-grid/raul-grid-row/raul-grid-row.tsx",
		encapsulation: "none",
		tag: "raul-grid-row",
		readme: "# raul-grid-row\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-grid/raul-grid-sorter/raul-grid-sorter.tsx",
		encapsulation: "none",
		tag: "raul-grid-sorter",
		readme: "# raul-grid-sorter\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "direction",
				type: "\"ascending\" | \"descending\"",
				mutable: true,
				attr: "direction",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "null",
				values: [
					{
						value: "ascending",
						type: "string"
					},
					{
						value: "descending",
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "field",
				type: "string",
				mutable: false,
				attr: "field",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
			{
				event: "raulSort",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-typography/raul-heading/raul-heading.tsx",
		encapsulation: "none",
		tag: "raul-heading",
		readme: "# raul-heading\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "variant",
				type: "\"content\" | \"page\" | \"section\"",
				mutable: false,
				attr: "variant",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						value: "content",
						type: "string"
					},
					{
						value: "page",
						type: "string"
					},
					{
						value: "section",
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-loaders",
			"docs-raul-modal",
			"docs-raul-sortable-list",
			"raul-sortable-list"
		],
		dependencies: [
			"raul-text"
		],
		dependencyGraph: {
			"raul-heading": [
				"raul-text"
			],
			"docs-raul-loaders": [
				"raul-heading"
			],
			"docs-raul-modal": [
				"raul-heading"
			],
			"docs-raul-sortable-list": [
				"raul-heading"
			],
			"raul-sortable-list": [
				"raul-heading"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-icon/raul-icon.tsx",
		encapsulation: "none",
		tag: "raul-icon",
		readme: "# raul-icon\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "icon",
				type: "string",
				mutable: false,
				attr: "icon",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "null",
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "kind",
				type: "string",
				mutable: false,
				attr: "kind",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "'icon'",
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-grid-list",
			"docs-menu-section",
			"raul-accordion-item-header",
			"raul-action-menu",
			"raul-action-menu-item",
			"raul-avatar",
			"raul-badge",
			"raul-breadcrumb",
			"raul-breadcrumb-back-icon",
			"raul-bulk-action-bar",
			"raul-button",
			"raul-chip",
			"raul-date-picker",
			"raul-dropdown-menu",
			"raul-filter-bar-button",
			"raul-filter-menu",
			"raul-filter-menu-item",
			"raul-input",
			"raul-modal-header",
			"raul-option",
			"raul-paging-bar",
			"raul-progress",
			"raul-select",
			"raul-simple-select",
			"raul-snackbar",
			"raul-switch",
			"raul-textarea",
			"raul-toast"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-grid-list": [
				"raul-icon"
			],
			"docs-menu-section": [
				"raul-icon"
			],
			"raul-accordion-item-header": [
				"raul-icon"
			],
			"raul-action-menu": [
				"raul-icon"
			],
			"raul-action-menu-item": [
				"raul-icon"
			],
			"raul-avatar": [
				"raul-icon"
			],
			"raul-badge": [
				"raul-icon"
			],
			"raul-breadcrumb": [
				"raul-icon"
			],
			"raul-breadcrumb-back-icon": [
				"raul-icon"
			],
			"raul-bulk-action-bar": [
				"raul-icon"
			],
			"raul-button": [
				"raul-icon"
			],
			"raul-chip": [
				"raul-icon"
			],
			"raul-date-picker": [
				"raul-icon"
			],
			"raul-dropdown-menu": [
				"raul-icon"
			],
			"raul-filter-bar-button": [
				"raul-icon"
			],
			"raul-filter-menu": [
				"raul-icon"
			],
			"raul-filter-menu-item": [
				"raul-icon"
			],
			"raul-input": [
				"raul-icon"
			],
			"raul-modal-header": [
				"raul-icon"
			],
			"raul-option": [
				"raul-icon"
			],
			"raul-paging-bar": [
				"raul-icon"
			],
			"raul-progress": [
				"raul-icon"
			],
			"raul-select": [
				"raul-icon"
			],
			"raul-simple-select": [
				"raul-icon"
			],
			"raul-snackbar": [
				"raul-icon"
			],
			"raul-switch": [
				"raul-icon"
			],
			"raul-textarea": [
				"raul-icon"
			],
			"raul-toast": [
				"raul-icon"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-input/raul-input.tsx",
		encapsulation: "none",
		tag: "raul-input",
		readme: "# raul-input\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "error",
				type: "string",
				mutable: false,
				attr: "error",
				reflectToAttr: false,
				docs: "Makes the input visually invalid and shows the feedback message.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "hint",
				type: "string",
				mutable: false,
				attr: "hint",
				reflectToAttr: false,
				docs: "Input's optional hint text.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "label",
				type: "string",
				mutable: false,
				attr: "label",
				reflectToAttr: false,
				docs: "Input's label text.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "type",
				type: "\"search\" | \"text\"",
				mutable: false,
				attr: "type",
				reflectToAttr: false,
				docs: "Input's type.",
				docsTags: [
				],
				"default": "'text'",
				values: [
					{
						value: "search",
						type: "string"
					},
					{
						value: "text",
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-preview-props",
			"docs-raul-filter-bar",
			"docs-raul-input"
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-input": [
				"raul-icon"
			],
			"docs-preview-props": [
				"raul-input"
			],
			"docs-raul-filter-bar": [
				"raul-input"
			],
			"docs-raul-input": [
				"raul-input"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-list/raul-list.tsx",
		encapsulation: "none",
		tag: "raul-list",
		readme: "# raul-list\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-list/raul-list-action/raul-list-action.tsx",
		encapsulation: "none",
		tag: "raul-list-action",
		readme: "# raul-list-action\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-list/raul-list-header/raul-list-header.tsx",
		encapsulation: "none",
		tag: "raul-list-header",
		readme: "# raul-list-header\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-list/raul-list-item/raul-list-item.tsx",
		encapsulation: "none",
		tag: "raul-list-item",
		readme: "# raul-list-item\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-list/raul-list-item-action/raul-list-item-action.tsx",
		encapsulation: "none",
		tag: "raul-list-item-action",
		readme: "# raul-list-item-action\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-list/raul-list-item-subtitle/raul-list-item-subtitle.tsx",
		encapsulation: "none",
		tag: "raul-list-item-subtitle",
		readme: "# raul-list-item-subtitle\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-list/raul-list-item-title/raul-list-item-title.tsx",
		encapsulation: "none",
		tag: "raul-list-item-title",
		readme: "# raul-list-item-title\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-list/raul-list-title/raul-list-title.tsx",
		encapsulation: "none",
		tag: "raul-list-title",
		readme: "# raul-list-title\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-modal/raul-modal.tsx",
		encapsulation: "none",
		tag: "raul-modal",
		readme: "# raul-modal\n\nRaul-modal is a component used for showing simple content with no major actions needed or displaying digital assets (video, image).\n\nA `raul-modal` has three building blocks: \n\n`<raul-modal-header>` takes two string props: `modalTitle` and `modalDescription`.\n`<raul-modal-body>` is where you insert the bulk of the modal's content (like some longer text or a media asset).\n`<raul-modal-footer>` is where you render modal actions, like buttons.\n\n## Usage\n\nYou render a `<raul-modal></raul-modal>` wherever and whenever you need one.\n\n### A very simple modal\n\n```html\n<raul-modal variant='normal'>\n  <raul-modal-header modalTitle='Lorem ipsum' modalDescription='Dolor sit amet'>\n  </raul-modal-header>\n</raul-modal>\n```\n\n### A modal with longer content\n```html\n<raul-modal variant='normal'>\n  <raul-modal-header modalTitle='Lorem ipsum' modalDescription='Dolor sit amet'></raul-modal-header>\n  <raul-modal-body>\n    <raul-content>\n      <p>\n        A...\n      </p>\n      <p>\n        Lot...\n      </p>\n      <p>\n        Of...\n      </p>\n      <p>\n        Content...\n      </p>\n    </raul-content>\n  </raul-modal-body>\n  <raul-modal-footer>\n    <raul-button>Dismiss</raul-button>\n    <raul-button>Submit</raul-button>\n  </raul-modal-footer>\n</raul-modal>\n```\n\n### A media modal\n\n```html\n<raul-modal variant='media'>\n  <raul-modal-header modalTitle='Lorem ipsum' modalDescription='Dolor sit amet'></raul-modal-header>\n  <raul-modal-body>\n    <img src='path/to/your/asset.jpeg'/>\n  </raul-modal-body>\n</raul-modal>\n```\n",
		docs: "Raul-modal is a component used for showing simple content with no major actions needed or displaying digital assets (video, image).\n\nA `raul-modal` has three building blocks: \n\n`<raul-modal-header>` takes two string props: `modalTitle` and `modalDescription`.\n`<raul-modal-body>` is where you insert the bulk of the modal's content (like some longer text or a media asset).\n`<raul-modal-footer>` is where you render modal actions, like buttons.",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "dismissable",
				type: "boolean",
				mutable: false,
				attr: "dismissable",
				reflectToAttr: false,
				docs: "Determines wether the modal can be closed via clicking away or the `Escape` key",
				docsTags: [
				],
				"default": "true",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "variant",
				type: "\"media\" | \"normal\"",
				mutable: false,
				attr: "variant",
				reflectToAttr: false,
				docs: "A `normal` modal will have a the header and body centered and no close button. A `media` modal will have the header content aligned to the left and a close button.",
				docsTags: [
				],
				"default": "'normal'",
				values: [
					{
						value: "media",
						type: "string"
					},
					{
						value: "normal",
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
			{
				event: "modalClose",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "Event emitted when the modal should be closed (because user clicked the close button, clicked away, pressed `Escape` or chose an option)",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-modal"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-modal": [
				"raul-modal"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-modal/raul-modal-body.tsx",
		encapsulation: "none",
		tag: "raul-modal-body",
		readme: "# raul-modal\n\nRaul-modal is a component used for showing simple content with no major actions needed or displaying digital assets (video, image).\n\nA `raul-modal` has three building blocks: \n\n`<raul-modal-header>` takes two string props: `modalTitle` and `modalDescription`.\n`<raul-modal-body>` is where you insert the bulk of the modal's content (like some longer text or a media asset).\n`<raul-modal-footer>` is where you render modal actions, like buttons.\n\n## Usage\n\nYou render a `<raul-modal></raul-modal>` wherever and whenever you need one.\n\n### A very simple modal\n\n```html\n<raul-modal variant='normal'>\n  <raul-modal-header modalTitle='Lorem ipsum' modalDescription='Dolor sit amet'>\n  </raul-modal-header>\n</raul-modal>\n```\n\n### A modal with longer content\n```html\n<raul-modal variant='normal'>\n  <raul-modal-header modalTitle='Lorem ipsum' modalDescription='Dolor sit amet'></raul-modal-header>\n  <raul-modal-body>\n    <raul-content>\n      <p>\n        A...\n      </p>\n      <p>\n        Lot...\n      </p>\n      <p>\n        Of...\n      </p>\n      <p>\n        Content...\n      </p>\n    </raul-content>\n  </raul-modal-body>\n  <raul-modal-footer>\n    <raul-button>Dismiss</raul-button>\n    <raul-button>Submit</raul-button>\n  </raul-modal-footer>\n</raul-modal>\n```\n\n### A media modal\n\n```html\n<raul-modal variant='media'>\n  <raul-modal-header modalTitle='Lorem ipsum' modalDescription='Dolor sit amet'></raul-modal-header>\n  <raul-modal-body>\n    <img src='path/to/your/asset.jpeg'/>\n  </raul-modal-body>\n</raul-modal>\n```\n",
		docs: "Raul-modal is a component used for showing simple content with no major actions needed or displaying digital assets (video, image).\n\nA `raul-modal` has three building blocks: \n\n`<raul-modal-header>` takes two string props: `modalTitle` and `modalDescription`.\n`<raul-modal-body>` is where you insert the bulk of the modal's content (like some longer text or a media asset).\n`<raul-modal-footer>` is where you render modal actions, like buttons.",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-modal"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-modal": [
				"raul-modal-body"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-modal/raul-modal-footer.tsx",
		encapsulation: "none",
		tag: "raul-modal-footer",
		readme: "# raul-modal\n\nRaul-modal is a component used for showing simple content with no major actions needed or displaying digital assets (video, image).\n\nA `raul-modal` has three building blocks: \n\n`<raul-modal-header>` takes two string props: `modalTitle` and `modalDescription`.\n`<raul-modal-body>` is where you insert the bulk of the modal's content (like some longer text or a media asset).\n`<raul-modal-footer>` is where you render modal actions, like buttons.\n\n## Usage\n\nYou render a `<raul-modal></raul-modal>` wherever and whenever you need one.\n\n### A very simple modal\n\n```html\n<raul-modal variant='normal'>\n  <raul-modal-header modalTitle='Lorem ipsum' modalDescription='Dolor sit amet'>\n  </raul-modal-header>\n</raul-modal>\n```\n\n### A modal with longer content\n```html\n<raul-modal variant='normal'>\n  <raul-modal-header modalTitle='Lorem ipsum' modalDescription='Dolor sit amet'></raul-modal-header>\n  <raul-modal-body>\n    <raul-content>\n      <p>\n        A...\n      </p>\n      <p>\n        Lot...\n      </p>\n      <p>\n        Of...\n      </p>\n      <p>\n        Content...\n      </p>\n    </raul-content>\n  </raul-modal-body>\n  <raul-modal-footer>\n    <raul-button>Dismiss</raul-button>\n    <raul-button>Submit</raul-button>\n  </raul-modal-footer>\n</raul-modal>\n```\n\n### A media modal\n\n```html\n<raul-modal variant='media'>\n  <raul-modal-header modalTitle='Lorem ipsum' modalDescription='Dolor sit amet'></raul-modal-header>\n  <raul-modal-body>\n    <img src='path/to/your/asset.jpeg'/>\n  </raul-modal-body>\n</raul-modal>\n```\n",
		docs: "Raul-modal is a component used for showing simple content with no major actions needed or displaying digital assets (video, image).\n\nA `raul-modal` has three building blocks: \n\n`<raul-modal-header>` takes two string props: `modalTitle` and `modalDescription`.\n`<raul-modal-body>` is where you insert the bulk of the modal's content (like some longer text or a media asset).\n`<raul-modal-footer>` is where you render modal actions, like buttons.",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-modal"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-modal": [
				"raul-modal-footer"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-modal/raul-modal-header.tsx",
		encapsulation: "none",
		tag: "raul-modal-header",
		readme: "# raul-modal\n\nRaul-modal is a component used for showing simple content with no major actions needed or displaying digital assets (video, image).\n\nA `raul-modal` has three building blocks: \n\n`<raul-modal-header>` takes two string props: `modalTitle` and `modalDescription`.\n`<raul-modal-body>` is where you insert the bulk of the modal's content (like some longer text or a media asset).\n`<raul-modal-footer>` is where you render modal actions, like buttons.\n\n## Usage\n\nYou render a `<raul-modal></raul-modal>` wherever and whenever you need one.\n\n### A very simple modal\n\n```html\n<raul-modal variant='normal'>\n  <raul-modal-header modalTitle='Lorem ipsum' modalDescription='Dolor sit amet'>\n  </raul-modal-header>\n</raul-modal>\n```\n\n### A modal with longer content\n```html\n<raul-modal variant='normal'>\n  <raul-modal-header modalTitle='Lorem ipsum' modalDescription='Dolor sit amet'></raul-modal-header>\n  <raul-modal-body>\n    <raul-content>\n      <p>\n        A...\n      </p>\n      <p>\n        Lot...\n      </p>\n      <p>\n        Of...\n      </p>\n      <p>\n        Content...\n      </p>\n    </raul-content>\n  </raul-modal-body>\n  <raul-modal-footer>\n    <raul-button>Dismiss</raul-button>\n    <raul-button>Submit</raul-button>\n  </raul-modal-footer>\n</raul-modal>\n```\n\n### A media modal\n\n```html\n<raul-modal variant='media'>\n  <raul-modal-header modalTitle='Lorem ipsum' modalDescription='Dolor sit amet'></raul-modal-header>\n  <raul-modal-body>\n    <img src='path/to/your/asset.jpeg'/>\n  </raul-modal-body>\n</raul-modal>\n```\n",
		docs: "Raul-modal is a component used for showing simple content with no major actions needed or displaying digital assets (video, image).\n\nA `raul-modal` has three building blocks: \n\n`<raul-modal-header>` takes two string props: `modalTitle` and `modalDescription`.\n`<raul-modal-body>` is where you insert the bulk of the modal's content (like some longer text or a media asset).\n`<raul-modal-footer>` is where you render modal actions, like buttons.",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "description",
				type: "string",
				mutable: false,
				attr: "description",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "modalTitle",
				type: "string",
				mutable: false,
				attr: "modal-title",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
			{
				event: "modalClose",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-modal"
		],
		dependencies: [
			"raul-content",
			"raul-icon"
		],
		dependencyGraph: {
			"raul-modal-header": [
				"raul-content",
				"raul-icon"
			],
			"docs-raul-modal": [
				"raul-modal-header"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-select/raul-option/raul-option.tsx",
		encapsulation: "none",
		tag: "raul-option",
		readme: "# raul-option\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "description",
				type: "string",
				mutable: false,
				attr: "description",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "disabled",
				type: "boolean",
				mutable: false,
				attr: "disabled",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "focused",
				type: "boolean",
				mutable: false,
				attr: "focused",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "icon",
				type: "string",
				mutable: false,
				attr: "icon",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "iconKind",
				type: "string",
				mutable: false,
				attr: "icon-kind",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "indeterminate",
				type: "boolean",
				mutable: false,
				attr: "indeterminate",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "multiple",
				type: "boolean",
				mutable: false,
				attr: "multiple",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "optionId",
				type: "string",
				mutable: false,
				attr: "option-id",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "selected",
				type: "boolean",
				mutable: false,
				attr: "selected",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "text",
				type: "string",
				mutable: false,
				attr: "text",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "value",
				type: "string",
				mutable: false,
				attr: "value",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "variant",
				type: "string",
				mutable: false,
				attr: "variant",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"raul-select"
		],
		dependencies: [
			"raul-icon",
			"raul-checkbox"
		],
		dependencyGraph: {
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"raul-select": [
				"raul-option"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-loaders/raul-page-loader.tsx",
		encapsulation: "none",
		tag: "raul-page-loader",
		readme: "# raul-page-loader\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-loaders"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-loaders": [
				"raul-page-loader"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-paging-bar/raul-paging-bar.tsx",
		encapsulation: "none",
		tag: "raul-paging-bar",
		readme: "# raul-paging-bar\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "entities",
				type: "any",
				mutable: false,
				attr: "entities",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "[10,20,30,40,50]",
				values: [
					{
						type: "any"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "rowsPerPage",
				type: "number",
				mutable: false,
				attr: "rows-per-page",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "10",
				values: [
					{
						type: "number"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "totalRows",
				type: "number",
				mutable: false,
				attr: "total-rows",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "0",
				values: [
					{
						type: "number"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
			{
				event: "pagingChange",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-grid-list",
			"docs-raul-paging-bar"
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-paging-bar": [
				"raul-icon"
			],
			"docs-grid-list": [
				"raul-paging-bar"
			],
			"docs-raul-paging-bar": [
				"raul-paging-bar"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-progress/raul-progress.tsx",
		encapsulation: "none",
		tag: "raul-progress",
		readme: "# raul-progress\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "color",
				type: "\"danger\" | \"primary\" | \"success\" | \"warning\"",
				mutable: false,
				attr: "color",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "'primary'",
				values: [
					{
						value: "danger",
						type: "string"
					},
					{
						value: "primary",
						type: "string"
					},
					{
						value: "success",
						type: "string"
					},
					{
						value: "warning",
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "hint",
				type: "string",
				mutable: false,
				attr: "hint",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "null",
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "label",
				type: "string",
				mutable: false,
				attr: "label",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "null",
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "static",
				type: "boolean",
				mutable: false,
				attr: "static",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "value",
				type: "string",
				mutable: false,
				attr: "value",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "'0'",
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
			{
				event: "raulProgressRemove",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-progress": [
				"raul-icon"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-radio/raul-radio.tsx",
		encapsulation: "none",
		tag: "raul-radio",
		readme: "# raul-radio\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "invalid",
				type: "boolean",
				mutable: false,
				attr: "invalid",
				reflectToAttr: false,
				docs: "If `true`, the radio border will become red. This can be useful for form validations.",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "labelText",
				type: "string",
				mutable: false,
				attr: "label-text",
				reflectToAttr: false,
				docs: "The text label.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "small",
				type: "boolean",
				mutable: false,
				attr: "small",
				reflectToAttr: true,
				docs: "If `true`, the radio size will be small.",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-date-picker/raul-range-picker.tsx",
		encapsulation: "none",
		tag: "raul-range-picker",
		readme: "# raul-date-picker\n\n## Usage\n\n```html\n<raul-date-picker>\n</raul-date-picker>\n```\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"raul-content",
			"raul-date-picker"
		],
		dependencyGraph: {
			"raul-range-picker": [
				"raul-content",
				"raul-date-picker"
			],
			"raul-date-picker": [
				"raul-icon"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-select/raul-select.tsx",
		encapsulation: "none",
		tag: "raul-select",
		readme: "# raul-select\n\nRaul Select is a custom form control which allows selecting an option, or options, similar to a native `<select>` element.\n\nA select should be used with child `<raul-option>` elements. If the child option is not given a `value` attribute then its text will be used as the value. Select options can be grouped within `<raul-options-group` elements.\n\nIf value is set on the `<raul-select>`, the selected option will be chosen based on that value. Otherwise, the selected attribute can be used on the `<raul-option>`.\n\n## Single Selection\n\nBy default, the select allows the user to select only one option. The select component's value receives the value of the selected option's value. If `<raul-options-group>` elements are used they will act just like a text label for the `<raul-option>` elements within.\n\n## Multiple Selection\n\nBy adding the `multiple` attribute to select, users are able to select multiple options. The select component's value receives an array of all of the selected option values. If `<raul-options-group>` elements are used they will also respond to events, e.g. on click/enter key they will check/uncheck all the `<raul-option>` elements within.\n\n## Usage\n\n### Single Selection\n\n```html\n<raul-select placeholder=\"Select one option\">\n  <raul-option value=\"dogs\">Dogs</raul-option>\n  <raul-option value=\"cats\" selected>Cats</raul-option>\n</raul-select>\n```\n\n### Multiple Selection\n\n```html\n<raul-select multiple>\n  <raul-option value=\"dogs\" selected>Dogs</raul-option>\n  <raul-option value=\"cats\" selected>Cats</raul-option>\n</raul-select>\n```\n",
		docs: "Raul Select is a custom form control which allows selecting an option, or options, similar to a native `<select>` element.\n\nA select should be used with child `<raul-option>` elements. If the child option is not given a `value` attribute then its text will be used as the value. Select options can be grouped within `<raul-options-group` elements.\n\nIf value is set on the `<raul-select>`, the selected option will be chosen based on that value. Otherwise, the selected attribute can be used on the `<raul-option>`.",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "borderless",
				type: "boolean",
				mutable: false,
				attr: "borderless",
				reflectToAttr: true,
				docs: "If `true`, removes select toggle border.",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "disabled",
				type: "boolean",
				mutable: false,
				attr: "disabled",
				reflectToAttr: true,
				docs: "If `true`, the user cannot interact with the select.",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "error",
				type: "string",
				mutable: false,
				attr: "error",
				reflectToAttr: false,
				docs: "Makes the select visually invalid and shows the feedback message.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "hint",
				type: "string",
				mutable: false,
				attr: "hint",
				reflectToAttr: false,
				docs: "Optional hint text.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "label",
				type: "string",
				mutable: false,
				attr: "label",
				reflectToAttr: true,
				docs: "The text label.",
				docsTags: [
				],
				"default": "null",
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "menuPlacement",
				type: "\"auto\" | \"auto-end\" | \"auto-start\" | \"bottom\" | \"bottom-end\" | \"bottom-start\" | \"left\" | \"left-end\" | \"left-start\" | \"right\" | \"right-end\" | \"right-start\" | \"top\" | \"top-end\" | \"top-start\"",
				mutable: false,
				attr: "menu-placement",
				reflectToAttr: false,
				docs: "Options menu's placement.",
				docsTags: [
				],
				"default": "'bottom-start'",
				values: [
					{
						value: "auto",
						type: "string"
					},
					{
						value: "auto-end",
						type: "string"
					},
					{
						value: "auto-start",
						type: "string"
					},
					{
						value: "bottom",
						type: "string"
					},
					{
						value: "bottom-end",
						type: "string"
					},
					{
						value: "bottom-start",
						type: "string"
					},
					{
						value: "left",
						type: "string"
					},
					{
						value: "left-end",
						type: "string"
					},
					{
						value: "left-start",
						type: "string"
					},
					{
						value: "right",
						type: "string"
					},
					{
						value: "right-end",
						type: "string"
					},
					{
						value: "right-start",
						type: "string"
					},
					{
						value: "top",
						type: "string"
					},
					{
						value: "top-end",
						type: "string"
					},
					{
						value: "top-start",
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "menuPositionFixed",
				type: "boolean",
				mutable: false,
				attr: "menu-position-fixed",
				reflectToAttr: false,
				docs: "If `true` the menu position will be 'fixed'.",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "multiple",
				type: "boolean",
				mutable: false,
				attr: "multiple",
				reflectToAttr: true,
				docs: "If `true`, allows multiple selections.",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "name",
				type: "string",
				mutable: false,
				attr: "name",
				reflectToAttr: true,
				docs: "The name of the control, which is submitted with the form data.",
				docsTags: [
				],
				"default": "`select-${randomUID()}`",
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "options",
				type: "Object[]",
				mutable: false,
				reflectToAttr: false,
				docs: "Options and groups of the select.",
				docsTags: [
				],
				"default": "[]",
				values: [
					{
						type: "Object[]"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "optionsMaxHeight",
				type: "string",
				mutable: false,
				attr: "options-max-height",
				reflectToAttr: false,
				docs: "Sets the max height of the select options list, e.g. `500px`, `calc(100vh - 50px)`.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "placeholder",
				type: "string",
				mutable: false,
				attr: "placeholder",
				reflectToAttr: true,
				docs: "The text to display when the select value is empty.",
				docsTags: [
				],
				"default": "'Select an option'",
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "searchable",
				type: "boolean",
				mutable: false,
				attr: "searchable",
				reflectToAttr: true,
				docs: "If `true`, adds a search field at the top of the select menu.",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "value",
				type: "string | string[]",
				mutable: true,
				attr: "value",
				reflectToAttr: false,
				docs: "The value of the select.",
				docsTags: [
				],
				"default": "this.multiple ? [] : ''",
				values: [
					{
						type: "string"
					},
					{
						type: "string[]"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
			{
				name: "close",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "close() => Promise<void>",
				parameters: [
				],
				docs: "Close the select menu.",
				docsTags: [
				]
			},
			{
				name: "open",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "open() => Promise<void>",
				parameters: [
				],
				docs: "Open the select menu.",
				docsTags: [
				]
			},
			{
				name: "toggle",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "toggle() => Promise<void>",
				parameters: [
				],
				docs: "Toggle the select menu.",
				docsTags: [
				]
			}
		],
		events: [
			{
				event: "raulChange",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "Emitted when the select value changes.",
				docsTags: [
				]
			},
			{
				event: "raulSelectClose",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "Emitted when the select menu closes.",
				docsTags: [
				]
			},
			{
				event: "raulSelectOpen",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "Emitted when the select menu opens.",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-preview-props",
			"docs-raul-filter-bar",
			"docs-raul-select",
			"raul-tabs",
			"raul-toggles"
		],
		dependencies: [
			"raul-icon",
			"raul-option"
		],
		dependencyGraph: {
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-preview-props": [
				"raul-select"
			],
			"docs-raul-filter-bar": [
				"raul-select"
			],
			"docs-raul-select": [
				"raul-select"
			],
			"raul-tabs": [
				"raul-select"
			],
			"raul-toggles": [
				"raul-select"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-simple-select/raul-simple-select.tsx",
		encapsulation: "none",
		tag: "raul-simple-select",
		readme: "# Raul Simple Select\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "error",
				type: "string",
				mutable: false,
				attr: "error",
				reflectToAttr: false,
				docs: "Makes the input visually invalid and shows the feedback message.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "hint",
				type: "string",
				mutable: false,
				attr: "hint",
				reflectToAttr: false,
				docs: "Input's optional hint text.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "label",
				type: "string",
				mutable: false,
				attr: "label",
				reflectToAttr: false,
				docs: "Input's label text.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-simple-select"
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-simple-select": [
				"raul-icon"
			],
			"docs-raul-simple-select": [
				"raul-simple-select"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-simple-table/raul-simple-table.tsx",
		encapsulation: "none",
		tag: "raul-simple-table",
		readme: "# raul-simple-table\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "hoverable",
				type: "boolean",
				mutable: false,
				attr: "hoverable",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "small",
				type: "boolean",
				mutable: false,
				attr: "small",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "striped",
				type: "boolean",
				mutable: false,
				attr: "striped",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-index",
			"docs-interface"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-index": [
				"raul-simple-table"
			],
			"docs-interface": [
				"raul-simple-table"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-simple-table/raul-simple-table-sorter/raul-simple-table-sorter.tsx",
		encapsulation: "none",
		tag: "raul-simple-table-sorter",
		readme: "# raul-simple-table-sorter\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "direction",
				type: "\"ascending\" | \"descending\"",
				mutable: true,
				attr: "direction",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "null",
				values: [
					{
						value: "ascending",
						type: "string"
					},
					{
						value: "descending",
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "field",
				type: "string",
				mutable: false,
				attr: "field",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
			{
				event: "raulSort",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-toaster/raul-snackbar/raul-snackbar.tsx",
		encapsulation: "none",
		tag: "raul-snackbar",
		readme: "# raul-snackbar\n\n\nYou render a snackbar by calling `window.RAUL.commands.toast(options)`\n\nThe `options` object has the following interface: \n\n```js\n{\n  tagName: 'raul-snackbar', //mandatory\n  variant: string, // 'information' | 'success' | 'warning' | 'danger'\n  dismissable: boolean,\n  heading: string,\n  content: string,\n  ctaMessage: string,\n  ctaUrl: string,\n  ctaCallback: Function,\n  timeout: number // miliseconds\n}\n```\n\n",
		docs: "You render a snackbar by calling `window.RAUL.commands.toast(options)`\n\nThe `options` object has the following interface: \n\n```js\n{\n  tagName: 'raul-snackbar', //mandatory\n  variant: string, // 'information' | 'success' | 'warning' | 'danger'\n  dismissable: boolean,\n  heading: string,\n  content: string,\n  ctaMessage: string,\n  ctaUrl: string,\n  ctaCallback: Function,\n  timeout: number // miliseconds\n}\n```",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "content",
				type: "string",
				mutable: false,
				attr: "content",
				reflectToAttr: false,
				docs: "The second line of text",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: true,
				required: false
			},
			{
				name: "ctaCallback",
				type: "Function",
				mutable: false,
				reflectToAttr: false,
				docs: "A callback function hat will be called on CTA click if an url was not provided",
				docsTags: [
				],
				values: [
					{
						type: "Function"
					}
				],
				optional: true,
				required: false
			},
			{
				name: "ctaMessage",
				type: "string",
				mutable: false,
				attr: "cta-message",
				reflectToAttr: false,
				docs: "The call-to-action text",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: true,
				required: false
			},
			{
				name: "ctaUrl",
				type: "string",
				mutable: false,
				attr: "cta-url",
				reflectToAttr: false,
				docs: "A call-to-action url",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: true,
				required: false
			},
			{
				name: "dismissable",
				type: "boolean",
				mutable: false,
				attr: "dismissable",
				reflectToAttr: false,
				docs: "Determines wether the snackbar has a close button. Defaults to `false`",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "heading",
				type: "string",
				mutable: false,
				attr: "heading",
				reflectToAttr: false,
				docs: "The first line of text",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "timeout",
				type: "number",
				mutable: false,
				attr: "timeout",
				reflectToAttr: false,
				docs: "The number of ms after which the snackbar self-destructs",
				docsTags: [
				],
				values: [
					{
						type: "number"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "variant",
				type: "\"danger\" | \"information\" | \"success\" | \"warning\"",
				mutable: false,
				attr: "variant",
				reflectToAttr: false,
				docs: "Determines the color of the left bar",
				docsTags: [
				],
				"default": "'information'",
				values: [
					{
						value: "danger",
						type: "string"
					},
					{
						value: "information",
						type: "string"
					},
					{
						value: "success",
						type: "string"
					},
					{
						value: "warning",
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-snackbar"
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-snackbar": [
				"raul-icon"
			],
			"docs-raul-snackbar": [
				"raul-snackbar"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-toaster/raul-snackbar/raul-snackbar-group.tsx",
		encapsulation: "none",
		tag: "raul-snackbar-group",
		readme: "# raul-snackbar\n\n\nYou render a snackbar by calling `window.RAUL.commands.toast(options)`\n\nThe `options` object has the following interface: \n\n```js\n{\n  tagName: 'raul-snackbar', //mandatory\n  variant: string, // 'information' | 'success' | 'warning' | 'danger'\n  dismissable: boolean,\n  heading: string,\n  content: string,\n  ctaMessage: string,\n  ctaUrl: string,\n  ctaCallback: Function,\n  timeout: number // miliseconds\n}\n```\n\n",
		docs: "You render a snackbar by calling `window.RAUL.commands.toast(options)`\n\nThe `options` object has the following interface: \n\n```js\n{\n  tagName: 'raul-snackbar', //mandatory\n  variant: string, // 'information' | 'success' | 'warning' | 'danger'\n  dismissable: boolean,\n  heading: string,\n  content: string,\n  ctaMessage: string,\n  ctaUrl: string,\n  ctaCallback: Function,\n  timeout: number // miliseconds\n}\n```",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-snackbar"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-snackbar": [
				"raul-snackbar-group"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-sortables/raul-sortable-list.tsx",
		encapsulation: "none",
		tag: "raul-sortable-list",
		readme: "# raul-sortable-list\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "group",
				type: "string",
				mutable: false,
				attr: "group",
				reflectToAttr: false,
				docs: "If two or more lists have the same `group` property, the user will be able to drag & drop items between them",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "listGroupLabel",
				type: "string",
				mutable: false,
				attr: "list-group-label",
				reflectToAttr: false,
				docs: "Label of a list group",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
			{
				event: "itemDrag",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "The event will be emitted at the end of a drag&drop",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-sortable-list"
		],
		dependencies: [
			"raul-heading"
		],
		dependencyGraph: {
			"raul-sortable-list": [
				"raul-heading"
			],
			"raul-heading": [
				"raul-text"
			],
			"docs-raul-sortable-list": [
				"raul-sortable-list"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-status/raul-status.tsx",
		encapsulation: "none",
		tag: "raul-status",
		readme: "# raul-status\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "variant",
				type: "\"default\" | \"destructive\" | \"success\" | \"warning\"",
				mutable: false,
				attr: "variant",
				reflectToAttr: false,
				docs: "Status variant.",
				docsTags: [
				],
				"default": "'default'",
				values: [
					{
						value: "default",
						type: "string"
					},
					{
						value: "destructive",
						type: "string"
					},
					{
						value: "success",
						type: "string"
					},
					{
						value: "warning",
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-accordion",
			"docs-raul-status",
			"docs-raul-tabs"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-accordion": [
				"raul-status"
			],
			"docs-raul-status": [
				"raul-status"
			],
			"docs-raul-tabs": [
				"raul-status"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-status-indicator/raul-status-indicator.tsx",
		encapsulation: "none",
		tag: "raul-status-indicator",
		readme: "# raul-status-indicator\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "variant",
				type: "\"default\" | \"destructive\" | \"success\" | \"warning\"",
				mutable: false,
				attr: "variant",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "'default'",
				values: [
					{
						value: "default",
						type: "string"
					},
					{
						value: "destructive",
						type: "string"
					},
					{
						value: "success",
						type: "string"
					},
					{
						value: "warning",
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-index",
			"docs-raul-status-indicator"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-index": [
				"raul-status-indicator"
			],
			"docs-raul-status-indicator": [
				"raul-status-indicator"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-switch/raul-switch.tsx",
		encapsulation: "none",
		tag: "raul-switch",
		readme: "# raul-switcher\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "labelText",
				type: "string",
				mutable: false,
				attr: "label-text",
				reflectToAttr: false,
				docs: "The text label.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "small",
				type: "boolean",
				mutable: false,
				attr: "small",
				reflectToAttr: true,
				docs: "If `true`, the switch size will be small.",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-preview-props",
			"docs-raul-switch"
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-switch": [
				"raul-icon"
			],
			"docs-preview-props": [
				"raul-switch"
			],
			"docs-raul-switch": [
				"raul-switch"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-tabs/raul-tab-pane/raul-tab-pane.tsx",
		encapsulation: "none",
		tag: "raul-tab-pane",
		readme: "# raul-tab-pane\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "name",
				type: "string",
				mutable: false,
				attr: "name",
				reflectToAttr: true,
				docs: "The name of the tab pane.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-tabs"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-tabs": [
				"raul-tab-pane"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-tabs/raul-tabs.tsx",
		encapsulation: "none",
		tag: "raul-tabs",
		readme: "# Tabs\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "activeTab",
				type: "string",
				mutable: false,
				attr: "active-tab",
				reflectToAttr: true,
				docs: "The name key of the active tab.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "selectOnMobile",
				type: "boolean",
				mutable: false,
				attr: "select-on-mobile",
				reflectToAttr: true,
				docs: "if `true`, tabs will render as a select on mobile devices.",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "tabs",
				type: "TabInterface[]",
				mutable: false,
				reflectToAttr: false,
				docs: "An array of objects representing the navigation.",
				docsTags: [
				],
				"default": "[]",
				values: [
					{
						type: "TabInterface[]"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
			{
				event: "raulTabChange",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "Emitted when a tab is clicked.",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-element",
			"docs-index",
			"docs-raul-tabs",
			"docs-raul-text"
		],
		dependencies: [
			"raul-select"
		],
		dependencyGraph: {
			"raul-tabs": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-element": [
				"raul-tabs"
			],
			"docs-index": [
				"raul-tabs"
			],
			"docs-raul-tabs": [
				"raul-tabs"
			],
			"docs-raul-text": [
				"raul-tabs"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-typography/raul-text/raul-text.tsx",
		encapsulation: "none",
		tag: "raul-text",
		readme: "# raul-text\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "align",
				type: "\"center\" | \"justify\" | \"left\" | \"right\"",
				mutable: false,
				attr: "align",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "'left'",
				values: [
					{
						value: "center",
						type: "string"
					},
					{
						value: "justify",
						type: "string"
					},
					{
						value: "left",
						type: "string"
					},
					{
						value: "right",
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "capitalize",
				type: "boolean",
				mutable: false,
				attr: "capitalize",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "color",
				type: "\"danger\" | \"primary\" | \"success\" | \"white\"",
				mutable: false,
				attr: "color",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						value: "danger",
						type: "string"
					},
					{
						value: "primary",
						type: "string"
					},
					{
						value: "success",
						type: "string"
					},
					{
						value: "white",
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "ellipsis",
				type: "boolean",
				mutable: false,
				attr: "ellipsis",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "emphasis",
				type: "boolean",
				mutable: false,
				attr: "emphasis",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "inline",
				type: "boolean",
				mutable: false,
				attr: "inline",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "lineThrough",
				type: "boolean",
				mutable: false,
				attr: "line-through",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "paragraph",
				type: "boolean",
				mutable: false,
				attr: "paragraph",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "size",
				type: "\"extra-large\" | \"hero\" | \"large\" | \"medium\" | \"small\"",
				mutable: false,
				attr: "size",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						value: "extra-large",
						type: "string"
					},
					{
						value: "hero",
						type: "string"
					},
					{
						value: "large",
						type: "string"
					},
					{
						value: "medium",
						type: "string"
					},
					{
						value: "small",
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "strong",
				type: "boolean",
				mutable: false,
				attr: "strong",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "underline",
				type: "boolean",
				mutable: false,
				attr: "underline",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "uppercase",
				type: "boolean",
				mutable: false,
				attr: "uppercase",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"raul-heading"
		],
		dependencies: [
		],
		dependencyGraph: {
			"raul-heading": [
				"raul-text"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-textarea/raul-textarea.tsx",
		encapsulation: "none",
		tag: "raul-textarea",
		readme: "# raul-textarea\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "error",
				type: "string",
				mutable: false,
				attr: "error",
				reflectToAttr: false,
				docs: "Makes the textarea visually invalid and shows the error message.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "hint",
				type: "string",
				mutable: false,
				attr: "hint",
				reflectToAttr: false,
				docs: "Textarea's optional hint text.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "label",
				type: "string",
				mutable: false,
				attr: "label",
				reflectToAttr: false,
				docs: "Textarea's label text.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-textarea"
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-textarea": [
				"raul-icon"
			],
			"docs-raul-textarea": [
				"raul-textarea"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-toaster/raul-toast/raul-toast.tsx",
		encapsulation: "none",
		tag: "raul-toast",
		readme: "# raul-toast\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "actions",
				type: "any[]",
				mutable: false,
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "null",
				values: [
					{
						type: "any[]"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "avatar",
				type: "string",
				mutable: false,
				attr: "avatar",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "body",
				type: "string",
				mutable: false,
				attr: "body",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "heading",
				type: "string",
				mutable: false,
				attr: "heading",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "meta",
				type: "string",
				mutable: false,
				attr: "meta",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "origin",
				type: "string",
				mutable: false,
				attr: "origin",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "read",
				type: "boolean",
				mutable: false,
				attr: "read",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "severity",
				type: "string",
				mutable: false,
				attr: "severity",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "null",
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "timeout",
				type: "number",
				mutable: false,
				attr: "timeout",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				values: [
					{
						type: "number"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
			{
				name: "dismiss",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "dismiss() => Promise<void>",
				parameters: [
				],
				docs: "",
				docsTags: [
				]
			}
		],
		events: [
			{
				event: "timedOut",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			},
			{
				event: "toastAction",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
			"raul-icon"
		],
		dependencyGraph: {
			"raul-toast": [
				"raul-icon"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-toaster/raul-toaster.tsx",
		encapsulation: "none",
		tag: "raul-toaster",
		readme: "# raul-toast\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-snackbar"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-snackbar": [
				"raul-toaster"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-toggles/raul-toggle-pane/raul-toggle-pane.tsx",
		encapsulation: "none",
		tag: "raul-toggle-pane",
		readme: "# raul-toggle-pane\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "name",
				type: "string",
				mutable: false,
				attr: "name",
				reflectToAttr: true,
				docs: "The name of the toggle pane.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-toggles"
		],
		dependencies: [
		],
		dependencyGraph: {
			"docs-raul-toggles": [
				"raul-toggle-pane"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-toggles/raul-toggles.tsx",
		encapsulation: "none",
		tag: "raul-toggles",
		readme: "# Tabs\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "activeToggle",
				type: "string",
				mutable: false,
				attr: "active-toggle",
				reflectToAttr: true,
				docs: "The name key of the active toggle.",
				docsTags: [
				],
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "fullWidth",
				type: "boolean",
				mutable: false,
				attr: "full-width",
				reflectToAttr: true,
				docs: "If `true`, the width of the parent will be distributed equally to the toggles.",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "selectOnMobile",
				type: "boolean",
				mutable: false,
				attr: "select-on-mobile",
				reflectToAttr: true,
				docs: "If `true`, toggles will render as a select on mobile devices.",
				docsTags: [
				],
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "toggles",
				type: "ToggleInterface[]",
				mutable: false,
				reflectToAttr: false,
				docs: "An array of objects representing the navigation.",
				docsTags: [
				],
				"default": "[]",
				values: [
					{
						type: "ToggleInterface[]"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
		],
		events: [
			{
				event: "raulToggleChange",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "Emitted when a toggle is clicked.",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
			"docs-raul-toggles"
		],
		dependencies: [
			"raul-select"
		],
		dependencyGraph: {
			"raul-toggles": [
				"raul-select"
			],
			"raul-select": [
				"raul-icon",
				"raul-option"
			],
			"raul-option": [
				"raul-icon",
				"raul-checkbox"
			],
			"docs-raul-toggles": [
				"raul-toggles"
			]
		}
	},
	{
		filePath: "src/components/elements/raul-tooltip/raul-tooltip.tsx",
		encapsulation: "none",
		tag: "raul-tooltip",
		readme: "# raul-avatar\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "disabledFocusListener",
				type: "boolean",
				mutable: false,
				attr: "disabled-focus-listener",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "disabledHoverListener",
				type: "boolean",
				mutable: false,
				attr: "disabled-hover-listener",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "placement",
				type: "\"auto\" | \"auto-end\" | \"auto-start\" | \"bottom\" | \"bottom-end\" | \"bottom-start\" | \"left\" | \"left-end\" | \"left-start\" | \"right\" | \"right-end\" | \"right-start\" | \"top\" | \"top-end\" | \"top-start\"",
				mutable: false,
				attr: "placement",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "'top'",
				values: [
					{
						value: "auto",
						type: "string"
					},
					{
						value: "auto-end",
						type: "string"
					},
					{
						value: "auto-start",
						type: "string"
					},
					{
						value: "bottom",
						type: "string"
					},
					{
						value: "bottom-end",
						type: "string"
					},
					{
						value: "bottom-start",
						type: "string"
					},
					{
						value: "left",
						type: "string"
					},
					{
						value: "left-end",
						type: "string"
					},
					{
						value: "left-start",
						type: "string"
					},
					{
						value: "right",
						type: "string"
					},
					{
						value: "right-end",
						type: "string"
					},
					{
						value: "right-start",
						type: "string"
					},
					{
						value: "top",
						type: "string"
					},
					{
						value: "top-end",
						type: "string"
					},
					{
						value: "top-start",
						type: "string"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "text",
				type: "string",
				mutable: false,
				attr: "text",
				reflectToAttr: true,
				docs: "",
				docsTags: [
				],
				"default": "''",
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
			{
				name: "hide",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "hide() => Promise<void>",
				parameters: [
				],
				docs: "",
				docsTags: [
				]
			},
			{
				name: "show",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "show() => Promise<void>",
				parameters: [
				],
				docs: "",
				docsTags: [
				]
			}
		],
		events: [
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	},
	{
		filePath: "src/components/elements/raul-video/raul-video.tsx",
		encapsulation: "none",
		tag: "raul-video",
		readme: "# raul-video\n\n\n",
		docs: "",
		docsTags: [
		],
		usage: {
		},
		props: [
			{
				name: "autoplay",
				type: "boolean",
				mutable: false,
				attr: "autoplay",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "false",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "controls",
				type: "boolean",
				mutable: false,
				attr: "controls",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "true",
				values: [
					{
						type: "boolean"
					}
				],
				optional: false,
				required: false
			},
			{
				name: "src",
				type: "string",
				mutable: false,
				attr: "src",
				reflectToAttr: false,
				docs: "",
				docsTags: [
				],
				"default": "null",
				values: [
					{
						type: "string"
					}
				],
				optional: false,
				required: false
			}
		],
		methods: [
			{
				name: "pauseVideo",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "pauseVideo() => Promise<void>",
				parameters: [
				],
				docs: "",
				docsTags: [
				]
			},
			{
				name: "playVideo",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "playVideo() => Promise<void>",
				parameters: [
				],
				docs: "",
				docsTags: [
				]
			},
			{
				name: "progress",
				returns: {
					type: "Promise<number>",
					docs: ""
				},
				signature: "progress() => Promise<number>",
				parameters: [
				],
				docs: "",
				docsTags: [
				]
			},
			{
				name: "stopVideo",
				returns: {
					type: "Promise<void>",
					docs: ""
				},
				signature: "stopVideo() => Promise<void>",
				parameters: [
				],
				docs: "",
				docsTags: [
				]
			}
		],
		events: [
			{
				event: "videoEnded",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			},
			{
				event: "videoPause",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			},
			{
				event: "videoPlay",
				detail: "any",
				bubbles: true,
				cancelable: true,
				composed: true,
				docs: "",
				docsTags: [
				]
			}
		],
		styles: [
		],
		slots: [
		],
		dependents: [
		],
		dependencies: [
		],
		dependencyGraph: {
		}
	}
];
const jsonDocs = {
	timestamp: timestamp,
	compiler: compiler,
	components: components
};

function getDocs(componentTag) {
    const componentDoc = jsonDocs.components.find(doc => doc.tag === componentTag);
    return Object.assign({}, componentDoc);
}

exports.getDocs = getDocs;
