import { r as registerInstance, c as createEvent, h } from './core-9263a98c.js';

const RaulVideo = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        this.src = null;
        this.autoplay = false;
        this.controls = true;
        this.videoPlay = createEvent(this, "videoPlay", 7);
        this.videoPause = createEvent(this, "videoPause", 7);
        this.videoEnded = createEvent(this, "videoEnded", 7);
    }
    componentDidLoad() {
        this.videoPlayer.addEventListener('play', () => this.videoPlay.emit());
        this.videoPlayer.addEventListener('pause', () => this.videoPause.emit());
        this.videoPlayer.addEventListener('ended', () => this.videoEnded.emit());
    }
    async playVideo() {
        this.videoPlayer.play().then(() => {
            // Great!
        }).catch(() => {
            // Maybe video is not loaded yet
            // Start playing when it is
            this.videoPlayer.autoplay = true;
        });
    }
    async pauseVideo() {
        this.videoPlayer.pause();
    }
    async stopVideo() {
        this.videoPlayer.pause();
        this.videoPlayer.currentTime = 0;
    }
    async progress() {
        const { currentTime, duration } = this.videoPlayer;
        return Math.ceil(currentTime / duration * 100.00);
    }
    render() {
        return (h("div", { class: "r-raul-video" }, h("video", { src: this.src, controls: this.controls, autoplay: this.autoplay, ref: (el) => this.videoPlayer = el })));
    }
    static get style() { return ".r-raul-video{display:block}.r-raul-video video{display:block;width:100%}.r-raul-video video::-webkit-media-controls-panel{background-image:-webkit-gradient(linear,left top,left bottom,from(transparent),to(transparent))!important;background-image:linear-gradient(transparent,transparent)!important;-webkit-filter:brightness(.4);filter:brightness(.4)}"; }
};

export { RaulVideo as raul_video };
