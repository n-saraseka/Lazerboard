import ScoreMod from "./ScoreMod";
import ModeWedge from "./ModeWedge.jsx";

function ScoreCard({score, usingStandardized}) {
    return (<div className="score-card">
        <div className="score" style={
            {background: `url("https://assets.ppy.sh/beatmaps/${score.beatmap.beatmapset.id}/covers/cover@2x.jpg") center, rgba(0, 0, 0, 0.7)`}
        }>
            <div className="score-column score-left-column">
                <a href={`/beatmapsets/${score.beatmap.beatmapset.id}`} className="score-song-name">
                    <strong>{`${score.beatmap.beatmapset.artist} - ${score.beatmap.beatmapset.title}`}</strong>
                </a>
                <a href={`/b/${score.beatmap.id}`} className="score-difficulty-name">{`[${score.beatmap.difficultyName}]`}</a>
                <strong className="score-rank">{`#${score.rank}`}</strong>
                <strong className="score-pp">{`${score.pp.toFixed(0)}pp`}</strong>
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
            <div className="score-column score-right-column">
                <a href={`/user/${score.user.id}`}>
                    <img src={`https://a.ppy.sh/${score.user.id}`} alt={score.user.username} title={score.user.username}
                         className="score-player-img"/>
                </a>
                <div className="score-mods">
                    {score.modAcronyms.map(modAcronym => <ScoreMod acronym={modAcronym}/>)}
                </div>
                <strong className="score-combo">{`${score.combo.toLocaleString('en-US')}x`}</strong>
                <div className="score-acc-misses">
                    <span className="score-acc">{`${(score.accuracy * 100).toFixed(2)}%`}</span>
                    {score.misses > 0 &&
                        <span className="score-misses">{`${score.misses}x`}</span>}
                </div>
            </div>
        </div>
        <ModeWedge mode={score.mode}/>
    </div>)
}

export default ScoreCard;