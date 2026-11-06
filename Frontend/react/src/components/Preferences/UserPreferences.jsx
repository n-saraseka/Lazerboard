import ScoringPreference from "./ScoringPreference.jsx";
import ThemePreference from "./ThemePreference.jsx";

function UserPreferences() {
    return <div className="user-preferences">
        <ScoringPreference/>
        <ThemePreference/>
    </div>
}

export default UserPreferences;