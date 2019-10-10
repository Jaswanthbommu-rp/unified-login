"use strict";

//Check the URL and verify it has the hashtag
var url = window.location.href; // Returns full URL
var path = location.pathname;
var domain = url.split(path)[0];
var hashURL = domain + "/#" + path;

if (url.indexOf('#') === -1 || url !== hashURL && path !== "/") {

    //console.log("url: " + url);
    //console.log("domain: " + domain);
    //console.log("path: " + path);
    //console.log("final URL: " + hashURL);

    //Redirect to hashtag version of URL
    window.location.href = hashURL;
}