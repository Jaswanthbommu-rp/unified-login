"use strict";

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

var getRoleData = function getRoleData(response) {
    console.log("ROLE DATA HAS BEEN RETRIEVED");
    console.log("=====================");
    console.log(response);
    console.log("=====================");

    if (response.records !== undefined) {
        $.sessionStorage.set("roleData", response.records);
    } else {
        $.sessionStorage.set("roleData", response.data);
    }

    var roleData = $.sessionStorage.get("roleData");

    var datatable = $('#role-datatable table').dataTable();

    //Clear the table before populating it
    datatable.fnClearTable();

    if (roleData !== null) {
        //Populate the table with roles
        roleData.forEach(function (role) {
            if (role.roletype === undefined) {
                role.roletype = ' ';
            }

            datatable.fnAddData(["<div class=\"valign-middle\">\n" + role.name + "</div>", "<div class=\"blue-link\">\n" + role.rightsAssigned + "</div>", "<div>\n" + role.roletype + "</div>", "<div class=\"valign-middle text-right position-relative\"><a href=\"#\" class=\"blue-link ft-s-24\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">\n" + "                        <i class=\"rp-icon-more\"></i>\n" + "                    </a>\n" + "                    <div class=\"dropdown-menu dropdown-menu-right max-width-60\">\n" + "                        <button class=\"dropdown-item\" id=\"assign-role\" type=\"button\" data-id=\"'+ role.id +'\">Assign Rights</button>\n" + "                        <button class=\"dropdown-item\" id=\"clone-role\" type=\"button\" data-id=\"'+ role.id +'\">Clone</button>\n" + "                        <button class=\"dropdown-item\" id=\"edit-role\" type=\"button\" data-id=\"'+ role.id +'\">Edit</button>\n" + "                        <button class=\"dropdown-item\" id=\"delete-role\" type=\"button\" data-id=\"'+ role.id +'\">Remove</button>\n" + "                    </div></div>"]);
        });
    }
};

var buildAccessUrl = function buildAccessUrl(requestMethod, target, product, roleId) {
    var userPersona = $.sessionStorage.get("userPersona");
    var parametersObj = product[target].find(function (item) {
        return item.method === requestMethod;
    }); //find parameters matching request method

    if (parametersObj) {
        //if there are parameters, build queries string
        var queriesString = parametersObj.queriesString,
            queries = parametersObj.queries;


        queries.forEach(function (item, i) {
            var query = void 0;
            if (item === "editorPersonaId" || item === "userPersonaId") {
                query = item + "=" + userPersona.personaId;
            }
            if (item === "partyId") {
                query = item + "=" + userPersona.organizationPartyId;
            }
            if (item === "roleName") {
                var roleValue = $('#new-role-create input').val();
                roleValue = roleValue.replace(/\s/g, "+");
                query = item + "=" + roleValue;
            }
            if (item === "roleId") {
                query = item + "=" + roleId;
            }
            if (i == queries.length - 1) {
                queriesString = "" + queriesString + query;
                return;
            }

            queriesString = "" + queriesString + query + "&";
        });

        return "api/products/" + product.apiName + queriesString;
    }
    return "api/products/" + product.apiName;
};

var selectedProduct = function selectedProduct(name) {
    console.log("SELECTED PRODUCT IS " + name);
    $.sessionStorage.set("selectedProduct", name);

    var userPersona = $.sessionStorage.get("userPersona");

    //Set the Product Access Title
    $('.assign-product-access-title span').html(name);
    $('#coming-soon').addClass("hidden");

    var product = rolesApiRequirements.find(function (item) {
        return item.name === name;
    });

    if (product) {
        //if product endpoint exist
        $('#add-roles-settings .tab-pane').addClass('active');
        var accessURL = buildAccessUrl('GET', 'roles', product, userPersona);

        //Pull the roles information for the product
        userAuthAPIService('GET', accessURL, '', 'getRoleData');
    } else {
        $('#add-roles-settings .tab-pane').removeClass('active');
        $('#coming-soon').removeClass("hidden");
    }
};

var populateRightsTable = function populateRightsTable(response) {
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

    var rightsData = $.sessionStorage.get("rightsData")[productName];
    var datatable = $('#roles-page-rights-datatable table').dataTable();

    //Clear the table before populating it
    datatable.fnClearTable();

    //Populate the table with roles
    rightsData.forEach(function (right) {
        datatable.fnAddData(["<label class=\"md-check dark-bluebox ng-scope\">\n" + "            <input type=\"checkbox\" onclick=\"saveSelectedRights(this)\" name=\"" + right.centerName + "\" data-parsley-multiple=\"d-s-c\" value=\"" + right.id + "\" class=\"md-check dark-bluebox\">\n" + "            <i class=\"primary\"></i>\n" + "        </label>", "<div class=\"valign-middle\">\n" + right.centerName + "</div>", "<div>\n" + right.description + "</div>"]);
    });

    $('#new-role-create').parsley().reset();
    datatable.fnSetColumnVis(1, true);

    if (rightsData.every(function (right) {
        return !right.centerName;
    })) {
        // if centerName is null, hide column
        datatable.fnSetColumnVis(1, false);
    }
};

var saveSelectedRights = function saveSelectedRights(currentTarget) {
    if (!currentTarget) {
        currentTarget = this;
    }
    var currentRole = $.sessionStorage.get('currentRole');

    var id = currentRole.id,
        rights = currentRole.rights;

    var rightToAdd = {
        id: currentTarget.value,
        checked: currentTarget.checked
    };

    if (rights.length) {
        var right = rights.find(function (item) {
            if (item.id === currentTarget.value) {
                item.checked = currentTarget.checked;
                return true;
            }
        });
        if (!right) {
            rights.push(rightToAdd);
        }
    } else {
        rights.push(rightToAdd);
    }

    rights = rights.filter(function (item) {
        return item.checked;
    }); //remove all unselected checkboxes

    $.sessionStorage.set('currentRole', { id: id, rights: rights });
};

//populate table with rights
var getAllRights = function getAllRights() {
    var productName = $.sessionStorage.get("selectedProduct");
    var rightsData = $.sessionStorage.get("rightsData") ? $.sessionStorage.get("rightsData")[productName] : '';
    var userPersona = $.sessionStorage.get("userPersona");

    if (!rightsData) {
        var _productName = $.sessionStorage.get("selectedProduct");
        var product = rolesApiRequirements.find(function (item) {
            return item.name === _productName;
        });
        var rightsURL = buildAccessUrl('GET', 'rights', product, userPersona);

        //Pull the roles information for the product
        userAuthAPIService('GET', rightsURL, '', 'populateRightsTable');
    } else {
        populateRightsTable();
    }
};

var getCurrentProductRequirements = function getCurrentProductRequirements() {
    var productName = $.sessionStorage.get('selectedProduct');
    return rolesApiRequirements.find(function (item) {
        return item.name === productName;
    });
};

//create new role functionality
var createRole = function createRole() {
    var product = getCurrentProductRequirements();
    var newRoleURL = buildAccessUrl('POST', 'roles', product);

    userAuthAPIService('POST', newRoleURL, '', 'updateNewRole');
};

var updateRolesTable = function updateRolesTable() {
    selectedProduct($.sessionStorage.get('selectedProduct'));
    closeModalWindow();
};

var openModal = function openModal(id, rights) {
    // to set new or existing role to sessionStorage
    $('#roles-aside-modal').modal('show');
    id = id ? id : null;
    rights = rights ? rights : [];

    var currentRole = { id: id, rights: rights };
    $.sessionStorage.set('currentRole', currentRole);
};

//assign rights to new role(PUT)
var updateNewRole = function updateNewRole(response) {
    if (response.isError) {
        //for Unified Login
        console.log(response.errorReason);
        return;
    }

    var newRole = response.records[0];
    var product = getCurrentProductRequirements();
    var rightsToAdd = $.sessionStorage.get('currentRole').rights.map(function (item) {
        return item.id;
    });

    var data = {
        rightsToAdd: rightsToAdd,
        rightsToDelete: []
    };
    var assignRightsURL = buildAccessUrl('PUT', 'roles', product, newRole.id || newRole);

    userAuthAPIService('PUT', assignRightsURL, data, 'updateRolesTable');
};

var closeModalWindow = function closeModalWindow() {
    $('#new-role-create').parsley().destroy();
    $('#new-role-create input[type="text"]').val('');
    $.sessionStorage.remove('currentRole');
    $('#roles-aside-modal').modal('hide');
};

//EVENT HANDLERS
$(document).ready(function () {
    $('#create-role').click(function (e) {
        getAllRights();

        openModal();
    });

    $('#new-role-create').submit(function (event) {
        event.preventDefault();
        var rightsCheckboxes = $('#roles-page-rights-datatable input[type="checkbox"]:checked');

        //CUSTOM PARSLEY VALIDATOR
        if (window.ParsleyValidator && !window.ParsleyValidator.validators.multiselection) {
            window.ParsleyValidator.addValidator('multiselection', function (value) {
                var currentRole = $.sessionStorage.get('currentRole');
                var rights = currentRole.rights;


                return !!rights.length;
            }).addMessage('en', 'multiselection', 'Please select a right');
        }

        if ($(this).parsley().isValid()) {
            createRole();
        }
    });

    $('#roles-page-rights-datatable .select-all').click(function () {
        $('#roles-page-rights-datatable input[type="checkbox"]').prop('checked', this.checked);
    });

    $('#cancel-role-btn').click(function () {
        closeModalWindow();
    });

    $('#roles-aside-modal .box-close').click(function () {
        closeModalWindow();
    });
});

//!!!!! TO DO

// Add role functionality:
// - fix filter in Product Center Column(for '50058' value);
// - modal window somtimes "freeze" while loading;
// - add error handler if role with the entered name already exists(from OneSite- error from server, from Unified login- notification from server);
// Edit/delete/clone/asiign roles functionality