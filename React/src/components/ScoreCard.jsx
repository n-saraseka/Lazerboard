import ScoreMod from "./ScoreMod";
import ModeWedge from "./ModeWedge.jsx";
import {useState} from "react";
import {dateFromDateTime} from "../utils/datetime-things.js";

function ScoreCard({score, usingStandardized}) {
    const [isExpanded, setIsExpanded] = useState(false);
    
    return (<div className="score-card">
        <div className="score" style={
            {background: `url("https://assets.ppy.sh/beatmaps/${score.beatmap.beatmapset.id}/covers/cover@2x.jpg") center, rgba(0, 0, 0, 0.7)`}
        }>
            <div className="scorecard-row title-row">
                <div className="scorecard-column title-column">
                    <a href={`/beatmapsets/${score.beatmap.beatmapset.id}?mode=${score.mode}`} className="score-song-name">
                        <strong>{`${score.beatmap.beatmapset.artist} - ${score.beatmap.beatmapset.title}`}</strong>
                    </a>
                </div>
                <div className="scorecard-column">
                    <a href={`/user/${score.user.id}`}>
                        <img src={`https://a.ppy.sh/${score.user.id}`} alt={score.user.username} title={score.user.username}
                             className="score-player-img"/>
                    </a>
                </div>
            </div>
            <div className="scorecard-row">
                <div className="scorecard-column">
                    <a href={`/b/${score.beatmap.id}?mode=${score.mode}`} className="score-difficulty-name">
                        {`[${score.beatmap.difficultyName}]`}
                    </a>
                </div>
                <div className="scorecard-column">
                    <div className="mods">
                        {score.modAcronyms.slice(0, 5).map(modAcronym => <ScoreMod acronym={modAcronym} speedChange={score.speedChange}/>)}
                        {score.modAcronyms.length > 5 && (<>
                            {!isExpanded && (
                                <span className="mod mod-unknown mods-expand" title="Click to expand" onClick={() => setIsExpanded(true)}>
                                {`+${score.modAcronyms.length - 5}`}
                            </span>
                            )}
                            {isExpanded && score.modAcronyms.slice(5).map(modAcronym => <ScoreMod acronym={modAcronym} speedChange={score.speedChange}/>)}
                        </>)}
                    </div>
                </div>
            </div>
            <div className="scorecard-row">
                <div className="scorecard-column">
                    <strong className="score-rank">{`#${score.rank}`}</strong>
                    <strong className="score-pp">{`${score.pp.toFixed(0)}pp`}</strong>
                </div>
                <div className="scorecard-column">
                    <div className="score-combo-misses">
                        <strong className="score-combo">{`${score.combo.toLocaleString('en-US')}x`}</strong>
                        {score.misses > 0 &&
                            <span className="score-misses" title="Misses">
                                {`(${score.misses}x)`}
                            </span>
                        }
                    </div>
                    <strong>{`${(score.accuracy * 100).toFixed(2)}%`}</strong>
                </div>
            </div>
            <div className="scorecard-row">
                <div className="scorecard-column">
                    <div className="score-total">
                        <a href={`https://osu.ppy.sh/scores/${score.id}`}
                           title={usingStandardized ? "Standardised score" : "Classic score"}
                           className="score-primary">
                            {usingStandardized ? score.totalScore.toLocaleString('en-US') : score.classicTotalScore.toLocaleString('en-US')}
                        </a>
                        <span title={usingStandardized ? "Classic score" : "Standardised score"}
                              className="score-secondary">
                            {usingStandardized ? score.classicTotalScore.toLocaleString('en-US') : score.totalScore.toLocaleString('en-US')}
                        </span>
                    </div>
                </div>
                <div className="scorecard-column">
                    <span title={dateFromDateTime(score.date)}>{score.date.split('T')[0]}</span>
                </div>
            </div>
        </div>
        <ModeWedge mode={score.mode}/>
    </div>)
}

export default ScoreCard;