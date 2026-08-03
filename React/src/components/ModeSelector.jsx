import { modeEnumToString } from "../utils/beatmap-things.js";

function ModeSelector({mode, allowedModes, selectedMode, onModeSwitch}) {
    return (<div className={`mode-icon-wrapper${allowedModes.includes(mode) ? ' enabled' : ' disabled'}${selectedMode === mode ? ' active' : ''}`}>
        <div className={`mode-icon mode-${modeEnumToString(mode)}`}
             style={{backgroundColor: "white"}} onClick={() => onModeSwitch()}></div>
    </div>)
}

export default ModeSelector;