import ScoresGrid from './ScoresGrid';
import ScoresTable from './ScoresTable';
import ScoreFilters from './ScoreFilters';
import {useState} from "react";
import Pagination from "./Pagination.jsx";

function ScoresPage({scores, pages}) {
    const [filters, setFilters] = useState({
        view: 'cards',
        scoresAmount: 25,
        sortBy: 'pp',
        sortDir: 'desc',
        dateStart: '',
        dateEnd: ''
    });
    const [currentPage, setCurrentPage] = useState(1);
    const [pageCount, setPageCount] = useState(pages);
    const [allScores, setAllScores] = useState(scores);
    
    async function getScores(mode = null, filterOptions, pageNumber) {
        const params = new URLSearchParams();
        if (mode !== null) {
            params.append("mode", mode.toString());
        }
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
    
    return (<>
        <ScoreFilters filters={filters} setFilters={setFilters} refetchScores={ async (newFilters) =>
            await getScores(null, newFilters)}/>
        {filters.view === 'cards' 
            ? <ScoresGrid scores={allScores} usingStandardized={true}/> 
            : <ScoresTable scores={allScores} usingStandardized={true}/>}
        <Pagination pages={pageCount} onPageChange={async (newPage) => await getScores(null, filters, newPage)}/>
    </>)
}

export default ScoresPage;