import DatabaseState from "./DatabaseState.jsx";

function Footer({scoresCount}) {
    return <footer>
        <div className="score-fetcher-stuff">
            <div>
                <span>Made by <a href="https://osu.ppy.sh/users/8706541"><strong>SomeMelGuy</strong></a></span>
            </div>
            <ul className="footer-links">
                <li>
                    Scores count: <strong>{scoresCount}</strong>
                </li>
                <li className="db-state">
                    <DatabaseState/>
                </li>
            </ul>
        </div>
        <div className="meta-stuff">
            <ul className="footer-links">
                <li><a href="https://discord.com/invite/Z3rWRrznfd"><div className="footer-icon discord" title="Discord server"></div></a></li>
                <li><a href="https://www.donationalerts.com/r/somemelguy"><div className="footer-icon support" title="Support project"></div></a></li>
                <li><a href="https://github.com/n-saraseka/Lazerboard/"><div className="footer-icon github" title="Source code"></div></a></li>
                <li><a href="/docs"><div className="footer-icon docs" title="API documentation"></div></a></li>
            </ul>
        </div>
    </footer>
}

export default Footer;