import {getModData} from "../utils/score-things.js";
import ScoreMod from "./ScoreMod.jsx";

function ModSelectorRow({acronym, mods, setMods}) {
    const mod = getModData(acronym);
    return (
        <div className={`mod-selector list-mod${mods.includes(acronym) ? " mod-active" : ""}`} onClick={() => setMods(acronym)}>
            <ScoreMod acronym={acronym} speedChange={null}></ScoreMod>
            <div className="selector-modname">
                <span>{mod.modData.name}</span>
            </div>
        </div>
    )
}

export default ModSelectorRow;