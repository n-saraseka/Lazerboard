import {useState, useMemo, useCallback, useEffect} from "react";
import ScoreRankingFilters from "../Filters/ScoreRankingFilters.jsx";
import ScoreRankingTable from "../Rankings/ScoreRankingTable.jsx";
import Pagination from "../Misc/Pagination.jsx";
import Error from "../Misc/Error.jsx";
import Loader from "../Misc/Loader.jsx";
import {createScoreQueryCommand} from "../../utils/score-things.js";
import {debounce} from "../../utils/server-things.js";

function ScoreRankingPage({countries}) {
    const [filters, setFilters] = useState({
        rankRange: {min: 1, max: 100},
        ppRange: {min: null, max: null},
        accRange: {min: null, max: null},
        rateRange: {min: null, max: null},
        country: {id: "All", name: "All countries"},
        includeMods: [],
        excludeMods: [],
        lenientMode: true,
        modes: Array(4).fill(0).map((m, i) => {
            return { value: i, enabled: true };
        }),
        amount: 10
    });
    const [isLoading, setIsLoading] = useState(false);
    const [isError, setIsError] = useState(false);
    
    const [currentPage, setCurrentPage] = useState(1);
    const [pageCount, setPageCount] = useState(1);
    const [userRankings, setUserRankings] = useState([]);
    
    const getRankings = useCallback(async (filterOptions, pageNumber = 1) => {
        setIsLoading(true);
        setIsError(false);

        const command = createScoreQueryCommand(filterOptions);

        const params = new URLSearchParams();
        params.append("amount", filters.amount.toString());
        params.append("page", pageNumber.toString());

        try {
            const response = await fetch(`/api/scores/ranking?` + params.toString(), {
                method: "POST",
                body: JSON.stringify(command),
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
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
                setPageCount(0);
                setIsError(true);
            }
        }
        catch (error) {
            setPageCount(0);
            setIsError(true);
        }

        setIsLoading(false);
    }, [currentPage])
    
    useEffect(() => {
        getRankings(filters);
    }, []);

    const debouncedGetRankings = useMemo(
        () => debounce(getRankings, 250),
        [currentPage]
    );
    
    return (<>
        <h1 className="section-header">Score filters:</h1>
        <div className="component-container">
            <ScoreRankingFilters isMania={false} filters={filters} setFilters={setFilters} countries={countries}/>
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