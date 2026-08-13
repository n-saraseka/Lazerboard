import {modeEnumToString} from "../../utils/beatmap-things.js";

function ModeWedge({mode}) {
    const modeString = modeEnumToString(mode);
    return (<div className={`score-mode color-${modeString}`}>
        <div className={`mode-icon mode-${modeString}`} style={{backgroundColor: "white"}}></div>
    </div>)
}

export default ModeWedge;