import ScoreMod from "./ScoreMod";
import { dateStringFromDatetime, dateFromDateTime } from "../utils/datetime-things.js";
import { gradeEnumToGradeLetter, gradeEnumToGradeClass } from "../utils/score-things.js";
import {getEncodedCountry} from "../utils/user-things.js";

function BeatmapScoreRow({score, usingStandardized}) {
    return (<tr className="score-row">
        <td className="score-row-rank">{`#${score.rank}`}</td>
        <td className="score-row-country">
            <img
                src={`https://osu.ppy.sh/assets/images/flags/${getEncodedCountry(score.user.countryCode)}.svg`}
                alt={score.user.country.name}
                title={score.user.country.name}
                className="country-img"/>
        </td>
        <td className="score-row-player-name">
            <a href={`/user/${score.user.id}`}>{score.user.username}</a>
        </td>
        <td className={`score-row-grade ${gradeEnumToGradeClass(score.grade)}`}>{gradeEnumToGradeLetter(score.grade)}</td>
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
        <td className="score-row-accuracy">{`${(score.accuracy * 100).toFixed(2)}%`}</td>
        <td className="score-row-combo">{`${score.combo.toLocaleString('en-US')}x`}</td>
        <td className="score-misses">{score.misses > 0 && `${score.misses}x`}</td>
        <td className="score-row-pp">{`${score.pp.toFixed(0)}pp`}</td>
        <td className="score-row-date" title={dateFromDateTime(score.date)}>{dateStringFromDatetime(score.date)}</td>
        <td className="score-row-mods">
            {score.modAcronyms.map(modAcronym => <ScoreMod acronym={modAcronym} speedChange={score.speedChange}/>)}
        </td>
    </tr>)
}

export default BeatmapScoreRow;