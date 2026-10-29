import {beatmapStatusEnumToText, modeEnumToString, modeEnumToDisplayString} from "../../utils/beatmap-things.js";
import {getEncodedCountry} from "../../utils/user-things.js";
import {useState} from "react";
import {getPpColor, getRankTierColor} from "../../utils/score-things.js";
import {dateFromDateTime, timeElapsedFromDateTime} from "../../utils/datetime-things.js";
import ScoreMods from "../Misc/ScoreMods.jsx";

function ScoreRow({score, usingStandardized}) {
    const [isExpanded, setIsExpanded] = useState(false);
    const modeString = modeEnumToString(score.mode);
    const modeDisplayString = modeEnumToDisplayString(score.mode);
    const statusString = beatmapStatusEnumToText(score.beatmap.status);
    
    return (<tr className="score-row">
        <td className="score-row-mode">
            <div className={`mode-icon mode-${modeString}`} style={{backgroundColor: "white"}} title={modeDisplayString}></div>
        </td>
        <td className="score-row-status">
            <div className={`status-icon status-${statusString.toLowerCase()}`} style={{backgroundColor: "white"}} title={statusString}></div>
        </td>
        <td className="score-row-rank" style={{color: getRankTierColor(score.rank)}}>{`#${score.rank}`}</td>
        <td className="score-row-country">
            <img
                src={`https://osu.ppy.sh/assets/images/flags/${getEncodedCountry(score.user.countryCode)}.svg`}
                alt={score.user.country.name}
                title={score.user.country.name}
                className="country-img"/>
        </td>
        <td className="score-row-player-name">
            <a href={`/users/${score.user.id}`}>{score.user.username}</a>
        </td>
        <td className="score-row-date" title={dateFromDateTime(score.date)}>{timeElapsedFromDateTime(score.date)}</td>
        <td className="score-row-pp" style={{color: getPpColor(score.pp)}} title={`${score.pp == null? 'PP has not been calculated for this score' : score.pp.toFixed(3)}`}>
            {`${score.pp === null ? '-' : score.pp.toFixed(0)}pp`}
        </td>
        <td className="score-row-mods">
            <ScoreMods modAcronyms={score.modAcronyms} speedChange={score.speedChange} isExpanded={isExpanded} setIsExpanded={setIsExpanded}/>
        </td>
        <td className="score-row-total">
            <div className="score-total">
                <a href={`https://osu.ppy.sh/scores/${score.id}`}
                   title={usingStandardized ? "Standardised score" : "Classic score"}
                   className="score-primary">
                    {usingStandardized ? score.totalScore.toLocaleString('en-US') : score.classicTotalScore.toLocaleString('en-US')}
                </a>
                <span title={usingStandardized ? "Classic score" : "Standardised score"} className="score-secondary">
                    {usingStandardized ? score.classicTotalScore.toLocaleString('en-US') : score.totalScore.toLocaleString('en-US')}
                </span>
            </div>
        </td>
        <td className={`score-combo${score.isPerfectCombo ? " score-perfect" : ""}`}>{`${score.combo.toLocaleString('en-US')}x`}</td>
        <td className={`score-row-accuracy${score.accuracy === 1 ? " score-perfect" : ""}`}>{`${(score.accuracy * 100).toFixed(2)}%`}</td>
        <td className="score-misses">{score.misses > 0 && `${score.misses}x`}</td>
        <td className="score-row-map-image">
            <a href={`/b/${score.beatmap.id}?mode=${score.mode}`}>
                <img src={`https://assets.ppy.sh/beatmaps/${score.beatmap.beatmapset.id}/covers/cover@2x.jpg`} alt="Beatmap image" onError={(event) => {
                    event.target.style.display = 'none';
                }}/>
            </a>
        </td>
        <td className="score-beatmap">
            <a href={`/b/${score.beatmap.id}?mode=${score.mode}`}>
                {`${score.beatmap.beatmapset.artist} - ${score.beatmap.beatmapset.title} [${score.beatmap.difficultyName}]`}
            </a>
        </td>
    </tr>)
}

export default ScoreRow;