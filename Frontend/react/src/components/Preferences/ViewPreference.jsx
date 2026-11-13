import {useContext, useMemo} from "react";
import {PreferencesContext} from "./PreferencesContext.jsx";

function ViewPreference() {
    const { preferences, setPreferences } = useContext(PreferencesContext);
    const preference = useMemo(() => preferences.view ?? preferences.globalPreferences.view ?? 'cards',
        [preferences.view, preferences.globalPreferences.view]);

    const setViewPreference = (newPreference) => {
        if (newPreference !== preference) {
            const newPreferences = {...preferences, view: newPreference};
            setPreferences(newPreferences);
        }
    }

    return (<div className="view-preference">
        <div className={`view-toggle ${preference === 'cards' ? 'active' : 'inactive'}`}
             onClick={() => setViewPreference('cards')}
             title="Cards scores view">
            <div className="view-icon view-cards"></div>
        </div>
        <div className={`view-toggle ${preference === 'table' ? 'active' : 'inactive'}`}
             onClick={() => setViewPreference('table')}
             title="Table scores view">
            <div className="view-icon view-table"></div>
        </div>
    </div>)
}

export default ViewPreference;