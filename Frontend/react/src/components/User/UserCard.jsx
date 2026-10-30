import { getEncodedCountry } from "../../utils/user-things.js";

function UserCard({user, scoreCount}) {
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
                                src={`https://osu.ppy.sh/assets/images/flags/${getEncodedCountry(user.country.id)}.svg`}
                                alt={user.country.name}
                                title={user.country.name}
                                className="country-img"/>
                            <a href={`https://osu.ppy.sh/users/${user.id}`} className="user-name">{user.username}</a>
                        </div>
                    </div>
                </div>
                <div className="user-card-row">
                    <span className="scores-amount">
                        { scoreCount === null ? 'Loading scores...' : `${scoreCount} ${scoreCount === 1
                            ? "score" : "scores"} stored in the database`}
                    </span>
                </div>
            </div>
        </div>
    )
}

export default UserCard;