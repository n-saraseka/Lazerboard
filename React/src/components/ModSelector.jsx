import {useState} from "react";
import ModSelectorRow from "./ModSelectorRow.jsx";

function ModSelector({availableMods, mods, setMods}) {
    const [isExpanded, setIsExpanded] = useState(false);
    return (
        <div className="selector">
            <div className="selector-item top-selector" onClick={() => setIsExpanded(!isExpanded)}>
                <span>Click to select</span>
                <div className="selector-chevron"></div>
            </div>
            <div className="selector-items" style={{display: isExpanded ? "block" : "none"}}>
                { availableMods.map((m, index) => (
                    <ModSelectorRow key={index} acronym={m} mods={mods} setMods={setMods}/>
                )) }
            </div>
        </div>
    )
}

export default ModSelector;