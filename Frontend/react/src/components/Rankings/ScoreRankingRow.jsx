import {getEncodedCountry} from "../../utils/user-things.js";

function ScoreRankingRow({ranking}) {
    return (
        <tr>
            <td>{ranking.rank}</td>
            <td>
                <div className="score-user-data">
                    <img src={`https://osu.ppy.sh/assets/images/flags/${getEncodedCountry(ranking.user.countryCode)}.svg`}
                         alt={ranking.user.countryCode}
                         title={ranking.user.countryCode}
                         className="country-img"/>
                    <img src={`https://a.ppy.sh/${ranking.user.id}`} alt={ranking.user.username} className="player-img"/>
                    <a href={`/users/${ranking.user.id}`}>{ranking.user.username}</a>
                </div>
            </td>
            <td>{ranking.scoresCount}</td>
        </tr>
    );
}

export default ScoreRankingRow;