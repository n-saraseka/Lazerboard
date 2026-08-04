import {modeEnumToString} from "../utils/beatmap-things.js";

function ScoreFilters({filters, setFilters, refetchScores}) {
    const currentDate = new Date().toISOString().split("T")[0];
    
    return (<>
        <table className="options">
            <tbody>
            <tr>
                <td>View:</td>
                <td>
                    <div className="filter-container">
                        <div className={`view-filter${filters.view === "cards" ? " view-active" : ""}`}
                             onClick={() => setFilters({...filters, view: "cards"})}>
                            <div className="view-icon view-cards"></div>
                            <span>Card</span>
                        </div>
                        <div className={`view-filter${filters.view === "table" ? " view-active" : ""}`}
                             onClick={() => setFilters({...filters, view: "table"})}>
                            <div className="view-icon view-table"></div>
                            <span>Table</span>
                        </div>
                    </div>
                </td>
            </tr>
            <tr>
                <td>Sort:</td>
                <td>
                    <div className="filter-container">
                        <select name="scores-sort" id="scores-sort" value={filters.sortBy} onChange={(e) => {
                            const allFilters = {...filters, sortBy: e.target.value};
                            setFilters(allFilters);
                            refetchScores(allFilters);
                        }}>
                            <option value="pp">PP</option>
                            <option value="totalScore">Standardized score</option>
                            <option value="classicTotalScore">Classic score</option>
                            <option value="date">Date and time set</option>
                        </select>
                        <span className="sort-thingy" onClick={() => {
                            const allFilters = {...filters, sortDir: filters.sortDir === "asc" ? "desc" : "asc"};
                            setFilters(allFilters);
                            refetchScores(allFilters);
                        }}>
                        {filters.sortDir === "asc" ? "↑" : "↓"}
                        </span>
                    </div>
                </td>
            </tr>
            <tr>
                <td>Date:</td>
                <td>
                    <div className="filter-container">
                        <span>From:</span>
                        <input type="date" value={filters.dateStart} max={currentDate} onChange={(e) => {
                            const allFilters = {...filters, dateStart: e.target.value};
                            setFilters(allFilters);
                            refetchScores(allFilters);
                        }}/>
                        <span>to:</span>
                        <input type="date" value={filters.dateEnd} max={currentDate}
                               onChange={(e) => {
                                   const allFilters = {...filters, dateEnd: e.target.value};
                                   setFilters(allFilters);
                                   refetchScores(allFilters);
                               }}/>
                    </div>
                </td>
            </tr>
            <tr>
                <td>Modes:</td>
                <td>
                    <div className="filter-container">
                        {filters.modes.map((mode, index) => (
                            <div className={`mode-icon-wrapper ${mode.enabled ? "enabled" : "disabled"}`} key={index}>
                                <div className={`mode-icon mode-${modeEnumToString(index)}`} onClick={() =>{
                                    const allFilters = {...filters, modes: filters.modes.map((m, i) => {
                                            if (i === index) {
                                                m.enabled = !m.enabled;
                                            }
                                            return m;
                                        })};
                                    setFilters(allFilters);
                                    refetchScores(allFilters);
                                }}></div>
                            </div>
                        ))}
                    </div>
                </td>
            </tr>
            <tr>
                <td>Count:</td>
                <td>
                    <select name="scores-amount" id="scores-amount" value={filters.scoresAmount} onChange={(e) => {
                        const allFilters = {...filters, scoresAmount: e.target.value};
                        setFilters(allFilters);
                        refetchScores(allFilters);
                    }}>
                        <option value="25">25</option>
                        <option value="50">50</option>
                        <option value="100">100</option>
                    </select>
                </td>
            </tr>
            </tbody>
        </table>
    </>)
}

export default ScoreFilters;