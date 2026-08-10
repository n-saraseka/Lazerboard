import {modeEnumToString} from "../utils/beatmap-things.js";
import CountrySelector from "./CountrySelector.jsx";
import ModSelector from "./ModSelector.jsx";
import {allMods} from "../utils/score-things.js";
import {useMemo, useState} from "react";
import CollapseUncollapseButton from "./CollapseUncollapseButton.jsx";

function ScoreRankingFilters({isMania, filters, setFilters, countries}) {
    const [collapsed, setCollapsed] = useState(true);
    
    const possibleMods = !isMania? useMemo(() => {
        let arr = [];
        for (const key of Object.keys(allMods)) {
            const matchingMods = allMods[key].filter(m => filters.modes.some(mode => mode.enabled && m.modes.includes(mode.value)));
            matchingMods.forEach(m => {
                arr.push(m.acronym);
            });
        }
        return arr;
    }, [filters]) : null;
    
    return (<>
        <CollapseUncollapseButton isCollapsed={collapsed} onCollapseUncollapse={() => setCollapsed(!collapsed)} entityName="filters"/>
        <div className={`options-wrapper ${collapsed? "collapsed" : ""}`}>
            <table className="options">
                <tbody>
                { isMania
                    ? <tr>
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
                    : <>
                        <tr>
                            <td>Modes:</td>
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
                            <td>Rank:</td>
                            <td>
                                <div className="filter-container">
                                    <label htmlFor="rankMin">From: <input id="rankMin" name="rankMin" type="number" step={1}
                                                                          min={1} max={100} value={filters.rankRange.min} onChange={(e) => {
                                        const allFilters = {...filters, rankRange: {...filters.rankRange, min: e.target.value}};
                                        setFilters(allFilters);
                                    }}/>
                                    </label>
                                    <label htmlFor="rankMax">to: <input id="rankMax" name="rankMax" type="number" step={1}
                                                                        min={1} max={100} value={filters.rankRange.max} onChange={(e) => {
                                        const allFilters = {...filters, rankRange: {...filters.rankRange, max: e.target.value}};
                                        setFilters(allFilters);
                                    }}/>
                                    </label>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td>PP:</td>
                            <td>
                                <div className="filter-container">
                                    <label htmlFor="ppMin">From: <input id="ppMin" name="ppMin" type="number" step={1}
                                                                        min={1} max={3000} value={filters.ppRange.min} onChange={(e) => {
                                        const allFilters = {...filters, ppRange: {...filters.ppRange, min: e.target.value}};
                                        setFilters(allFilters);
                                    }}/>
                                    </label>
                                    <label htmlFor="ppMax">to: <input id="ppMax" name="ppMax" type="number" step={1}
                                                                      min={1} max={3000} value={filters.ppRange.max} onChange={(e) => {
                                        const allFilters = {...filters, ppRange: {...filters.ppRange, max: e.target.value}};
                                        setFilters(allFilters);
                                    }}/>
                                    </label>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td>Accuracy:</td>
                            <td>
                                <div className="filter-container">
                                    <label htmlFor="accMin">From: <input id="accMin" name="accMin" type="number" step={0.01}
                                                                         min={1} max={100} value={filters.accRange.min} onChange={(e) => {
                                        const allFilters = {...filters, accRange: {...filters.accRange, min: e.target.value}};
                                        setFilters(allFilters);
                                    }}/>
                                    </label>
                                    <label htmlFor="accMax">to: <input id="accMax" name="accMax" type="number" step={0.01}
                                                                       min={1} max={100} value={filters.accRange.max} onChange={(e) => {
                                        const allFilters = {...filters, accRange: {...filters.accRange, max: e.target.value}};
                                        setFilters(allFilters);
                                    }}/>
                                    </label>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td>Speed:</td>
                            <td>
                                <div className="filter-container">
                                    <label htmlFor="rateMin">From: <input id="rateMin" name="rateMin" type="number" step={1}
                                                                          min={0} max={2} value={filters.rateRange.min} onChange={(e) => {
                                        const allFilters = {...filters, rateRange: {...filters.rateRange, min: e.target.value}};
                                        setFilters(allFilters);
                                    }}/>
                                    </label>
                                    <label htmlFor="rateMax">to: <input id="rateMax" name="rateMax" type="number" step={1}
                                                                        min={0} max={2} value={filters.rateRange.max} onChange={(e) => {
                                        const allFilters = {...filters, rateRange: {...filters.rateRange, max: e.target.value}};
                                        setFilters(allFilters);
                                    }}/>
                                    </label>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td>Mods:</td>
                            <td>
                                <div className="filter-container">
                                    <ModSelector availableMods={possibleMods} filters={filters} setFilters={setFilters}/>
                                </div>
                            </td>
                        </tr>
                    </>
                }
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
        </div>
    </>)
}

export default ScoreRankingFilters;