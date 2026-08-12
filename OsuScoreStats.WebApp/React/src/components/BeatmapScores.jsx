import { useState, useMemo } from "react";
import BeatmapScoreRow from "./BeatmapScoreRow.jsx";
import SortableColumn from "./SortableColumn.jsx";

function BeatmapScores({scores}) {
    const [sort, setSort] = useState({
        rank: "none",
        totalScore: "none",
        accuracy: "none",
        combo: "none",
        misses: "none",
        pp: "none",
        date: "none",
    });

    const [hover, setHover] = useState({
        rank: false,
        totalScore: false,
        accuracy: false,
        combo: false,
        misses: false,
        pp: false,
        date: false,
    });

    const sortedScores = useMemo(() => {
        const activeSorts = {};
        Object.keys(sort).forEach((key) => {
            if (sort[key] !== "none") {
                activeSorts[key] = sort[key];
            }
        });
        const copy = [...scores];
        copy.sort((a, b) => compareSort(a, b, activeSorts));
        return copy;
    }, [scores, sort]);
    
    function compareSort(a, b, sortProperties) {
        const keys = Object.keys(sortProperties);
        for (const key of keys) {
            // This is scuffed but will do for now.
            if (key === "rank") {
                if (a[key] > b[key]) {
                    return sortProperties[key] === "desc" ? 1 : -1;
                }
                if (a[key] < b[key]) {
                    return sortProperties[key] === "desc" ? -1 : 1;
                }
            }
            else {
                if (a[key] < b[key]) {
                    return sortProperties[key] === "desc" ? 1 : -1;
                }
                if (a[key] > b[key]) {
                    return sortProperties[key] === "desc" ? -1 : 1;
                }
            }
        }
        
        return 0;
    }
    
    return (
        <div className="table-wrapper">
            <table className="scores-table">
                <thead>
                <tr>
                    <SortableColumn columnName="Rank" propertyName="rank" hover={hover} setHover={setHover} sort={sort} setSort={setSort}/>
                    <td colSpan={2}>Player</td>
                    <td>Grade</td>
                    <SortableColumn columnName="Score" propertyName="totalScore" hover={hover} setHover={setHover} sort={sort} setSort={setSort}/>
                    <SortableColumn columnName="Accuracy" propertyName="accuracy" hover={hover} setHover={setHover} sort={sort} setSort={setSort}/>
                    <SortableColumn columnName="Combo" propertyName="combo" hover={hover} setHover={setHover} sort={sort} setSort={setSort}/>
                    <SortableColumn columnName="Misses" propertyName="misses" hover={hover} setHover={setHover} sort={sort} setSort={setSort}/>
                    <SortableColumn columnName="PP" propertyName="pp" hover={hover} setHover={setHover} sort={sort} setSort={setSort}/>
                    <SortableColumn columnName="Date" propertyName="date" hover={hover} setHover={setHover} sort={sort} setSort={setSort}/>
                    <td>Mods</td>
                </tr>
                </thead>
                <tbody>
                {sortedScores.map((score, index) => (<BeatmapScoreRow key={index} score={score} usingStandardized={true}/>))}
                </tbody>
            </table>
        </div>
    )
}

export default BeatmapScores;