function SortableColumn({columnName, propertyName, sort, setSort, hover, setHover}) {
    return (
        <td onMouseOver={(event) => {
            let newHover = {...hover};
            newHover[propertyName] = true;
            setHover(newHover);
        }}
            onMouseLeave={(event) => {
                let newHover = {...hover};
                newHover[propertyName] = false;
                setHover(newHover);
            }}>
            <span>
                {columnName}
            </span>
            {(hover[propertyName] || sort[propertyName] !== "none") &&
                <span className="sort-thingy" onClick={() => {
                    let newSort = {...sort};
                    switch (sort[propertyName]) {
                        case "none":
                            newSort[propertyName] = "asc";
                            break;
                        case "asc":
                            newSort[propertyName] = "desc";
                            break;
                        case "desc":
                            newSort[propertyName] = "none";
                    }
                    setSort(newSort);
                }}>
                    {sort[propertyName] === "none" || sort[propertyName] === "asc" ? "↑" : "↓"}
                </span>
            }
        </td>
    )
}

export default SortableColumn;