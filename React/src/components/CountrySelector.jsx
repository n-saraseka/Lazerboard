import {useState} from "react";
import CountrySelectorRow from "./CountrySelectorRow.jsx";

function CountrySelector({countries, filters, setFilters}) {
    const allCountries = [{ id: "All", name: "All countries" }].concat(countries);
    const [dropdownEnabled, setDropdownEnabled] = useState(false);
    return (
        <div className="country-selector">
            <div className="top-country">
                <CountrySelectorRow country={filters.country}
                                    isPartOfList={false}
                                    onClickAction={() => setDropdownEnabled(!dropdownEnabled)} 
                                    hasChevron={true}/>
            </div>
            <div className="countries" style={{display: dropdownEnabled ? "block" : "none"}}>
                { allCountries.map((c, index) => (
                    <CountrySelectorRow key={index} 
                                        country={c}
                                        isPartOfList={true}
                                        onClickAction={() => {
                                            setFilters({...filters, country: c});
                                            setDropdownEnabled(false);
                                        }} 
                                        hasChevron={false}/>
                )) }
            </div>
        </div>
    )
}

export default CountrySelector;