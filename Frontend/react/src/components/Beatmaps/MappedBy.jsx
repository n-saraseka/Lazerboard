import {dateFromDateTime, dateStringFromDatetime} from "../../utils/datetime-things.js";

function MappedBy({user, rankedDate}) {
    return (<div className="mapped-by">
        <a href={`https://osu.ppy.sh/users/${user.id}`}>
            <img className="score-player-img" src={`https://a.ppy.sh/${user.id}`} alt={`${user.username}`}/>
        </a>
        <div className="mapped-user-date">
            <span>Created by <a href={`https://osu.ppy.sh/users/${user.id}`}><strong>{user.username}</strong></a></span>
            <span>Ranked on <strong title={dateFromDateTime(rankedDate)}>{dateStringFromDatetime(rankedDate)}</strong></span>
        </div>
    </div>)
}

export default MappedBy;