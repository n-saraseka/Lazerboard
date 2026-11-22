function getTitleTextFromUserGroup(group) {
    if (group === 1) return "Registered user";
    if (group === 2) return "Supporter";
    if (group === 3) return "Moderator";
    if (group === 4) return "Administrator";
    return "Unknown";
}

function getClassTextFromUserGroup(group) {
    if (group === 1) return "registered";
    if (group === 2) return "supporter";
    if (group === 3) return "moderator";
    if (group === 4) return "administrator";
    return "unknown";
}

function UserGroupLabel({group}) {
    return (<div className={`user-label ${getClassTextFromUserGroup(group)}`} title={getTitleTextFromUserGroup(group)}>
        <div className="user-label-img"></div>
    </div>);
}

export default UserGroupLabel;