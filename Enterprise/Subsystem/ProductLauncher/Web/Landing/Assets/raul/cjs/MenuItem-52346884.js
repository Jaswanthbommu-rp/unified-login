'use strict';

const core = require('./core-40bc5f02.js');

const MenuItem = (props) => {
    const { title, icon, payload, url, disabled, onClickCallback, onBlurCallback, event } = props;
    const Tag = (url && !disabled) ? 'a' : 'div';
    return (core.h(Tag, { class: {
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
        core.h("div", { class: 'r-dropdown-menu-item__focus-utility', tabindex: disabled ? undefined : -1 },
            core.h("div", { class: 'r-dropdown-menu__menu-item__container' },
                icon &&
                    core.h("div", { class: 'r-dropdown-menu__menu-item__icon' },
                        core.h("raul-icon", { icon: icon })),
                core.h("div", { class: 'r-dropdown-menu__menu-item__label' },
                    title,
                    core.h("slot", null))))));
};

exports.MenuItem = MenuItem;
