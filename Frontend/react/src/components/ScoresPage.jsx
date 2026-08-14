import ScoresGrid from './ScoresGrid';
import ScoresTable from './ScoresTable';
import ScoreFilters from './ScoreFilters';
import {useState} from "react";
import Pagination from "./Pagination.jsx";
import {dateStringFromDatetime} from "../utils/datetime-things.js";
import Error from "./Error.jsx";
import Loader from "./Loader.jsx";
import {assembleSearchParams} from "../utils/score-things.js";

function ScoresPage({scores, pages, countries}) {
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
        country: {id: "All", name: "All countries"},
        includeMods: [],
        excludeMods: [],
        lenientMode: true,
        sortBy: 'pp',
        sortDir: 'desc',
        amount: 25,
    });
    
    const [currentPage, setCurrentPage] = useState(1);
    const [scoresCount, setScoresCount] = useState(scores.length);
    const [pageCount, setPageCount] = useState(pages);
    const [allScores, setAllScores] = useState(scores);
    const [isLoading, setIsLoading] = useState(false);
    const [isError, setIsError] = useState(false);

    async function getScores(filterOptions, pageNumber = 1) {
        setIsLoading(true);
        setIsError(false);
        
        const params = new URLSearchParams();
        assembleSearchParams(params, filterOptions, pageNumber);

        setIsLoading(true);
        
        try {
            const response = await fetch(`/api/scores?` + params.toString(), {
                method: "GET",
                headers: { "Accept": "application/json" },
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
    }

    let dateRangeString;
    if (filters.dateRange.min === '' && filters.dateRange.max === '') {
        dateRangeString = ' from today';
    }
    else {
        dateRangeString = ' from ';
        const dateStrings = [];
        [filters.dateRange.min, filters.dateRange.max].forEach((dateFilter) => {
            dateStrings.push((dateFilter === '' || dateFilter === currentDate)  ? 'today' : dateStringFromDatetime(dateFilter));
        });
        dateRangeString += dateStrings[0] === dateStrings[1] ? dateStrings[0] : 'between ' + dateStrings.join(' and ');
    }
    
    return (<>
        <h1 className="section-header">Penis:</h1>
        <div className="component-container">
            <ScoreFilters isUser={false} filters={filters} setFilters={setFilters} countries={countries}
                          refetchScores={ async (newFilters) => await getScores(newFilters, currentPage)}/>
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
        <Pagination page={currentPage} pages={pageCount} onPageChange={async (newPage) => await getScores(filters, newPage)}/>
    </>)
}

export default ScoresPage;