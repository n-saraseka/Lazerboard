import {getEncodedCountry} from "../../utils/user-things.js";

function CountrySelectorRow({country, isHeaderRow, onClickAction, onTextChangeAction}) {
    return (
        isHeaderRow
        ? <div className="selector-item" onClick={(event) => onClickAction(event)}>
                {country !== null && (
                    <div className="selector-image">
                        <img src={`https://osu.ppy.sh/assets/images/flags/${getEncodedCountry(country.id)}.svg`}
                             alt={country.name}
                             title={country.name}/>
                    </div>
                )}
                <div className="selector-name">
                    <input type="text" onChange={(event) => onTextChangeAction(event.target.value)}
                           value={country !== null ? country.name : null} placeholder="Click or type to select"></input>
                    <div className="selector-chevron"></div>
                </div>
            </div>
        : <div className="selector-item list-selector" onClick={() => onClickAction()}>
            {country.id !== "All" && (
                <div className="selector-image">
                    <img src={`https://osu.ppy.sh/assets/images/flags/${getEncodedCountry(country.id)}.svg`}
                         alt={country.name}
                         title={country.name}/>
                </div>
            )}
            <div className="selector-name">
                <span>{country.name}</span>
            </div>
        </div>
    )
}

export default CountrySelectorRow;