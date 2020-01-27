import { h } from './core-9263a98c.js';
var MenuItem = function (props) {
    var title = props.title, icon = props.icon, payload = props.payload, url = props.url, disabled = props.disabled, onClickCallback = props.onClickCallback, onBlurCallback = props.onBlurCallback, event = props.event;
    var Tag = (url && !disabled) ? 'a' : 'div';
    return (h(Tag, { class: {
            'r-dropdown-menu__menu-item': true,
            'r-dropdown-menu__menu-item--disabled': disabled,
        }, onClick: function (e) {
            if (onClickCallback) {
                onClickCallback(e, payload);
            }
            //@ts-ignore
            event.emit(e, payload);
        }, onKeyDown: function (e) {
            if (e.key === 'Enter') {
                if (onClickCallback) {
                    onClickCallback(e, payload);
                }
            }
            //@ts-ignore
            event.emit(e, payload);
        }, onBlur: function (e) {
            if (onBlurCallback) {
                onBlurCallback(e, payload);
            }
        }, role: url ? undefined : 'button', tabindex: disabled ? undefined : 0, href: url ? disabled ? '#' : url : undefined, title: title }, h("div", { class: 'r-dropdown-menu-item__focus-utility', tabindex: disabled ? undefined : -1 }, h("div", { class: 'r-dropdown-menu__menu-item__container' }, icon &&
        h("div", { class: 'r-dropdown-menu__menu-item__icon' }, h("raul-icon", { icon: icon })), h("div", { class: 'r-dropdown-menu__menu-item__label' }, title, h("slot", null))))));
};
export { MenuItem as M };
