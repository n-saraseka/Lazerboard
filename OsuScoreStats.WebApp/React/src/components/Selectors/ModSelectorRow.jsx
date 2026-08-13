import {getModData} from "../../utils/score-things.js";
import ScoreMod from "../Scores/ScoreMod.jsx";

function ModSelectorRow({acronym, mods, setMods}) {
    const mod = getModData(acronym);
    return (
        <div className={`selector-item list-selector${mods.includes(acronym) ? " mod-active" : ""}`} onClick={() => setMods(acronym)}>
            <ScoreMod acronym={acronym} speedChange={null}></ScoreMod>
            <div className="selector-name">
                <span>{mod.modData.name}</span>
            </div>
        </div>
    )
}

export default ModSelectorRow;