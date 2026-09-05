import ScoresGrid from '../Scores/ScoresGrid';
import ScoresTable from '../Scores/ScoresTable';
import ScoreFilters from '../Filters/ScoreFilters';
import {useState, useEffect, useCallback, useMemo} from "react";
import Pagination from "../Misc/Pagination.jsx";
import UserCard from "../User/UserCard.jsx";
import {dateStringFromDatetime} from "../../utils/datetime-things.js";
import Error from "../Misc/Error.jsx";
import Loader from "../Misc/Loader.jsx";
import {createScoreQueryCommand} from "../../utils/score-things.js";
import UserStats from "../User/UserStats.jsx";
import CollapseUncollapseButton from "../Inputs/CollapseUncollapseButton.jsx";
import {debounce} from "../../utils/server-things.js";

function UserPage({user}) {
    const currentDate = new Date().toISOString().split("T")[0];
    
    const [filters, setFilters] = useState({
        view: 'cards',
        modes: Array(4).fill(0).map((m, i) => {
            return { value: i, enabled: true };
        }),
        dateRange: {min: null, max: null},
        rankRange: {min: null, max: null},
        ppRange: {min: null, max: null},
        accRange: {min: null, max: null},
        rateRange: {min: null, max: null},
        includeMods: [],
        excludeMods: [],
        lenientMode: true,
        sortBy: 'date',
        sortDir: 'desc',
        amount: 25,
    });
    
    const [currentPage, setCurrentPage] = useState(1);
    const [scoresCount, setScoresCount] = useState(null);
    const [pageCount, setPageCount] = useState(1);
    const [allScores, setAllScores] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const [isError, setIsError] = useState(false);
    
    const [userData, setUserData] = useState({
        history: null,
        ranks: null,
        stars: null,
        speed: null
    });
    const [showUserData, setShowUserData] = useState(true);
    const [statsLoading, setStatsLoading] = useState({
        history: true,
        ranks: true,
        stars: true,
        speed: true
    });
    const [statsError, setStatsError] = useState({
        history: false,
        ranks: false,
        stars: false,
        speed: false,
    });
    
    const getUserHistory = useCallback(async () => {
        setStatsLoading(prevState => ({...prevState, history: true}));
        setStatsError(prevState => ({...prevState, history: false}));

        const params = new URLSearchParams();

        try {
            const response = await fetch(`/api/users/${user.id}/history?` + params.toString(), {
                method: "GET",
                headers: { "Accept": "application/json" },
            });

            if (response.ok) {
                const json = await response.json();
                setUserData(prevState => ({...prevState, history: json.history}));
            }
            else {
                setStatsError(prevState => ({...prevState, history: true}));
            }
        }
        catch (error) {
            setUserData(prevState => ({...prevState, history: null}));
            setStatsError(prevState => ({...prevState, history: true}));
        }

        setStatsLoading(prevState => ({...prevState, history: false}));
    }, [])

    useEffect( () => {
        getUserHistory();
    }, []);

    const getUserRanks = useCallback(async () => {
        setStatsLoading(prevState => ({...prevState, ranks: true}));
        setStatsError(prevState => ({...prevState, ranks: false}));

        const params = new URLSearchParams();

        try {
            const response = await fetch(`/api/users/${user.id}/rankdistribution?` + params.toString(), {
                method: "GET",
                headers: { "Accept": "application/json" },
            });

            if (response.ok) {
                const json = await response.json();
                setUserData(prevState => ({...prevState, ranks: json.rankStats}));
            }
            else {
                setStatsError(prevState => ({...prevState, ranks: true}));
            }
        }
        catch (error) {
            setUserData(prevState => ({...prevState, ranks: null}));
            setStatsError(prevState => ({...prevState, ranks: true}));
        }

        setStatsLoading(prevState => ({...prevState, ranks: false}));
    }, [])

    useEffect( () => {
        getUserRanks();
    }, []);

    const getUserStars = useCallback(async () => {
        setStatsLoading(prevState => ({...prevState, stars: true}));
        setStatsError(prevState => ({...prevState, stars: false}));

        const params = new URLSearchParams();

        try {
            const response = await fetch(`/api/users/${user.id}/stardistribution?` + params.toString(), {
                method: "GET",
                headers: { "Accept": "application/json" },
            });

            if (response.ok) {
                const json = await response.json();
                setUserData(prevState => ({...prevState, stars: json.starStats}));
            }
            else {
                setStatsError(prevState => ({...prevState, stars: true}));
            }
        }
        catch (error) {
            setUserData(prevState => ({...prevState, stars: null}));
            setStatsError(prevState => ({...prevState, stars: true}));
        }

        setStatsLoading(prevState => ({...prevState, stars: false}));
    }, [])

    useEffect( () => {
        getUserStars();
    }, []);

    const getUserSpeed = useCallback(async () => {
        setStatsLoading(prevState => ({...prevState, speed: true}));
        setStatsError(prevState => ({...prevState, speed: false}));

        const params = new URLSearchParams();

        try {
            const response = await fetch(`/api/users/${user.id}/speeddistribution?` + params.toString(), {
                method: "GET",
                headers: { "Accept": "application/json" },
            });

            if (response.ok) {
                const json = await response.json();
                setUserData(prevState => ({...prevState, speed: json.speedStats}));
            }
            else {
                setStatsError(prevState => ({...prevState, speed: true}));
            }
        }
        catch (error) {
            setUserData(prevState => ({...prevState, speed: null}));
            setStatsError(prevState => ({...prevState, speed: true}));
        }

        setStatsLoading(prevState => ({...prevState, speed: false}));
    }, [])

    useEffect( () => {
        getUserSpeed();
    }, []);

    const getScores = useCallback(async (filterOptions, pageNumber = 1) => {
        setIsLoading(true);
        setIsError(false);

        const command = createScoreQueryCommand(filterOptions);

        const params = new URLSearchParams();
        params.append("amount", filters.amount.toString());
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
                setScoresCount(0);
                setCurrentPage(1);
                setPageCount(0);
                setIsError(true);
            }
        }
        catch (error) {
            setScoresCount(0);
            setCurrentPage(1);
            setPageCount(0);
            setIsError(true);
        }

        setIsLoading(false);
    }, [currentPage]);

    useEffect( () => {
        getScores(filters);
    }, []);

    const debouncedGetScores = useMemo(
        () => debounce(getScores, 250),
        [currentPage]
    );

    let dateRangeString = '';
    if (filters.dateRange.min !== null || filters.dateRange.max !== null) {
        dateRangeString = ' from ';
        const dateStrings = [];
        dateStrings.push(filters.dateRange.min === null ? '' : (filters.dateRange.min === currentDate ? 'today' : dateStringFromDatetime(filters.dateRange.min)))
        dateStrings.push((filters.dateRange.max === null || filters.dateRange.max === currentDate)  ? 'today' : dateStringFromDatetime(filters.dateRange.max));
        if (dateStrings[0] === '') {
            dateRangeString += `until ${dateStrings[1]}`;
        }
        else {
            dateRangeString += dateStrings[0] === dateStrings[1] ? dateStrings[0] : 'between ' + dateStrings.join(' and ');
        }
    }

    return (<>
        <div className="card-stats">
            <div className="card-stats-column">
                <UserCard user={user} scoreCount={scoresCount}/>
                <div className="component-container">
                    <CollapseUncollapseButton onCollapseUncollapse={() => setShowUserData(!showUserData)} isCollapsed={!showUserData} entityName="statistics"/>
                </div>
            </div>
            <div className={`card-stats-column ${!showUserData ? 'collapsed' : ''}`}>
                <UserStats data={userData} loadingData={statsLoading} errorData={statsError}/>
            </div>
        </div>
        <h1 className="section-header">Score filters:</h1>
        <div className="component-container">
            <ScoreFilters isUser={true} filters={filters} setFilters={setFilters} 
                          refetchScores={(newFilters) => debouncedGetScores(newFilters, currentPage)}/>
        </div>
        <h1 className="section-header">{`All scores${dateRangeString}:`}</h1>
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

export default UserPage;