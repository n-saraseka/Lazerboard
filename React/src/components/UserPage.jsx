import ScoresGrid from './ScoresGrid';
import ScoresTable from './ScoresTable';
import ScoreFilters from './ScoreFilters';
import {useState} from "react";
import Pagination from "./Pagination.jsx";
import UserCard from "./UserCard.jsx";

function UserPage({user, scores, count, pages}) {
    const [filters, setFilters] = useState({
        view: 'cards',
        scoresAmount: 25,
        sortBy: 'date',
        sortDir: 'desc',
        dateStart: '',
        dateEnd: ''
    });
    const [scoreCount, setScoreCount] = useState(count);
    const [pageCount, setPageCount] = useState(pages);
    const [allScores, setAllScores] = useState(scores);

    async function getScores(mode = 0, filterOptions, pageNumber = 1) {
        const params = new URLSearchParams();
        params.append("mode", mode.toString());
        params.append("amount", filterOptions.scoresAmount.toString());
        params.append("sort", filterOptions.sortBy);
        params.append("isDesc", (filterOptions.sortDir === "desc").toString());
        if (filterOptions.dateStart !== '') {
            params.append("dateStart", filterOptions.dateStart);
        }
        if (filterOptions.dateEnd !== '') {
            params.append("dateEnd", filterOptions.dateEnd);
        }
        params.append("page", pageNumber);
        const response = await fetch(`/api/users/${user.id}/scores?` + params.toString(), {
            method: "GET",
            headers: { "Accept": "application/json" },
        });

        if (response.ok) {
            const json = await response.json();
            setAllScores(json.scores);
            setPageCount(Math.ceil(json.count / filterOptions.scoresAmount));
            setScoreCount(json.count);
        }
    }

    return (<>
        <UserCard user={user} scoreCount={scoreCount}/>
        <ScoreFilters filters={filters} setFilters={setFilters} refetchScores={ async (newFilters) => 
            await getScores(0, newFilters)}/>
        {filters.view === 'cards'
            ? <ScoresGrid scores={allScores} usingStandardized={true}/>
            : <ScoresTable scores={allScores} usingStandardized={true}/>}
        <Pagination pages={pageCount} onPageChange={async (newPage) => await getScores(0, filters, newPage)}/>
    </>)
}

export default UserPage;