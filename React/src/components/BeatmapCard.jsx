import { getDifficultyColor } from "../utils/beatmap-things.js";
import { timeSpanToString } from "../utils/datetime-things.js";
import BeatmapStatusLabel from "./BeatmapStatusLabel.jsx";

function BeatmapCard({beatmap}) {
    return (<div className="beatmap-card" >
        <div className="beatmap-title">
            <a href={`https://osu.ppy.sh/b/${beatmap.id}`} className="map-name"><strong>{`[${beatmap.difficultyName}]`}</strong></a>
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
            <span title="AR">Approach Rate:</span>
            <div className="beatmap-stat">
                <div className="difficulty-bar"
                     style={{background: `linear-gradient(to right, cyan ${beatmap.approachRate * 10}%, gray ${beatmap.approachRate * 10}%)`}}>
                </div>
                <span>{beatmap.approachRate}</span>
            </div>
            <span title="CS">Circle Size:</span>
            <div className="beatmap-stat">
                <div className="difficulty-bar"
                     style={{background: `linear-gradient(to right, cyan ${beatmap.circleSize * 10}%, gray ${beatmap.circleSize * 10}%)`}}>
                </div>
                <span>{beatmap.circleSize}</span>
            </div>
            <span title="OD">Accuracy:</span>
            <div className="beatmap-stat">
                <div className="difficulty-bar"
                     style={{background: `linear-gradient(to right, cyan ${beatmap.overallDifficulty * 10}%, gray ${beatmap.overallDifficulty * 10}%)`}}>
                </div>
                <span>{beatmap.overallDifficulty}</span>
            </div>
            <span title="HP">Health Drain:</span>
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