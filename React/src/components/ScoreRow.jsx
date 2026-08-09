import ScoreMod from "./ScoreMod";
import {modeEnumToString} from "../utils/beatmap-things.js";
import {getEncodedCountry} from "../utils/user-things.js";
import {useState} from "react";

function ScoreRow({score, usingStandardized}) {
    const [isExpanded, setIsExpanded] = useState(false);
    const modeString = modeEnumToString(score.mode);
    
    return (<tr className="score-row">
        <td className="score-row-mode">
            <div className={`mode-icon mode-${modeString}`} style={{backgroundColor: "white"}}></div>
        </td>
        <td className="score-row-rank">{`#${score.rank}`}</td>
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
        <td className="score-row-pp">{`${score.pp.toFixed(0)}pp`}</td>
        <td className="score-row-mods">
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
            <a href={`/beatmapsets/${score.beatmap.beatmapset.id}?mode=${score.mode}`}>
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