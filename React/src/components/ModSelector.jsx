import {useState} from "react";
import ModSelectorRow from "./ModSelectorRow.jsx";

function ModSelector({availableMods, filters, setFilters, refetchScores}) {
    function updateMods(mod) {
        const newMods = filters.mods.includes(mod) ? filters.mods.filter(m => m !== mod) : filters.mods.concat(mod);
        const newFilters = {...filters, mods: newMods}
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
                        <ModSelectorRow key={index} acronym={m} mods={filters.mods} setMods={() => updateMods(m)}/>
                    )) }
                </div>
            </div>
            <input name="lenientMode" id="lenientMode" type="checkbox" checked={filters.lenientMode} onClick={() => {
                const newFilters = {...filters, lenientMode: !filters.lenientMode}
                setFilters(newFilters);
                if (refetchScores !== undefined) {
                    refetchScores(newFilters);
                }
            }}/>
            <label htmlFor="lenientMode">Allow other mods</label>
        </>
    )
}

export default ModSelector;