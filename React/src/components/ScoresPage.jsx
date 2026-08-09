import ScoresGrid from './ScoresGrid';
import ScoresTable from './ScoresTable';
import ScoreFilters from './ScoreFilters';
import {useState} from "react";
import Pagination from "./Pagination.jsx";
import {dateStringFromDatetime} from "../utils/datetime-things.js";
import Error from "./Error.jsx";
import Loader from "./Loader.jsx";

function ScoresPage({scores, pages}) {
    const currentDate = new Date().toISOString().split("T")[0];
    
    const [filters, setFilters] = useState({
        view: 'cards',
        scoresAmount: 25,
        sortBy: 'pp',
        sortDir: 'desc',
        dateStart: '',
        dateEnd: '',
        modes: Array(4).fill(0).map((m, i) => {
            return { value: i, enabled: true };
        })
    });
    const [currentPage, setCurrentPage] = useState(1);
    const [pageCount, setPageCount] = useState(pages);
    const [allScores, setAllScores] = useState(scores);
    const [isLoading, setIsLoading] = useState(false);
    const [isError, setIsError] = useState(false);
    
    async function getScores(filterOptions, pageNumber) {
        setIsLoading(true);
        setIsError(false);
        
        const params = new URLSearchParams();
        filterOptions.modes.forEach((mode) => {
            if (mode.enabled) {
                params.append("modes", mode.value.toString());
            }
        })
        params.append("amount", filterOptions.scoresAmount.toString());
        params.append("sort", filterOptions.sortBy);
        params.append("isDesc", (filterOptions.sortDir === "desc").toString());
        if (filterOptions.dateStart !== '') {
            params.append("dateStart", filterOptions.dateStart);
        }
        if (filterOptions.dateEnd !== '') {
            params.append("dateEnd", filterOptions.dateEnd);
        }
        params.append("page", pageNumber ?? currentPage);

        setIsLoading(true);
        
        try {
            const response = await fetch("/api/scores?" + params.toString(), {
                method: "GET",
                headers: { "Accept": "application/json" },
            });

            if (response.ok) {
                const json = await response.json();
                setAllScores(json.scores);

                const pages = Math.ceil(json.count / filterOptions.scoresAmount);
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

    let dateRangeString;
    if (filters.dateStart === '' && filters.dateEnd === '') {
        dateRangeString = ' from today';
    }
    else {
        dateRangeString = ' from ';
        const dateStrings = [];
        [filters.dateStart, filters.dateEnd].forEach((dateFilter) => {
            dateStrings.push((dateFilter === '' || dateFilter === currentDate)  ? 'today' : dateStringFromDatetime(dateFilter));
        });
        dateRangeString += dateStrings[0] === dateStrings[1] ? dateStrings[0] : 'between ' + dateStrings.join(' and ');
    }
    
    return (<>
        <h1 className="section-header">Filter scores:</h1>
        <div className="component-container">
            <ScoreFilters filters={filters} setFilters={setFilters} refetchScores={ async (newFilters) =>
                await getScores(newFilters, currentPage)}/>
        </div>
        <h1 className="section-header">{`All scores${dateRangeString}:`}</h1>
        <div className="component-container">
            {isError
                ? (<Error/>)
                : (isLoading
                    ? (<Loader/>)
                    : filters.view === 'cards'
                            ? <ScoresGrid scores={allScores} usingStandardized={true}/>
                            : <ScoresTable scores={allScores} usingStandardized={true}/>)}
        </div>
        <Pagination pages={pageCount} onPageChange={async (newPage) => await getScores(filters, newPage)}/>
    </>)
}

export default ScoresPage;