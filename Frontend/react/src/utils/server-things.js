export function debounce(func, waitTimeMs) {
    let previousCallTime = null;
    let lastCallTimer = null;
    
    return function debounced(...args) {
        const lastCallTime = Date.now();
        if (previousCallTime && lastCallTime - previousCallTime <= waitTimeMs) {
            clearTimeout(lastCallTimer);
        }
        previousCallTime = lastCallTime;
        lastCallTimer = setTimeout(() => func(...args), waitTimeMs);
    }
}