// Javascript for People

///// VARIABLES - BEGIN /////////////

var dataTable;

///// VARIABLES - END /////////////

///// FUNCTIONS - BEGIN /////////////

var getPeopleData = function(response){
    console.log("PEOPLE DATA HAS BEEN RETRIEVED");
    console.log("=====================");
    console.log(response);
    console.log("=====================");

    $.sessionStorage.set("peopleData",response.data);

    dataTable = $('#people-datatable table').dataTable();

    if($.sessionStorage.get("peopleData") !== null) {
        //Populate the radius table with results
        $.each($.sessionStorage.get("peopleData"), function (i, person) {

            var loginDate = "";
            var loginTime = "";
            var statusClass = "expired-acct";
            var userIcon;
            var userRoleClass = person.userLogin.isSuperUser ? 'user-role' : '';
            var isLockedClass = person.userLogin.isLocked ? 'locked-user' : '';

            if (person.userLogin.lastLogin) {
                loginDate = moment(person.userLogin.lastLogin).format('MMMM Do YYYY');
                loginTime = moment(person.userLogin.lastLogin).format('h:mm A');
            }

            switch (person.userLogin.status) {
                case 'Active':
                    statusClass = "active-acct";
                    break;
                case 'Disabled':
                    statusClass = "disabled-acct";
                    break;
                case 'Pending':
                    statusClass = "pending-acct";
                    break;
            }

            if (person.avatar !== null && person.avatar !== undefined) {
                userIcon = "<div class=\"rp-avatar user-img-wrapper-small\"></div>";
            } else {
                userIcon = "<div class=\"rp-people-initials rp-initials text-blue-gray\">" + person.firstName.charAt(0) + person.lastName.charAt(0) + "</div>";
            }

            var disableActionVisible = "visible";

            var lockAction = "Locked";
            var lockMessage = "Lock";
            var disableAction = "Disabled";
            var disableMessage = "Disable";

            if(person.userLogin.isLocked === true){
                lockAction = "Unlocked";
                lockMessage = "Unlock";
            }

            if(person.userLogin.isActive === false){
                disableAction = "Active";
                disableMessage = "Activate";
            }

            if(person.userLogin.status === "Expired"){
                disableActionVisible = "hidden";
            }

            dataTable.fnAddData([
                "<label class=\"md-check dark-bluebox ng-scope\">\n" +
                "            <input type=\"checkbox\" class=\"md-check dark-bluebox\">\n" +
                "            <i class=\"primary\"></i>\n" +
                "        </label>",
                "<div class=\"float-left\">\n" +
                "            <span class=\"user-photo\">\n" +
                "                    " + userIcon + "\n" +
                "                <i class=\"on b-white bottom\"></i>\n" +
                "            </span>\n" +
                "        </div>\n" +
                "        <div class=\"float-left p-l-05 max-width-minus-60\">\n" +
                "            <a href=\"/people/" + person.userLogin.realPageId + "\">\n" +
                "                <div class=\"_500 blue-link " + userRoleClass + "\">\n" +
                "                    " + person.firstName + " " + person.lastName + "\n" +
                "                </div>\n" +
                "                <div>\n" +
                "                    " + person.userLogin.loginName + "\n" +
                "                </div>\n" +
                "            </a>\n" +
                "        </div>",
                "<div class=\"valign-middle blue-link ft-s-16 _500 \">\n" + person.summaryCounts.products + "</div>",
                "<div class=\"valign-middle ft-s-12 \">\n" + loginDate + "</br>" + loginTime + "</div>",
                "<div class=\"account-status-label ft-s-12 _500 " + statusClass + "\">\n" +
                "            " + person.userLogin.status + "\n" +
                "        </div>\n",
                "<div class=\"locked-user-label valign-middle " + isLockedClass + "\">\n" + "</div>\n",
                "<div class=\"valign-middle\">\n" +
                "        <div class=\"rp-actions-menu blue-link ft-s-24 rp-icon-more\" data-toggle=\"dropdown\"></div>\n" +
                "        <ul class=\"dropdown-menu max-width-100\" style=\"right:0;left:auto;\">\n" +
                "            <a class=\"dropdown-item\" href=\"/people/" + person.userLogin.realPageId + "\">\n" +
                "                <span>Edit</span>\n" +
                "            </a>\n" +
                "            <a class=\"dropdown-item\" onclick=\"changeUserStatus(event, '" + person.realPageId + "')\" data-status=\"" + lockAction + "\">\n" +
                "                <span>" + lockMessage + "</span>\n" +
                "            </a>\n" +
                "            <a class=\"dropdown-item " + disableActionVisible + "\" onclick=\"changeUserStatus(event, '" + person.realPageId + "')\" data-status=\"" + disableAction + "\">\n" +
                "                <span>" + disableMessage + "</span>\n" +
                "            </a>\n" +
                "        </ul>\n" +
                "    </div>"
            ]);

        });
    }

    dataTable.fnSort([ [0, ''], [5, ''], [6, ''] ]);

};

var changeUserStatus = function(event, rpId) {

    var status = $(event.target).closest('.dropdown-item').attr('data-status');

    console.log("CHANGE THE STATUS OF " + rpId + " TO: " + status);
    console.log("EVENT OBJECT",event);
    console.log("=====================");

    var updateStatusAction;
    var updateStatusContent;

    switch (status) {
        case 'Disabled':
            updateStatusAction = "Active";
            updateStatusContent = "Activate";
            $(event.target).closest('tr').find('.account-status-label').removeClass('active-acct');
            $(event.target).closest('tr').find('.account-status-label').addClass('disabled-acct');
            $(event.target).closest('tr').find('.account-status-label').html('Disabled');
            break;
        case 'Active':
            updateStatusAction = "Disabled";
            updateStatusContent = "Disable";
            $(event.target).closest('tr').find('.account-status-label').removeClass('disabled-acct');
            $(event.target).closest('tr').find('.account-status-label').addClass('active-acct');
            $(event.target).closest('tr').find('.account-status-label').html('Active');
            break;
        case 'Locked':
            updateStatusAction = "Unlocked";
            updateStatusContent = "Unlock";
            $(event.target).closest('tr').find('.locked-user-label').addClass('locked-user');
            break;
        case 'Unlocked':
            updateStatusAction = "Locked";
            updateStatusContent = "Lock";
            $(event.target).closest('tr').find('.locked-user-label').removeClass('locked-user');
    }

    //Update the DOM elements for the user status this is being changed
    $(event.target).html(updateStatusContent);
    $(event.target).closest('.dropdown-item').attr('data-status', updateStatusAction);

    //Make service call to update user status
    userAuthAPIService('POST','api/userlogin/status?realPageId='+ rpId +'&statusTypeName='+ status, '', 'userStatusChanged');

}

var userStatusChanged = function(response){
    console.log("CHANGED THE USER STATUS");
    console.log("=====================");
    console.log(response);
    console.log("=====================");
}

///// FUNCTIONS - END /////////////

///// BUTTON CLICK HANDLERS - BEGIN /////////////

$('#people-datatable .select-all').click(function () {
    $('#people-datatable input[type="checkbox"]').prop('checked', this.checked);
});

///// BUTTON CLICK HANDLERS - END /////////////


///// FORM SUBMISSION HANDLERS - BEGIN /////////////


///// FORM SUBMISSION HANDLERS - END /////////////


