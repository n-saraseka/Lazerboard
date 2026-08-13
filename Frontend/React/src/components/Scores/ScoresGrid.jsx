import ScoreCard from "./ScoreCard";

function ScoresGrid({scores, usingStandardized}) {
    return (<div className="scores">
        {scores.map((score, index) => <ScoreCard score={score} usingStandardized={usingStandardized} key={index}/>)}
    </div>)
}

export default ScoresGrid;