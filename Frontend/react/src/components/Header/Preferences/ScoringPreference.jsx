import {PreferencesContext} from "./PreferencesContext.jsx";
import { useContext } from "react";

function ScoringPreference() {
    const { preferences, setPreferences } = useContext(PreferencesContext);
    
    const setScoringPreference = () => {
        const newPreferences = {...preferences, preferStandardizedScoring: !preferences.preferStandardizedScoring};
        localStorage.setItem("preferences", JSON.stringify(newPreferences));
        setPreferences(newPreferences);
    }
    
    return (
        <span className={`scoring-preference ${preferences.preferStandardizedScoring ? 'active' : 'inactive'}`}
              title="Toggle standardized scoring" onClick={() => setScoringPreference()}>1M</span>
    )
}

export default ScoringPreference;