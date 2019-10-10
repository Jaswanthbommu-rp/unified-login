;(function() {
  window.SIMON_API_BASE = 'https://simonlearn-dev.corp.realpage.com/api';

  window.SIMON_MOCK_DATA = {
    title: 'Meet Simon',
    description: 'Find out how Simon makes it easy to learn how to navigate, use, and stay up to date with the RealPage Unified Platform.',
    videoLength: 1487146670,
    videoUrl: 'https://learning.realpage.com/downloads/Simon/Simon_UnifiedLogin.mp4',
  };

  window.SIMON_MOCK_SETTINGS = {
    settingId: 0,
    showVideoForNewUser: true,
    allowSkipVideos: true,
    skipVideoAfterPercentage: 50,
    markVideoCompleteAfterPercentage: 90,
    companyId: '123456789',
  };

  var TICKS_TO_SECONDS_DIVIDER = 10000000;

  var utils = {
    throttle: function(callback, limit) {
      var wait = false;

      return function () {
        if (!wait) {
          callback.apply(this, arguments);

          wait = true;

          setTimeout(function () {
            wait = false;
          }, limit);
        }
      }
    },

    once: function(callback) {
      var called = false;
      var context = this;

      return function() {
        if (called) return;
        callback.apply(context, arguments);
        called = true;
      }
    },

    secondsToTicks: function(seconds) {
      return seconds * TICKS_TO_SECONDS_DIVIDER;
    },

    ticksToSeconds: function(ticks) {
      if (ticks === 0) return 0;
      return ticks / TICKS_TO_SECONDS_DIVIDER;
    },
  };

  var templates = {
    modal: function(title, description) {
      return (
        '<div class="simon-modal modal fade bd-example-modal-lg" tabindex="-1" role="dialog" aria-hidden="true">' +
          '<div class="modal-dialog modal-lg">' +
            '<div class="modal-content">' +
              '<div class="modal-header">' +
                '<h4 class="modal-title" id="exampleModalLongTitle" style="font-size:24px;">' + title + '</h5>' +
              '</div>' +
              '<div class="modal-body">' +
                '<p class="video-description">' + description + '</p>' +
                '<div class="video-disclaimer">' +
                  '<p class="video-disclaimer-header"><strong>Why am I seeing this?</strong></p>' +
                  '<p class="video-disclaimer-text">' +
                    'This video is designed to help you learn how to use this product effectively. Your company requires that all new users watch this tutorial.' +
                  '</p>' +
                '</div>' +
              '</div>' +
            '</div>' +
          '</div>' +
        '</div>'
      );
    },

    closeButton: function() {
      return (
        '<button type="button" class="close" data-dismiss="modal" aria-label="Close" style="margin-top:5px;">' +
          '<span aria-hidden="true" style="font-size:28px;">&times;</span>' +
        '</button>'
      );
    },
  };

  function generateVideo() {
    var video = document.createElement('video');

    video.style.width = '100%';
    video.style.height = 'auto';

    video.controls = 'controls';
    video.autoplay = false;

    return video;
  }

  window.StaticVideoPlayer = (function() {
    function StaticVideoPlayer(options) {
      this.title = options.title || '';
      this.description = options.description || '';
      this.src = options.src || '';
      this.video = generateVideo();

      var instance = this;

      var modalTemplate = templates.modal(
        this.title,
        this.description
      );

      this.modal = jQuery(modalTemplate);
      this.modal.find('.modal-header').prepend(templates.closeButton());
      this.modal.on('hidden.bs.modal', function(e) { instance.pause(); });
      this.modal.find('.video-description').after(this.video);
      this.modal.find('.video-disclaimer').hide();
      this.video.src = this.src;
      this.pause();
    }

    StaticVideoPlayer.prototype = {
      show: function(video) {
        this.modal.modal('show');
        this.play();
      },

      hide: function(video) {
        this.modal.modal('hide');
        this.pause();
      },

      play: function() {
        if (this.video.readyState > this.video.HAVE_FUTURE_DATA) {
          this.video.play();
        } else {
          this.video.addEventListener('canplaythrough', this.video.play);
        }
      },

      pause: function() {
        if (this.video.readyState > this.video.HAVE_METADATA) {
          this.video.pause();
        } else {
          this.video.addEventListener('loadedmetadata', this.video.pause);
        }
      },
    };

    return StaticVideoPlayer;
  })();


  window.VideoPlayer = (function() {
    var USER_API_URL = SIMON_API_BASE + '/User';

    /**
     * API Calls
     */
    function checkUserExistence(uuid, callback) {
      return jQuery.ajax({
        url: USER_API_URL + '/' + uuid,
        type: 'GET',
        success: function() { callback(true, uuid); },
        error: function(xhr) { if (xhr.status === 404) callback(false, uuid); },
      });
    }

    function createUser(uuid, callback) {
      return jQuery.ajax({
        url: SIMON_API_BASE + '/User',
        type: 'POST',
        contentType : "application/json",
        data: JSON.stringify({"userUniqueIdentifier": uuid}),
        success: function() { callback(uuid); },
        error: function(status) { if (status === 404) callback(false, uuid); },
      });
    }

    function getWatchHistory(uuid, callback) {
      return jQuery.ajax({
        url: SIMON_API_BASE + '/UserEducationWatchHistory',
        type: 'GET',
        data: {
          'relativeUrl' : 'SimonLearn',
          'userUniqueIdentifier': uuid,
        },
        success: function(response) { if (callback) callback(response); },
      });
    }

    function updateUserWatchHistory(data, callback) {
      return jQuery.ajax({
        url: SIMON_API_BASE + '/UserEducationWatchHistory',
        type: 'POST',
        contentType : "application/json",
        data: JSON.stringify({
          deferredCount: data.deferredCount || 0,
          educationId: data.educationId,
          userUniqueIdentifier: data.userUniqueIdentifier,
          watchProgress: data.watchProgress,
        }),
        success: function(response) { if (callback) callback(response); },
      });
    }

    /**
     * Encapsulated functions
     */
    function updateVideoFromState(video, videoState) {
      var shouldSetSrc = videoState.videoUrl && video.src !== videoState.videoUrl;
      if (shouldSetSrc) video.src = videoState.videoUrl;

      var callback = function() {
        video.currentTime = utils.ticksToSeconds(videoState.watchProgress);
      }

      if (video.readyState > video.HAVE_METADATA) {
        callback();
      } else {
        video.addEventListener('loadedmetadata', callback);
      }
    }

    function watchedPercent(watched, total) {
      return (watched / total) * 100;
    }

    function videoCompleted(watchProgress, videoLength) {
      var completeTarget = SIMON_MOCK_SETTINGS.markVideoCompleteAfterPercentage;
      var watched = watchedPercent(watchProgress, videoLength);

      return watched >= completeTarget;
    }

    function disableVideoScrubbing(video, videoState) {
      video.addEventListener('seeking', function() {
        var totalWatchedPercent = watchedPercent(
          videoState.maxWatchedInSeconds,
          utils.ticksToSeconds(videoState.videoLength)
        );

        if (totalWatchedPercent >= SIMON_MOCK_SETTINGS.skipVideoAfterPercentage) return;

        var delta = video.currentTime - videoState.maxWatchedInSeconds;
        if (Math.abs(delta) > 0.01) video.currentTime = videoState.maxWatchedInSeconds;
      });
    }

    function setupModal(instance) {
      var modalTemplate = templates.modal(
        instance.videoState.title,
        instance.videoState.description
      );

      instance.modal = jQuery(modalTemplate);
      instance.modal.on('hidden.bs.modal', function(e) { instance.pause(); });
    }

    function setupVideo(instance) {
      instance.video = generateVideo();
      updateVideoFromState(instance.video, instance.videoState);
      disableVideoScrubbing(instance.video, instance.videoState);
    }

    function setupEventListeners(instance) {
      var addCloseButton = utils.once.call(instance, instance.showCloseButton);

      instance.video.addEventListener('timeupdate', function() {
        var totalWatchedPercent = watchedPercent(
          instance.videoState.maxWatchedInSeconds,
          utils.ticksToSeconds(instance.videoState.videoLength)
        );

        if (totalWatchedPercent >= SIMON_MOCK_SETTINGS.skipVideoAfterPercentage) {
          addCloseButton();
        }
      });

      instance.video.addEventListener('timeupdate', utils.throttle(function() {
        var data = {watchProgress: utils.secondsToTicks(this.currentTime)};
        instance.setState(data);
        instance.setVideoProgress(data);
        updateUserWatchHistory(instance.videoState);
      }, 1000));
    }

    function onUserLoad(instance, response) {
      instance.setState(response);

      if (videoCompleted(response.watchProgress, response.videoLength)) return;

      setupModal(instance);
      setupVideo(instance);
      setupEventListeners(instance);

      instance.setVideoProgress(response);
      instance.modal.find('.video-description').after(instance.video);
      instance.show();
    }

    var playerInstances = [];

    function VideoPlayer(uuid) {
      var instance = this;

      playerInstances.push(instance);

      this.videoState = {
        deferredCount: null,
        description: SIMON_MOCK_DATA.description,
        educationId: null,
        maxWatchedInSeconds: null,
        title: SIMON_MOCK_DATA.title,
        videoUrl: SIMON_MOCK_DATA.videoUrl,
        videoLength: SIMON_MOCK_DATA.videoLength,
        userUniqueIdentifier: uuid,
        watchProgress: null,
      };

      checkUserExistence(this.videoState.userUniqueIdentifier, function(hasUser, uuid) {
        var callback = function(response) { onUserLoad(instance, response); };
        if (!hasUser && !uuid) return;
        if (!hasUser) createUser(uuid, callback);
        if (hasUser) getWatchHistory(uuid, callback);
      });
    }

    VideoPlayer.resetAll = function() {
      var instancesReset = 0;

      playerInstances.forEach(function(instance) {
        instance.reset(function() {
          instancesReset++;
          if (instancesReset === playerInstances.length) window.location.reload();
        });
      });
    };

    VideoPlayer.prototype = {
      show: function(video) {
        this.modal.modal({keyboard: false, show: true, backdrop: 'static'});
        this.play();
      },

      hide: function(video) {
        this.modal.modal('hide');
        this.pause();
      },

      play: function() {
        if (this.video.readyState > this.video.HAVE_FUTURE_DATA) {
          this.video.play();
        } else {
          this.video.addEventListener('canplaythrough', this.video.play);
        }
      },

      pause: function() {
        if (this.video.readyState > this.video.HAVE_METADATA) {
          this.video.pause();
        } else {
          this.video.addEventListener('loadedmetadata', this.video.pause);
        }
      },

      reset: function(callback) {
        this.setState({watchProgress: 0});
        updateUserWatchHistory(this.videoState, callback);
      },

      setState: function(data) {
        if (data.educationId) this.videoState.educationId = data.educationId;
        if (typeof data.deferredCount === 'number') this.videoState.deferredCount = data.deferredCount;
        if (typeof data.watchProgress === 'number') this.videoState.watchProgress = data.watchProgress;
      },

      setVideoProgress: function(data) {
        var state = this.videoState;
        var video = this.video;

        if (!video.seeking && video.currentTime > state.maxWatchedInSeconds) {
          state.maxWatchedInSeconds = video.currentTime;
        } else if (state.maxWatchedInSeconds === null && data.watchProgress) {
          state.maxWatchedInSeconds = utils.ticksToSeconds(data.watchProgress);
        }
      },

      showCloseButton: function() {
        var header = this.modal.find('.modal-header');
        if (header.find('.close').length > 0) return;
        header.prepend(templates.closeButton());
      },
    };

    return VideoPlayer;
  })();
})();
