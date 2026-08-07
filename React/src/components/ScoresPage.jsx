import ScoresGrid from './ScoresGrid';
import ScoresTable from './ScoresTable';
import ScoreFilters from './ScoreFilters';
import {useState} from "react";
import Pagination from "./Pagination.jsx";
import {dateStringFromDatetime} from "../utils/datetime-things.js";

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
    
    async function getScores(filterOptions, pageNumber) {
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
        const response = await fetch("/api/scores?" + params.toString(), {
            method: "GET",
            headers: { "Accept": "application/json" },
        });

        if (response.ok) {
            const json = await response.json();
            if (pageNumber !== undefined && pageNumber !== currentPage) {
                setCurrentPage(pageNumber);
            }
            setAllScores(json.scores);
            setPageCount(Math.ceil(json.count / filterOptions.scoresAmount));
        }
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
        <h1 className="score-filters">Filter scores:</h1>
        <div className="component-container">
            <ScoreFilters filters={filters} setFilters={setFilters} refetchScores={ async (newFilters) =>
                await getScores(newFilters)}/>
        </div>
        <h1 className="score-range">{`All scores${dateRangeString}:`}</h1>
        <div className="component-container">
            {filters.view === 'cards'
                ? <ScoresGrid scores={allScores} usingStandardized={true}/>
                : <ScoresTable scores={allScores} usingStandardized={true}/>}
        </div>
        <Pagination pages={pageCount} onPageChange={async (newPage) => await getScores(filters, newPage)}/>
    </>)
}

export default ScoresPage;