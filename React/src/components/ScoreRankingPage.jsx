import {useState} from "react";
import ScoreRankingFilters from "./ScoreRankingFilters.jsx";
import ScoreRankingTable from "./ScoreRankingTable.jsx";
import Pagination from "./Pagination.jsx";
import Error from "./Error.jsx";
import Loader from "./Loader.jsx";

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
    
    const [currentPage, setCurrentPage] = useState(1);
    const [pageCount, setPageCount] = useState(0);
    const [userRankings, setUserRankings] = useState([]);

    async function getRankings(filterOptions, pageNumber = 1) {
        setIsLoading(true);
        setIsError(false);
        
        const params = new URLSearchParams();
        filterOptions.modes.forEach((mode) => {
            if (mode.enabled) {
                params.append("modes", mode.value.toString());
            }
        });
        filterOptions.mods.forEach((mod) => {
            params.append("mods", mod);
        })
        params.append("lenientMode", filters.lenientMode.toString());
        params.append("page", pageNumber.toString());
        params.append("amount", filterOptions.amount.toString());

        params.append("rankMin", filterOptions.rankRange.min);
        params.append("rankMax", filterOptions.rankRange.max);
        
        if (filterOptions.ppRange.min !== null) {
            params.append("ppMin", filterOptions.ppRange.min);
        }
        if (filterOptions.ppRange.max !== null) {
            params.append("ppMax", filterOptions.ppRange.max );
        }

        if (filterOptions.accRange.min !== null) {
            params.append("accMin", filterOptions.accRange.min);
        }
        if (filterOptions.accRange.max !== null) {
            params.append("accMax", filterOptions.accRange.max);
        }
        
        if (filterOptions.rateRange.min !== null) {
            params.append("speedMin", filterOptions.rateRange.min);
        }

        if (filterOptions.rateRange.max !== null) {
            params.append("speedMax", filterOptions.rateRange.max);
        }
        
        if (filters.country.id !== "All") {
            params.append("countryCode", filters.country.id);
        }
        
        params.append("page", pageNumber);
        
        try {
            const response = await fetch(`/api/scores/ranking?` + params.toString(), {
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
            <ScoreRankingFilters filters={filters} setFilters={setFilters} countries={countries}/>
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