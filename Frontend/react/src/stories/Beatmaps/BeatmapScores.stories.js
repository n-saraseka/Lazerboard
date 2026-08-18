import BeatmapScores from '../../components/Beatmaps/BeatmapScores';

import './styles/BeatmapScores.css';

export default {
    title: 'BeatmapScores',
    component: BeatmapScores,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    scores: Array(10).fill({
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
        grade: 6,
        date: "2026-01-01",
        beatmap: {
            id: 75,
            beatmapsetId: 1,
            mode: 'osu',
            difficultyName: 'Normal',
            beatmapset: {
                id: 1,
                artist: 'Kenji Ninuma',
                title: 'DISCO PRINCE'
            },
            difficulty: 2.41
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
    }),
};

export const Default = {
    args: {
        ...mockProps,
    },
};