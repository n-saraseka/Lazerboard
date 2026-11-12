import { createRoot } from 'react-dom/client'

import Page from './components/Pages/Page.jsx';
import ScoresPage from './components/Pages/ScoresPage.jsx';
import UserPage from './components/Pages/UserPage.jsx';
import BeatmapsetPage from './components/Pages/BeatmapsetPage.jsx';
import ScoreRankingPage from "./components/Pages/ScoreRankingPage.jsx";
import ManiaRankingPage from "./components/Pages/ManiaRankingPage.jsx";
import AboutPage from "./components/Pages/AboutPage.jsx";
import ErrorPage from "./components/Pages/ErrorPage.jsx";
import NotFoundPage from "./components/Pages/NotFoundPage.jsx";
import MillionScoresPage from "./components/Pages/MillionScoresPage.jsx";

const allComponents = {
    "scores": ScoresPage,
    "millionscores": MillionScoresPage,
    "user": UserPage,
    "beatmap": BeatmapsetPage,
    "userranking": ScoreRankingPage,
    "millionsranking": ManiaRankingPage,
    "about": AboutPage,
    "error": ErrorPage,
    "notfound": NotFoundPage,
}

document.querySelectorAll(".react-app").forEach((el) => {
    const currentPage = el.getAttribute("data-page");
    const count = el.getAttribute("data-count");
    
    const Component = allComponents[currentPage] ?? NotFoundPage;
    
    if (Component) {
        const rawProps = el.getAttribute("data-props");
        const props = rawProps ? JSON.parse(rawProps) : {};

        createRoot(el).render(
            <Page currentPage={currentPage} scoresCount={count} pageComponent={<Component {...props}/>}/>
        );
    }
})
