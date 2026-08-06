import {useState} from "react";
import ScoreRankingFilters from "./ScoreRankingFilters.jsx";
function ScoreRankingPage({countries}) {
    const [filters, setFilters] = useState({
        rankRange: {min: 1, max: 100},
        ppRange: {min: null, max: null},
        scoreRange: {min: null, max: null},
        country: {id: "All", name: "All countries"},
        mods: [],
        lenientMode: false,
        modes: Array(4).fill(0).map((m, i) => {
            return { value: i, enabled: true };
        })
    });
    
    return (<>
        <div className="component-container">
            <ScoreRankingFilters filters={filters} setFilters={setFilters} countries={countries}/>
        </div>
    </>)
}

export default ScoreRankingPage;