import {useState} from "react";
import ModSelectorRow from "./ModSelectorRow.jsx";

function ModSelector({availableMods, filters, setFilters}) {
    function updateMods(mod) {
        const newMods = filters.mods.includes(mod) ? filters.mods.filter(m => m !== mod) : filters.mods.concat(mod);
        setFilters({...filters, mods: newMods});
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
            <input name="lenientMode" id="lenientMode" type="checkbox" checked={filters.lenientMode} onClick={() =>
                setFilters({...filters, lenientMode: !filters.lenientMode})}/>
            <label htmlFor="lenientMode">Allow other mods</label>
        </>
    )
}

export default ModSelector;