function ChangelogChange({ node, children, ...props}) {
    return (
        <li {...props}><div>{children}</div></li>
    )
}

export default ChangelogChange;