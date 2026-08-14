import {useState} from "react";
import ModSelectorRow from "./ModSelectorRow.jsx";

function ModSelector({availableMods, filters, setFilters, excludeMode, refetchScores}) {
    function updateMods(mod) {
        let newMods;
        if (excludeMode) {
            newMods = filters.excludeMods.includes(mod) ? filters.excludeMods.filter(m => m !== mod) : filters.excludeMods.concat(mod);
        }
        else {
            newMods = filters.includeMods.includes(mod) ? filters.includeMods.filter(m => m !== mod) : filters.includeMods.concat(mod);
        }
        const newFilters = excludeMode ? {...filters, excludeMods: newMods} : {...filters, includeMods: newMods}
        setFilters(newFilters);
        if (refetchScores !== undefined) {
            refetchScores(newFilters, true);
        }
    }
    
    const [isExpanded, setIsExpanded] = useState(false);
    return (
        <>
            <div className="selector selector-mods">
                <div className="selector-item top-selector" onClick={() => setIsExpanded(!isExpanded)}>
                    <span>Click to select</span>
                    <div className="selector-chevron"></div>
                </div>
                <div className="selector-items" style={{display: isExpanded ? "block" : "none"}}>
                    { availableMods.map((m, index) => (
                        <ModSelectorRow key={index} acronym={m} mods={excludeMode ? filters.excludeMods : filters.includeMods} 
                                        setMods={() => updateMods(m)}/>
                    )) }
                </div>
            </div>
            {!excludeMode && (<>
                    <input name="lenientMode" id="lenientMode" type="checkbox" checked={filters.lenientMode} onClick={() => {
                        const newFilters = {...filters, lenientMode: !filters.lenientMode}
                        setFilters(newFilters);
                        if (refetchScores !== undefined) {
                            refetchScores(newFilters);
                        }
                    }}/>
                    <label htmlFor="lenientMode">Allow other mods</label>
            </>)}
        </>
    )
}

export default ModSelector;