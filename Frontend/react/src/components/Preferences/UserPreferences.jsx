import ScoringPreference from "./ScoringPreference.jsx";
import ThemePreference from "./ThemePreference.jsx";
import ViewPreference from "./ViewPreference.jsx";

function UserPreferences() {
    return <div className="user-preferences">
        <ScoringPreference/>
        <ThemePreference/>
        <ViewPreference/>
    </div>
}

export default UserPreferences;