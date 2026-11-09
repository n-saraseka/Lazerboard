import { useState } from "react";

function UserButton({user}) {
    const [showDropdown, setShowDropdown] = useState(false);
    return (<div className="header-user-profile">
        <div  className="authorized-user" onClick={() => setShowDropdown(!showDropdown)}>
            <img src={`https://a.ppy.sh/${user.id}`} alt={`${user.username}`} className="user-avatar"/>
            <span>{user.username}</span>
        </div>
        {showDropdown && (
            <div className="header-user-dropdown">
                <a href={`/users/${user.id}`}>Profile</a>
                <a href="/oauth/signout">Log out</a>
            </div>
        )}
    </div>)
}

export default UserButton;