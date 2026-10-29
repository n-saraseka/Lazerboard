import {modeEnumToDisplayString, modeEnumToString} from "../../utils/beatmap-things.js";

function ModeWedge({mode}) {
    const modeString = modeEnumToString(mode);
    const modeDisplayString = modeEnumToDisplayString(mode);
    return (<div className={`score-mode color-${modeString}`}>
        <div className={`mode-icon mode-${modeString}`} title={modeDisplayString}></div>
    </div>)
}

export default ModeWedge;