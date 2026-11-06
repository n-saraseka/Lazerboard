import {useContext, useMemo} from "react";
import {PreferencesContext} from "./PreferencesContext.jsx";

function ThemePreference() {
    const { preferences, setPreferences } = useContext(PreferencesContext);
    const preference = useMemo(() => preferences.theme ?? 'dark',
        [preferences]) ;

    const setThemePreference = () => {
        const newPreferences = {...preferences, theme: preferences.theme === 'dark' ? 'light' : 'dark'};
        setPreferences(newPreferences);
    }
    
    return (<div className={`theme-preference ${preference}`}>
        <div className="theme-icon icon-night"></div>
        <div className="theme-toggle" onClick={() => setThemePreference()}>
            <div className="theme-circle"></div>
        </div>
        <div className="theme-icon icon-day"></div>
    </div>)
}

export default ThemePreference;