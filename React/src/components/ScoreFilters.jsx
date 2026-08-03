function ScoreFilters({filters, setFilters, refetchScores}) {
    
    return (<div className="options">
        <label htmlFor="scores-view">View type:
            <select name="scores-view" id="scores-view" value={filters.view} onChange={(e) => {
                setFilters({...filters, view: e.target.value});
                const allFilters = {...filters, view: e.target.value};
                refetchScores(allFilters);
            }}>
                <option value="cards">Cards</option>
                <option value="condensed">Condensed</option>
            </select>
        </label>
        <label htmlFor="scores-amount">Scores per page:
            <select name="scores-amount" id="scores-amount" value={filters.scoresAmount} onChange={(e) => {
                setFilters({...filters, scoresAmount: e.target.value});
                const allFilters = {...filters, scoresAmount: e.target.value};
                refetchScores(allFilters);
            }}>
                <option value="25">25</option>
                <option value="50">50</option>
                <option value="75">75</option>
                <option value="100">100</option>
            </select>
        </label>
        <label htmlFor="scores-sort">Sort by:
            <select name="scores-sort" id="scores-sort" value={filters.sortBy} onChange={(e) => {
                setFilters({...filters, sortBy: e.target.value});
                const allFilters = {...filters, sortBy: e.target.value};
                refetchScores(allFilters);
            }}>
                <option value="pp">PP</option>
                <option value="totalScore">Standardized score</option>
                <option value="classicTotalScore">Classic score</option>
                <option value="date">Date and time set</option>
            </select>
        </label>
        <label htmlFor="scores-sort-direction">Sort direction:
            <select name="scores-sort-direction" id="scores-sort-direction" value={filters.sortDir} onChange={(e) => {
                setFilters({...filters, sortDir: e.target.value});
                const allFilters = {...filters, sortDir: e.target.value};
                refetchScores(allFilters);
            }}>
                <option value="desc">Descending</option>
                <option value="asc">Ascending</option>
            </select>
        </label>
        <label htmlFor="date-start"> Show scores starting from:
            <input type="date" value={filters.dateStart} max={new Date().toISOString().split("T")[0]} onChange={(e) => {
                setFilters({...filters, dateStart: e.target.value});
                const allFilters = {...filters, dateStart: e.target.value};
                refetchScores(allFilters);
            }}/>
        </label>
        <label htmlFor="date-start"> Show scores ending at:
            <input type="date" value={filters.dateEnd} max={new Date().toISOString().split("T")[0]} 
                   onChange={(e) => {
                       setFilters({...filters, dateEnd: e.target.value});
                       const allFilters = {...filters, dateEnd: e.target.value};
                       refetchScores(allFilters);
            }}/>
        </label>
    </div>)
}

export default ScoreFilters;