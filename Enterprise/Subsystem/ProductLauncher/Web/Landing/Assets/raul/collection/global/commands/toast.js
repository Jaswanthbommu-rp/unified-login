export default function toast(options = {}) {
    //@ts-ignore
    const toaster = maybeCreateToaster(options.tagName);
    const toast = makeToast(toaster, options);
    return toast;
}
function maybeCreateToaster(tagName) {
    if (tagName === 'raul-toast') {
        if (!document.querySelector('raul-toaster')) {
            const toaster = document.createElement('raul-toaster');
            document.body.insertBefore(toaster, document.body.firstChild || null);
        }
        return document.querySelector('raul-toaster');
    }
    if (tagName === 'raul-snackbar') {
        if (!document.querySelector('raul-snackbar-group')) {
            const snackbarGroup = document.createElement('raul-snackbar-group');
            document.body.insertBefore(snackbarGroup, document.body.firstChild || null);
        }
        return document.querySelector('raul-snackbar-group');
    }
}
function makeToast(toaster, options) {
    // const toast = document.createElement('raul-toast')
    const {} = options;
    const toast = document.createElement(options.tagName);
    const { timeout, avatar, origin, variant, dismissable, heading, body, content, ctaMessage, ctaUrl, ctaCallback, meta, actions, read, severity } = options;
    toast.timeout = timeout;
    toast.avatar = avatar;
    toast.origin = origin;
    toast.variant = variant;
    toast.dismissable = dismissable;
    toast.heading = heading;
    toast.body = body;
    toast.meta = meta,
        toast.read = read,
        toast.severity = severity,
        toast.content = content;
    toast.ctaMessage = ctaMessage;
    toast.ctaUrl = ctaUrl;
    toast.ctaCallback = ctaCallback;
    toast.meta = meta;
    toast.actions = actions;
    if (toaster)
        toaster.insertBefore(toast, toaster.firstChild || null);
    return toast;
}
