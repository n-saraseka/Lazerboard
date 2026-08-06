import {getEncodedCountry} from "../utils/user-things.js";

function CountrySelectorRow({country, isPartOfList, onClickAction, hasChevron}) {
    return (
        <div className={`selector-country${isPartOfList ? ' list-country' : ''}`} onClick={() => onClickAction()}>
            <div className="selector-flag">
                {country.id !== "All" && (
                    <img src={`https://osu.ppy.sh/assets/images/flags/${getEncodedCountry(country.id)}.svg`}
                         alt={country.name}
                         title={country.name}/>
                )}
            </div>
            <div className="selector-countryname">
                <span>{country.name}</span>
                { hasChevron && (<div className="selector-chevron"></div>) }
            </div>
        </div>
    )
}

export default CountrySelectorRow;