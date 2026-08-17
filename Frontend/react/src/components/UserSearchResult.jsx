import {getEncodedCountry} from "../utils/user-things.js";

function UserSearchResult({user}) {
    return (
        <a href={`/users/${user.id}`} className="search-result">
            <img src={`https://a.ppy.sh/${user.id}`} alt={user.username} className="user-avatar"/>
            <img
                src={`https://osu.ppy.sh/assets/images/flags/${getEncodedCountry(user.country.id)}.svg`}
                alt={user.country.name}
                title={user.country.name}
                className="country-img"/>
            <span className="user-name">{user.username}</span>
        </a>
    )
}

export default UserSearchResult;