import { h } from "@stencil/core";
export class RaulVideo {
    constructor() {
        this.src = null;
        this.autoplay = false;
        this.controls = true;
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
        return (h("div", { class: "r-raul-video" },
            h("video", { src: this.src, controls: this.controls, autoplay: this.autoplay, ref: (el) => this.videoPlayer = el })));
    }
    static get is() { return "raul-video"; }
    static get originalStyleUrls() { return {
        "$": ["raul-video.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-video.css"]
    }; }
    static get properties() { return {
        "src": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "src",
            "reflect": false,
            "defaultValue": "null"
        },
        "autoplay": {
            "type": "boolean",
            "mutable": false,
            "complexType": {
                "original": "boolean",
                "resolved": "boolean",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "autoplay",
            "reflect": false,
            "defaultValue": "false"
        },
        "controls": {
            "type": "boolean",
            "mutable": false,
            "complexType": {
                "original": "boolean",
                "resolved": "boolean",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "controls",
            "reflect": false,
            "defaultValue": "true"
        }
    }; }
    static get events() { return [{
            "method": "videoPlay",
            "name": "videoPlay",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": ""
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }, {
            "method": "videoPause",
            "name": "videoPause",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": ""
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }, {
            "method": "videoEnded",
            "name": "videoEnded",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": ""
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }]; }
    static get methods() { return {
        "playVideo": {
            "complexType": {
                "signature": "() => Promise<void>",
                "parameters": [],
                "references": {
                    "Promise": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "",
                "tags": []
            }
        },
        "pauseVideo": {
            "complexType": {
                "signature": "() => Promise<void>",
                "parameters": [],
                "references": {
                    "Promise": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "",
                "tags": []
            }
        },
        "stopVideo": {
            "complexType": {
                "signature": "() => Promise<void>",
                "parameters": [],
                "references": {
                    "Promise": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "",
                "tags": []
            }
        },
        "progress": {
            "complexType": {
                "signature": "() => Promise<number>",
                "parameters": [],
                "references": {
                    "Promise": {
                        "location": "global"
                    }
                },
                "return": "Promise<number>"
            },
            "docs": {
                "text": "",
                "tags": []
            }
        }
    }; }
}
