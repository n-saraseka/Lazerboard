import {sortedModAcronyms} from "../../utils/score-things.js";
import ScoreMod from "../Scores/ScoreMod.jsx";

function ScoreMods({modAcronyms, isExpanded, setIsExpanded, speedChange}) {
    return (
        <div className="mods">
            {sortedModAcronyms(modAcronyms.slice(0, 5)).map(modAcronym => <ScoreMod key={modAcronym} acronym={modAcronym} speedChange={speedChange}/>)}
            {modAcronyms.length > 5 && (<>
                {!isExpanded && (
                    <span className="mod mod-unknown mods-expand" title="Click to expand" onClick={() => setIsExpanded(true)}>
                                {`+${modAcronyms.length - 5}`}
                            </span>
                )}
                {isExpanded && sortedModAcronyms(modAcronyms.slice(5)).map(modAcronym => <ScoreMod acronym={modAcronym} speedChange={speedChange}/>)}
            </>)}
        </div>
    )
}

export default ScoreMods;