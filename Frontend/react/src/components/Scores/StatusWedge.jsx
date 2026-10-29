import {beatmapStatusEnumToText} from "../../utils/beatmap-things.js";

function StatusWedge({status}) {
    const statusString = beatmapStatusEnumToText(status);
    return (<div className={`score-status status-${statusString.toLowerCase()}`}>
        <div className={`status-icon status-${statusString.toLowerCase()}`} title={statusString}></div>
    </div>)
}

export default StatusWedge;