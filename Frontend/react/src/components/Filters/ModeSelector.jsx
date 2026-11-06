import { modeEnumToString } from "../../utils/beatmap-things.js";

function ModeSelector({mode, allowedModes, selectedMode, onModeSwitch}) {
    return (<div className={`mode-icon-wrapper${allowedModes.includes(mode) ? ' enabled' : ' disabled'}${selectedMode === mode ? ' active' : ''}`}>
        <div className={`mode-icon mode-${modeEnumToString(mode)}`} onClick={() => onModeSwitch()}></div>
    </div>)
}

export default ModeSelector;