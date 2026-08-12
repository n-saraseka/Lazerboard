function Header() {
    return (
        <header>
            <div className="logo-navigation">
                <strong>OsuScoreStats</strong>
                <ul className="navigation">
                    <li>
                        <a asp-controller="General" asp-action="Index" className="@(ViewData[" ActivePage"] == "
                           home" ? " active" : " inactive")">Home page</a>
                    </li>
                    <li>
                        <a asp-controller="General" asp-action="About" className="@(ViewData[" ActivePage"] == "
                           about" ? " active" : " inactive")">About</a>
                    </li>
                    <li>
                        <a asp-controller="ScoreRanking" asp-action="ScoreRanking" className="@(ViewData["
                           ActivePage"] == " scoreranking" ? " active" : " inactive")">Score ranking</a>
                    </li>
                    <li>
                        <a asp-controller="ScoreRanking" asp-action="ManiaMillions" className="@(ViewData["
                           ActivePage"] == " maniamillions" ? " active" : " inactive")">Mania millions</a>
                    </li>
                </ul>
            </div>
        </header>
    )
}