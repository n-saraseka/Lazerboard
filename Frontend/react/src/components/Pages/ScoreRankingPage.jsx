import {useState, useMemo, useContext, useEffect} from "react";
import Filters from "../Filters/Filters.jsx";
import ScoreRankingTable from "../Rankings/ScoreRankingTable.jsx";
import Pagination from "../Misc/Pagination.jsx";
import Error from "../Misc/Error.jsx";
import Loader from "../Misc/Loader.jsx";
import {createScoreQueryCommand} from "../../utils/score-things.js";
import {debounce} from "../../utils/server-things.js";
import {PreferencesContext} from "../Preferences/PreferencesContext.jsx";

function ScoreRankingPage({countries}) {
    const { preferences, setPreferences } = useContext(PreferencesContext);
    const globalPreferences = useMemo(() => preferences.globalPreferences ?? {
        modes: Array(4).fill(0).map((m, i) => {
            return { value: i, enabled: true };
        }),
        excludeConverts: true,
        amount: 25
    }, [preferences.globalPreferences]);
    const rankingPreferences = useMemo(() => preferences.rankingPreferences ?? {
        amount: 10
    }, [preferences.rankingPreferences]);
    
    const [filters, setFilters] = useState({
        rankRange: {min: null, max: null},
        ppRange: {min: null, max: null},
        accRange: {min: null, max: null},
        rateRange: {min: null, max: null},
        country: null,
        modes: globalPreferences.modes,
        excludeConverts: globalPreferences.excludeConverts,
        amount: rankingPreferences.amount,
        beatmapStatus: 'all'
    });
    
    useEffect(() => setPreferences({...preferences,
        globalPreferences: {...globalPreferences,
            modes: filters.modes,
            excludeConverts: filters.excludeConverts,
            beatmapStatus: filters.beatmapStatus},
        rankingPreferences: {...rankingPreferences, 
            amount: filters.amount}
    }), [filters]);
    
    const [isLoading, setIsLoading] = useState(false);
    const [isError, setIsError] = useState(false);
    
    const [currentPage, setCurrentPage] = useState(1);
    const [morePagesAvailable, setMorePagesAvailable] = useState(false);
    const [userRankings, setUserRankings] = useState([]);

    async function getRankings(filterOptions, pageNumber = 1) {
        setIsLoading(true);
        setIsError(false);

        const command = createScoreQueryCommand(filterOptions);

        const params = new URLSearchParams();
        params.append("amount", filterOptions.amount.toString());
        params.append("page", pageNumber.toString());

        try {
            const response = await fetch(`/api/ranking?` + params.toString(), {
                method: "POST",
                body: JSON.stringify(command),
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
            });

            if (response.ok) {
                const json = await response.json();
                const rankings = json.userRankings;
                setUserRankings(rankings);

                if (pageNumber !== currentPage) {
                    if (rankings.length === 0) {
                        setCurrentPage(currentPage + 1);
                    }
                    else {
                        setCurrentPage(pageNumber);
                    }
                }

                setMorePagesAvailable(json.moreAvailable);
            }
            else {
                setCurrentPage(1);
                setMorePagesAvailable(false);
                setIsError(true);
            }
        }
        catch (error) {
            setCurrentPage(1);
            setMorePagesAvailable(false);
            setIsError(true);
        }

        setIsLoading(false);
    }

    const debouncedGetRankings = useMemo(
        () => debounce(getRankings, 300),
        [currentPage]
    );
    
    return (<>
        <h1 className="section-header">Filter rankings:</h1>
        <div className="component-container">
            <Filters filters={filters} setFilters={setFilters} isRanking={true} countries={countries}/>
        </div>
        <button className="calc-button" disabled={isLoading}
                onClick={() => debouncedGetRankings(filters, 1)}>
            Get ranking
        </button>
        <h1 className="section-header">User ranking:</h1>
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
        {(userRankings.length > 0 || currentPage > 1) && 
            (<Pagination page={currentPage} 
                         moreAvailable={morePagesAvailable}
                         onPageChange={(newPage) => debouncedGetRankings(filters, newPage)}/>)
        }
    </>)
}

export default ScoreRankingPage;
