import ScoresPage from '../../components/Pages/ScoresPage';

import './styles/ScoreCard.css';

export default {
    title: 'ScoresPage',
    component: ScoresPage,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const arrayLen = 200;
const perPageDefault = 25;
const scores = Array(arrayLen).fill({
    id: 2319215043,
    accuracy: 0.9785,
    combo: 314,
    modAcronyms: ['DT(2x)', 'FL', 'HD'],
    misses: 0,
    totalScore: 1699546,
    classicTotalScore: 0,
    legacyTotalScore: 0,
    userId: 8706541,
    beatmap_id: 75,
    pp: 100,
    rank: 1,
    beatmap: {
        id: 75,
        beatmapsetId: 1,
        mode: 'osu',
        difficultyName: 'Normal',
        beatmapset: {
            id: 1,
            artist: 'Kenji Ninuma',
            title: 'DISCO PRINCE'
        }
    },
    user: {
        id: 8706541,
        username: 'SomeMelGuy',
        country_code: 'UA',
        country: {
            id: 'UA',
            name: 'Ukraine'
        }
    }
});

const mockProps = {
    scoreProps: scores.slice(0, perPageDefault),
    pages: Math.floor(arrayLen / perPageDefault),
};

export const Default = {
    args: {
        ...mockProps,
    },
};