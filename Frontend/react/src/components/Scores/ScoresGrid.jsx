import ScoreCard from "./ScoreCard";

function ScoresGrid({scores}) {
    return (<div className="scores">
        {scores.map((score, index) => <ScoreCard score={score} key={index}/>)}
    </div>)
}

export default ScoresGrid;