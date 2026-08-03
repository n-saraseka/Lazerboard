import { getEncodedCountry } from "../utils/user-things.js";

function UserCard({user, scoreCount}) {
    return (
        <div className="user-card">
            <img src={`https://a.ppy.sh/${user.id}`} alt={user.username} className="user-avatar"/>
            <div className="user-data">
                <div className="user-data-name">
                    <a href={`https://osu.ppy.sh/users/${user.id}`} className="user-name">{user.username}</a>
                    <img 
                        src={`https://osu.ppy.sh/assets/images/flags/${getEncodedCountry(user.country.id)}.svg`} 
                        alt={user.country.name} 
                        title={user.country.name} 
                        className="country-img"/>
                </div>
                <span className="scores-amount">{`${scoreCount} ${scoreCount === 1 ? "score" : "scores"} stored in the database`}</span>
            </div>
        </div>
    )
}

export default UserCard;