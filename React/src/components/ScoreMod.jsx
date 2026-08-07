import {getModData} from "../utils/score-things.js";

function ScoreMod({acronym, speedChange}) {
    const mod = getModData(acronym);
    const isRateChange = mod.modData.isRateChange;
    const rateString = isRateChange && ![0.75, 1.5].includes(speedChange) ? `(${speedChange}x)` : '';
    
    return (<span className={`mod mod-${mod.category}`} title={mod.modData.name}>
        {`${acronym}${rateString}`}
    </span>)
}

export default ScoreMod;