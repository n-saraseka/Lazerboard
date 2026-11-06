import {PreferencesContext} from "./PreferencesContext.jsx";
import { useContext, useMemo } from "react";

function ScoringPreference() {
    const { preferences, setPreferences } = useContext(PreferencesContext);
    const preference = useMemo(() => preferences.preferStandardizedScoring ?? true,
        [preferences.preferStandardizedScoring]); 
    
    const setScoringPreference = () => {
        const newPreferences = {...preferences, preferStandardizedScoring: !preference};
        setPreferences(newPreferences);
    }
    
    return (
        <span className={`scoring-preference ${preference ? 'active' : 'inactive'}`}
              title="Toggle standardized scoring" onClick={() => setScoringPreference()}>1M</span>
    )
}

export default ScoringPreference;