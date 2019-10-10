let getRoleData = (response) => {
    console.log("ROLE DATA HAS BEEN RETRIEVED");
    console.log("=====================");
    console.log(response);
    console.log("=====================");

    if(response.records !== undefined){
        $.sessionStorage.set("roleData",response.records);
    } else {
        $.sessionStorage.set("roleData",response.data);
    }

    let roleData = $.sessionStorage.get("roleData");

    let datatable = $('#role-datatable table').dataTable();

    //Clear the table before populating it
    datatable.fnClearTable();

    if(roleData !== null) {
        //Populate the table with roles
        roleData.forEach( role => {
            if (role.roletype === undefined) {
                role.roletype = ' ';
            }

            datatable.fnAddData([
                "<div class=\"valign-middle\">\n" + role.name + "</div>",
                "<div class=\"blue-link\">\n" + role.rightsAssigned + "</div>",
                "<div>\n" + role.roletype + "</div>",
                "<div class=\"valign-middle text-right position-relative\"><a href=\"#\" class=\"blue-link ft-s-24\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">\n" +
                "                        <i class=\"rp-icon-more\"></i>\n" +
                "                    </a>\n" +
                "                    <div class=\"dropdown-menu dropdown-menu-right max-width-60\">\n" +
                "                        <button class=\"dropdown-item\" id=\"assign-role\" type=\"button\" data-id=\"'+ role.id +'\">Assign Rights</button>\n" +
                "                        <button class=\"dropdown-item\" id=\"clone-role\" type=\"button\" data-id=\"'+ role.id +'\">Clone</button>\n" +
                "                        <button class=\"dropdown-item\" id=\"edit-role\" type=\"button\" data-id=\"'+ role.id +'\">Edit</button>\n" +
                "                        <button class=\"dropdown-item\" id=\"delete-role\" type=\"button\" data-id=\"'+ role.id +'\">Remove</button>\n" +
                "                    </div></div>"
            ]);
        });
    }
};

let buildAccessUrl = (requestMethod, target, product, roleId) => {
    let userPersona = $.sessionStorage.get("userPersona");
    let parametersObj = product[target].find( item => item.method === requestMethod);//find parameters matching request method

    if(parametersObj) {//if there are parameters, build queries string
        let { queriesString, queries } = parametersObj;

        queries.forEach( (item, i) => {
            let query;
            if(item === "editorPersonaId" || item === "userPersonaId") {
                query = `${item}=${userPersona.personaId}`;
            }
            if(item === "partyId") {
                query = `${item}=${userPersona.organizationPartyId}`;
            }
            if(item === "roleName") {
                let roleValue = $('#new-role-create input').val();
                roleValue = roleValue.replace(/\s/g, "+");
                query = `${item}=${roleValue}`;
            }
            if(item === "roleId") {
                query = `${item}=${roleId}`;
            }
            if(i == queries.length-1) {
                queriesString = `${queriesString}${query}`;
                return;
            }

            queriesString = `${queriesString}${query}&`;
        });

        return `api/products/${product.apiName}${queriesString}`;
    }
    return `api/products/${product.apiName}`;
};

let selectedProduct = (name) => {
    console.log("SELECTED PRODUCT IS " + name);
    $.sessionStorage.set("selectedProduct", name);

    let userPersona = $.sessionStorage.get("userPersona");

    //Set the Product Access Title
    $('.assign-product-access-title span').html(name);
    $('#coming-soon').addClass("hidden");

    let product = rolesApiRequirements.find( item => item.name === name);

    if(product) { //if product endpoint exist
        $('#add-roles-settings .tab-pane').addClass('active');
        let accessURL = buildAccessUrl('GET', 'roles', product, userPersona);

        //Pull the roles information for the product
        userAuthAPIService('GET', accessURL,'', 'getRoleData');
    } else {
        $('#add-roles-settings .tab-pane').removeClass('active');
        $('#coming-soon').removeClass("hidden");
    }
};

let populateRightsTable = function(response) {
    let productName = $.sessionStorage.get('selectedProduct');
    let storageRightsData = $.sessionStorage.get("rightsData");

    if(response) {
        if(response.records !== undefined){
            $.sessionStorage.set("rightsData",Object.assign(storageRightsData?storageRightsData : {}, { [productName]: response.records } ));
        } else if(Array.isArray(response)) {// for OneSite product
            $.sessionStorage.set("rightsData", Object.assign(storageRightsData?storageRightsData : {}, { [productName]: response } ));
            // $.sessionStorage.set("rightsData", response);
        } else {
            $.sessionStorage.set("rightsData", Object.assign(storageRightsData?storageRightsData : {}, { [productName]: response.data } ));
        }
    }

    let rightsData = $.sessionStorage.get("rightsData")[productName];
    let datatable = $('#roles-page-rights-datatable table').dataTable();

    //Clear the table before populating it
    datatable.fnClearTable();

    //Populate the table with roles
    rightsData.forEach( right => {
        datatable.fnAddData([
            "<label class=\"md-check dark-bluebox ng-scope\">\n" +
            "            <input type=\"checkbox\" onclick=\"saveSelectedRights(this)\" name=\"" + right.centerName +"\" data-parsley-multiple=\"d-s-c\" value=\"" + right.id + "\" class=\"md-check dark-bluebox\">\n" +
            "            <i class=\"primary\"></i>\n" +
            "        </label>",
            "<div class=\"valign-middle\">\n" + right.centerName + "</div>",
            "<div>\n" + right.description + "</div>",
        ]);
    });

    $('#new-role-create').parsley().reset();
    datatable.fnSetColumnVis( 1, true );

    if(rightsData.every( right => !right.centerName )) { // if centerName is null, hide column
        datatable.fnSetColumnVis( 1, false );
    }
};

let saveSelectedRights = function (currentTarget) {
    if(!currentTarget) {
        currentTarget = this;
    }
    let currentRole = $.sessionStorage.get('currentRole');

    let { id, rights } = currentRole;
    let rightToAdd = {
        id: currentTarget.value,
        checked: currentTarget.checked
    };

    if(rights.length) {
        let right = rights.find(item => {
            if(item.id === currentTarget.value) {
                item.checked = currentTarget.checked;
                return true;
            }
        });
        if(!right) {
            rights.push(rightToAdd);
        }
    } else {
        rights.push(rightToAdd);
    }

    rights = rights.filter( item => item.checked ); //remove all unselected checkboxes

    $.sessionStorage.set('currentRole', { id, rights });

};

//populate table with rights
let getAllRights = () => {
    let productName = $.sessionStorage.get("selectedProduct");
    let rightsData = $.sessionStorage.get("rightsData") ? $.sessionStorage.get("rightsData")[productName] : '';
    let userPersona = $.sessionStorage.get("userPersona");

    if(!rightsData) {
        let productName = $.sessionStorage.get("selectedProduct");
        let product = rolesApiRequirements.find( item => item.name === productName);
        let rightsURL = buildAccessUrl('GET', 'rights', product, userPersona);

        //Pull the roles information for the product
        userAuthAPIService('GET', rightsURL,'', 'populateRightsTable');
    } else {
        populateRightsTable();
    }
};

let getCurrentProductRequirements = function () {
    let productName = $.sessionStorage.get('selectedProduct');
    return rolesApiRequirements.find( item => item.name === productName);
};

//create new role functionality
let createRole = function() {
    let product = getCurrentProductRequirements();
    let newRoleURL = buildAccessUrl('POST', 'roles', product);

    userAuthAPIService('POST', newRoleURL,'', 'updateNewRole');
};

let updateRolesTable = function() {
    selectedProduct($.sessionStorage.get('selectedProduct'));
    closeModalWindow();
};

let openModal = function (id, rights) {// to set new or existing role to sessionStorage
    $('#roles-aside-modal').modal('show');
    id = id ? id : null;
    rights = rights ? rights : [];

    let currentRole = { id, rights };
    $.sessionStorage.set('currentRole', currentRole);
};

//assign rights to new role(PUT)
let updateNewRole = function(response) {
    if(response.isError) {//for Unified Login
        console.log(response.errorReason);
        return;
    }

    let newRole = response.records[0];
    let product = getCurrentProductRequirements();
    let rightsToAdd = $.sessionStorage.get('currentRole').rights.map( item => item.id );

    let data = {
        rightsToAdd,
        rightsToDelete: []
    };
    let assignRightsURL = buildAccessUrl('PUT', 'roles', product, newRole.id || newRole);

    userAuthAPIService('PUT', assignRightsURL, data, 'updateRolesTable');
};

let closeModalWindow = function () {
    $('#new-role-create').parsley().destroy();
    $('#new-role-create input[type="text"]').val('');
    $.sessionStorage.remove('currentRole');
    $('#roles-aside-modal').modal('hide');
};

//EVENT HANDLERS
$(document).ready(function() {
    $('#create-role').click( e => {
        getAllRights();

        openModal();
    });

    $('#new-role-create').submit(function (event) {
        event.preventDefault();
        let rightsCheckboxes = $('#roles-page-rights-datatable input[type="checkbox"]:checked');

        //CUSTOM PARSLEY VALIDATOR
        if(window.ParsleyValidator && !window.ParsleyValidator.validators.multiselection) {
            window.ParsleyValidator.addValidator('multiselection', function(value) {
                let currentRole = $.sessionStorage.get('currentRole');
                let { rights } = currentRole;

                return !!rights.length;
            }).addMessage('en', 'multiselection', 'Please select a right');
        }

        if( $(this).parsley().isValid() ) {
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


