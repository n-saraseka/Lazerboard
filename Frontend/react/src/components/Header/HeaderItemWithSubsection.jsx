import {useState} from "react";
function HeaderItemWithSubsection({sectionTitle, currentPage, subItems}) {
    const [collapsed, setCollapsed] = useState(true);
    
    return (<div className="item-with-subsection">
        <span className="subsection-header" onClick={() => setCollapsed(!collapsed)}>{sectionTitle}</span>
        <div className={`subsection${!collapsed ? ' uncollapsed' : ''}`}>
            {subItems.map((item, i) => (
                <a key={i} href={item.url} className={`subsection-item ${currentPage === item.techTitle ? 'active' : 'inactive'}`}>{item.title}</a>
            ))}
        </div>
    </div>)
}
export default HeaderItemWithSubsection;