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
                <li><a href="https://github.com/n-saraseka/Lazerboard/"><strong>Source code</strong></a></li>
                <li><a href="https://discord.com/invite/Z3rWRrznfd">Discord</a></li>
                <li><a href="https://www.donationalerts.com/r/somemelguy">Support project</a></li>
                <li><a href="/docs">API documentation</a></li>
            </ul>
        </div>
    </footer>
}

export default Footer;