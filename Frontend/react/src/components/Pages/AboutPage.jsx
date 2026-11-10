function AboutPage() {
    return <div className="about">
        <div className="about-section">
            <h1>What is this exactly?</h1>
            <div className="about-explanation">
                <p>This is Lazerboard - a database of all top 100 scores on all <a
                    href="https://osu.ppy.sh">osu!</a> beatmaps that have a leaderboard,
                    focusing on the version of leaderboards that appears in the rapidly growing <a
                        href="https://osu.ppy.sh/wiki/en/Help_centre/Upgrading_to_lazer">osu!lazer</a> client.
                    It provides an ability to view these scores with filters and see various user statistics based on
                    their leaderboard plays and placements, including <a href="/scoreranking">global rankings</a>.</p>
            </div>
        </div>
        <div className="about-section">
            <h1>Why should I use this?</h1>
            <div className="about-explanation">
                <p>While there are score databases like <a href="https://osustats.ppy.sh/">osu!Stats</a> and <a
                    href="https://osualternative.com/">osu!alternative</a>,
                    neither of them provide the information that this project does. <strong>This is the first and only
                        project that provides information based on osu!lazer beatmap rankings,
                        including new gameplay mods, new score multipliers and Performance Points recalculation of
                        scores with mods that are not eligible for ranking just yet.</strong> If you
                    want to compete with other players in osu!lazer leaderboard efficiency, see currently unranked top scores,
                    or to see who ranks the highest with features unique to osu!lazer in <strong>all modes</strong>,
                    this is the place to go.</p>
            </div>
        </div>
        <div className="about-section">
            <h1>How do I submit a score?</h1>
            <div className="about-explanation">
                <p>The scores are fetched live using <a href="https://osu.ppy.sh/docs/index.html#get-scores">osu!
                    API</a>. A new batch is processed every 10 seconds.</p>
            </div>
        </div>
        <div className="about-section">
            <h1>What data is available?</h1>
            <div className="about-explanation">
                <p>You can filter scores by the following:</p>
                <ul>
                    <li>Gameplay mode</li>
                    <li>Gameplay modifiers <strong>(including osu!lazer exclusive mods)</strong></li>
                    <li><strong>Gameplay speed (from 0.5x to 2.0x)</strong></li>
                    <li>Position on the leaderboard (also known as beatmap rank)</li>
                    <li>PP</li>
                    <li>Accuracy</li>
                    <li>Date and time set</li>
                    <li>Country</li>
                    <li>Whether to include or exclude converted scores.</li>
                </ul>
                <p>Along with that, you can view <a href="/scoreranking">global rankings</a> based 
                    on the amount of scores set with selected filters.</p>
                <p>For the <a href="https://osu.ppy.sh/wiki/en/Game_mode/osu%21mania">osu!mania</a> enthusiasts,
                    there also are global rankings based on how many <a href="/millionsranking">1 million (perfect) scores</a> a user has.</p>
                <p>Alongside these, there are <a href="/b/75">beatmap rankings</a> and <a href="/users/7562902">user pages.</a></p>
                <p>The beatmap pages allow you to sort the leaderboards by different columns and view the leaderboards
                    of all available modes of the beatmap.</p>
                <p>Besides providing all leaderboard scores set by user with the ability to filter them, the user pages
                    also provide charts with the following information:</p>
                <ul>
                    <li>Score count history</li>
                    <li>Beatmap rank distribution for all user scores</li>
                    <li>Star rating distribution for all user scores</li>
                    <li>Gameplay speed distribution for all user scores</li>
                </ul>
            </div>
        </div>
        <div className="about-section">
            <h1>My score doesn't appear here!</h1>
            <div className="about-explanation">
                <p>The website focuses on scores that appear in the top 100 rankings on a map. If your score is outside
                    the top 100, it won't be collected. We apologize for the inconvenience.</p>
            </div>
        </div>
        <div className="about-section">
            <h1>What about cheaters?</h1>
            <div className="about-explanation">
                <p>There are considerations for removing scores from users who got restricted for foul play. All scores
                    set in the last 24 hours are checked every 2 hours to ensure as many cheated scores are caught as
                    possible. You may also <strong>report</strong> users who did not get caught automatically on their profile.</p>
            </div>
        </div>
        <div className="about-section">
            <h1>I found a bug / want to suggest a feature / have another question!</h1>
            <div className="about-explanation">
                <p>For any bug reports, feature requests or questions, feel free to report them on <a
                    href="https://github.com/n-saraseka/Lazerboard/issues">Github</a>, <a 
                    href="https://discord.com/invite/Z3rWRrznfd">Discord</a> or <a href="https://osu.ppy.sh/users/8706541">
                        via osu! DM</a>.</p>
            </div>
        </div>
    </div>
}

export default AboutPage;