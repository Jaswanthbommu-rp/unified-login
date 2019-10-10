"use strict";

// Javascript for Company Search

///// VARIABLES - BEGIN /////////////

var companies = [{
    "owned": 9580,
    "managed": 1061,
    "postal": 2945,
    "state": "FL",
    "city": "Romeville",
    "address": "651 Vandam Street",
    "name": "BUGSALL",
    "picture": "https://placehold.it/90x90",
    "id": 2868,
    "lat": 33.61791935,
    "long": 143.88346752
}, {
    "owned": 8724,
    "managed": 5157,
    "postal": 4619,
    "state": "HI",
    "city": "Thornport",
    "address": "798 Diamond Street",
    "name": "ZILLAN",
    "picture": "https://placehold.it/90x90",
    "id": 2315,
    "lat": -10.84435568,
    "long": -84.39835741
}, {
    "owned": 3114,
    "managed": 4939,
    "postal": 6576,
    "state": "SC",
    "city": "Cecilia",
    "address": "201 Menahan Street",
    "name": "AFFLUEX",
    "picture": "https://placehold.it/90x90",
    "id": 5280,
    "lat": 25.23260124,
    "long": 131.33173142
}, {
    "owned": 1882,
    "managed": 4392,
    "postal": 2699,
    "state": "LA",
    "city": "Elwood",
    "address": "968 Bijou Avenue",
    "name": "GEEKOLOGY",
    "picture": "https://placehold.it/90x90",
    "id": 3516,
    "lat": -41.66608797,
    "long": -131.01644988
}, {
    "owned": 4419,
    "managed": 8776,
    "postal": 5074,
    "state": "AZ",
    "city": "Dyckesville",
    "address": "239 Seton Place",
    "name": "ENQUILITY",
    "picture": "https://placehold.it/90x90",
    "id": 6879,
    "lat": -33.75562269,
    "long": 54.37805284
}, {
    "owned": 6600,
    "managed": 6896,
    "postal": 1674,
    "state": "AS",
    "city": "Bancroft",
    "address": "303 Langham Street",
    "name": "ESSENSIA",
    "picture": "https://placehold.it/90x90",
    "id": 3821,
    "lat": 69.38913846,
    "long": -84.98623601
}, {
    "owned": 8798,
    "managed": 665,
    "postal": 2845,
    "state": "DE",
    "city": "Fairview",
    "address": "971 Foster Avenue",
    "name": "ZIDOX",
    "picture": "https://placehold.it/90x90",
    "id": 5918,
    "lat": -33.46015453,
    "long": 101.21248538
}, {
    "owned": 6817,
    "managed": 4770,
    "postal": 4288,
    "state": "KY",
    "city": "Westphalia",
    "address": "731 Cypress Court",
    "name": "TELPOD",
    "picture": "https://placehold.it/90x90",
    "id": 3062,
    "lat": 28.15600575,
    "long": 126.2213111
}, {
    "owned": 5980,
    "managed": 894,
    "postal": 3500,
    "state": "MD",
    "city": "Kidder",
    "address": "582 Ashford Street",
    "name": "MAGNAFONE",
    "picture": "https://placehold.it/90x90",
    "id": 5959,
    "lat": 27.09152718,
    "long": 137.89124361
}, {
    "owned": 6443,
    "managed": 5641,
    "postal": 3756,
    "state": "OR",
    "city": "Chumuckla",
    "address": "700 Stratford Road",
    "name": "SECURIA",
    "picture": "https://placehold.it/90x90",
    "id": 6864,
    "lat": -23.91554712,
    "long": -123.26227473
}, {
    "owned": 7745,
    "managed": 3593,
    "postal": 8957,
    "state": "WY",
    "city": "Heil",
    "address": "182 Pitkin Avenue",
    "name": "EXPOSA",
    "picture": "https://placehold.it/90x90",
    "id": 6939,
    "lat": 54.59576058,
    "long": -71.12239237
}, {
    "owned": 1321,
    "managed": 272,
    "postal": 5385,
    "state": "ID",
    "city": "Bowmansville",
    "address": "378 Stewart Street",
    "name": "DATAGENE",
    "picture": "https://placehold.it/90x90",
    "id": 2657,
    "lat": -35.57609394,
    "long": -135.47737652
}, {
    "owned": 5900,
    "managed": 1058,
    "postal": 6125,
    "state": "IA",
    "city": "Sims",
    "address": "769 Vanderveer Street",
    "name": "ICOLOGY",
    "picture": "https://placehold.it/90x90",
    "id": 7407,
    "lat": -47.52343348,
    "long": -13.22539569
}, {
    "owned": 5593,
    "managed": 2193,
    "postal": 2098,
    "state": "VI",
    "city": "Hegins",
    "address": "772 Hutchinson Court",
    "name": "GEOFORMA",
    "picture": "https://placehold.it/90x90",
    "id": 4741,
    "lat": -17.71351647,
    "long": 52.93672567
}];
var map;
var bounds;

///// VARIABLES - END /////////////

///// FUNCTIONS - BEGIN /////////////

var addCompanies = function addCompanies(response) {

    var dataTableCompanyList = $('#company-list-view-datatable table').dataTable();

    $.each(response, function (i, company) {

        dataTableCompanyList.fnAddData(["<div class=\"valign-middle blue-link ft-s-12 \">\n" + "     <a href=\"/companies/" + company.id + "\">\n" + "           " + company.id + "\n" + "    </a>\n" + "</div>", "<div class=\"valign-middle blue-link ft-s-12 \">\n" + "     <a href=\"/companies/" + company.id + "\">\n" + "           " + company.name + "\n" + "    </a>\n" + "</div>", "<div class=\"valign-middle ft-s-12 \">\n" + company.address + "</div>", "<div class=\"valign-middle ft-s-12 \">\n" + company.city + "</div>", "<div class=\"valign-middle ft-s-12 \">\n" + company.state + "</div>", "<div class=\"valign-middle ft-s-12 \">\n" + company.postal + "</div>", "<div class=\"valign-middle ft-s-12 \">\n" + company.managed + "</div>", "<div class=\"valign-middle ft-s-12 \">\n" + company.owned + "</div>"]);
    });

    var dataTableCompanyGrid = $('#company-grid-view-datatable table').dataTable();

    $.each(response, function (i, company) {

        dataTableCompanyGrid.fnAddData(["<div class=\"float-left\">\n" + "            <span class=\"company-photo\">\n" + "                    <img src=\"" + company.picture + "\" /> \n" + "                <i class=\"on b-white bottom\"></i>\n" + "            </span>\n" + "        </div>", "<div class=\"float-left p-l-05 max-width-minus-60\">\n" + "     <a href=\"/companies/" + company.id + "\">\n" + "        <div class=\"ft-s-16 blue-link \">\n" + "           " + company.name + "\n" + "        </div>\n" + "        <div class=\"ft-s-12 \">\n" + "            " + company.address + "\n" + "        </div>\n" + "        <div class=\"ft-s-12 \">\n" + "           " + company.city + "," + company.state + " " + company.postal + "\n" + "        </div>\n" + "        <div class=\"ft-s-12 \">\n" + "            United States" + "\n" + "        </div>\n" + "    </a>\n" + "</div>", "<div class=\"valign-middle ft-s-12 \">\n" + company.id + "</div>", "<div class=\"valign-middle ft-s-12 \">\n" + company.owned + "</div>", "<div class=\"valign-middle ft-s-12 \">\n" + company.managed + "</div>"]);
    });

    dataTableCompanyGrid.fnSort([[0, '']]);

    var dataTableCompanyMap = $('#company-map-view-datatable table').dataTable();

    $.each(response, function (i, company) {

        dataTableCompanyMap.fnAddData(["<div data-location-id=\"" + company.id + "\" class=\"float-left\">\n" + "     <span class=\"company-photo\">\n" + "           <img src=\"" + company.picture + "\" /> \n" + "           <i class=\"on b-white bottom\"></i>\n" + "     </span>\n" + "</div>", "<div class=\"float-left p-l-05 max-width-minus-60\">\n" + "     <a href=\"/companies/" + company.id + "\">\n" + "        <div class=\"ft-s-16 blue-link \">\n" + "           " + company.name + "\n" + "        </div>\n" + "        <div class=\"ft-s-12 \">\n" + "            " + company.address + "\n" + "        </div>\n" + "        <div class=\"ft-s-12 \">\n" + "           " + company.city + "," + company.state + " " + company.postal + "\n" + "        </div>\n" + "        <div class=\"ft-s-12 \">\n" + "            United States" + "\n" + "        </div>\n" + "    </a>\n" + "</div>"]);
    });

    dataTableCompanyMap.fnSort([[0, '']]);
    //Change the page length to 5
    $('#company-map-view-datatable').find('.dataTables_length select').val('5').change();
    //Hide the page length selector
    $('#company-map-view-datatable').find('.dataTables_length').hide();
    //Hide the Info container
    $('#company-map-view-datatable').find('.dataTables_info').hide();
    //Fix padding
    $('#company-map-view-datatable').find('.col-sm-12').addClass('p-x-0');
};

function initMap() {

    // Create a map object and specify the DOM element for display.
    map = new google.maps.Map(document.getElementById('map'), {
        center: { lat: companies[0].lat, lng: companies[0].long },
        zoom: 10
    });

    //Bounds container for Latitude & Longitude
    bounds = new google.maps.LatLngBounds();

    $.each(companies, function (i, company) {

        var myLatLng = { lat: company.lat, lng: company.long };

        // Create a marker and set its position.
        var marker = new google.maps.Marker({
            map: map,
            position: myLatLng,
            title: company.name,
            locationId: company.id

        });

        marker.addListener('click', function () {
            $('#company-map-view-datatable').find("[data-location-id]").closest('tr').removeClass("active");
            $('#company-map-view-datatable').find("[data-location-id='" + company.id + "']").closest('tr').addClass("active");

            //plugin.openInfoWindow(location, gMarker);
            //plugin.map.setCenter(gMarker.getPosition());
        });

        //Add to bounds
        bounds.extend(marker.getPosition());

        //Refit the map to the markers
        map.fitBounds(bounds);
    });
}

///// FUNCTIONS - END /////////////

///// BUTTON CLICK HANDLERS - BEGIN /////////////

//Refresh the Google Map when the tab is selected
$(document).on('click', '.nav-link[data-target="#search-page-map-view"]', function (e) {

    console.log("=========================");
    console.log("REFRESHING THE GOOGLE MAP");
    console.log("=========================");

    google.maps.event.trigger(map, "resize");

    //Refit the map to the markers
    map.fitBounds(bounds);
});

///// BUTTON CLICK HANDLERS - END /////////////


///// FORM SUBMISSION HANDLERS - BEGIN /////////////


///// FORM SUBMISSION HANDLERS - END /////////////