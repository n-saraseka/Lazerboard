function CollapseUncollapseButton({isCollapsed, onCollapseUncollapse, entityName}) {
    return (
        <div className="collapse-button" onClick={onCollapseUncollapse}>
            <span>Click to {isCollapsed ? "unhide" : "hide"} {entityName}</span>
        </div>
    )
}

export default CollapseUncollapseButton;