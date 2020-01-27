export function groupBy(items, keyGetter) {
    const grouper = (result, item) => {
        const key = keyGetter(item);
        return Object.assign(Object.assign({}, result), { [key]: [
                ...(result[key] || []),
                item,
            ] });
    };
    return items.reduce(grouper, {});
}
;
