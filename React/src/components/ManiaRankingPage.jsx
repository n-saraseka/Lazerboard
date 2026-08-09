import {useState} from "react";
import ManiaRankingFilters from "./ManiaRankingFilters.jsx";
import ScoreRankingTable from "./ScoreRankingTable.jsx";
import Pagination from "./Pagination.jsx";
import Error from "./Error.jsx";
import Loader from "./Loader.jsx";
function ScoreRankingPage({countries, userRanking}) {
    const [filters, setFilters] = useState({
        starRange: {min: null, max: null},
        country: {id: "All", name: "All countries"},
        amount: 10
    });
    const [isLoading, setIsLoading] = useState(false);
    const [isError, setIsError] = useState(false);

    const [currentPage, setCurrentPage] = useState(1);
    const [pageCount, setPageCount] = useState(Math.ceil(userRanking.count / 10));
    const [userRankings, setUserRankings] = useState(userRanking.userRankings);

    async function getRankings(filterOptions, pageNumber = 1) {
        setIsLoading(true);
        setIsError(false);
        
        const params = new URLSearchParams();

        if (filterOptions.starRange.min !== null) {
            params.append("minStars", filterOptions.starRange.min);
        }
        if (filterOptions.starRange.max !== null) {
            params.append("maxStars", filterOptions.starRange.max );
        }

        if (filters.country.id !== "All") {
            params.append("countryCode", filters.country.id);
        }

        params.append("page", pageNumber);
        
        try {
            const response = await fetch(`/api/scores/millions?` + params.toString(), {
                method: "GET",
                headers: { "Accept": "application/json" },
            });

            if (response.ok) {
                const json = await response.json();
                setUserRankings(json.userRankings);

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
                setCurrentPage(1);
                setPageCount(0);
                setIsError(true);
            }
        }
        catch (error) {
            setCurrentPage(1);
            setPageCount(0);
            setIsError(true);
        }
        
        setIsLoading(false);
    }

    return (<>
        <h1 className="section-header">Filter scores:</h1>
        <div className="component-container">
            <ManiaRankingFilters filters={filters} setFilters={setFilters} countries={countries}/>
        </div>
        <button className="calc-button" onClick={async () => await getRankings(filters, currentPage)}>
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
                        <Pagination pages={pageCount} onPageChange={async (newPage) => await getRankings(filters, newPage)}/>
                    </>
                ))}
        </div>
    </>)
}

export default ScoreRankingPage;