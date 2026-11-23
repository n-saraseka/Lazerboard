import { createRoot } from 'react-dom/client'

import Page from './components/Pages/Page.jsx';
import ScoresPage from './components/Pages/ScoresPage.jsx';
import MillionScoresPage from './components/Pages/MillionScoresPage.jsx';
import UserPage from './components/Pages/UserPage.jsx';
import BeatmapsetPage from './components/Pages/BeatmapsetPage.jsx';
import ScoreRankingPage from './components/Pages/ScoreRankingPage.jsx';
import ManiaRankingPage from './components/Pages/ManiaRankingPage.jsx';
import AboutPage from './components/Pages/AboutPage.jsx';
import ChangelogPage from './components/Pages/Changelog/ChangelogPage.jsx';
import AdminPage from './components/Pages/AdminPage.jsx';
import UnauthorizedPage from './components/Pages/UnauthorizedPage.jsx';
import ErrorPage from './components/Pages/ErrorPage.jsx';
import NotFoundPage from './components/Pages/NotFoundPage.jsx';

const allComponents = {
    "": ScoresPage,
    "millions": MillionScoresPage,
    "user": UserPage,
    "beatmap": BeatmapsetPage,
    "scoreranking": ScoreRankingPage,
    "millionsranking": ManiaRankingPage,
    "about": AboutPage,
    "changelog": ChangelogPage,
    "error": ErrorPage,
    "notfound": NotFoundPage,
    "adminpanel": AdminPage,
    "unauthorized": UnauthorizedPage,
};

function getPage(url) {
    if (url.startsWith("users")) return "user";
    if (url.startsWith("b") || url.startsWith("beatmapsets")) return "beatmap";
    return url;
}

const url = window.location.pathname.substring(1, window.location.pathname.length);
const page = getPage(url);

document.querySelectorAll(".react-app").forEach((el) => {
    const dataPage = el.getAttribute("data-page");
    const count = el.getAttribute("data-count");

    const Component = dataPage !== null 
        ? allComponents[dataPage] ?? NotFoundPage
        : allComponents[page] ?? NotFoundPage;

    if (Component) {
        const rawProps = el.getAttribute("data-props");
        const props = rawProps ? JSON.parse(rawProps) : {};

        createRoot(el).render(
            <Page currentPage={dataPage ?? page} scoresCount={count} pageComponent={<Component {...props}/>}/>
        );
    }
})