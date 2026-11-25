import { getEncodedCountry } from "../../utils/user-things.js";
import { useContext } from "react";
import {UserContext} from "../Misc/UserContext.jsx";
import ReportButton from "../Inputs/ReportButton.jsx";
import RestrictedIndicator from "./RestrictedIndicator.jsx";
import UserGroupLabel from "./UserGroupLabel.jsx";

function UserCard({user}) {
    const { currentUser, setCurrentUser } = useContext(UserContext);
    return (
        <div className="user-card">
            <div className="user-card-column">
                <img src={`https://a.ppy.sh/${user.id}`} alt={user.username} className="user-avatar"/>
            </div>
            <div className="user-card-column">
                <div className="user-card-row">
                    <a href={`https://osu.ppy.sh/users/${user.id}`} className="user-name">{user.username}</a>
                    {user.userGroups[0] !== 0 && (<UserGroupLabel group={user.userGroups[user.userGroups.length - 1]}/>)}
                    {user.isRestricted && (<RestrictedIndicator/>)}
                    {currentUser !== null && (
                        (currentUser.id !== user.id && !user.isRestricted) && (<ReportButton user={user}/>)
                    )}
                </div>
                <div className="user-card-row">
                    <span>from</span>
                    <img
                        src={`https://osu.ppy.sh/assets/images/flags/${getEncodedCountry(user.countryCode ?? 'XX')}.svg`}
                        alt={user.countryCode === null ? 'Unknown' : user.country.name}
                        title={user.countryCode === null ? 'Unknown' : user.country.name}
                        className="country-img"/>
                    <strong>{user.countryCode === null ? 'Unknown' : user.country.name}</strong>
                </div>
            </div>
        </div>
    )
}

export default UserCard;