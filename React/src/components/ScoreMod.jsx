
const difficultyDecreasingMods = ["DC", "EZ", "HT", "NF"];
const difficultyIncreasingMods = ["AC", "BL", "DT", "FL", "HD", "HR", "NC", "PF", "SD", "ST", "TC"];
const conversionMods = ["AL", "CL", "DA", "MR", "RD", "SG", "TP"];
const automationMods = ["AP", "RX", "SO"];
const funMods = ["AD", "AS", "BM", "BR", "BU", "DF", "DP", "FR", "GR", "MG", "MU", "NS", "RP", "SI", "SY", "TR", "WD", "WG", "WU"]
const systemMods = ["TD"];

const modCategories = {};
difficultyDecreasingMods.forEach(mod => modCategories[mod] = "difficulty-decrease");
difficultyIncreasingMods.forEach(mod => modCategories[mod] = "difficulty-increase");
conversionMods.forEach(mod => modCategories[mod] = "conversion");
automationMods.forEach(mod => modCategories[mod] = "automation");
funMods.forEach(mod => modCategories[mod] = "fun");
systemMods.forEach(mod => modCategories[mod] = "system");

function getModCategory(mod) {
    return modCategories[mod] || "unknown";
}

function ScoreMod({acronym}) {
    return (<span className={`mod mod-${getModCategory(acronym.slice(0,2))}`}>{acronym}</span>)
}

export default ScoreMod;