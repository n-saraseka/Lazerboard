import {beatmapStatusEnumToText} from "../utils/beatmap-things.js";

function BeatmapStatusLabel({status}) {
    const labelText = beatmapStatusEnumToText(status);
    return (<div className={`beatmap-status status-${labelText.toLowerCase()}`}>{labelText}</div>)
}

export default BeatmapStatusLabel;