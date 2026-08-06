import {useState} from "react";
import ModSelectorRow from "./ModSelectorRow.jsx";

function ModSelector({availableMods, mods, setMods}) {
    const [isExpanded, setIsExpanded] = useState(false);
    return (
        <div className="mod-selection">
            <div className="mod-selector top-selector" onClick={() => setIsExpanded(!isExpanded)}>
                <span>Click to select</span>
                <div className="selector-chevron"></div>
            </div>
            <div className="mods-to-select" style={{display: isExpanded ? "block" : "none"}}>
                { availableMods.map((m, index) => (
                    <ModSelectorRow key={index} acronym={m} mods={mods} setMods={setMods}/>
                )) }
            </div>
        </div>
    )
}

export default ModSelector;