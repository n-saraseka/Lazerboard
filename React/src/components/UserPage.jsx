import ScoresGrid from './ScoresGrid';
import ScoresTable from './ScoresTable';
import ScoreFilters from './ScoreFilters';
import {useState, useEffect, useCallback} from "react";
import Pagination from "./Pagination.jsx";
import UserCard from "./UserCard.jsx";
import {dateStringFromDatetime} from "../utils/datetime-things.js";
import Error from "./Error.jsx";
import Loader from "./Loader.jsx";
import {assembleSearchParams} from "../utils/score-things.js";
import UserStats from "./UserStats.jsx";
import CollapseUncollapseButton from "./CollapseUncollapseButton.jsx";

function UserPage({user, scores, count, pages}) {
    const currentDate = new Date().toISOString().split("T")[0];
    
    const [filters, setFilters] = useState({
        view: 'cards',
        modes: Array(4).fill(0).map((m, i) => {
            return { value: i, enabled: true };
        }),
        dateRange: {
            min: '',
            max: ''
        },
        rankRange: {min: null, max: null},
        ppRange: {min: null, max: null},
        accRange: {min: null, max: null},
        rateRange: {min: null, max: null},
        mods: [],
        lenientMode: true,
        sortBy: 'date',
        sortDir: 'desc',
        amount: 25,
    });

    const getUserStats = useCallback(async () => {
        setStatsLoading(true);
        setStatsError(false);

        const params = new URLSearchParams();

        try {
            const response = await fetch(`/api/users/${user.id}/data?` + params.toString(), {
                method: "GET",
                headers: { "Accept": "application/json" },
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
    }, [user.id])
    
    useEffect( () => {
        getUserStats();
    }, [getUserStats]);
    
    const [currentPage, setCurrentPage] = useState(1);
    const [paginationFlag, setPaginationFlag] = useState(false); // blinks when pagination should reset
    const [scoresCount, setScoresCount] = useState(count);
    const [pageCount, setPageCount] = useState(pages);
    const [allScores, setAllScores] = useState(scores);
    const [isLoading, setIsLoading] = useState(false);
    const [isError, setIsError] = useState(false);
    
    const [userData, setUserData] = useState(null);
    const [showUserData, setShowUserData] = useState(true);
    const [statsLoading, setStatsLoading] = useState(true);
    const [statsError, setStatsError] = useState(false);

    let dateRangeString = '';
    if (filters.dateRange.min !== '' || filters.dateRange.max !== '') {
        dateRangeString = ' from ';
        const dateStrings = [];
        dateStrings.push(filters.dateRange.min === '' ? '' : (filters.dateRange.min === currentDate ? 'today' : dateStringFromDatetime(filters.dateRange.min)))
        dateStrings.push((filters.dateRange.max === '' || filters.dateRange.max === currentDate)  ? 'today' : dateStringFromDatetime(filters.dateRange.max));
        if (dateStrings[0] === '') {
            dateRangeString += `until ${dateStrings[1]}`;
        }
        else {
            dateRangeString += dateStrings[0] === dateStrings[1] ? dateStrings[0] : 'between ' + dateStrings.join(' and ');
        }
    }

    async function getScores(filterOptions, couldChangePagination, pageNumber = 1) {
        setIsLoading(true);
        setIsError(false);
        
        const actualPage = couldChangePagination ? 1 : pageNumber;
        if (couldChangePagination) setPaginationFlag(!paginationFlag);
        
        const params = new URLSearchParams();
        assembleSearchParams(params, filterOptions, actualPage);
        
        try {
            const response = await fetch(`/api/users/${user.id}/scores?` + params.toString(), {
                method: "GET",
                headers: { "Accept": "application/json" },
            });

            if (response.ok) {
                const json = await response.json();
                setAllScores(json.scores);
                setScoresCount(json.count);

                const pages = Math.ceil(json.count / filterOptions.amount);
                if (actualPage !== currentPage) {
                    setCurrentPage(actualPage);
                }
                if (actualPage > pages) {
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
    }

    return (<>
        <div className="card-stats">
            <div className="card-stats-column">
                <UserCard user={user} scoreCount={count}/>
                <div className="component-container">
                    <CollapseUncollapseButton onCollapseUncollapse={() => setShowUserData(!showUserData)} isCollapsed={!showUserData}/>
                </div>
            </div>
            {statsError
                ? (<Error/>)
                : (statsLoading
                    ? (<Loader/>)
                    : (<UserStats data={userData} isCollapsed={!showUserData}/>))
            }
        </div>
        <h1 className="section-header">Score filters:</h1>
        <div className="component-container">
            <ScoreFilters isUser={true} filters={filters} setFilters={setFilters} refetchScores={ async (newFilters, couldChangePagination) =>
                await getScores(newFilters, couldChangePagination, currentPage)}/>
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
        <Pagination key={paginationFlag} pages={pageCount} onPageChange={async (newPage) => await getScores(filters, false, newPage)}/>
    </>)
}

export default UserPage;