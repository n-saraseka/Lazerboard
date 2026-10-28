export function debounce(func, waitTimeMs) {
    let timer = null;
    return function debounced(...args) {
        clearTimeout(timer)
        timer = setTimeout(() => func(...args), waitTimeMs);
    }
}
