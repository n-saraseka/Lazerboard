import ScoreCard from '../components/ScoreCard';

import './ScoreCard.css';

export default {
    title: 'ScoreCard',
    component: ScoreCard,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    score: {
        id: 2319215043,
        accuracy: 0.9785,
        combo: 314,
        modAcronyms: ['DT', 'FL', 'HD'],
        misses: 0,
        totalScore: 1699546,
        classicTotalScore: 0,
        legacyTotalScore: 0,
        userId: 8706541,
        beatmap_id: 75,
        pp: 100,
        rank: 1,
        speedChange: 2,
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
        },
        mode: 0,
    },
    usingStandardized: true,
};

export const Default = {
    args: {
        ...mockProps,
    },
};