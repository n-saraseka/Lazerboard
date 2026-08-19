import {useState} from "react";
import ScoreRankingTable from "../Rankings/ScoreRankingTable.jsx";
import Pagination from "../Misc/Pagination.jsx";
import Error from "../Misc/Error.jsx";
import Loader from "../Misc/Loader.jsx";
import {assembleSearchParams} from "../../utils/score-things.js";
import ScoreRankingFilters from "../Filters/ScoreRankingFilters.jsx";
import {debounce} from "../../utils/server-things.js";

function ScoreRankingPage({countries, userRanking}) {
    const [filters, setFilters] = useState({
        starRange: {min: null, max: null},
        country: {id: "All", name: "All countries"},
        amount: 10
    });
    const [isLoading, setIsLoading] = useState(false);
    const [isError, setIsError] = useState(false);

    const [currentPage, setCurrentPage] = useState(1);
    const [usersCount, setUsersCount] = useState(0);
    const [pageCount, setPageCount] = useState(Math.ceil(userRanking.count / 10));
    const [userRankings, setUserRankings] = useState(userRanking.userRankings);

    async function getRankings(filterOptions, pageNumber = 1) {
        setIsLoading(true);
        setIsError(false);
        
        const params = new URLSearchParams();
        assembleSearchParams(params, filterOptions, pageNumber);
        
        try {
            const response = await fetch(`/api/scores/millions?` + params.toString(), {
                method: "GET",
                headers: { "Accept": "application/json" },
            });

            if (response.ok) {
                const json = await response.json();
                setUserRankings(json.userRankings);
                setUsersCount(json.count);

                const pages = Math.ceil(json.count / filterOptions.amount);
                if (pageNumber !== currentPage) {
                    setCurrentPage(pageNumber);
                }
                if (pageNumber > pages) {
                    setCurrentPage(Math.max(1, pages));
                }
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
    
    const debouncedGetRankings = debounce(getRankings, 500);

    return (<>
        <h1 className="section-header">Score filters:</h1>
        <div className="component-container">
            <ScoreRankingFilters isMania={true} filters={filters} setFilters={setFilters} countries={countries}/>
        </div>
        <button className="calc-button" disabled={isLoading} 
                onClick={() => debouncedGetRankings(filters, currentPage)}>
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
        <Pagination page={currentPage} pages={pageCount} onPageChange={(newPage) => debouncedGetRankings(filters, newPage)}/>
    </>)
}

export default ScoreRankingPage;