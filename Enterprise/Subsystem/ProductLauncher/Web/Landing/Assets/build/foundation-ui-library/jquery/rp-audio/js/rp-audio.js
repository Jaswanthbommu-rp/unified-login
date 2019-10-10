
$(function() {

    var rpAudio = function(element,options){

        var container = $(element).find('.container');
        var cover = $(element).find('.cover');
        var play = $(element).find('.play');
        var pause = $(element).find('.pause');
        var mute = $(element).find('.mute');
        var muted = $(element).find('.muted');
        var close = $(element).find('.close');
        var song = new Audio(options['audio']);
        var duration = song.duration;
        var currentTime = song.currentTime;
        var setTime;
        var setDuration;

        if (song.canPlayType('audio/mpeg;')) {
            song.type= 'audio/mpeg';
            song.src= options.audio;
            duration = song.duration;
        } else if (song.canPlayType('audio/x-wav;')) {
            song.type= 'audio/x-wav';
            song.src= options.audio;
            duration = song.duration;
        } else {
            song.type= 'audio/ogg';
            song.src= options.audio;
            duration = song.duration;
        }

        play.on('click', function(e) {
            e.preventDefault();

            //Stop any other audio that is currently playing
            $('.rp-audio').find('.pause').click();

            song.play();
            $(this).hide();
            $(element).find('.pause').show();

            $(element).find('.seek').attr('max',song.duration);
            setTime = getCurrentTime();
        });

        pause.on('click', function(e) {
            e.preventDefault();
            song.pause();
            $(this).hide();
            $(element).find('.play').show();
            window.clearInterval(setTime);
        });

        $(element).find(".seek").bind("change", function() {
            song.currentTime = $(this).val();
            $(element).find(".seek").attr("max", song.duration);
        });

        song.addEventListener('timeupdate',function (){
            curtime = parseInt(song.currentTime, 10);
            $(element).find(".seek").val(curtime);
            $(element).find(".currentTime").text(getMinutesSeconds(song.currentTime));
            $(element).find(".duration").text('-' + getMinutesSeconds(song.duration - song.currentTime));
            var percentage = curtime / song.duration;
            $(element).find(".seek").css("background-image","-webkit-gradient(linear,left bottom,right bottom,color-stop("+ percentage +", #ff6437),color-stop("+ percentage +", #cbcbcb)");
        });

        song.addEventListener('ended',function (){
            $(element).find('.pause').click();
            $(element).find(".currentTime").text('0:00');
            $(element).find(".duration").text('-' + getMinutesSeconds(song.duration));
            song.currentTime = 0;
            curtime = song.currentTime;
            $(element).find(".seek").val(curtime);
            $(element).find(".seek").css("background-image","-webkit-gradient(linear,left bottom,right bottom,color-stop(0, #ff6437),color-stop(0, #cbcbcb)");
        });

        function str_pad_left(string,pad,length) {
            return (new Array(length+1).join(pad)+string).slice(-length);
        }

        function getMinutesSeconds(timeInSeconds){
            var minutes = Math.floor(timeInSeconds.toFixed(2) / 60);
            var seconds = timeInSeconds.toFixed(0) - (minutes * 60);
            return str_pad_left(minutes,'0',1)+':'+str_pad_left(seconds,'0',2);
        }

        function getDuration() {
            return window.setInterval( function() {
                $(element).find(".duration").text('-' + getMinutesSeconds(song.duration));
                if (song.duration > 0){
                    window.clearInterval(setDuration);
                }
            }, 100);
        }

        function getCurrentTime() {
            return window.setInterval( function() {
                $(element).find(".currentTime").text(getMinutesSeconds(song.currentTime));
                $(element).find(".duration").text('-' + getMinutesSeconds(song.duration - song.currentTime));
            }, 100);
        };

        $(element).find(".currentTime").text('0:00');
        $(element).find(".duration").text('-0:00');

        setDuration = getDuration();
        pause.hide();

    };

    $.fn.rpAudio = function (options) {

        rpAudio(this,options);

    };

});