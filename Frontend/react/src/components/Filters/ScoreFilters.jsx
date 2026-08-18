import {modeEnumToString} from "../../utils/beatmap-things.js";
import ModSelector from "../Inputs/ModSelector.jsx";
import CountrySelector from "../Inputs/CountrySelector.jsx";
import {useMemo, useState} from "react";
import {allMods} from "../../utils/score-things.js";
import CollapseUncollapseButton from "../Inputs/CollapseUncollapseButton.jsx";

function ScoreFilters({isUser, filters, setFilters, refetchScores, countries}) {
    const [collapsed, setCollapsed] = useState(true);
    
    let allModData = [];
    for (const key of Object.keys(allMods)) {
        allModData = allModData.concat(allMods[key]);
    }
    const possibleIncludeMods = useMemo(() => {
        let arr = [];
        const matchingMods = allModData.filter(m => filters.modes.some(mode => mode.enabled && m.modes.includes(mode.value)));
        matchingMods.forEach(m => {
            if (!filters.excludeMods.includes(m.acronym)) {
                arr.push(m.acronym);
            }
        });
        return arr;
    }, [filters]);

    const possibleExcludeMods = useMemo(() => {
        let arr = [];
        const matchingMods = allModData.filter(m => filters.modes.some(mode => mode.enabled && m.modes.includes(mode.value)));
        matchingMods.forEach(m => {
            if (!filters.includeMods.includes(m.acronym)) {
                arr.push(m.acronym);
            }
        });
        return arr;
    }, [filters]);
    
    const currentDate = new Date().toISOString().split("T")[0];
    
    return (<>
        <CollapseUncollapseButton isCollapsed={collapsed} onCollapseUncollapse={() => setCollapsed(!collapsed)} entityName="filters"/>
        <div className={`options-wrapper ${collapsed? "collapsed" : ""}`}>
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
                    <td>Modes:</td>
                    <td>
                        <div className="filter-container">
                            {filters.modes.map((mode, index) => (
                                <div className={`mode-icon-wrapper ${mode.enabled ? "enabled" : "disabled"}`} key={index}>
                                    <div className={`mode-icon mode-${modeEnumToString(index)}`} onClick={() => {
                                        let legitMods = [];
                                        const matchingMods = allModData.filter(m => filters.modes.some(mode => mode.enabled && m.modes.includes(mode.value)));
                                        matchingMods.forEach(m => {
                                            legitMods.push(m.acronym);
                                        });
                                        const newIncludeMods = filters.includeMods.filter(m => legitMods.includes(m));
                                        const newExcludeMods = filters.excludeMods.filter(m => legitMods.includes(m));
                                        const allFilters = {...filters, modes: filters.modes.map((m, i) => {
                                                if (i === index) {
                                                    m.enabled = !m.enabled;
                                                }
                                                return m;
                                            }),
                                            includeMods: newIncludeMods,
                                            excludeMods: newExcludeMods};
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
                                                                  min={1} max={100} value={filters.rankRange.min ?? ''} onChange={(e) => {
                                const allFilters = {...filters, rankRange: {...filters.rankRange, min: e.target.value}};
                                setFilters(allFilters);
                                refetchScores(allFilters, true);
                            }}/>
                            </label>
                            <label htmlFor="rankMax">to: <input id="rankMax" name="rankMax" type="number" step={1}
                                                                min={1} max={100} value={filters.rankRange.max ?? ''} onChange={(e) => {
                                const allFilters = {...filters, rankRange: {...filters.rankRange, max: e.target.value}};
                                setFilters(allFilters);
                                refetchScores(allFilters, true);
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
                                refetchScores(allFilters, true);
                            }}/>
                            </label>
                            <label htmlFor="ppMax">to: <input id="ppMax" name="ppMax" type="number" step={1}
                                                              min={1} max={3000} value={filters.ppRange.max} onChange={(e) => {
                                const allFilters = {...filters, ppRange: {...filters.ppRange, max: e.target.value}};
                                setFilters(allFilters);
                                refetchScores(allFilters, true);
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
                                refetchScores(allFilters, true);
                            }}/>
                            </label>
                            <label htmlFor="accMax">to: <input id="accMax" name="accMax" type="number" step={0.01}
                                                               min={1} max={100} value={filters.accRange.max} onChange={(e) => {
                                const allFilters = {...filters, accRange: {...filters.accRange, max: e.target.value}};
                                setFilters(allFilters);
                                refetchScores(allFilters, true);
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
                                refetchScores(allFilters, true);
                            }}/>
                            </label>
                            <label htmlFor="rateMax">to: <input id="rateMax" name="rateMax" type="number" step={1}
                                                                min={0} max={2} value={filters.rateRange.max} onChange={(e) => {
                                const allFilters = {...filters, rateRange: {...filters.rateRange, max: e.target.value}};
                                setFilters(allFilters);
                                refetchScores(allFilters, true);
                            }}/>
                            </label>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>Date:</td>
                    <td>
                        <div className="filter-container">
                            <label htmlFor="dateStart">From: <input id="dateStart" name="dateStart" type="date"
                                                                    value={filters.dateRange.min} max={currentDate} onChange={(e) => {
                                const allFilters = {...filters, dateRange: {...filters.dateRange, min: e.target.value}};
                                setFilters(allFilters);
                                refetchScores(allFilters, true);
                            }}/>
                            </label>
                            <label htmlFor="dateEnd">to: <input id="dateEnd" name="dateEnd" type="date"
                                                                value={filters.dateRange.max} max={currentDate} onChange={(e) => {
                                const allFilters = {...filters, dateRange: {...filters.dateRange, max: e.target.value}};
                                setFilters(allFilters);
                                refetchScores(allFilters, true);
                            }}/>
                            </label>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>Include mods:</td>
                    <td>
                        <div className="filter-container">
                            <ModSelector availableMods={possibleIncludeMods} excludeMode={false} filters={filters} setFilters={setFilters} refetchScores={refetchScores}/>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>Exclude mods:</td>
                    <td>
                        <div className="filter-container">
                            <ModSelector availableMods={possibleExcludeMods} excludeMode={true} filters={filters} setFilters={setFilters} refetchScores={refetchScores}/>
                        </div>
                    </td>
                </tr>
                {!isUser && (
                    <tr>
                        <td>Country:</td>
                        <td>
                            <div className="filter-container">
                                <CountrySelector filters={filters} setFilters={setFilters} countries={countries} refetchScores={refetchScores}/>
                            </div>
                        </td>
                    </tr>
                )}
                <tr>
                    <td>Sort:</td>
                    <td>
                        <div className="filter-container">
                            <select name="scores-sort" id="scores-sort" value={filters.sortBy} onChange={(e) => {
                                const allFilters = {...filters, sortBy: e.target.value};
                                setFilters(allFilters);
                                refetchScores(allFilters, false);
                            }}>
                                <option value="pp">PP</option>
                                <option value="rank">Rank</option>
                                <option value="accuracy">Accuracy</option>
                                <option value="combo">Combo</option>
                                <option value="totalScore">Standardized score</option>
                                <option value="classicTotalScore">Classic score</option>
                                <option value="date">Date and time set</option>
                            </select>
                            <span className="sort-thingy" onClick={() => {
                                const allFilters = {...filters, sortDir: filters.sortDir === "asc" ? "desc" : "asc"};
                                setFilters(allFilters);
                                refetchScores(allFilters, false);
                            }}>
                        {filters.sortDir === "asc" ? "↑" : "↓"}
                        </span>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>Count:</td>
                    <td>
                        <select name="scores-amount" id="scores-amount" value={filters.amount} onChange={(e) => {
                            const allFilters = {...filters, amount: e.target.value};
                            setFilters(allFilters);
                            refetchScores(allFilters, true);
                        }}>
                            <option value="10">10</option>
                            <option value="25">25</option>
                            <option value="50">50</option>
                            <option value="100">100</option>
                        </select>
                    </td>
                </tr>
                </tbody>
            </table>
        </div>
    </>)
}

export default ScoreFilters;