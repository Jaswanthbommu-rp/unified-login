"use strict";

/*jshint esversion: 6 */

var data = {
    "searchText": "email",
    "totalResultsFound": 70,
    "drawPage": 1,
    "totalNumberPages": 3,
    "searchResults": [{
        "pageId": 17,
        "parentPageId": 5,
        "pageName": "Identity Provider",
        "breadCrumb": "General Setup / System & Security / Identity Provider",
        "description": "This page work email related settings."
    }, {
        "pageId": 23,
        "parentPageId": 15,
        "pageName": "Unified Platform Setup",
        "breadCrumb": "General Setup / System & Security / Unified Platform Setup",
        "description": "This page work email related settings."
    }, {
        "pageId": 20,
        "parentPageId": 95,
        "pageName": "Video Tutorial",
        "breadCrumb": "General Setup / Help & Training / Video Tutorial",
        "description": "This page work email related settings."
    }, {
        "pageId": 1,
        "parentPageId": 5,
        "pageName": "Identity Provider1",
        "breadCrumb": "General Setup / System & Security / Identity Provider",
        "description": "This page work email related settings."
    }, {
        "pageId": 2,
        "parentPageId": 15,
        "pageName": "Unified Platform Setup2",
        "breadCrumb": "General Setup / System & Security / Unified Platform Setup",
        "description": "This page work email related settings."
    }, {
        "pageId": 21,
        "parentPageId": 95,
        "pageName": "Video Tutorial3",
        "breadCrumb": "General Setup / Help & Training / Video Tutorial",
        "description": "This page work email related settings."
    }]
};

var populateSerchResults = function populateSerchResults(data) {
    var company = $.sessionStorage.get('company');
    var CompanyId = company.CompanyId,
        CompanyName = company.CompanyName,
        search = company.search; //remove after real ApI call wil done

    var searchText = data.searchText,
        totalResultsFound = data.totalResultsFound,
        drawPage = data.drawPage,
        totalNumberPages = data.totalNumberPages,
        searchResults = data.searchResults;

    var resultsWrapper = $('#search-settings-results');
    var pagination = resultsWrapper.find('.pagination-wrapper');
    var table = resultsWrapper.find('table');

    //populate page with the data
    $('#search-results-header').html("<strong>" + totalResultsFound + "</strong> results found for <strong>" + search + "</strong>");
    $('#search-result-bar').find('input').val(search);

    pagination.find('.pagination-total-pages').text(" " + totalNumberPages);
    pagination.find('.pagination-page-number').attr('max', totalNumberPages);
    pagination.find('.pagination-page-number').val(drawPage);

    resultsWrapper.find('.content-loader-wrapper').addClass('hidden');
    pagination.removeClass('hidden');
    table.removeClass('hidden');

    var tableBody = table.find('tbody');
    var settingsHtml = '';

    searchResults.forEach(function (result) {
        settingsHtml += "<tr>\n                                     <td class=\"p-0\">\n                                        <div class=\"table-item-details text-left py-4\">\n                                            <a class=\"text-decoration-none text-primary p-1 ft-s-16\" href=\"\">" + result.pageName + "</a>\n                                            <div class=\"raul-list-item-second-line p-1 ft-s-12\">" + result.breadCrumb + "</div>\n                                            <div class=\"search-result-description pt-1 px-1\">" + result.description + "</div>\n                                        </div>\n                                     </td>\n                                   </tr>";
    });

    tableBody.html(settingsHtml);
};

var clearResultsTable = function clearResultsTable() {
    var resultsWrapper = $('#search-settings-results');
    //let pagination = resultsWrapper.find('.pagination-wrapper'); ???
    var table = resultsWrapper.find('table');

    resultsWrapper.find('.content-loader-wrapper').removeClass('hidden');
    // pagination.addClass('hidden');
    table.addClass('hidden');
};

$(document).ready(function () {
    var settingsPages = $.sessionStorage.get('settingsPages');
    var company = $.sessionStorage.get('company');
    var CompanyId = company.CompanyId,
        CompanyName = company.CompanyName,
        search = company.search;


    Promise.resolve(settingsPages).then(function (settingsPages) {

        //Pull the Settings pages data
        return settingsPages ? settingsPages : userAPIService('GET', '', 'api/Pages', '');
    }).then(function (response) {
        return rpSaveSettingsPages(response);
    }).then(function (settingsPages) {
        //get search-results page by id

        // get Settings list from API
        return Promise.resolve(data); //replace with real API call
        /*let searchPageId = settingsPages.find(page => page.name.toLowerCase() === 'search results').id;
        let url = `api/Pages/${searchPageId}?companyId=${CompanyId}`;
          return userAPIService('GET', '', url, '');*/
    }).then(function (data) {
        populateSerchResults(data);
    }).catch(function (err) {
        //add error handler  
        console.log(err);
    });

    //EVENT Handlers

    $('#search-result-bar').submit(function (e) {
        e.preventDefault();

        var form = $(this);

        if (form.parsley().isValid) {
            var searchQuery = form.find('input').val();

            window.location.href = "/setting/searchresults" + window.location.search + "&search=" + searchQuery;
        }
    });

    $('#search-results-left').click(function (e) {
        var pageField = $('.pagination-page-number');
        var currentPage = +pageField.val();

        if (currentPage <= 1) return;

        var nextPage = currentPage - 1;

        pageField.val(nextPage);
        clearResultsTable();

        Promise.resolve(data) // replace with API call
        .then(function (data) {
            //populateSerchResults(data);
        }).catch(function (err) {
            console.log(err);
        });
    });

    $('#search-results-right').click(function (e) {
        var pageField = $('.pagination-page-number');
        var currentPage = +pageField.val();
        var maxValue = +pageField.attr('max');

        if (currentPage >= maxValue) return;

        var nextPage = currentPage + 1;

        pageField.val(nextPage);
        clearResultsTable();

        Promise.resolve(data) // replace with API call
        .then(function (data) {
            //populateSerchResults(data);
        }).catch(function (err) {
            console.log(err);
        });
    });

    $('.pagination-page-number').bind('input change', function (e) {
        var nextPage = $(this).val();
        console.log('nextPage', nextPage);

        if (nextPage) {
            clearResultsTable();

            Promise.resolve(data) // replace with API call
            .then(function (data) {
                //populateSerchResults(data);
            }).catch(function (err) {
                console.log(err);
            });
        }
    });

    $("[type='number']").bind('keypress keyup', function (e) {
        e.preventDefault();
    });
});