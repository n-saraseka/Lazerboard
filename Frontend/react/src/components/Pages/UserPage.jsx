import ScoresGrid from '../Scores/ScoresGrid';
import ScoresTable from '../Scores/ScoresTable';
import Filters from '../Filters/Filters';
import {useState, useEffect, useCallback, useMemo, useContext} from "react";
import Pagination from "../Misc/Pagination.jsx";
import UserCard from "../User/UserCard.jsx";
import {dateStringFromDatetime} from "../../utils/datetime-things.js";
import Error from "../Misc/Error.jsx";
import Loader from "../Misc/Loader.jsx";
import {createScoreQueryCommand} from "../../utils/score-things.js";
import UserStats from "../User/UserStats.jsx";
import CollapseUncollapseButton from "../Inputs/CollapseUncollapseButton.jsx";
import {debounce} from "../../utils/server-things.js";
import {PreferencesContext} from "../Preferences/PreferencesContext.jsx";

function UserPage({user}) {
    const currentDate = new Date().toISOString().split("T")[0];
    const { preferences, setPreferences } = useContext(PreferencesContext);
    const globalPreferences = useMemo(() => preferences.globalPreferences ?? {
        modes: Array(4).fill(0).map((m, i) => {
            return { value: i, enabled: true };
        }),
        excludeConverts: true,
        beatmapStatus: 'all',
        amount: 25
    }, [preferences.globalPreferences]);
    const viewPreference = useMemo(() => preferences.view ?? globalPreferences.view ?? 'cards', [preferences.view]);
    
    const [filters, setFilters] = useState({
        modes: globalPreferences.modes,
        excludeConverts: globalPreferences.excludeConverts,
        dateRange: {min: null, max: null},
        rankRange: {min: null, max: null},
        ppRange: {min: null, max: null},
        accRange: {min: null, max: null},
        rateRange: {min: null, max: null},
        sortBy: 'date',
        sortDir: 'desc',
        amount: globalPreferences.amount,
        beatmapStatus: 'all',
    });

    useEffect(() => setPreferences({...preferences,
        globalPreferences: {...globalPreferences,
            modes: filters.modes,
            excludeConverts: filters.excludeConverts,
            amount: filters.amount}
    }), [filters]);
    
    const [currentPage, setCurrentPage] = useState(1);
    const [morePagesAvailable, setMorePagesAvailable] = useState(false);
    const [allScores, setAllScores] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const [isError, setIsError] = useState(false);
    
    const [userData, setUserData] = useState(null);
    const [showUserData, setShowUserData] = useState(true);
    const [statsLoading, setStatsLoading] = useState(true);
    const [statsError, setStatsError] = useState(false);
    
    const [dateRangeString, setDateRangeString] = useState("");
    
    const getUserStats = useCallback(async (filterOptions) => {
        setStatsLoading(true);
        setStatsError(false);

        const command = createScoreQueryCommand(filterOptions);

        const params = new URLSearchParams();

        try {
            const response = await fetch(`/api/users/${user.id}/stats?` + params.toString(), {
                method: "POST",
                body: JSON.stringify(command),
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
            });

            if (response.ok) {
                const json = await response.json();
                setUserData(json);
            }
            else {
                setStatsError(true);
            }
        }
        catch (error) {
            setUserData(null);
            setStatsError(true);
        }

        setStatsLoading(false);
    }, [filters])

    const getScores = useCallback(async (filterOptions, pageNumber = 1) => {
        setIsLoading(true);
        setIsError(false);

        if (filterOptions.dateRange.min !== null || filterOptions.dateRange.max !== null) {
            let tempDateString = ' from ';
            const dateStrings = [];
            dateStrings.push(filterOptions.dateRange.min === null ? '' : (filterOptions.dateRange.min === currentDate ? 'today' : dateStringFromDatetime(filterOptions.dateRange.min)))
            dateStrings.push((filterOptions.dateRange.max === null || filterOptions.dateRange.max === currentDate)  ? 'today' : dateStringFromDatetime(filterOptions.dateRange.max));
            if (dateStrings[0] === '') {
                tempDateString += `until ${dateStrings[1]}`;
            }
            else {
                tempDateString += dateStrings[0] === dateStrings[1] ? dateStrings[0] : 'between ' + dateStrings.join(' and ');
            }
            setDateRangeString(tempDateString);
        }

        const command = createScoreQueryCommand(filterOptions);

        const params = new URLSearchParams();
        params.append("amount", filterOptions.amount.toString());
        params.append("page", pageNumber.toString());

        try {
            const response = await fetch(`/api/users/${user.id}/scores?` + params.toString(), {
                method: "POST",
                body: JSON.stringify(command),
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
            });

            if (response.ok) {
                const json = await response.json();
                const scores = json.scores;
                setAllScores(scores);

                if (pageNumber !== currentPage) {
                    console.log(scores.length);
                    console.log(morePagesAvailable);
                    if (scores.length === 0) {
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
    }, [currentPage]);

    const debouncedGetScores = useMemo(
        () => debounce(getScores, 300),
        [currentPage]
    );

    const getData = useCallback(async (filterOptions, page) => {
        await Promise.all([getScores(filterOptions, page), getUserStats(filterOptions)]);
    }, [currentPage]);

    useEffect( () => {
        getData(filters, 1);
    }, []);

    const debouncedGetData = useMemo(
        () => debounce(getData, 300),
        [currentPage]
    );
    
    return (<>
        <div className="card-stats">
            <div className="card-stats-column">
                <UserCard user={user}/>
                <div className="component-container">
                    <CollapseUncollapseButton onCollapseUncollapse={() => setShowUserData(!showUserData)} isCollapsed={!showUserData} entityName="statistics"/>
                </div>
            </div>
            <div className={`card-stats-column ${!showUserData ? 'collapsed' : ''}`}>
                {statsError 
                    ? (<Error/>)
                    : (statsLoading
                        ? (<Loader/>)
                        : <UserStats data={userData}/>)}
            </div>
        </div>
        <h1 className="section-header">Filter user scores:</h1>
        <div className="component-container">
            <Filters filters={filters} setFilters={setFilters} isRanking={false}/>
        </div>
        <button className="calc-button" disabled={isLoading && statsLoading}
                onClick={() => debouncedGetData(filters, 1)}>
            Get user scores
        </button>
        <h1 className="section-header">{ allScores.length === 0 ? 'No scores to show...' : `All scores${dateRangeString}:` }</h1>
        <div className="component-container">
            {isError
                ? (<Error/>)
                : (isLoading
                    ? (<Loader/>)
                    : allScores.length > 0 && (viewPreference === 'cards'
                        ? <ScoresGrid scores={allScores}/>
                        : <ScoresTable scores={allScores}/>))}
        </div>
        {(allScores.length > 0 || currentPage > 0) &&
            (<Pagination page={currentPage} 
                         moreAvailable={morePagesAvailable} 
                         onPageChange={(newPage) => debouncedGetScores(filters, newPage)}/>)
        }
    </>)
}

export default UserPage;