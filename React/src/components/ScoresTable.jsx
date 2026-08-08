import ScoreRow from "./ScoreRow";

function ScoresTable({scores, usingStandardized}) {
    return (
        <div className="table-wrapper">
            <table className="scores-table">
                <thead>
                <tr>
                    <td>Mode</td>
                    <td>Rank</td>
                    <td colSpan="2">Player</td>
                    <td>PP</td>
                    <td>Mods</td>
                    <td>Score</td>
                    <td>Combo</td>
                    <td>Accuracy</td>
                    <td>Misses</td>
                    <td colSpan="2">Beatmap</td>
                </tr>
                </thead>
                <tbody>
                {scores.map((score, index) => <ScoreRow key={index} score={score} usingStandardized={usingStandardized}/>)}
                </tbody>
            </table>
        </div>)
}

export default ScoresTable;