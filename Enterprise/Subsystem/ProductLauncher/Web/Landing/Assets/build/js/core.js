'use strict';

// lazyload config
var MODULE_CONFIG = {
    easyPieChart: ['Assets/build/foundation-ui-library/jquery/easy-pie-chart/js/jquery-easypiechart.js'],
    sparkline: ['Assets/build/foundation-ui-library/jquery/sparkline/js/jquery.sparkline.retina.js'],
    plot: ['Assets/build/foundation-ui-library/jquery/flot/js/jquery.flot.js', 'Assets/build/foundation-ui-library/jquery/flot/js/jquery.flot.resize.js', 'Assets/build/foundation-ui-library/jquery/flot/js/jquery.flot.pie.js', 'Assets/build/foundation-ui-library/jquery/flot-tooltip/js/jquery.flot.tooltip.js', 'Assets/build/foundation-ui-library/jquery/flot-spline/js/jquery.flot.spline.js', 'Assets/build/foundation-ui-library/jquery/flot-orderbars/js/jquery.flot.orderBars.js'],
    vectorMap: ['Assets/build/foundation-ui-library/jquery/jvectormap/js/jquery-jvectormap.js', 'Assets/build/foundation-ui-library/jquery/jvectormap/css/jquery-jvectormap.css', 'Assets/build/foundation-ui-library/jquery/jvectormap/js/jquery-jvectormap-world-mill-en.js', 'Assets/build/foundation-ui-library/jquery/jvectormap/js/jquery-jvectormap-us-aea-en.js'],
    contextMenu: ['Assets/build/foundation-ui-library/jquery/jquery-context-menu/jquery.contextMenu.js', 'Assets/build/foundation-ui-library/jquery/jquery-context-menu/jquery.contextMenu.css', 'Assets/build/foundation-ui-library/jquery/jquery-context-menu/jquery.ui.position.js'],
    dataTable: ['/Assets/build/foundation-ui-library/jquery/data-table/js/jquery.dataTables.js', '/Assets/build/foundation-ui-library/jquery/data-table/js/dataTables.bootstrap.js', '/Assets/build/foundation-ui-library/jquery/data-table/js/dataTables.responsive.js', '/Assets/build/foundation-ui-library/jquery/data-table/js/dataTables.fixedColumns.js', 'https://cdn.datatables.net/v/dt/dt-1.10.16/rr-1.2.3/datatables.min.js', '/Assets/build/js/plugins/ui-dtpagination-input.js', '/Assets/build/js/ui-datatable-config.js', 'https://cdn.datatables.net/v/dt/dt-1.10.16/rr-1.2.3/datatables.min.css', '/Assets/build/foundation-ui-library/jquery/data-table/css/dataTables-bootstrap.css', '/Assets/build/foundation-ui-library/jquery/data-table/css/fixedColumns-bootstrap.css', '/Assets/build/foundation-ui-library/jquery/data-table/css/responsive-bootstrap.css'],
    footable: ['Assets/build/foundation-ui-library/jquery/footable/js/footable.all.js', 'Assets/build/foundation-ui-library/jquery/footable/css/footable.css', 'Assets/build/foundation-ui-library/jquery/footable/css/font-face.css'],
    screenfull: ['Assets/build/foundation-ui-library/jquery/screenfull/_base/js/screenfull.js'],
    sortable: ['Assets/build/foundation-ui-library/jquery/html-sortable/js/html-sortable.js'],
    nestable: ['Assets/build/foundation-ui-library/jquery/nestable/css/jquery-nestable.css', 'Assets/build/foundation-ui-library/jquery/nestable/js/jquery.nestable.js'],
    summernote: ['Assets/build/foundation-ui-library/jquery/summernote/css/summernote.css', 'Assets/build/foundation-ui-library/jquery/summernote/js/summernote.js'],
    parsley: ['/Assets/build/foundation-ui-library/jquery/parsleyjs/css/parsley.css', '/Assets/build/js/plugins/parsley.js',
    //'/Assets/build/foundation-ui-library/jquery/parsleyjs/js/parsley.js',
    '/Assets/build/js/plugins/parsley-validator-comparison.js'],
    select2: ['Assets/build/foundation-ui-library/jquery/select2/css/select2.css', 'Assets/build/foundation-ui-library/jquery/select2-bootstrap-theme/css/select2-bootstrap.css', 'Assets/build/foundation-ui-library/jquery/select2-bootstrap-theme/css/select2-bootstrap-4.css', 'Assets/build/foundation-ui-library/jquery/select2/js/select2.js'],
    datetimepicker: ['Assets/build/foundation-ui-library/jquery/eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.css', 'Assets/build/foundation-ui-library/jquery/eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.dark.css', 'Assets/build/foundation-ui-library/js/moment/_base/js/moment.js', 'Assets/build/foundation-ui-library/jquery/eonasdan-bootstrap-datetimepicker/build/js/bootstrap-datetimepicker.min.js'],
    daterangepicker: ['Assets/build/foundation-ui-library/jquery/daterange-picker/css/daterangepicker.css', 'Assets/build/foundation-ui-library/jquery/daterange-picker/js/daterangepicker.js', 'Assets/build/foundation-ui-library/jquery/daterange-picker/js/daterangesetup.js'],
    chart: ['Assets/build/foundation-ui-library/js/echarts/_base/js/echarts-all.js', 'Assets/build/foundation-ui-library/js/echarts/_base/js/theme.js', 'Assets/build/foundation-ui-library/js/echarts/_base/js/jquery.echarts.js'],
    highcharts: ['Assets/build/foundation-ui-library/jquery/highstock/highcharts/js/highstock.src.js', 'Assets/build/foundation-ui-library/jquery/highstock/highcharts/js/highcharts-more.js', 'Assets/build/foundation-ui-library/jquery/highstock/highcharts/js/modules/heatmap.js', 'Assets/build/foundation-ui-library/jquery/highstock/highcharts/js/modules/exporting.js'],
    bootstrapWizard: ['Assets/build/foundation-ui-library/jquery/twitter-bootstrap-wizard/jquery.bootstrap.wizard.js'],
    fullCalendar: ['Assets/build/foundation-ui-library/js/moment/_base/js/moment.js', 'Assets/build/foundation-ui-library/jquery/fullcalendar/js/fullcalendar.js', 'Assets/build/foundation-ui-library/jquery/fullcalendar/css/fullcalendar.css', 'Assets/build/foundation-ui-library/jquery/fullcalendar/css/fullcalendar.theme.css', 'Assets/build/js/plugins/calendar.js'],
    dropzone: ['Assets/build/foundation-ui-library/js/dropzone/_base/js/dropzone.js', 'Assets/build/foundation-ui-library/js/dropzone/_base/css/dropzone.css'],
    rpCarousel: ['Assets/build/foundation-ui-library/jquery/rp-carousel/css/rp-carousel.css', 'Assets/build/foundation-ui-library/jquery/rp-carousel/js/rp-carousel.js'],
    tooltipster: ['Assets/build/foundation-ui-library/jquery/tooltipster/css/tooltipster.bundle.min.css', 'Assets/build/foundation-ui-library/jquery/tooltipster/css/tooltipster-sideTip-shadow.min.css', 'Assets/build/foundation-ui-library/jquery/tooltipster/js/tooltipster.bundle.min.js'],
    bootstrapSwitch: ['Assets/build/foundation-ui-library/bootstrap/bootstrap-switch/css/bootstrap-switch.min.css', 'Assets/build/foundation-ui-library/bootstrap/bootstrap-switch/js/bootstrap-switch.min.js'],
    rpAudio: ['Assets/build/foundation-ui-library/jquery/rp-audio/css/rp-audio.css', 'Assets/build/foundation-ui-library/jquery/rp-audio/js/rp-audio.js']
};
'use strict';

/**
 * 0.1.0
 * Deferred load js/css file, used for ui-jq.js and Lazy Loading.
 *
 * @ flatfull.com All Rights Reserved.
 * Author url: http://themeforest.net/user/flatfull
 */
var uiLoad = uiLoad || {};

(function ($, $document, uiLoad) {
    "use strict";

    var loaded = [],
        promise = false,
        deferred = $.Deferred();

    /**
     * Chain loads the given sources
     * @param srcs array, script or css
     * @returns {*} Promise that will be resolved once the sources has been loaded.
     */
    uiLoad.load = function (srcs) {
        srcs = $.isArray(srcs) ? srcs : srcs.split(/\s+/);
        if (!promise) {
            promise = deferred.promise();
        }

        $.each(srcs, function (index, src) {
            promise = promise.then(function () {
                return src.indexOf('.css') >= 0 ? loadCSS(src) : loadScript(src);
            });
        });
        deferred.resolve();
        return promise;
    };

    /**
     * Dynamically loads the given script
     * @param src The url of the script to load dynamically
     * @returns {*} Promise that will be resolved once the script has been loaded.
     */
    var loadScript = function loadScript(src) {
        if (loaded[src]) return loaded[src].promise();

        var deferred = $.Deferred();
        var script = $document.createElement('script');

        if (!src.startsWith('http')) {
            src = '/home' + src;
        }

        script.src = src;
        script.onload = function (e) {
            deferred.resolve(e);
        };
        script.onerror = function (e) {
            deferred.reject(e);
        };
        $document.body.appendChild(script);
        loaded[src] = deferred;

        return deferred.promise();
    };

    /**
     * Dynamically loads the given CSS file
     * @param href The url of the CSS to load dynamically
     * @returns {*} Promise that will be resolved once the CSS file has been loaded.
     */
    var loadCSS = function loadCSS(href) {
        if (loaded[href]) return loaded[href].promise();

        var deferred = $.Deferred();
        var style = $document.createElement('link');
        style.rel = 'stylesheet';
        style.type = 'text/css';

        if (!href.startsWith('http')) {
            href = '/home' + href;
        }

        style.href = href;
        style.onload = function (e) {
            deferred.resolve(e);
        };
        style.onerror = function (e) {
            deferred.reject(e);
        };
        $document.head.appendChild(style);
        loaded[href] = deferred;

        return deferred.promise();
    };
})(jQuery, document, uiLoad);
'use strict';

(function ($, MODULE_CONFIG) {
    "use strict";

    $.fn.uiJp = function () {

        var lists = this;

        lists.each(function () {
            var self = $(this);
            var options = eval('[' + self.attr('ui-options') + ']');
            if ($.isPlainObject(options[0])) {
                options[0] = $.extend({}, options[0]);
            }

            uiLoad.load(MODULE_CONFIG[self.attr('ui-jp')]).then(function () {
                if ($.fn.dataTable && $.fn.dataTable.isDataTable(self)) {
                    //!!!for some reason uiLoad "loads" 2 times, here we check if dataTable is already initialised to prevent popup datatables error in IE
                    return;
                };

                self[self.attr('ui-jp')].apply(self, options);
            });
        });

        return lists;
    };
})(jQuery, MODULE_CONFIG);
'use strict';

(function ($) {
    "use strict";

    var promise = false,
        deferred = $.Deferred();
    _.templateSettings.interpolate = /{{([\s\S]+?)}}/g;
    $.fn.uiInclude = function () {
        if (!promise) {
            promise = deferred.promise();
        }
        //console.log('start: includes');

        compile(this);

        function compile(node) {
            node.find('[ui-include]').each(function () {
                var that = $(this),
                    url = that.attr('ui-include');
                promise = promise.then(function () {
                    //console.log('start: compile '+ url);
                    var request = $.ajax({
                        url: eval(url),
                        method: "GET",
                        dataType: "text"
                    });
                    //console.log('start: loading '+ url);
                    var chained = request.then(function (text) {
                        //console.log('done: loading '+ url);
                        var compiled = _.template(text.toString());
                        var html = compiled({ app: app });
                        var ui = that.replaceWithPush(html);
                        ui.find('[ui-jp]').uiJp();
                        ui.find('[ui-include]').length && compile(ui);
                    });
                    return chained;
                });
            });
        }

        deferred.resolve();
        return promise;
    };

    $.fn.replaceWithPush = function (o) {
        var $o = $(o);
        this.replaceWith($o);
        return $o;
    };
})(jQuery);
'use strict';

//JSON API Service Handler for
//GET       /mailboxes
//POST      /mailboxes
//GET       /mailboxes/id
//PUT       /mailboxes/id
//DELETE    /mailboxes/id

var jsonAPIService = function jsonAPIService(action, server, url, data, successFunc) {
    //console.log('successFunc', successFunc);
    var defaultServer = 'settings-domain';
    if (server !== '') {
        defaultServer = server;
    }
    $.ajax({
        headers: {
            "Accept": "application/vnd.api+json",
            "Content-Type": "application/vnd.api+json",
            "companyId": 1
        },
        type: action,
        url: $("meta[name=" + defaultServer + "]").attr('content') + url,
        data: data,
        success: function success(response) {
            console.log('json-api-services', response);
            var fn = window[successFunc];
            fn(response);
        },
        error: function error(XMLHttpRequest, textStatus, errorThrown) {
            console.log("Status: " + textStatus);
            alert("Error: " + errorThrown);
        }
    });
};

// Create the proper data object to be consumed by the JSON API

var createDataObject = function createDataObject(element) {

    var data = {};

    $(element).find('input, textarea, select').each(function (x, field) {
        if (field.name && field.type === "radio") {
            if (field.checked) {
                data[field.name] = field.value;
            }
        } else if (field.name && field.type === "checkbox") {
            if (field.name.indexOf('[]') > 0) {
                if (!$.isArray(data[field.name])) {
                    data[field.name] = [];
                }
                if (field.checked) {
                    data[field.name].push(field.value);
                }
            } else {
                if (field.checked) {
                    data[field.name] = field.value;
                } else {
                    data[field.name] = false;
                }
            }
        } else if (field.name) {
            if (field.name.indexOf('[]') > 0) {
                if (!$.isArray(data[field.name])) {
                    data[field.name] = [];
                }
                data[field.name].push(field.value);
            } else {
                data[field.name] = field.value;
            }
        }
    });

    return data;
};
'use strict';

//When a product is selected, it will call the function "selectedProduct" with the product ID
var rpGetProducts = function rpGetProducts(response) {
    console.log("PRODUCT OPTIONS", response);

    $.each(response.data, function (i, item) {
        console.log(item.familyId);
        var productFamily = $('#product-family-template').clone();
        $(productFamily).removeAttr('id');
        $(productFamily).find('li[data-target="#product-family-sub-menu"]').attr('data-target', "#" + item.title.replace(/\s/g, ''));
        $(productFamily).find('#product-family-sub-menu').attr('id', item.title.replace(/\s/g, ''));
        $(productFamily).find('.assign-product-family-title').html(item.title);
        $(productFamily).find('.rp-select-all input[type=checkbox]').attr('value', item.familyId);
        $(productFamily).css('display', 'block');

        $.each(item.solutions, function (x, solution) {
            var productSolution = $('#product-solution-template').clone();
            $(productSolution).removeClass('hidden');
            $(productSolution).find('.assign-product-solution-title').html(solution.titleId);
            $(productSolution).find('.assign-product-solution-products').html(solution.products);
            $(productSolution).find('input[type=checkbox]').attr('value', solution.solutionId);
            if (solution.isAssigned === true) {
                console.log("Found a solution: " + solution.isAssigned);
                $(productSolution).find('input[type=checkbox]').prop('checked', solution.isAssigned);
            }

            $(productSolution).click(function (e) {
                console.log("EVENT: ", e);
                if (e.target.localName === 'i' || e.target.localName === 'input') {
                    console.log("CHECKBOX EVENT: ", e.target);
                    return;
                } else {
                    console.log("CONTAINER EVENT: ", e.target);
                    $('.assign-product-solution').removeClass('selected');
                    $(this).addClass('selected');
                    selectedProduct(solution.titleId);
                }
            });

            /*$(productSolution).on('click', '.md-check input[type=checkbox]', function( e ) {
                console.log("EVENT: " , e);
                e.stopPropagation();
            });*/

            $(productFamily).find('#' + item.title.replace(/\s/g, '')).append(productSolution);

            if (i == 0 && x == 0) {
                $(productFamily).find('.collapse').addClass('in');
                $(productFamily).find('li:first-child').removeClass('collapsed');
                $(productSolution).click();
            }
        });

        $('#product-menu-list').append(productFamily);
    });

    $(document).on('click', '#product-menu-list .md-check input[type=checkbox]', function (e) {
        e.stopPropagation();

        if ($(this).parents(".rp-select-all").length === 1) {
            var checkboxState = $(this).prop('checked');
            var productsToSelect = $(this).closest('ul').find('.sub-menu').find('.md-check input[type=checkbox]');

            $.each(productsToSelect, function (key, value) {
                if ($(value).prop('checked') !== checkboxState) {
                    $(value).prop('checked', checkboxState);
                }
            });
        }
    });

    var $productMenu = $('#product-menu-list');
    $productMenu.on('show.bs.collapse', '.collapse', function () {
        $productMenu.find('.collapse.in').collapse('hide');
    });
};
"use strict";

var getProductIcon = function getProductIcon(productId) {

    var productIconPath = "https://cdn.realpage.com/styles/v2.0/icons/product/";

    switch (productId) {
        case 1:
            //Onesite
            productIconPath += "rpi-hq.svg";
            break;
        case 4:
            //Asset Optimization
            productIconPath += "house-with-chart.svg";
            break;
        case 6:
            //Lead2Lease
            productIconPath += "square-with-L2L.svg";
            break;
        case 7:
            //YieldStar
            productIconPath += "folder-1.svg";
            break;
        case 8:
            //RealPage Accounting
            productIconPath += "calculator-buttons.svg";
            break;
        case 9:
            //Websites & Syndication
            productIconPath += "monitor-with-www.svg";
            break;
        case 10:
            //Prospect Contact Center
            productIconPath += "dollar-sign-headset.svg";
            break;
        case 13:
            //Spend Management
            productIconPath += "cart-with-gear.svg";
            break;
        case 15:
            //Renters Insurance
            productIconPath += "house-on-shield.svg";
            break;
        case 14:
            productIconPath += "toolbox.svg";
            break;
        case 16:
            //Vendor Services
            productIconPath += "people-desk-dollar.svg";
            break;
        case 17:
            //Active Building
            productIconPath += "monitor-with-user.svg";
            break;
        case 18:
            //Utility Management
            productIconPath += "bulb-2.svg";
            break;
        case 19:
            productIconPath += "education-online.svg";
            break;
        case 20:
            //RealPage Document Management
            productIconPath += "folder-document.svg";
            break;
        default:
            //OneSite Conversions
            productIconPath += "docs-with-checkmark.svg";
    }

    //console.log("Product Name: " + productName);
    //console.log("Product Icon Path: " + productIconPath);
    return productIconPath;
};
'use strict';

var buildAppPicker = function buildAppPicker(products) {

    //Clear the All Products section
    $('.app-switcher-services').not(':first').remove();
    //Clear the Favorites section
    $('.app-switcher-favorites .app-switcher-product').remove();

    $.each(products, function (productIndex, product) {

        //Initialize the Parent Container
        var parentContainer;

        if (product.family !== null && $('div[data-family="' + product.family + '"]').length > 0) {
            //console.log("PARENT ALREADY CREATED: " + product.family);
            parentContainer = $('div[data-family="' + product.family + '"]');
        } else if (product.family === null && $('div[data-family="Miscellaneous"]').length > 0) {
            //console.log("PARENT ALREADY CREATED: " + product.family);
            parentContainer = $('div[data-family="Miscellaneous"]');
        } else {
            //console.log("CREATING PARENT: " + product.family);
            parentContainer = $('.app-switcher-service-template').clone();
            $(parentContainer).attr('data-family', product.family);
            $(parentContainer).removeClass('app-switcher-service-template');
            $(parentContainer).removeClass('hidden');
            if (product.family === null || product.family === undefined) {
                $(parentContainer).find('.app-switcher-service-title').html("Miscellaneous");
            } else {
                $(parentContainer).find('.app-switcher-service-title').html(product.family);
            }

            $('.app-switcher-services').append(parentContainer);
        }

        //console.log("ADDING PRODUCT: " + product.productName);
        var productTemplate = $(parentContainer).find('.app-switcher-product-template').clone();
        $(productTemplate).removeClass('app-switcher-product-template');
        $(productTemplate).removeClass('hidden');

        $(productTemplate).find('.app-switcher-product-title').click(function (e) {
            //Send the SAML information for the user on the application
            sendSamlData(product.productId, product.personaId);
        });

        $(productTemplate).find('.app-switcher-product-link').click(function (e) {
            e.preventDefault();
            //Send the SAML information for the user on the application
            sendSamlData(product.productId, product.personaId);
        });

        $(productTemplate).find('.app-switcher-product-icon').attr('src', getProductIcon(product.productId));
        $(productTemplate).find('.app-switcher-product-title').html(product.productName);

        if (product.isFavorite) {
            var favorite = $(productTemplate).clone();
            $('.app-switcher-favorites').append(favorite);
        }

        $(parentContainer).append(productTemplate);
    });
};
'use strict';

var buildProductCard = function buildProductCard(template, id, personaId, name, description, productUrl, newTab, learnMore, subsolution, isFavorite, hasAccess, parentContainer) {

    var subsolutionElement = $(template).find('.product-card-subsolution');
    $(template).removeClass('rp-product-card-template');
    $(template).removeClass('hidden');

    $(template).find('.product-card').click(function (e) {
        var target = $(event.target);
        if (target.is('.product-card-favorite') || target.is('.product-tooltip-icon .fa-info-circle')) {
            e.preventDefault();
            return;
        }
        //Send the SAML information for the user on the application
        e.preventDefault();
        sendSamlData(id, personaId);
    });

    $(template).find('.product-card-favorite').click(function (e) {
        //Check if product will ve fave
        var isFave = !$(template).find('.product-card-favorite').hasClass('active');

        var data = {};
        data.name = 'IsFavorite';
        data.productId = id;

        if (isFave) {
            data.value = 1;
            //Call to favorite this product
            userAuthAPIService('PUT', 'api/personas/products/' + id + '/productSettings?name=IsFavorite&value=1', data, 'updatedFavorite');
            $(template).find('.product-card-favorite').addClass('active');
            updateProductsSession(id, true);
        } else {
            data.value = 0;
            //Call to favorite this product
            userAuthAPIService('PUT', 'api/personas/products/' + id + '/productSettings?name=IsFavorite&value=0', data, 'updatedFavorite');
            $(template).find('.product-card-favorite').removeClass('active');
            updateProductsSession(id, false);
        }
    });

    $(template).find('.product-card-icon').attr('src', getProductIcon(id));

    $(template).find('.product-card-title').html(name);

    if (subsolution) {
        subsolutionElement.html(subsolution);
    } else {
        subsolutionElement.hide();
    }

    $(template).find('.tooltip-product-card-title').html(name);
    $(template).find('.tooltip-product-card-description').html(description);

    $(template).find('.tooltip-product-card-learn-more').attr('href', learnMore);

    $(template).find('.product-tooltip-icon').attr('data-tooltip-content', '#ap-tooltip-content-' + id);
    $(template).find('#ap-tooltip-content').attr('id', 'ap-tooltip-content-' + id);

    if (newTab) {
        $(template).find('.tooltip-product-card-learn-more').attr('target', '_blank');
    }

    if (hasAccess !== "" && hasAccess === false) {
        $(template).find('.product-card').prepend('<div class="disabled-product-card"></div>');
    }

    if (isFavorite !== "" && isFavorite === true) {
        $(template).find('.product-card-favorite').addClass('active');
    }

    parentContainer.append(template);
    $(template).find('[ui-jp]').uiJp();
};

var updateProductsSession = function updateProductsSession(id, value) {
    var profileDetails = $.sessionStorage.get("profileDetails");

    //PULL THE PRODUCTS
    var products = profileDetails.assignedProducts;

    for (i = 0; i < products.length; i++) {
        if (products[i].productId === id) {
            products[i].isFavorite = value;
        }
    }

    buildAppPicker(products);
    profileDetails.assignedProducts = products;
    $.sessionStorage.set("profileDetails", profileDetails);
    console.log(products);
};

var updatedFavorite = function updatedFavorite(response) {
    console.log("updated favorite for product");
    console.log(response);
};

var sendSamlData = function sendSamlData(productId, personaId) {
    //Send the SAML information for the user on the application
    window.open('/saml?productId=' + productId + '&personaId=' + personaId);
};
"use strict";

/*jshint esversion: 6 */

//Verify that User Data is in the Session

var rpSuccessFunc;

var rpGetUserPersona = function rpGetUserPersona(response) {
    $.sessionStorage.set("userPersona", response);
};

var rpGetUserData = function rpGetUserData(response) {

    $.sessionStorage.set("profileResources", response.dashboardElements.resources);
    $.sessionStorage.set("profileDetails", response.dashboardElements.profileDetail);
    $.sessionStorage.set("realpageID", response.dashboardElements.profileDetail.userLogin.realPageId);
    $.sessionStorage.set("realpageOrganizationID", response.dashboardElements.profileDetail.organization[0].realPageId);

    //Get products for App switcher
    var realPageId = $.sessionStorage.get('realpageID');
    var url = "api/user/" + realPageId + "/products";

    userAuthAPIService('GET', url, '', 'populateInitialData');
};

function buildAppSwitcher() {
    var products = $.sessionStorage.get('products');
    RAUL.AppSwitcher.fromOptions({ switcherData: products });

    //remove this event handler after disabled styles will be added in Raul
    $('.raul-header-app-switcher').on('click', function () {
        var disabledProducts = products.products.filter(function (product) {
            return product.status == 7;
        });

        disabledProducts.forEach(function (product) {
            $('#raul-switcher-context').find('[href="' + product.url + '"]').addClass('disabled');
        });
    });
};

function populateInitialData(productsResponse) {
    var products = productsResponse || $.sessionStorage.get("products");
    var profileDetails = $.sessionStorage.set("products", products);
    var profileDetails = $.sessionStorage.get("profileDetails");

    //BEGIN : Place the Username, Title, Company Name
    var username = profileDetails.firstName + " " + profileDetails.middleName + " " + profileDetails.lastName;
    var jobTitle = profileDetails.title;
    var header = $('ui-header');

    header.attr('user-name', username);
    header.attr('user-title', jobTitle);

    header.click('raul-header-user-angle', function () {
        header.find('[href="#user-settings"]').addClass('hidden');
    });

    buildAppSwitcher();

    checkLeftNavRights();

    //COMMENT this since company name is populated from init-app.js
    /*if (organizations.length > 0) {
        $('.rp-user-credential-organization').html(organizations[0].name);
    }*/

    //END : Place the Username, Title, Company Name

    var fn = window[rpSuccessFunc];
    if ($.isFunction(fn)) {
        fn();
    }
}

function rpReadCookie(name) {
    //console.log("TRYING TO READ THE COOKIE",document.cookie.split(';'));

    var nameEQ = encodeURIComponent(name) + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) === ' ') {
            c = c.substring(1, c.length);
        }
        if (c.indexOf(nameEQ) === 0) {
            console.log("FOUND THE COOKIE", decodeURIComponent(c.substring(nameEQ.length, c.length)));
            return decodeURIComponent(c.substring(nameEQ.length, c.length));
        }
    }
    return null;
}

var rpUserSession = function rpUserSession(successFunc) {

    rpSuccessFunc = successFunc;

    if (!$.sessionStorage.get("access_token") || $.sessionStorage.get("access_token") !== rpReadCookie('access_token')) {
        //Get the access token before making User Authentication Calls
        $.sessionStorage.set("access_token", rpReadCookie('access_token'));

        //Pull the dashboard information for the logged in user
        userAuthAPIService('GET', 'api/dashboard/', '', 'rpGetUserData');

        //Pull the persona information for the logged in user
        userAuthAPIService('GET', 'api/persona/', '', 'rpGetUserPersona');
    } else {
        populateInitialData(); // added for non SPA

        // uncomment for SPA
        //Call the Populate Page Data Function
        //var fn = window[rpSuccessFunc]; 

        //if ($.isFunction(fn)) { 
        //fn();
        //}
    }
};

var checkLeftNavRights = function checkLeftNavRights() {
    var gbURI = $('body').attr('data-gb-uri');

    userAPIService('GET', gbURI, 'api/sidemenu/rights', '').then(function (response) {
        var data = response.data;


        $('#raul-left-navigation-items li.hidden').each(function (item) {
            var rightTitle = $(this).attr('data-right');
            if (rightTitle === 'Settings') {
                $(this).removeClass('hidden');
                return;
            }

            var isInLeftNav = data.rights.some(function (right) {
                return right.toLowerCase() === rightTitle.toLowerCase();
            });

            if (isInLeftNav) {
                $(this).removeClass('hidden');
            }
        });
    }).catch(function (err) {
        return console.log('error', err);
    });
};
'use strict';

//User Authentication Service Handler for
//GET       https://my2dev.corp.realpage.com/api/dashboard
//GET       https://my2dev.corp.realpage.com/api/profiles/details
//GET       https://my2dev.corp.realpage.com/api/SideMenu/rights
//GET       http://my2dev.corp.realpage.com/api/organization/<userID>/products?mergePersonaAccess=true

var userAuthAPIService = function userAuthAPIService(action, url, data, successFunc) {
    //console.log('successFunc', successFunc);
    var token = $.sessionStorage.get("access_token");
    var domain = $('body').attr('data-gb-uri');

    $.ajax({
        headers: {
            "Content-Type": "application/json",
            "Authorization": "Bearer " + token
        },
        crossDomain: true,
        type: action,
        url: domain + url,
        data: JSON.stringify(data),
        success: function success(response) {
            console.log('user-auth-api-services', response);
            var fn = window[successFunc];
            fn(response);
        },
        error: function error(XMLHttpRequest, textStatus, errorThrown) {
            console.log("Status: " + textStatus);
            console.log("XMLHttpRequest", XMLHttpRequest);
        }
    });
};
'use strict';

/*jshint esversion: 6 */

var userAPIService = function userAPIService(action, domainName, url, data) {
    return new Promise(function (resolve, reject) {
        var token = $.sessionStorage.get("access_token");

        if (!domainName) {
            //default for Red book API 
            domainName = $('body').attr('data-rb-uri');
        }

        $.ajax({
            headers: {
                "Content-Type": "application/json",
                "Authorization": "Bearer " + token
            },
            crossDomain: true,
            type: action,
            url: domainName + url,
            data: JSON.stringify(data),
            success: function success(response) {
                console.log('user-auth-api-services', response);
                resolve(response);
            },
            error: function error(XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
                console.log("XMLHttpRequest", XMLHttpRequest);

                if (XMLHttpRequest.responseJSON) {
                    resolve(XMLHttpRequest.responseJSON);
                } else {
                    resolve(XMLHttpRequest);
                }
            }
        });
    });
};
'use strict';

(function ($) {
    "use strict";

    // Javascript for keeping the footer at the bottom of a scrolling Aside

    var repositionAsideActionFooter = function repositionAsideActionFooter() {
        var offset = $('.responsive-aside-scrolling').scrollTop();
        $('.responsive-aside-scrolling .stay-visible-bottom').css({
            bottom: -1 * offset + 'px'
        });
    };

    // Javascript for keeping the footer at the bottom of a scrolling content, but also above the footer content

    var repositionActionFooter = function repositionActionFooter() {
        console.log("running the reposition");
        var down = $(window).height() + 234;
        if ($(window).scrollTop() > $(document).height() - down) {
            $('.stay-visible-bottom').css('bottom', '-40');
            $('.stay-visible-bottom').css('position', 'absolute');
            $('.stay-visible-bottom').css('padding-left', '0');
            $('.stay-visible-bottom').css('width', '100%');
        } else {
            $('.stay-visible-bottom').css('bottom', '0');
            $('.stay-visible-bottom').css('position', 'fixed');
            $('.stay-visible-bottom').css('padding-left', $('.raul-page-container').css('margin-left'));
            $('.stay-visible-bottom').css('width', '100%');
        }
    };

    window.repositionActionFooter = repositionActionFooter;

    //window.addEventListener('scroll', repositionActionFooter);
    //window.addEventListener('resize', repositionActionFooter);

    $(window).scroll(function () {
        repositionActionFooter();
    });

    $(window).resize(function () {
        repositionActionFooter();
    });

    $(window).ready(function () {
        repositionActionFooter();
    });

    $('.responsive-aside-scrolling').scroll(function () {
        repositionAsideActionFooter();
    });
})(jQuery);
'use strict';

/*jshint esversion: 6 */

/*
* BEGIN: PAGE SETUP for non SPA
*/
var defineUserRole = function defineUserRole() {
    var orgId = $('body').attr('data-orgid');

    var userRole = orgId === '-1' ? 'employee' : 'customer';

    $.sessionStorage.set('company', '');
    $.sessionStorage.set('userRole', userRole);

    //set company to sessionStorage
    var queries = document.location.search.substr(1).split('&');
    var company = {};

    queries.forEach(function (query) {
        var companyValues = query.split('=');

        company[companyValues[0]] = decodeURI(companyValues[1]);
    });

    var CompanyId = company.CompanyId,
        CompanyName = company.CompanyName;


    if (CompanyId && CompanyName) {
        CompanyName = decodeURIComponent(CompanyName);
        $.sessionStorage.set('company', company);

        var header = $('ui-header');
        header.attr('company-name', CompanyName); //set company name to header
    } else {
        // if there is no right to view settings pages - redirect user to home page
        window.location.href = window.location.origin + '/#/';
    }

    return userRole;
};

//};

/*let showShellComponents = () => {
   // $('#raul-page-header-breadcrumbs').removeClass('hidden');
   // $('#raul-page-header-breadcrumbs-back').removeClass('d-none').addClass('d-inline-block');
    $('.icon-container').has('#raul-header-settings').removeClass('hidden');
};*/

var pageSetup = function pageSetup() {
    //BEGIN: PUT WHATEVER YOU NEED IN THIS FUNCTION FOR INITIALIZATION OF EACH PAGE

    //Manage rights for Settings pages
    if (window.location.pathname.startsWith('/home/setting')) {
        var rightToView = $('body').attr('data-viewright') === 'True' ? true : false;
        var userRole = defineUserRole();

        if (!rightToView) {
            // ???? for both employee and customer ????
            // if there is no right to view settings pages - redirect user to home page
            window.location.href = window.location.origin + '/#/';
        }

        $('#raul-left-navigation').find('.settings-gear').attr('href', '/home/setting' + window.location.search);
        /*if (userRole === 'customer') {
            showShellComponents();
        }*/
    }

    //CHECK IF THE USER SESSION HAS BEEN CREATED. IF NOT, CREATE IT AND THEN RUN THE SUCCESS FUNCTION ON THE PAGE LEVEL.
    rpUserSession('populatePageData');

    //REPOSITION THE ACTION BAR
    $(document).ready(function () {
        repositionActionFooter();
    });

    //END: PUT WHATEVER YOU NEED IN THIS FUNCTION FOR INITIALIZATION OF EACH PAGE
};
/*
* END: PAGE SETUP
*/

$(document).ready(function () {
    if (document.cookie.indexOf('access_token=') === -1) {
        window.location.href = '/home/signout';
    }

    pageSetup();
});
'use strict';

var getUrlParameter = function getUrlParameter(sParam) {
    var sPageURL = decodeURIComponent(window.location.search.substring(1)),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;

    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');

        if (sParameterName[0] === sParam) {
            return sParameterName[1] === undefined ? true : sParameterName[1];
        }
    }
};
'use strict';

(function ($) {
    'use strict';

    window.app = {
        name: 'RAUL',
        version: '1.1.0',
        // for chart colors
        color: {
            'primary': '#0cc2aa',
            'accent': '#a88add',
            'warn': '#fcc100',
            'info': '#6887ff',
            'success': '#6cc788',
            'warning': '#f77a99',
            'danger': '#f44455',
            'white': '#ffffff',
            'light': '#f1f2f3',
            'dark': '#2e3e4e',
            'black': '#2a2b3c'
        },
        setting: {
            theme: {
                primary: 'primary',
                accent: 'accent',
                warn: 'warn'
            },
            color: {
                primary: '#0cc2aa',
                accent: '#a88add',
                warn: '#fcc100'
            },
            folded: false,
            boxed: false,
            container: false,
            themeID: 1,
            bg: ''
        }
    };

    var setting = 'jqStorage-' + app.name + '-Setting',
        storage = $.localStorage;

    if (storage.isEmpty(setting)) {
        storage.set(setting, app.setting);
    } else {
        app.setting = storage.get(setting);
    }

    if (getParams('bg')) {
        app.setting.bg = getParams('bg');
        storage.set(setting, app.setting);
    }

    // init
    $('body').addClass(app.setting.bg);
    app.setting.boxed && $('body').addClass('container');
    app.setting.folded && $('#aside').addClass('folded');
    setTimeout(function () {
        $('[ng-model="app.setting.folded"]').prop('checked', app.setting.folded);
        $('[ng-model="app.setting.boxed"]').prop('checked', app.setting.boxed);
        $('#settingColor input[value=' + app.setting.themeID + ']').prop('checked', 'checked');
    }, 1000);

    // folded, boxed, container
    //Clear event before setting the event
    $(document).off('change', '#settingLayout input');
    //Setting the event
    $(document).on('change', '#settingLayout input', function (e) {
        eval($(this).attr('ng-model') + "=" + $(this).prop('checked'));
        storage.set(setting, app.setting);
        location.reload();
    });
    // color and theme
    //Clear event before setting the event
    $(document).off('click', '[ng-click]');
    //Setting the event
    $(document).on('click', '[ng-click]', function (e) {
        eval($(this).attr('ng-click'));
        if ($(this).find('input')) {
            app.setting.themeID = $(this).find('input').val();
        }
        storage.set(setting, app.setting);
        location.reload();
    });

    init();

    function setTheme(theme) {
        app.setting.theme = theme.theme;
        setColor();
        if (theme.url) {
            setTimeout(function () {
                var layout = theme.url.split('=');
                window.location.href = 'dashboard.' + (layout[1] ? layout[1] + '.' : '') + 'html';
            }, 1);
        }
    }
    function setColor() {
        app.setting.color = {
            primary: getColor(app.setting.theme.primary),
            accent: getColor(app.setting.theme.accent),
            warn: getColor(app.setting.theme.warn)
        };
    }
    function getColor(name) {
        return app.color[name] ? app.color[name] : palette.find(name);
    }
    function getParams(name) {
        name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
        var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
            results = regex.exec(location.search);
        return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
    }

    function init() {
        $('[ui-jp]').uiJp();
        $('body').uiInclude();
    }

    $(window).on('show.bs.modal', function () {
        $('.overlay-cover').show();
    });

    $(window).on('hidden.bs.modal', function () {
        $('.overlay-cover').hide();
    });

    /*
    functionality to update dropdown to have sticky values
    note adding format-text class around initial button value
      <div class="btn-group dropdown half w-full format-dropdown m-t-1">
        <button class="btn btn-block white w-lg" data-toggle="dropdown" aria-expanded="false">
            <i class="fa fa-angle-down pull-right"></i>
            <span class="format-text">None<span>
        </button>
        <div class="dropdown-menu  dropdown-menu-scale dropdown-menu-width template-dropdown-choices">
            <a class="dropdown-item" href="#">None</a>
            <a class="dropdown-item" href="#">Template 1</a>
            <a class="dropdown-item" href="#">Template 2</a>
        </div>
     </div>
     */
    $('.dropdown-menu a').on('click', function () {
        var dataText = $(this).html();
        console.log('dataText', dataText);
        if (dataText !== undefined && dataText.length > 0) {
            $(this).parent().siblings('.btn').children('.format-text').html(dataText);
        }
    });
})(jQuery);