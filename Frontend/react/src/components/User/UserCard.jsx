import { getEncodedCountry } from "../../utils/user-things.js";
import { useContext } from "react";
import {UserContext} from "../Misc/UserContext.jsx";
import ReportButton from "../Inputs/ReportButton.jsx";
import RestrictedIndicator from "./RestrictedIndicator.jsx";

function UserCard({user, scoreCount, filteredCount}) {
    const { currentUser, setCurrentUser } = useContext(UserContext);
    return (
        <div className="user-card">
            <div className="user-card-column">
                <img src={`https://a.ppy.sh/${user.id}`} alt={user.username} className="user-avatar"/>
            </div>
            <div className="user-card-column">
                <div className="user-card-row">
                    <div className="user-data-name">
                        <div className="user-card-name">
                            <img
                                src={`https://osu.ppy.sh/assets/images/flags/${getEncodedCountry(user.countryCode ?? 'XX')}.svg`}
                                alt={user.countryCode === null ? 'Unknown' : user.country.name}
                                title={user.countryCode === null ? 'Unknown' : user.country.name}
                                className="country-img"/>
                            <a href={`https://osu.ppy.sh/users/${user.id}`} className="user-name">{user.username}</a>
                            {currentUser !== null && (
                                (currentUser.id !== user.id && !user.isRestricted) && (<ReportButton user={user}/>)
                            )}
                            {user.isRestricted && (<RestrictedIndicator/>)}
                        </div>
                    </div>
                </div>
                <div className="user-card-row">
                    <span className="scores-amount">
                        { scoreCount === null ? 'Loading scores...' : `${scoreCount} ${scoreCount === 1
                            ? "score" : "scores"} stored${scoreCount !== filteredCount ? ` (${filteredCount} filtered)` : ""}`}
                    </span>
                </div>
            </div>
        </div>
    )
}

export default UserCard;