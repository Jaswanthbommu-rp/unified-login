// ===== file: tetherWrapper.js =====
require(['core/vendor/jquery-tether/tether'], function(Tether) {
    window.Tether = Tether; // attach to global scope
    // it's important to have this, to keep original module definition approach
    return Tether;
});