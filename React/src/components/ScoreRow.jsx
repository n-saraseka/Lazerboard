import ScoreMod from "./ScoreMod";

function ScoreRow({score, usingStandardized}) {
    return (<tr className="score-row">
        <td className="score-row-player-name">
            <a href={`/user/${score.user.id}`}>{score.user.username}</a>
        </td>
        <td className="score-row-rank">{`#${score.rank}`}</td>
        <td className="score-row-pp">{`${score.pp.toFixed(0)}pp`}</td>
        <td className="score-row-mods">
            {score.modAcronyms.map(modAcronym => <ScoreMod acronym={modAcronym}/>)}
        </td>
        <td className="score-total">
            <a href={`https://osu.ppy.sh/scores/${score.id}`}
               title={usingStandardized ? "Standardised score" : "Classic score"}
               className="score-primary">
                {usingStandardized ? score.totalScore.toLocaleString('en-US') : score.classicTotalScore.toLocaleString('en-US')}
            </a>
            <span title={usingStandardized ? "Classic score" : "Standardised score"} className="score-secondary">
                {usingStandardized ? score.classicTotalScore.toLocaleString('en-US') : score.totalScore.toLocaleString('en-US')}
            </span>
        </td>
        <td className="score-row-combo">{`${score.combo.toLocaleString('en-US')}x`}</td>
        <td className="score-row-accuracy">{`${(score.accuracy * 100).toFixed(2)}%`}</td>
        <td className="score-misses">{score.misses > 0 && `${score.misses}x`}</td>
        <td className="score-row-map-image">
            <a href={`/beatmapsets/${score.beatmap.beatmapset.id}`}>
                <img src={`https://assets.ppy.sh/beatmaps/${score.beatmap.beatmapset.id}/covers/cover@2x.jpg`} alt="Beatmap image" onError={(event) => {
                    event.target.style.display = 'none';
                }}/>
            </a>
        </td>
        <td className="score-beatmap">
            <a href={`/b/${score.beatmap.id}`}>
                {`${score.beatmap.beatmapset.artist} - ${score.beatmap.beatmapset.title} [${score.beatmap.difficultyName}]`}
            </a>
        </td>
    </tr>)
}

export default ScoreRow;