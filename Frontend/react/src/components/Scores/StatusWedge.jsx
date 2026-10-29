import {beatmapStatusEnumToText} from "../../utils/beatmap-things.js";

function StatusWedge({status}) {
    const labelText = beatmapStatusEnumToText(status).toLowerCase();
    return (<div className={`score-status status-${labelText}`}>
        <div className={`status-icon status-${labelText}`}></div>
    </div>)
}

export default StatusWedge;