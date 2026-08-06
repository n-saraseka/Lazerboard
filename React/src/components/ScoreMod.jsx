import {getModData} from "../utils/score-things.js";

function ScoreMod({acronym, speedChange}) {
    const mod = getModData(acronym);
    const isRateChange = mod.modData.isRateChange && speedChange !== null;
    return (<span className={`mod mod-${mod.category}`} title={mod.modData.name}>
        {`${acronym}${isRateChange ? `(${speedChange}x)` : ''}`}
    </span>)
}

export default ScoreMod;