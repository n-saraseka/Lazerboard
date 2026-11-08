function UserButton({user}) {
    return (<a href={`/users/${user.id}`} className="authorized-user">
        <img src={`https://a.ppy.sh/${user.id}`} alt={`${user.username}`} className="user-avatar"/>
        <span>{user.username}</span>
    </a>)
}

export default UserButton;