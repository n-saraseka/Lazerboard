import {useState} from "react";
import ManiaRankingFilters from "./ManiaRankingFilters.jsx";
import ScoreRankingTable from "./ScoreRankingTable.jsx";
import Pagination from "./Pagination.jsx";
function ScoreRankingPage({countries, userRanking}) {
    const [filters, setFilters] = useState({
        starRange: {min: null, max: null},
        country: {id: "All", name: "All countries"},
        amount: 10
    });

    const [currentPage, setCurrentPage] = useState(1);
    const [pageCount, setPageCount] = useState(Math.ceil(userRanking.count / 10));
    const [userRankings, setUserRankings] = useState(userRanking.userRankings);
    console.log(userRankings);

    async function getRankings(filterOptions, pageNumber = 1) {
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
    }

    return (<>
        <h1 className="section-header">Filter scores:</h1>
        <div className="component-container">
            <ManiaRankingFilters filters={filters} setFilters={setFilters} countries={countries}/>
        </div>
        <button className="calc-button" onClick={async () => await getRankings(filters, currentPage)}>
            Get ranking
        </button>
        {userRankings.length > 0 && (
            <>
                <h1 className="section-header">User rankings:</h1>
                <div className="component-container">
                    <ScoreRankingTable rankings={userRankings}/>
                    <Pagination pages={pageCount} onPageChange={async (newPage) => await getRankings(filters, newPage)}/>
                </div>
            </>
        )}
    </>)
}

export default ScoreRankingPage;