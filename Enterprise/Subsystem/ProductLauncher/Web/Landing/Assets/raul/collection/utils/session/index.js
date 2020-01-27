export function setSession(key, value) {
    return sessionStorage.setItem(key, value);
}
export function getSession(key) {
    const value = sessionStorage.getItem(key);
    try {
        return JSON.parse(value);
    }
    catch (_a) {
        return value;
    }
}
export function clearSession() {
    sessionStorage.clear();
}
