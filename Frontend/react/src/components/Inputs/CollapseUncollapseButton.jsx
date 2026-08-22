function CollapseUncollapseButton({isCollapsed, onCollapseUncollapse, entityName}) {
    return (
        <div className="collapse-button" onClick={onCollapseUncollapse}>
            <span>{isCollapsed ? "Show" : "Hide"} {entityName}</span>
        </div>
    )
}

export default CollapseUncollapseButton;