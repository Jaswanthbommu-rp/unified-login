import { h } from './core-9263a98c.js';

const MenuItem = (props) => {
    const { title, icon, payload, url, disabled, onClickCallback, onBlurCallback, event } = props;
    const Tag = (url && !disabled) ? 'a' : 'div';
    return (h(Tag, { class: {
            'r-dropdown-menu__menu-item': true,
            'r-dropdown-menu__menu-item--disabled': disabled,
        }, onClick: (e) => {
            if (onClickCallback) {
                onClickCallback(e, payload);
            }
            //@ts-ignore
            event.emit(e, payload);
        }, onKeyDown: e => {
            if (e.key === 'Enter') {
                if (onClickCallback) {
                    onClickCallback(e, payload);
                }
            }
            //@ts-ignore
            event.emit(e, payload);
        }, onBlur: e => { if (onBlurCallback) {
            onBlurCallback(e, payload);
        } }, role: url ? undefined : 'button', tabindex: disabled ? undefined : 0, href: url ? disabled ? '#' : url : undefined, title: title },
        h("div", { class: 'r-dropdown-menu-item__focus-utility', tabindex: disabled ? undefined : -1 },
            h("div", { class: 'r-dropdown-menu__menu-item__container' },
                icon &&
                    h("div", { class: 'r-dropdown-menu__menu-item__icon' },
                        h("raul-icon", { icon: icon })),
                h("div", { class: 'r-dropdown-menu__menu-item__label' },
                    title,
                    h("slot", null))))));
};

export { MenuItem as M };
