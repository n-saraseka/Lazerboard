import {getModData} from "../utils/score-things.js";
import ScoreMod from "./ScoreMod.jsx";

function ModSelectorRow({acronym, mods, setMods}) {
    const mod = getModData(acronym);
    const matchingData = mods.find(m => m.acronym === acronym);
    return (
        <div className={`mod-selector list-mod${matchingData.active ? " mod-active" : ""}`} onClick={() => setMods(mods.map(m => {
            m.active = m.acronym === acronym ? !m.active : m.active;
        }))}>
            <ScoreMod acronym={acronym} speedChange={null}></ScoreMod>
            <div className="selector-modname">
                <span>{mod.modData.name}</span>
            </div>
        </div>
    )
}

export default ModSelectorRow;