export function getEncodedCountry(countryCode) {
    countryCode = countryCode.toUpperCase();
    const baseCode = 0x1F1E6;

    const code1 = baseCode + (countryCode.charCodeAt(0) - 'A'.charCodeAt(0));
    const code2 = baseCode + (countryCode.charCodeAt(1) - 'A'.charCodeAt(0));

    const hex1 = code1.toString(16).toLowerCase();
    const hex2 = code2.toString(16).toLowerCase();

    return `${hex1}-${hex2}`;
}