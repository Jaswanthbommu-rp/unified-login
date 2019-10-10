'use strict';

// Javascript for People

///// VARIABLES - BEGIN /////////////

var passwordValid = false;
var productName = '';
var initialSection = '';
var selectedAccess = [];
var roleRightsUrl = '';

var productRequirements = [];
productRequirements["OneSite"] = {
    apiName: "onesite/user",
    access: [{ name: "properties", data: "properties", selection: "multi", additionalParameters: "", title: "Properties" }, { name: "roles", data: "roles", selection: "multi", additionalParameters: "", title: "Roles", rightsUrl: "onesite/rights" }],
    status: "active"
};
productRequirements["RealPage Accounting"] = {
    apiName: "onesiteaccounting/user",
    access: [{ name: "properties", data: "properties", selection: "multi", additionalParameters: "", title: "Entities / Groups" }, { name: "roles", data: "roles", selection: "multi", additionalParameters: "", title: "Roles", rightsUrl: "" }],
    status: "active"
};
productRequirements["Spend Management"] = {
    apiName: "ops",
    access: [{ name: "propertyGroups", data: "assets", selection: "single", additionalParameters: "", title: "Property Groups" }, { name: "roles", data: "roles", selection: "single", additionalParameters: "", title: "Roles", rightsUrl: "" }],
    status: "active"
};
productRequirements["Vendor Services"] = {
    apiName: "vendorcompliance",
    access: [{ name: "propertyGroups", data: "propertygroups", selection: "single", additionalParameters: "", title: "Property Groups" }, { name: "properties", data: "properties", selection: "single", additionalParameters: "", title: "Properties" }, { name: "roles", data: "roles", selection: "single", additionalParameters: "accessType=Property", title: "Roles", rightsUrl: "" }, { name: "notifications",
        data: "notifications",
        descriptions: [{ name: "isInsuranceExpired", description: "Notify by email when any vendor's insurance is about to expire." }, { name: "isVendorRecommendationChanges", description: "Notify by email when any vendor's recommendation changes." }],
        additionalParameters: "",
        title: "Notifications"
    }],
    status: "active"
};
productRequirements["Renters Insurance"] = {
    apiName: "rentersinsurance",
    access: [{ name: "properties", data: "properties", selection: "multi", additionalParameters: "", title: "Properties" }, { name: "roles:", data: "roles", selection: "single", additionalParameters: "", title: "Roles", rightsUrl: "" }],
    status: "active"
};
productRequirements["Resident Portal"] = {
    apiName: "residentportal",
    access: [{ name: "properties", data: "properties", selection: "multi", additionalParameters: "", title: "Properties" }, { name: "roles", data: "levels", selection: "single", additionalParameters: "", title: "Roles", rightsUrl: "" }, { name: "messagingGroups", data: "messagegroups", selection: "multi", additionalParameters: "", title: "Messaging Groups" }, { name: "notifications",
        data: "notifications",
        descriptions: [{ name: "managerFdiViaEmail", description: "Front desk instructions." }, { name: "amenitiesViaEmail", description: "New amenity reservation." }, { name: "managerMrViaEmail", description: "Service request submission & updates." }],
        additionalParameters: "",
        title: "Notifications"
    }],
    status: "active"
};
productRequirements["Utility Management"] = {
    apiName: "utilitymanagement",
    access: [],
    status: "inactive"
};
productRequirements["Lead2Lease"] = {
    apiName: "lead2lease",
    access: [{ name: "propertyGroups", data: "properties", selection: "multi", additionalParameters: "", title: "Properties" }, { name: "roles", data: "roles", selection: "multi", additionalParameters: "", title: "Rights", rightsUrl: "" }],
    status: "active"
};
productRequirements["Websites & Syndication"] = {
    apiName: "marketingcenter",
    access: [{ name: "properties", data: "properties", selection: "multi", additionalParameters: "", title: "Properties" }, { name: "roles", data: "roles", selection: "single", additionalParameters: "", title: "Roles", rightsUrl: "" }],
    status: "active"
};
productRequirements["Prospect Contact Center"] = {
    apiName: "prospectcontactcenter",
    access: [{ name: "properties", data: "properties", selection: "multi", additionalParameters: "", title: "Properties" }],
    status: "active"
};
productRequirements["Asset Optimization"] = {
    apiName: "assetoptimization",
    access: [],
    status: "inactive"
};
productRequirements["YieldStar"] = {
    apiName: "yieldstar",
    access: [],
    status: "inactive"
};
productRequirements["Unified Login"] = {
    apiName: "unifiedlogin/user",
    access: [{ name: "roles", data: "roles", selection: "single", additionalParameters: "partyId=userPersona.organizationPartyId", title: "Roles", rightsUrl: "unifiedlogin/role/rights" }],
    status: "active"
};
productRequirements["Client Portal"] = {
    apiName: "clientportal",
    access: [{ name: "properties", data: "properties", selection: "multi", additionalParameters: "", title: "Properties" }, { name: "roles", data: "roles", selection: "single", additionalParameters: "", title: "Roles", rightsUrl: "clientportal/role/rights" }],
    status: "active"
};

///// VARIABLES - END /////////////

///// FUNCTIONS - BEGIN /////////////

var getPropertyData = function getPropertyData(response) {
    console.log("PROPERTY DATA HAS BEEN RETRIEVED");
    console.log("=====================");
    console.log(response);
    console.log("=====================");

    if (typeof selectedAccess[productName]['property']['data'] === 'undefined' || selectedAccess[productName]['property']['data'].length === 0) {
        //Add the data response to the access array
        selectedAccess[productName]['property']['data'] = response.records;
    }

    $.sessionStorage.set("propertyData", selectedAccess[productName]['property']['data']);
    var propertyData = $.sessionStorage.get("propertyData");

    var datatable = $('#property-datatable table').dataTable();

    //Clear the table before populating it
    datatable.fnClearTable();

    //Decide the form input type
    var propertyChecked = "";
    var inputType = "checkbox";
    var inputName = "property[]";
    if (selectedAccess[productName]['property']['access'].selection === "single") {
        inputType = "radio";
        inputName = "property";
    }

    if (propertyData !== null) {
        //Populate the table with properties
        $.each(propertyData, function (i, property) {

            if (property.isAssigned === true) {
                propertyChecked = 'checked=\"checked\"';
            } else {
                propertyChecked = '';
            }

            datatable.fnAddData(["<label class=\"md-check dark-bluebox ng-scope\">\n" + "            <input name=\"" + inputName + "\" type=\"" + inputType + "\" " + propertyChecked + " value=\"" + property.id + "\" class=\"md-check dark-bluebox\">\n" + "            <i class=\"primary\"></i>\n" + "        </label>", "<div class=\"valign-middle\">\n" + property.name + "</div>", "<div class=\"valign-middle\">\n" + property.state + "</div>"]);
        });
    }

    //Remove the sort for the selectable column
    datatable.fnSort([[0, '']]);

    if (initialSection === 'properties-tab') {
        $('#properties-tab a').click();
    }
};

var getRoleData = function getRoleData(response) {
    console.log("ROLE DATA HAS BEEN RETRIEVED");
    console.log("=====================");
    console.log(response);
    console.log("=====================");

    if (typeof selectedAccess[productName]['role']['data'] === 'undefined' || selectedAccess[productName]['role']['data'].length === 0) {
        //Add the data response to the data array
        if (response.records !== undefined) {
            //Add the data response to the access array
            selectedAccess[productName]['role']['data'] = response.records;
        } else {
            //Add the data response to the access array
            selectedAccess[productName]['role']['data'] = response.data;
        }
    }

    $.sessionStorage.set("roleData", selectedAccess[productName]['role']['data']);

    var roleData = $.sessionStorage.get("roleData");

    var datatable = $('#role-datatable table').dataTable();

    //Clear the table before populating it
    datatable.fnClearTable();

    //Decide the form input type
    var inputType = "checkbox";
    var inputName = "role[]";
    if (selectedAccess[productName]['role']['access'].selection === "single") {
        inputType = "radio";
        inputName = "role";
    }

    //Show/Hide Info Icon
    var roleChecked = "";
    var showInfo = 'hidden';
    if (roleRightsUrl.length > 0) {
        //Show icon
        showInfo = 'visible';
    }

    if (roleData !== null) {
        //Populate the table with roles
        $.each(roleData, function (i, role) {

            if (role.roletype === undefined) {
                role.roletype = ' ';
            }

            if (role.isAssigned === true) {
                roleChecked = 'checked=\"checked\"';
            } else {
                roleChecked = '';
            }

            datatable.fnAddData(["<label class=\"md-check dark-bluebox ng-scope\">\n" + "            <input name=\"" + inputName + "\" type=\"" + inputType + "\"  " + roleChecked + " value=\"" + role.id + "\" class=\"md-check dark-bluebox\">\n" + "            <i class=\"primary\"></i>\n" + "        </label>", "<div class=\"valign-middle\">\n" + role.name + "</div>", "<div class=\"valign-middle\">\n" + role.roletype + "</div>", "<div class=\"valign-middle text-right " + showInfo + "\"><span class=\"fa fa-info-circle text-primary\" onclick=\"getRoleRights('" + role.name + "'," + role.id + ")\"></span></div>"]);
        });
    }

    //Remove the sort for the selectable column
    datatable.fnSort([[0, '']]);

    if (initialSection === 'roles-tab') {
        $('#roles-tab a').click();
    }
};

var getPropertyGroupData = function getPropertyGroupData(response) {
    console.log("PROPERTY GROUP DATA HAS BEEN RETRIEVED");
    console.log("=====================");
    console.log(response);
    console.log("=====================");

    if (typeof selectedAccess[productName]['propertyGroup']['data'] === 'undefined' || selectedAccess[productName]['propertyGroup']['data'].length === 0) {
        selectedAccess[productName]['propertyGroup']['data'] = response.records;
    }

    $.sessionStorage.set("propertyGroupData", selectedAccess[productName]['propertyGroup']['data']);
    var propertyGroupData = $.sessionStorage.get("propertyGroupData");

    var datatable = $('#property-group-datatable table').dataTable();

    //Clear the table before populating it
    datatable.fnClearTable();

    //Decide the form input type
    var propertyGroupChecked = "";
    var inputType = "checkbox";
    var inputName = "propertyGroup[]";
    if (selectedAccess[productName]['propertyGroup']['access'].selection === "single") {
        inputType = "radio";
        inputName = "propertyGroup";
    }

    if (propertyGroupData !== null) {
        //Populate the table with roles
        $.each(propertyGroupData, function (i, group) {

            if (group.isAssigned === true) {
                propertyGroupChecked = 'checked=\"checked\"';
            } else {
                propertyGroupChecked = '';
            }

            datatable.fnAddData(["<label class=\"md-check dark-bluebox ng-scope\">\n" + "            <input name=\"" + inputName + "\" type=\"" + inputType + "\"  \" + propertyGroupChecked + \" value=\"" + group.id + "\" class=\"md-check dark-bluebox\">\n" + "            <i class=\"primary\"></i>\n" + "        </label>", "<div class=\"valign-middle\">\n" + group.name + "</div>"]);
        });
    }

    //Remove the sort for the selectable column
    datatable.fnSort([[0, '']]);

    if (initialSection === 'property-groups-tab') {
        $('#property-groups-tab a').click();
    }
};

var getMessageGroupData = function getMessageGroupData(response) {
    console.log("MESSAGE GROUP DATA HAS BEEN RETRIEVED");
    console.log("=====================");
    console.log(response);
    console.log("=====================");

    if (typeof selectedAccess[productName]['messageGroup']['data'] === 'undefined' || selectedAccess[productName]['messageGroup']['data'].length === 0) {
        selectedAccess[productName]['messageGroup']['data'] = response.data;
    }

    $.sessionStorage.set("messageGroupData", selectedAccess[productName]['messageGroup']['data']);
    var messageGroupData = $.sessionStorage.get("messageGroupData");

    var datatable = $('#message-datatable table').dataTable();

    //Clear the table before populating it
    datatable.fnClearTable();

    //Decide the form input type
    var messageGroupChecked = "";
    var inputType = "checkbox";
    var inputName = "messageGroup[]";
    if (selectedAccess[productName]['messageGroup']['access'].selection === "single") {
        inputType = "radio";
        inputName = "messageGroup";
    }

    if (messageGroupData !== null) {
        //Populate the table with roles
        $.each(messageGroupData, function (i, group) {

            if (group.isAssigned === true) {
                messageGroupChecked = 'checked=\"checked\"';
            } else {
                messageGroupChecked = '';
            }

            datatable.fnAddData(["<label class=\"md-check dark-bluebox ng-scope\">\n" + "            <input name=\"" + inputName + "\" type=\"" + inputType + "\"  \" + messageGroupChecked + \" value=\"" + group.id + "\" class=\"md-check dark-bluebox\">\n" + "            <i class=\"primary\"></i>\n" + "        </label>", "<div class=\"valign-middle\">\n" + group.name + "</div>"]);
        });
    }

    //Remove the sort for the selectable column
    datatable.fnSort([[0, '']]);

    if (initialSection === 'property-groups-tab') {
        $('#property-groups-tab a').click();
    }
};

var getNotificationsData = function getNotificationsData(response) {
    console.log("NOTIFICATIONS DATA HAS BEEN RETRIEVED");
    console.log("=====================");
    console.log(response);
    console.log("=====================");

    if (typeof selectedAccess[productName]['notification']['data'] === 'undefined' || selectedAccess[productName]['notification']['data'].length === 0) {
        if (response.data !== undefined) {
            selectedAccess[productName]['notification']['data'] = response.data;
        } else {
            selectedAccess[productName]['notification']['data'] = response;
        }
    }

    $.sessionStorage.set("notificationsData", selectedAccess[productName]['notification']['data']);
    var notificationsData = $.sessionStorage.get("notificationsData");

    var notifications = selectedAccess[productName]['notification']['access'];
    //console.log('NOTIFICATIONS',notifications);
    var notificationDescriptions = notifications.descriptions;
    var descriptionObject;
    console.log('DESCRIPTIONS', notificationDescriptions);

    //Clear the listing before populating it
    $('#notifications > div:not(:first)').remove();

    if (notificationsData !== null) {
        //Populate the table with notifications
        $.each(notificationsData, function (key, notification) {
            var notificationObject = $("#notification-template").clone();
            $(notificationObject).removeClass('hidden');
            $(notificationObject).attr('id', '');
            $(notificationObject).find('input').attr('name', key);
            if (notification === true) {
                $(notificationObject).find('input').attr("checked", "checked");
            }
            descriptionObject = notificationDescriptions.find(function (obj) {
                return obj.name === key;
            });
            $(notificationObject).find('.notification-description').html(descriptionObject.description);
            $('#notifications').append(notificationObject);
        });
    }

    if (initialSection === 'notifications-tab') {
        $('#notifications-tab a').click();
    }
};

var getUserData = function getUserData(response) {
    console.log("GET USER DATA RESPONSE");
    console.log("=====================");
    console.log(response);
    console.log("=====================");

    var userData = response;

    var userbreadCrumb = $("#raul-page-header-breadcrumbs").find(".raul-page-header-breadcrumb").last();
    userbreadCrumb.find("strong").html(userData.data.firstName + " " + userData.data.lastName);

    console.log("GET PERSONA ID");
    console.log("=====================");
    console.log(userData.data.persona[0].personaId);
    console.log("=====================");

    //Add the User Persona Id
    $("#userCreatePage").attr('data-user-persona-id', userData.data.persona[0].personaId);

    //Populate the name of the user
    $('#userCreatePage').find('input[name="first-name"]').val(userData.data.firstName);
    $('#userCreatePage').find('input[name="middle-name"]').val(userData.data.middleName);
    $('#userCreatePage').find('input[name="last-name"]').val(userData.data.lastName);

    //Populate the username of the user
    $('#userCreatePage').find('input[name="username-input"]').val(userData.data.userLogin.loginName);

    //Populate the user type of the user
    $('#userCreatePage').find('input[name="user-type"]').val(userData.data.userTypeId);

    //Populate the User Effective Date of the user
    $('#userCreatePage').find('input[name="user-effective"]').val(moment(userData.data.userLogin.fromDate).format("MM/DD/YYYY"));

    //Populate the User Expiration Date of the user
    if (userData.data.userLogin.thruDate !== null) {
        $('#userCreatePage').find('input[name="user-expires"]').val(moment(userData.data.userLogin.thruDate).format("MM/DD/YYYY"));
    }

    //Populate the User Access toggle for the user
    $('#userCreatePage').find('input[name="user-details-enable-access"]').val(userData.data.userLogin.status);

    //Pull the Product Family data
    userAuthAPIService('GET', 'api/productFamilies?personRealPageId=' + $('#userCreatePage').attr('data-user-id'), '', 'rpGetProducts');
};

var addUserAccount = function addUserAccount(response) {

    console.log("ADDING THE USER ACCOUNT");
    console.log("=====================");
    console.log(response);
    console.log("=====================");

    // Create JSON and call API
    var createUserJSON = {
        firstName: $("#first-name").val(),
        lastName: $("#last-name").val(),
        middleName: $("#middle-initial").val(),
        notificationEmail: $("#notification-email-input").is(":visible") ? $("#notification-email-input").val() : "",
        password: $("#user-details-password").is(":visible") ? $("#user-details-password").val() : "",
        persona: [],
        realPageId: "",
        userTypeId: $("#user-type").val(),
        userLogin: {
            thruDate: $("#user-expires").val(),
            loginName: $("#username-input").val(),
            isActive: $('#user-details-enable-access').is(':checked'),
            fromDate: $("#user-effective").val(),
            is3rdPartyIDP: false
        },
        productBatch: []
    };

    //Make call to the API
    userAuthAPIService('POST', 'api/user', createUserJSON, 'rpCreateUser');
};

var checkShowTabs = function checkShowTabs() {
    if (!$('#user-details-enable-access').is(':checked') || $("#user-type").val() == 402) {
        return $("#product-access-link").hide();
    } else {
        return $("#product-access-link").show();
    }
};

var rpCreateUser = function rpCreateUser(response) {
    if (response.status.success) {
        console.log("User was created successfully!");
        window.location.href = "/people";
    } else {
        alert("User creation failed : " + response.status.errorMsg);
    }
};

var rpGetUserTypes = function rpGetUserTypes(response) {
    $.each(response.data, function (i, item) {
        if (item.partyRoleTypeId === 401) {
            item.value = 401;
            item.text = "Regular User";
        } else if (item.partyRoleTypeId === 402) {
            item.value = 402;
            item.text = "RealPage System Administrator";
        } else if (item.partyRoleTypeId === 404) {
            item.value = 404;
            item.text = "Regular User (no email)";
        }
        $('#user-type').append($('<option>', {
            value: item.value,
            text: item.text
        }));
    });
    $('#user-type').val(401);
};

var selectedProduct = function selectedProduct(name) {

    console.log("SELECTED PRODUCT IS " + name);
    console.log("PRODUCT REQUIREMENTS", productRequirements[name]);

    productName = name;

    if (typeof selectedAccess[productName] === 'undefined') {
        selectedAccess[productName] = {};
    }

    var userPersona = $.sessionStorage.get("userPersona");
    console.log("USER PERSONA", userPersona);

    //User Being Created
    var userIdEdited = 0;
    // If Edited, this would be the id of the user being edited
    if ($("#userCreatePage").attr('data-user-persona-id').length > 0) {
        userIdEdited = $("#userCreatePage").attr('data-user-persona-id');
    }

    //Set the Product Access Title
    $('.assign-product-access-title span').html(name);

    //Reset the tab view
    $('#add-product-tabs .nav-item').hide();
    $('#add-product-tabs .nav-link').removeClass('active');
    $('#add-product-settings .tab-pane').removeClass('active');

    var sectionsInitialized = 0;

    if (productRequirements[name].access.length > 0) {
        $('#coming-soon').addClass("hidden");

        var accessURL;

        //Populate the table with results
        $.each(productRequirements[name].access, function (i, access) {

            if (access.name === "propertyGroups") {
                //Show the Properties Tab
                $('#property-groups-tab').show();
                $('#property-groups-tab a').html(access.title);

                //Count Section Setup
                sectionsInitialized++;
                if (sectionsInitialized === 1) {
                    initialSection = 'property-groups-tab';
                }

                if (typeof selectedAccess[productName]['propertyGroup'] === 'undefined') {
                    selectedAccess[productName]['propertyGroup'] = [];
                }
                selectedAccess[productName]['propertyGroup']['access'] = access;

                accessURL = buildAccessUrl(productRequirements[name].apiName, access.data, access.additionalParameters, userPersona, userIdEdited);

                //Pull the property group information for the product
                userAuthAPIService('GET', accessURL, '', 'getPropertyGroupData');
            }

            if (access.name === "properties") {
                //Show the Properties Tab
                $('#properties-tab').show();
                $('#properties-tab a').html(access.title);

                //Count Section Setup
                sectionsInitialized++;
                if (sectionsInitialized === 1) {
                    initialSection = 'properties-tab';
                }

                if (typeof selectedAccess[productName]['property'] === 'undefined') {
                    selectedAccess[productName]['property'] = [];
                }
                selectedAccess[productName]['property']['access'] = access;

                accessURL = buildAccessUrl(productRequirements[name].apiName, access.data, access.additionalParameters, userPersona, userIdEdited);

                //Pull the property information for the product
                userAuthAPIService('GET', accessURL, '', 'getPropertyData');
            }

            if (access.name === "roles") {
                //Show the Properties Tab
                $('#roles-tab').show();
                $('#roles-tab a').html(access.title);

                //Count Section Setup
                sectionsInitialized++;
                if (sectionsInitialized === 1) {
                    initialSection = 'roles-tab';
                }

                if (typeof selectedAccess[productName]['role'] === 'undefined') {
                    selectedAccess[productName]['role'] = [];
                }
                selectedAccess[productName]['role']['access'] = access;

                //Get Access URL
                accessURL = buildAccessUrl(productRequirements[name].apiName, access.data, access.additionalParameters, userPersona, userIdEdited);

                //Pull the role information for the product
                userAuthAPIService('GET', accessURL, '', 'getRoleData');

                if (access.rightsUrl.length > 0) {
                    //Build out the Rights endpoint
                    roleRightsUrl = buildRightsUrl(access.rightsUrl, access.additionalParameters, userPersona);
                } else {
                    roleRightsUrl = "";
                }
            }

            if (access.name === "messagingGroups") {
                //Show the Properties Tab
                $('#message-groups-tab').show();
                $('#message-groups-tab a').html(access.title);

                //Count Section Setup
                sectionsInitialized++;
                if (sectionsInitialized === 1) {
                    initialSection = 'message-groups-tab';
                }

                if (typeof selectedAccess[productName]['messageGroup'] === 'undefined') {
                    selectedAccess[productName]['messageGroup'] = [];
                }
                selectedAccess[productName]['messageGroup']['access'] = access;

                accessURL = buildAccessUrl(productRequirements[name].apiName, access.data, access.additionalParameters, userPersona, userIdEdited);

                //Pull the messaging group information for the product
                userAuthAPIService('GET', accessURL, '', 'getMessageGroupData');
            }

            if (access.name === "notifications") {
                //Show the Properties Tab
                $('#notifications-tab').show();
                $('#notifications-tab a').html(access.title);

                //Count Section Setup
                sectionsInitialized++;
                if (sectionsInitialized === 1) {
                    initialSection = 'notifications-tab';
                }

                if (typeof selectedAccess[productName]['notification'] === 'undefined') {
                    selectedAccess[productName]['notification'] = [];
                }
                selectedAccess[productName]['notification']['access'] = access;

                accessURL = buildAccessUrl(productRequirements[name].apiName, access.data, access.additionalParameters, userPersona, userIdEdited);

                //Pull the property information for the product
                userAuthAPIService('GET', accessURL, '', 'getNotificationsData');
            }
        });
    } else {
        $('#coming-soon').removeClass("hidden");
    }
};

var buildAccessUrl = function buildAccessUrl(api, data, parameters, persona, editedUserId) {
    if (parameters.length > 0) {

        if (parameters.indexOf('userPersona.organizationPartyId') >= 0) {
            //REPLACING THE STRING
            parameters = parameters.replace('userPersona.organizationPartyId', persona.organizationPartyId);
        }

        return 'api/products/' + api + '/' + data + '?' + parameters + '&assignedOnly=false&editorPersonaId=' + persona.personaId + '&userPersonaId=' + editedUserId;
    } else {
        return 'api/products/' + api + '/' + data + '?assignedOnly=false&editorPersonaId=' + persona.personaId + '&userPersonaId=' + editedUserId;
    }
};

var buildRightsUrl = function buildRightsUrl(api, parameters, persona) {
    if (parameters.length > 0) {

        if (parameters.indexOf('userPersona.organizationPartyId') >= 0) {
            //REPLACING THE STRING
            parameters = parameters.replace('userPersona.organizationPartyId', persona.organizationPartyId);
        }
        return 'api/products/' + api + '?' + parameters + '&assignedToRoleOnly=true&editorPersonaId=' + persona.personaId;
    } else {
        return 'api/products/' + api + '?assignedToRoleOnly=true&editorPersonaId=' + persona.personaId;
    }
};

var getRoleRights = function getRoleRights(name, roleId) {
    //Pull the prights for a specific role
    userAuthAPIService('GET', roleRightsUrl + '&roleId=' + roleId, '', 'showRightsData');

    //Update the modal header
    $('#rights-modal').find('.role-name').html(name);
};

var showRightsData = function showRightsData(response) {
    console.log("RIGHTS DATA HAS BEEN RETRIEVED");
    console.log("=====================");
    console.log(response);
    console.log("=====================");

    //Show the modal
    $('#rights-modal').modal('show');

    //Populate the Datatable
    var datatable = $('#rights-datatable table').dataTable();

    //Clear the table before populating it
    datatable.fnClearTable();

    if (response !== null) {
        //Populate the table with roles
        $.each(response.records, function (i, record) {

            datatable.fnAddData(["<div class=\"\">\n" + record.description + "</div>"]);
        });
    }

    //Hide the record length selector
    $('#rights-datatable').find('.dataTables_length').hide();

    //Push the record count to the left side
    $('#rights-datatable').find('.dataTables_info').css('float', 'left');
    $('#rights-datatable').find('.dataTables_info').css('margin-top', '4px');
};

var validatePassword = function validatePassword() {
    var isValid = true,
        minChars = 8,
        maxChars = 20,
        uppercaseChars = /[A-Z]+/,
        lowercaseChars = /[a-z]+/,
        numChars = /[0-9]+/,
        specialChars = /[!"#\$%&'\(\)\*\+,-\.\/\:\;<=>\?@\[\\\]\^_`\{\|\}~ ]/;

    if ($("#user-details-password").val() === "") {
        isValid = false;
        $("#user-details-password").addClass("create-user-input-error");
        $("#password-error").text("Password is required");
    } else if ($("#user-details-password").val().length < 8 || $("#user-details-password").val().length > 20 || !uppercaseChars.test($("#user-details-password").val()) || !lowercaseChars.test($("#user-details-password").val()) || !numChars.test($("#user-details-password").val()) || !specialChars.test($("#user-details-password").val())) {
        isValid = false;
        $("#user-details-password").addClass("create-user-input-error");
        $("#password-error").text("Password does not meet requirements");
    } else {
        $("#user-details-password").removeClass("create-user-input-error");
        $("#password-error").text("");
    }

    return isValid;
};

var validateCheckPassword = function validateCheckPassword() {
    var isValid = true;

    if ($("#re-enter-password").val() === "") {
        isValid = false;
        $("#re-enter-password").addClass("create-user-input-error");
        $("#re-enter-password-error").text("Please re-enter password");
    } else if ($("#user-details-password").val() !== $("#re-enter-password").val()) {
        isValid = false;
        $("#re-enter-password").addClass("create-user-input-error");
        $("#re-enter-password-error").text("The passwords you typed do not match. Please try again");
    } else {
        $("#re-enter-password").removeClass("create-user-input-error");
        $("#re-enter-password-error").text("");
    }

    return isValid;
};

/*var passwordModalFix = function() {
    var popover = $("#user-details-password").data("bs.popover");
    popover.config.content = $(".pass-req").get(0).outerHTML;
};*/

///// FUNCTIONS - END /////////////

///// BUTTON CLICK HANDLERS - BEGIN /////////////

// User Type Dropdown
// People Add Page
$("#user-type").change(function () {
    if ($("#user-type").val() == 401) {
        // Regular User
        checkShowTabs();
        $("#username").text('Email (Username)');
        $("#password-fields").hide();
        $("#user-details-password").attr('type', 'hidden');
        $("#re-enter-password").attr('type', 'hidden');
        $('#notification-email').hide();
    } else if ($("#user-type").val() == 402) {
        // Realpage System Administrator
        checkShowTabs();
        $("#username").text('Email (Username)');
        $("#password-fields").hide();
        $("#user-details-password").attr('type', 'hidden');
        $("#re-enter-password").attr('type', 'hidden');
        $('#notification-email').hide();
    } else if ($("#user-type").val() == 404) {
        // Regular User (No-Email)
        checkShowTabs();
        $("#username").text('Username');
        $("#password-fields").show();
        $("#user-details-password").attr('type', 'password');
        $("#re-enter-password").attr('type', 'password');
        $('#notification-email').show();
    }
});

$('#user-details-enable-access').click(function () {
    checkShowTabs();
});

$("#user-details-password").keyup(function () {
    var passwordVal = $("#user-details-password").val();
    // Check the length
    if (passwordVal.length >= 8 && passwordVal.length <= 20) {
        $("#password-characters").addClass("valid");
        //passwordModalFix();
        passwordValid = true;
    } else {
        $("#password-characters").removeClass("valid");
        //passwordModalFix();
        passwordValid = false;
    }
    // Check for Upper Character
    var pattern = /[A-Z]+/;
    if (passwordVal.match(pattern)) {
        $("#password-upper").addClass("valid");
        //passwordModalFix();
        passwordValid = true;
    } else {
        $("#password-upper").removeClass("valid");
        //passwordModalFix();
        passwordValid = false;
    }
    // Check for Lower Character
    pattern = /[a-z]+/;
    if (passwordVal.match(pattern)) {
        $("#password-lower").addClass("valid");
        //passwordModalFix();
        passwordValid = true;
    } else {
        $("#password-lower").removeClass("valid");
        //passwordModalFix();
        passwordValid = false;
    }
    // Check for Number Character
    pattern = /[0-9]+/;
    if (passwordVal.match(pattern)) {
        $("#password-number").addClass("valid");
        //passwordModalFix();
        passwordValid = true;
    } else {
        $("#password-number").removeClass("valid");
        //passwordModalFix();
        passwordValid = false;
    }
    // Check for Special Character
    pattern = /[!"#\$%&'\(\)\*\+,-\.\/\:\;<=>\?@\[\\\]\^_`\{\|\}~ ]/;
    if (passwordVal.match(pattern)) {
        $("#password-special").addClass("valid");
        //passwordModalFix();
        passwordValid = true;
    } else {
        $("#password-special").removeClass("valid");
        //passwordModalFix();
        passwordValid = false;
    }
});

//Account for options that are selected or deselected on the page and update the data object that will be submitted
$(document).on('click', '#product-access-options .md-check input[type=checkbox]', function (e) {

    var optionId = $(e.target).attr('value');
    var isSelected = !$(e.target).is(':not(:checked)');
    var accessType = $(e.target).attr('name').replace("[]", "");
    var arrayIndex = selectedAccess[productName][accessType]['data'].findIndex(function (option) {
        return option.id == optionId;
    });

    console.log("CAPTURE THE SELECTION AND UNSELECTION OF PRODUCTS ACCESS OPTIONS");
    console.log("========================================");
    console.log("Option Id: " + optionId);
    console.log("Is Selected: " + isSelected);
    console.log("Access Type: " + accessType);
    console.log("Array Index: " + arrayIndex);
    console.log("========================================");

    selectedAccess[productName][accessType]['data'][arrayIndex].isAssigned = isSelected;

    console.log("====== RECORD AFTER CHANGE =========");
    console.log(selectedAccess[productName][accessType]['data'][arrayIndex]);
    console.log("========================================");
});

///// BUTTON CLICK HANDLERS - END /////////////


///// FORM SUBMISSION HANDLERS - BEGIN /////////////

// 'Add Person' Form Handler
// People Add Page
$("#userCreatePage").submit(function (event) {
    event.preventDefault();
    if ($(this).parsley().isValid()) {
        var data = createDataObject($(this));
        addUserAccount(data);
    }
});

///// FORM SUBMISSION HANDLERS - END /////////////