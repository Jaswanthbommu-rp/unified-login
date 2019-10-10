// Common

{{{ include("templates/templates.inc.js") }}}


// Assign Products

{{{ include("controllers/add-products.js") }}}

{{{ include("filters/products-translator.js") }}}

{{{ include("models/add-product-config.js") }}}
{{{ include("models/add-product-data.js") }}}
{{{ include("models/add-product.js") }}}

{{{ include("services/persona-products.js") }}}


// Product Access

{{{ include("components/product-item-access.js") }}}

{{{ include("controllers/product-access.js") }}}

{{{ include("filters/products-access-translator.js") }}}

{{{ include("models/product-scroll-tab-config.js") }}}
