import { getDifficultyColor, modeEnumToString } from "../utils/beatmap-things.js";

function DifficultyIcon({difficulty, name, mode, isActive, onDifficultySwitch}) {
    return (
        <div className={`mode-icon-wrapper enabled${isActive ? ' active' : ''}`}>
            <div className={`mode-icon mode-${modeEnumToString(mode)} difficulty-icon`}
                 style={{backgroundColor: getDifficultyColor(difficulty)}} title={name} onClick={() => onDifficultySwitch()}></div>
        </div>
    )
}

export default DifficultyIcon;