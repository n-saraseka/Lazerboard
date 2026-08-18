import {getEncodedCountry} from "../../utils/user-things.js";

function CountrySelectorRow({country, isPartOfList, onClickAction, hasChevron}) {
    return (
        <div className={`selector-item${isPartOfList ? ' list-selector' : ''}`} onClick={() => onClickAction()}>
            {country.id !== "All" && (
                <div className="selector-image">
                    <img src={`https://osu.ppy.sh/assets/images/flags/${getEncodedCountry(country.id)}.svg`}
                         alt={country.name}
                         title={country.name}/>
                </div>
            )}
            <div className="selector-name">
                <span>{country.name}</span>
                { hasChevron && (<div className="selector-chevron"></div>) }
            </div>
        </div>
    )
}

export default CountrySelectorRow;