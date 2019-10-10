/**
 * Created by mgardner on 3/28/17.
 */
(function(){
    /*
    This function allows for utilizing a button click to show / hide existing tables.  As is being done on
    Communications History
    example:::
     <button type="button" class="table-data-swap-btn" data-open="#comm-hist-table">Line-by-line</button>
     <div class="table-data-swap-loc" id="comm-hist-table">
     */
    $('.table-data-swap-btn').on('click', function(){
        /* fix RAUL bug where there is no logic to add active class */
        $('.table-data-swap-btn').removeClass('active');
        $(this).addClass('active');

        /* reads what the data-open target is, and hides all of the class then opens the appropriate one */
        var thisOpen = $(this).attr('data-open');
        $('.table-data-swap-loc').hide();
        $(thisOpen).show();
    });

    $('.check-all').on('click', function() {
        let nameAtr = $(this).data('check-name');
        let isChecked = $(this).find('input').is(':checked');
        var arrAllChecks = document.getElementsByName(nameAtr);
        for (var i = 0; i < arrAllChecks.length; i++) {
            arrAllChecks[i].checked = isChecked;
        }
    });

    /* throwaway code that will eventually be replaced by whatever language we end up using,
    whether React or Angular */
    $('.dmod-trigger').on('click', function(){
        console.log('dmod-trigger hit');
        var thisMod = $(this).attr('data-modalopen');
        var dataDir = $(thisMod).attr('data-direction');
        console.log('thisMod', thisMod);
        console.log('dataDir', dataDir);
        dmodOpen(thisMod, dataDir);
    });

    function dmodOpen(thisMod, dataDir){
        $('.dmod-overlay').show();
        $('body').addClass('dmod-open');
        switch(dataDir){
            case 'right':
                $(thisMod).show().animate({"right":0}, "fast", function(){
                    $(thisMod).addClass('active');
                });
                break;
        }
    }

    $('.dmod-close').on('click', function(){
        console.log('dmod-close hit');
        var thisParent = $(this).attr('data-ref');
        var dataDir = $(thisParent).attr('data-direction');
        console.log('thisParent', thisParent);
        console.log('dataDir', dataDir);
        dmodClose(thisParent, dataDir);
    });

    function dmodClose(thisParent, dataDir){
        console.log('thisParent', thisParent);
        console.log('dataDir', dataDir);
        $('.dmod-overlay').hide();
        $('body').removeClass('dmod-open');
        switch(dataDir){
            case 'right':
                console.log('case right hit');
                $(thisParent).animate({"right":"-100%"}, "fast", function(){
                    $(thisParent).hide().removeClass('active');
                });
                break;
        }
    }

    $('.dmod-overlay').on('click', function(){
        console.log('dmod-overlay click');
        var openDmod = $('.dmod-modal.active');
        var openDmodId = openDmod.attr('id');
        var dataDir = $('#'+openDmodId).attr('data-direction');

        console.log('openDmod', openDmod);
        console.log('openDmodId', openDmodId);
        console.log('dataDir', dataDir);
        dmodClose('#'+openDmodId, dataDir);
    });

})();