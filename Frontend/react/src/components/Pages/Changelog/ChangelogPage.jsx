import Markdown from 'react-markdown';
import markdownString from './CHANGELOG.md?raw'
import ChangelogChange from "./ChangelogChange.jsx";
import {flattenChangelog} from "./flattenChangelog.js";

function ChangelogPage() {
    return (<>
        <h1>Changelog</h1>
        <div className="component-container">
            <Markdown
                remarkPlugins={[flattenChangelog]}
                components={{
                    li: ChangelogChange,
                }}>{markdownString}</Markdown>
        </div>
    </>)
}

export default ChangelogPage;