import { h } from "@stencil/core";
import Sortable from 'sortablejs';
export class RaulSortableList {
    emitItemDragEvent(e) {
        this.itemDrag.emit({
            from: e.from,
            fromIndex: e.oldIndex,
            to: e.to,
            toIndex: e.newIndex
        });
    }
    componentDidLoad() {
        Sortable.create(this.el.querySelector('.r-sortable-list__items-container'), {
            group: this.group,
            animation: 150,
            threshold: 1,
            forceFallback: true,
            ghostClass: "sortable-ghost",
            dragClass: "sortable-drag",
            pull: 'clone',
            filter: ".not-draggable",
            onEnd: e => this.emitItemDragEvent(e)
        });
    }
    render() {
        return (h("div", { class: "r-sortable-list", ref: el => this.el = el },
            this.listGroupLabel &&
                h("div", { class: 'r-sortable-list__group-label' },
                    h("raul-heading", { variant: 'content' }, this.listGroupLabel)),
            h("div", { class: 'r-sortable-list__items-container' },
                h("slot", null))));
    }
    static get is() { return "raul-sortable-list"; }
    static get originalStyleUrls() { return {
        "$": ["raul-sortable-list.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-sortable-list.css"]
    }; }
    static get properties() { return {
        "group": {
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
                "text": "If two or more lists have the same `group` property, the user will be able to drag & drop items between them"
            },
            "attribute": "group",
            "reflect": false
        },
        "listGroupLabel": {
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
                "text": "Label of a list group"
            },
            "attribute": "list-group-label",
            "reflect": false
        }
    }; }
    static get events() { return [{
            "method": "itemDrag",
            "name": "itemDrag",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": "The event will be emitted at the end of a drag&drop"
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }]; }
}
