import {useState} from "react";
import CountrySelectorRow from "./CountrySelectorRow.jsx";

function CountrySelector({countries, filters, setFilters, refetchScores}) {
    const allCountries = [{ id: "All", name: "All countries" }].concat(countries);
    const [dropdownEnabled, setDropdownEnabled] = useState(false);
    return (
        <div className="selector selector-countries">
            <div className="selector-top">
                <CountrySelectorRow country={filters.country}
                                    isPartOfList={false}
                                    onClickAction={() => setDropdownEnabled(!dropdownEnabled)} 
                                    hasChevron={true}/>
            </div>
            <div className="selector-items" style={{display: dropdownEnabled ? "block" : "none"}}>
                { allCountries.map((c, index) => (
                    <CountrySelectorRow key={index} 
                                        country={c}
                                        isPartOfList={true}
                                        onClickAction={() => {
                                            const newFilters = {...filters, country: c}
                                            setFilters(newFilters);
                                            if (refetchScores !== undefined) {
                                                refetchScores(newFilters);
                                            }
                                            setDropdownEnabled(false);
                                        }} 
                                        hasChevron={false}/>
                )) }
            </div>
        </div>
    )
}

export default CountrySelector;