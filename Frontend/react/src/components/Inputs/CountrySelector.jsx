import {useState} from "react";
import CountrySelectorRow from "./CountrySelectorRow.jsx";

function CountrySelector({countries, filters, setFilters, refetchScores}) {
    const [filteredCountries, setFilteredCountries] = useState(countries);
    const [dropdownEnabled, setDropdownEnabled] = useState(false);
    
    const filterCountries = (searchValue) => {
        const valueLower = searchValue.toLowerCase();
        setFilteredCountries(countries.filter(country => country.name.toLowerCase().startsWith(valueLower)));
        const respectiveCountry = countries.find(country => country.name.toLowerCase() === valueLower)
        if (respectiveCountry !== undefined) {
            const newFilters = {...filters, country: respectiveCountry}
            setFilters(newFilters);
            if (refetchScores !== undefined) {
                refetchScores(newFilters);
            }
        }
        else {
            const newFilters = {...filters, country: null}
            setFilters(newFilters);
            if (refetchScores !== undefined) {
                refetchScores(newFilters);
            }
        }
    }
    
    const setDropdownOrDoNothing = (event) => {
        if (event.target.tagName === "DIV") {
            setDropdownEnabled(!dropdownEnabled);
        }
        else if (!dropdownEnabled) {
            setDropdownEnabled(true);
        }
    }
    
    return (
        <div className="selector selector-countries">
            <div className="selector-top">
                <CountrySelectorRow country={filters.country}
                                    isHeaderRow={true}
                                    onClickAction={setDropdownOrDoNothing}
                                    onTextChangeAction={filterCountries}/>
            </div>
            <div className="selector-items" style={{display: dropdownEnabled ? "block" : "none"}}>
                { filteredCountries.map((c, index) => (
                    <CountrySelectorRow key={index} 
                                        country={c}
                                        isHeaderRow={false}
                                        onClickAction={() => {
                                            const newFilters = {...filters, country: c}
                                            setFilters(newFilters);
                                            if (refetchScores !== undefined) {
                                                refetchScores(newFilters);
                                            }
                                            setDropdownEnabled(false);
                                        }}/>
                )) }
            </div>
        </div>
    )
}

export default CountrySelector;