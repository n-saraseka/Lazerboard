function CollapseUncollapseButton({isCollapsed, onCollapseUncollapse}) {
    return (
        <div className="collapse-button" onClick={onCollapseUncollapse}>
            <span>Click to {isCollapsed ? "unhide" : "hide"}</span>
        </div>
    )
}

export default CollapseUncollapseButton;