"use strict";

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

var populateRightsData = function populateRightsData(response) {
    console.log("RIGHTS DATA HAS BEEN RETRIEVED");
    console.log("=====================");
    console.log(response);
    console.log("=====================");

    var productName = $.sessionStorage.get('selectedProduct');
    var storageRightsData = $.sessionStorage.get("rightsData");

    if (response) {
        if (response.records !== undefined) {
            $.sessionStorage.set("rightsData", Object.assign(storageRightsData ? storageRightsData : {}, _defineProperty({}, productName, response.records)));
        } else if (Array.isArray(response)) {
            // for OneSite product
            $.sessionStorage.set("rightsData", Object.assign(storageRightsData ? storageRightsData : {}, _defineProperty({}, productName, response)));
            // $.sessionStorage.set("rightsData", response);
        } else {
            $.sessionStorage.set("rightsData", Object.assign(storageRightsData ? storageRightsData : {}, _defineProperty({}, productName, response.data)));
        }
    }

    $('#loading').addClass("hidden");
    $('#add-rights-settings .tab-pane').addClass('active');

    var rightsData = $.sessionStorage.get("rightsData")[productName];
    var datatable = $('#rights-datatable table').dataTable();

    //Clear the table before populating it
    datatable.fnClearTable();

    if (rightsData !== null) {
        //Populate the table with roles
        rightsData.forEach(function (right) {
            datatable.fnAddData(["<div class=\"valign-middle\">\n" + right.centerName + "</div>", "<div>\n" + right.description + "</div>", "<div class=\"blue-link text-center\"><a href=\"\#\">\n" + right.rolesAssigned + "</a></div>", "<div class=\"valign-middle text-right position-relative\"><a href=\"#\" class=\"blue-link ft-s-24\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">\n" + "                        <i class=\"rp-icon-more\"></i>\n" + "                    </a>\n" + "                    <div class=\"dropdown-menu dropdown-menu-right max-width-60\">\n" + "                        <button class=\"dropdown-item\" id=\"assign-role\" type=\"button\" data-id=\"'+ role.id +'\">Assign Rights</button>\n" + "                    </div></div>"]);
        });

        datatable.fnSetColumnVis(0, true);

        if (rightsData.every(function (right) {
            return !right.centerName;
        })) {
            // if centerName is null, hide column
            datatable.fnSetColumnVis(0, false);
        }
    }
};

var buildAccessUrl = function buildAccessUrl(requestMethod, target, product, userPersona) {
    var parametersObj = product[target].find(function (item) {
        return item.method === requestMethod;
    }); //find parameters matching request method

    if (parametersObj) {
        //if there are parameters, build queries string
        var queriesString = parametersObj.queriesString,
            queries = parametersObj.queries;


        queries.forEach(function (item, i) {
            var query;
            if (item === "editorPersonaId" || item === "userPersonaId") {
                query = item + "=" + userPersona.personaId;
            }
            if (item === "partyId") {
                query = item + "=" + userPersona.organizationPartyId;
            }
            queriesString = "" + queriesString + query + "&";
            if (i == queries.length - 1) {
                queriesString = queriesString.slice(0, queriesString.length - 1);
            }
        });
        return "api/products/" + product.apiName + queriesString;
    }
    return "api/products/" + product.apiName;
};

var centersResponse;
var getCentersData;

var getRightsData = function getRightsData(rightsResponse) {
    if (centersResponse) {
        var _centersResponse = centersResponse,
            centers = _centersResponse.records;
        var rights = rightsResponse.records;


        rights = rights.filter(function (right) {
            //filter rights with centers from cenersResponse
            return centers.indexOf(right.centerName) !== -1;
        });

        populateRightsData(rights);
    }
};

var selectedProduct = function selectedProduct(name) {
    console.log("SELECTED PRODUCT IS " + name);

    $.sessionStorage.set("selectedProduct", name);
    var userPersona = $.sessionStorage.get("userPersona");
    var rightsData = $.sessionStorage.get("rightsData");

    //Set the Product Title
    $('.assign-product-access-title span').html(name);

    $('#coming-soon').addClass("hidden");
    $('#add-rights-settings .tab-pane').removeClass('active');

    var product = rolesApiRequirements.find(function (item) {
        return item.name === name;
    });

    if (rightsData && rightsData[name]) {
        // if rights data for selected product is already in sessionStorage
        populateRightsData();
        return;
    }

    if (product) {
        //and if no data in sessionStorage and if product endpoint exist
        $('#loading').removeClass("hidden");

        getCentersData = function getCentersData(response) {
            // getCentersData function for OneSite product
            centersResponse = response;

            var accessURL = buildAccessUrl('GET', 'rights', product, userPersona);

            userAuthAPIService('GET', accessURL, '', 'getRightsData');
        };

        if (product.name === 'OneSite') {
            var centersURL = "api/products/onesite/right/center?editorPersonaId=" + userPersona.personaId;

            userAuthAPIService('GET', centersURL, '', 'getCentersData');
            return;
        }
        var rightsURL = buildAccessUrl('GET', 'rights', product, userPersona);

        //Pull the roles information for the product
        userAuthAPIService('GET', rightsURL, '', 'populateRightsData');
    } else {
        $('#add-rights-settings .tab-pane').removeClass('active');
        $('#coming-soon').removeClass("hidden");
    }
};