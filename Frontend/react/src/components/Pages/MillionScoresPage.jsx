import ScoresGrid from '../Scores/ScoresGrid';
import ScoresTable from '../Scores/ScoresTable';
import Filters from '../Filters/Filters.jsx';
import {useState, useMemo, useEffect, useCallback, useContext} from "react";
import Pagination from "../Misc/Pagination.jsx";
import {dateStringFromDatetime} from "../../utils/datetime-things.js";
import Error from "../Misc/Error.jsx";
import Loader from "../Misc/Loader.jsx";
import {createScoreQueryCommand} from "../../utils/score-things.js";
import {debounce} from "../../utils/server-things.js";
import {PreferencesContext} from "../Preferences/PreferencesContext.jsx";

function MillionScoresPage({countries}) {
    const currentDate = new Date().toISOString().split("T")[0];
    const { preferences, setPreferences } = useContext(PreferencesContext);
    const globalPreferences = useMemo(() => preferences.globalPreferences ?? {
        view: 'cards',
        modes: Array(4).fill(0).map((m, i) => {
            return { value: i, enabled: true };
        }),
        excludeConverts: true,
        beatmapStatus: 'all',
        amount: 25
    }, [preferences.globalPreferences]);

    const [filters, setFilters] = useState({
        view: globalPreferences.view,
        excludeConverts: globalPreferences.excludeConverts,
        dateRange: {min: null, max: null},
        starRange: {min: null, max: null},
        ppRange: {min: null, max: null},
        rateRange: {min: null, max: null},
        country: null,
        includeMods: [],
        excludeMods: [],
        lenientMode: true,
        sortBy: 'pp',
        sortDir: 'desc',
        amount: globalPreferences.amount,
        beatmapStatus: globalPreferences.beatmapStatus,
    });

    useEffect(() => setPreferences({...preferences,
        globalPreferences: {...globalPreferences,
            view: filters.view,
            excludeConverts: filters.excludeConverts,
            amount: filters.amount,
            beatmapStatus: filters.beatmapStatus}
    }), [filters]);

    const [currentPage, setCurrentPage] = useState(1);
    const [scoresCount, setScoresCount] = useState(0);
    const [pageCount, setPageCount] = useState(1);
    const [allScores, setAllScores] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const [isError, setIsError] = useState(false);

    const [dateRangeString, setDateRangeString] = useState(" from today");

    const getScores = useCallback(async (filterOptions, pageNumber = 1) => {
        setIsLoading(true);
        setIsError(false);

        if (filterOptions.dateRange.min === null && filterOptions.dateRange.max === null) {
            setDateRangeString(' from today');
        }
        else {
            let tempString = ' from ';
            const dateStrings = [];
            [filterOptions.dateRange.min, filterOptions.dateRange.max].forEach((dateFilter) => {
                dateStrings.push((dateFilter === null || dateFilter === currentDate)  ? 'today' : dateStringFromDatetime(dateFilter));
            });
            tempString += dateStrings[0] === dateStrings[1] ? dateStrings[0] : 'between ' + dateStrings.join(' and ');
            setDateRangeString(tempString);
        }

        const command = createScoreQueryCommand(filterOptions);

        const params = new URLSearchParams();
        params.append("amount", filterOptions.amount.toString());
        params.append("page", pageNumber.toString());

        setIsLoading(true);

        try {
            const response = await fetch(`/api/scores/millions?` + params.toString(), {
                method: "POST",
                body: JSON.stringify(command),
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
            });

            if (response.ok) {
                const json = await response.json();
                setAllScores(json.scores);
                setScoresCount(json.count);

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
                setScoresCount(0);
                setPageCount(0);
                setIsError(true);
            }
        }
        catch (error) {
            setCurrentPage(1);
            setScoresCount(0);
            setPageCount(0);
            setIsError(true);
        }

        setIsLoading(false);
    }, [currentPage])

    useEffect( () => {
        getScores(filters);
    }, []);

    const debouncedGetScores = useMemo(
        () => debounce(getScores, 250),
        [currentPage]
    );

    return (<>
        <h1 className="section-header">Filter scores:</h1>
        <div className="component-container">
            <Filters filters={filters} setFilters={setFilters} countries={countries}/>
        </div>
        <button className="calc-button" disabled={isLoading}
                onClick={() => debouncedGetScores(filters, currentPage)}>
            Get scores
        </button>
        <h1 className="section-header">{ scoresCount === 0 ? 'No scores to show...' : `All scores${dateRangeString}:` }</h1>
        <div className="component-container">
            {isError
                ? (<Error/>)
                : (isLoading
                    ? (<Loader/>)
                    : scoresCount > 0 && (filters.view === 'cards'
                    ? <ScoresGrid scores={allScores} usingStandardized={true}/>
                    : <ScoresTable scores={allScores} usingStandardized={true}/>))}
        </div>
        <Pagination page={currentPage} pages={pageCount} onPageChange={(newPage) => debouncedGetScores(filters, newPage)}/>
    </>)
}

export default MillionScoresPage;