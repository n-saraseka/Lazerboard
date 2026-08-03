function MappedBy({user}) {
    return (<div className="mapped-by">
        <a href={`https://osu.ppy.sh/users/${user.id}`}>
            <img className="score-player-img" src={`https://a.ppy.sh/${user.id}`} alt={`${user.username}`}/>
        </a>
        <span>Created by</span>
        <a href={`https://osu.ppy.sh/users/${user.id}`}><strong>{user.username}</strong></a>
    </div>)
}

export default MappedBy;