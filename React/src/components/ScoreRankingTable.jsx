import ScoreRankingRow from "./ScoreRankingRow.jsx";

function ScoreRankingTable({rankings}) {
    return (
        <table className="scores-table user-ranking">
            <thead>
                <tr>
                    <td>Rank</td>
                    <td>User</td>
                    <td>Count</td>
                </tr>
            </thead>
            <tbody>
                {rankings.map((ranking, index) => (
                    <ScoreRankingRow key={index} ranking={ranking}/>
                ))}
            </tbody>
        </table>
    )
}

export default ScoreRankingTable;