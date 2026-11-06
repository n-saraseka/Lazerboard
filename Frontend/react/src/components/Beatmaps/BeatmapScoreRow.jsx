import {dateFromDateTime, timeElapsedFromDateTime} from "../../utils/datetime-things.js";
import {gradeEnumToGradeLetter, gradeEnumToGradeClass, getRankTierColor, getPpColor} from "../../utils/score-things.js";
import {getEncodedCountry} from "../../utils/user-things.js";
import {useContext, useState, useMemo} from "react";
import ScoreMods from "../Misc/ScoreMods.jsx";
import {PreferencesContext} from "../Preferences/PreferencesContext.jsx";

function BeatmapScoreRow({score}) {
    const [isExpanded, setIsExpanded] = useState(false);
    
    const { preferences, setPreferences } = useContext(PreferencesContext);
    const themePreference = useMemo(() => preferences.theme ?? 'dark', [preferences.theme]);
    const scoreDisplayPreference = useMemo(() => preferences.preferStandardizedScoring ?? true, [preferences.preferStandardizedScoring]);
    
    return (<tr className="score-row">
        <td className="score-row-rank" style={{color: getRankTierColor(themePreference, score.rank)}}>{`#${score.rank}`}</td>
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
        <td className={`score-row-grade ${gradeEnumToGradeClass(score.grade)}`}>{gradeEnumToGradeLetter(score.grade)}</td>
        <td className="score-row-total">
            <div className="score-total">
                <a href={`https://osu.ppy.sh/scores/${score.id}`}
                   title={scoreDisplayPreference ? "Standardised score" : "Classic score"}
                   className="score-primary">
                    {scoreDisplayPreference ? score.totalScore.toLocaleString('en-US') : score.classicTotalScore.toLocaleString('en-US')}
                </a>
                <span title={scoreDisplayPreference ? "Classic score" : "Standardised score"} className="score-secondary">
                    {scoreDisplayPreference ? score.classicTotalScore.toLocaleString('en-US') : score.totalScore.toLocaleString('en-US')}
                </span>
            </div>
        </td>
        <td className={`score-row-accuracy${score.accuracy === 1 ? " score-perfect" : ""}`}>{`${(score.accuracy * 100).toFixed(2)}%`}</td>
        <td className={`score-combo${score.isPerfectCombo ? " score-perfect" : ""}`}>{`${score.combo.toLocaleString('en-US')}x`}</td>
        <td className="score-misses">{score.misses > 0 && `${score.misses}x`}</td>
        <td className="score-row-pp" style={{color: getPpColor(themePreference, score.pp)}} title={`${score.pp == null? 'PP has not been calculated for this score' : score.pp.toFixed(3)}`}>
            {`${score.pp === null ? '-' : score.pp.toFixed(0)}pp`}
        </td>
        <td className="score-row-date" title={dateFromDateTime(score.date)}>{timeElapsedFromDateTime(score.date)}</td>
        <td className="score-row-mods">
            <ScoreMods modAcronyms={score.modAcronyms} speedChange={score.speedChange} isExpanded={isExpanded} setIsExpanded={setIsExpanded}/>
        </td>
    </tr>)
}

export default BeatmapScoreRow;