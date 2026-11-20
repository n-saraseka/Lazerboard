import {getDifficultyColor, modeEnumToString} from "../../utils/beatmap-things.js";
import { timeSpanToString } from "../../utils/datetime-things.js";
import BeatmapStatusLabel from "./BeatmapStatusLabel.jsx";

function BeatmapCard({beatmap, beatmapset, selectedMode}) {
    return (<div className="beatmap-card" >
        <div className="beatmap-title">
            <a href={`https://osu.ppy.sh/beatmapsets/${beatmapset.id}#${modeEnumToString(selectedMode)}/${beatmap.id}`} className="map-name">
                <strong>{`[${beatmap.difficultyName}]`}</strong>
            </a>
            <BeatmapStatusLabel status={beatmap.status}/>
        </div>
        <div className="stats-grid">
            <span>BPM:</span>
            <span>{beatmap.bpm}</span>
            <span>Length:</span>
            <span>{timeSpanToString(beatmap.drainLength)}</span>
        </div>
        <div className="stats-grid">
            <span title="SR">Star Rating:</span>
            <div className="beatmap-stat">
                <div className="difficulty-bar"
                     style={{background: `linear-gradient(to right, ${getDifficultyColor(beatmap.difficulty)} ${Math.min(beatmap.difficulty, 10) * 10}%, gray ${Math.min(beatmap.difficulty, 10) * 10}%)`}}>
                </div>
                <span>{beatmap.difficulty.toFixed(2)}</span>
            </div>
            {
                // Only display AR for standard and catch.
                ![1, 3].includes(beatmap.mode) && (
                <>
                    <span title="AR">Approach Rate:</span>
                    <div className="beatmap-stat">
                        <div className="difficulty-bar"
                             style={{background: `linear-gradient(to right, cyan ${beatmap.approachRate * 10}%, gray ${beatmap.approachRate * 10}%)`}}>
                        </div>
                        <span>{beatmap.approachRate}</span>
                    </div>
                </>)
            }
            {
                beatmap.mode !== 1 && (
                    <>
                        <span title={`${beatmap.mode === 3 ? 'KC' : 'CS'}`}>{`${beatmap.mode === 3 ? "Key Count" : "Circle Size"}`}:</span>
                        <div className="beatmap-stat">
                            <div className="difficulty-bar"
                                 style={{background: `linear-gradient(to right, cyan ${Math.min(beatmap.circleSize * 10, 100)}%, gray ${Math.min(beatmap.circleSize * 10, 100)}%)`}}>
                            </div>
                            <span>{beatmap.circleSize}</span>
                        </div>
                    </>
                )
            }
            <span title="OD">Accuracy:</span>
            <div className="beatmap-stat">
                <div className="difficulty-bar"
                     style={{background: `linear-gradient(to right, cyan ${beatmap.overallDifficulty * 10}%, gray ${beatmap.overallDifficulty * 10}%)`}}>
                </div>
                <span>{beatmap.overallDifficulty}</span>
            </div>
            <span title="HP">HP Drain:</span>
            <div className="beatmap-stat">
                <div className="difficulty-bar"
                     style={{background: `linear-gradient(to right, cyan ${beatmap.health * 10}%, gray ${beatmap.health * 10}%)`}}>
                </div>
                <span>{beatmap.health}</span>
            </div>
        </div>
    </div>)
}

export default BeatmapCard;