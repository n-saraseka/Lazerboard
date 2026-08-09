import {useState} from "react";
import ScoreRankingFilters from "./ScoreRankingFilters.jsx";
import ScoreRankingTable from "./ScoreRankingTable.jsx";
import Pagination from "./Pagination.jsx";
import Error from "./Error.jsx";
import Loader from "./Loader.jsx";
import {assembleSearchParams} from "../utils/score-things.js";

function ScoreRankingPage({countries}) {
    const [filters, setFilters] = useState({
        rankRange: {min: 1, max: 100},
        ppRange: {min: null, max: null},
        accRange: {min: null, max: null},
        rateRange: {min: null, max: null},
        country: {id: "All", name: "All countries"},
        mods: [],
        lenientMode: true,
        modes: Array(4).fill(0).map((m, i) => {
            return { value: i, enabled: true };
        }),
        amount: 10
    });
    const [isLoading, setIsLoading] = useState(false);
    const [isError, setIsError] = useState(false);

    const [usersCount, setUsersCount] = useState(0);
    const [pageCount, setPageCount] = useState(0);
    const [userRankings, setUserRankings] = useState([]);

    async function getRankings(filterOptions, pageNumber = 1) {
        setIsLoading(true);
        setIsError(false);
        
        const params = new URLSearchParams();
        assembleSearchParams(params, filterOptions, pageNumber);
        
        try {
            const response = await fetch(`/api/scores/ranking?` + params.toString(), {
                method: "GET",
                headers: { "Accept": "application/json" },
            });

            if (response.ok) {
                const json = await response.json();
                setUserRankings(json.userRankings);
                setUsersCount(json.count);

                const pages = Math.ceil(json.count / filterOptions.amount);
                setPageCount(pages);
            }
            else {
                setUsersCount(0);
                setPageCount(0);
                setIsError(true);
            }
        }
        catch (error) {
            setUsersCount(0);
            setPageCount(0);
            setIsError(true);
        }
        
        setIsLoading(false);
    }
    
    return (<>
        <h1 className="section-header">Filter scores:</h1>
        <div className="component-container">
            <ScoreRankingFilters filters={filters} setFilters={setFilters} countries={countries}/>
        </div>
        <button className="calc-button" onClick={async () => await getRankings(filters)}>
            Get ranking
        </button>
        {userRankings.length > 0 && <h1 className="section-header">User rankings:</h1>}
        <div className="component-container">
            {isError
                ? (<Error/>)
                : (isLoading
                    ? (<Loader/>)
                    : userRankings.length > 0 && (
                    <>
                        <ScoreRankingTable rankings={userRankings}/>
                    </>
                ))}
        </div>
        <Pagination key={usersCount} pages={pageCount} onPageChange={async (newPage) => await getRankings(filters, newPage)}/>
    </>)
}

export default ScoreRankingPage;