import CountrySelector from "./CountrySelector.jsx";

function ManiaRankingFilters({filters, setFilters, countries}) {
    return (<>
        <table className="options">
            <tbody>
            <tr>
                <td>Star rating:</td>
                <td>
                    <div className="filter-container">
                        <label htmlFor="starMin">From: <input id="starMin" name="starMin" type="number" 
                                                              step={0.1} min={0} max={20} value={filters.starRange.min ?? ''} 
                                                              onChange={(e) => {
                                                                  const allFilters = {...filters, starRange: {...filters.starRange, min: e.target.value}};
                                                                  setFilters(allFilters);
                                                              }}/>
                        </label>
                        <label htmlFor="starMax">to: <input id="starMax" name="starMax" type="number" 
                                                            step={0.1} min={0} max={20} value={filters.starRange.max ?? ''} 
                                                            onChange={(e) => {
                                                                const allFilters = {...filters, starRange: {...filters.starRange, max: e.target.value}};
                                                                setFilters(allFilters);
                                                            }}/>
                        </label>
                    </div>
                </td>
            </tr>
            <tr>
                <td>Country:</td>
                <td>
                    <div className="filter-container">
                        <CountrySelector filters={filters} setFilters={setFilters} countries={countries}/>
                    </div>
                </td>
            </tr>
            <tr>
                <td>Count:</td>
                <td>
                    <select name="scores-amount" id="scores-amount" value={filters.amount} onChange={(e) =>
                        setFilters({...filters, amount: e.target.value})}>
                        <option value="10">10</option>
                        <option value="25">25</option>
                        <option value="50">50</option>
                    </select>
                </td>
            </tr>
            </tbody>
        </table>
    </>)
}

export default ManiaRankingFilters;