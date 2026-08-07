import {modeEnumToString} from "../utils/beatmap-things.js";
import CountrySelector from "./CountrySelector.jsx";
import ModSelector from "./ModSelector.jsx";
import {allMods} from "../utils/score-things.js";
import {useMemo} from "react";

function ScoreRankingFilters({filters, setFilters, countries}) {
    const possibleMods = useMemo(() => {
        let arr = [];
        for (const key of Object.keys(allMods)) {
            const matchingMods = allMods[key].filter(m => filters.modes.some(mode => mode.enabled && m.modes.includes(mode.value)));
            matchingMods.forEach(m => {
                arr.push(m.acronym);
            });
        }
        return arr;
    }, [filters]);
    
    function updateMods(mod) {
        console.log(mod);
        const newMods = filters.mods.includes(mod) ? filters.mods.filter(m => m !== mod) : filters.mods.concat(mod);
        setFilters({...filters, mods: newMods});
    }
    
    return (<>
        <table className="options">
            <tbody>
            <tr>
                <td>Rank:</td>
                <td>
                    <div className="filter-container">
                        <span>From:</span>
                        <input type="number" step={1} min={1} max={100} value={filters.rankRange.min} onChange={(e) => {
                            const allFilters = {...filters, rankRange: {...filters.rankRange, min: e.target.value}};
                            setFilters(allFilters);
                        }}/>
                        <span>to:</span>
                        <input type="number" step={1} min={1} max={100} value={filters.rankRange.max} onChange={(e) => {
                            const allFilters = {...filters, rankRange: {...filters.rankRange, max: e.target.value}};
                            setFilters(allFilters);
                        }}/>
                    </div>
                </td>
            </tr>
            <tr>
                <td>PP:</td>
                <td>
                    <div className="filter-container">
                        <span>From:</span>
                        <input type="number" step={1} min={1} max={3000} value={filters.ppRange.min ?? ""} onChange={(e) => {
                            const allFilters = {...filters, ppRange: {...filters.ppRange, min: e.target.value}};
                            setFilters(allFilters);
                        }}/>
                        <span>to:</span>
                        <input type="number" step={1} min={1} max={3000} value={filters.ppRange.max ?? ""} onChange={(e) => {
                            const allFilters = {...filters, ppRange: {...filters, max: e.target.value}};
                            setFilters(allFilters);
                        }}/>
                    </div>
                </td>
            </tr>
            <tr>
                <td>Total score:</td>
                <td>
                    <div className="filter-container">
                        <span>From:</span>
                        <input type="number" step={50000} min={0} max={1e6} value={filters.scoreRange.min ?? ""} onChange={(e) => {
                            const allFilters = {...filters, scoreRange: {...filters.scoreRange, min: e.target.value}};
                            setFilters(allFilters);
                        }}/>
                        <span>to:</span>
                        <input type="number" step={50000} min={0} max={1e6} value={filters.scoreRange.max ?? ""} onChange={(e) => {
                            const allFilters = {...filters, scoreRange: {...filters.scoreRange, max: e.target.value}};
                            setFilters(allFilters);
                        }}/>
                    </div>
                </td>
            </tr>
            <tr>
                <td>Mode:</td>
                <td>
                    <div className="filter-container">
                        {filters.modes.map((mode, index) => (
                            <div className={`mode-icon-wrapper ${mode.enabled ? "enabled" : "disabled"}`} key={index}>
                                <div className={`mode-icon mode-${modeEnumToString(index)}`} onClick={() => {
                                    let legitMods = [];
                                    for (const key of Object.keys(allMods)) {
                                        const matchingMods = allMods[key].filter(m => filters.modes.some(mode => mode.enabled && m.modes.includes(mode.value)));
                                        matchingMods.forEach(m => {
                                            legitMods.push(m.acronym);
                                        });
                                    }
                                    const newMods = filters.mods.filter(m => legitMods.includes(m));
                                    const allFilters = {...filters, modes: filters.modes.map((m, i) => {
                                            if (i === index) {
                                                m.enabled = !m.enabled;
                                            }
                                            return m;
                                        }), 
                                        mods: newMods};
                                    setFilters(allFilters);
                                }}></div>
                            </div>
                        ))}
                    </div>
                </td>
            </tr>
            <tr>
                <td>Mods:</td>
                <td>
                    <div className="filter-container">
                        <ModSelector availableMods={possibleMods} mods={filters.mods} setMods={(mod) => updateMods(mod)}/>
                        <label htmlFor="lenientMode">Allow other mods:
                            <input name="lenientMode" id="lenientMode" type="checkbox" checked={filters.lenientMode} onClick={() => 
                                setFilters({...filters, lenientMode: !filters.lenientMode})}/>
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

export default ScoreRankingFilters;