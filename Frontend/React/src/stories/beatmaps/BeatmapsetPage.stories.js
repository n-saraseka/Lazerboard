import BeatmapsetPage from '../../components/Pages/BeatmapsetPage.jsx';

import '../styles/BeatmapsetPage.css';

export default {
    title: 'BeatmapsetPage',
    component: BeatmapsetPage,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const arrayLen = 100;
const scoresMock = Array(arrayLen).fill({
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
    beatmap: {
        id: 75,
        beatmapsetId: 1,
        mode: 0,
        difficultyName: "Normal",
        difficulty: 2.41431,
        bpm: 120,
        approachRate: 6,
        circleSize: 4,
        overallDifficulty: 6,
        health: 6,
        drainLength: 109,
        status: 1,
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
    beatmapset: {
        id: 1,
        artist: 'Kenji Ninuma',
        title: 'DISCO PRINCE'
    },
    beatmaps: [{
        id: 75,
        beatmapsetId: 1,
        mode: 0,
        difficultyName: "Normal",
        difficulty: 2.41431,
        bpm: 120,
        approachRate: 6,
        circleSize: 4,
        overallDifficulty: 6,
        health: 6,
        drainLength: 109,
        status: 1,
        beatmapset: {
            id: 1,
            artist: 'Kenji Ninuma',
            title: 'DISCO PRINCE'
        }
    },],
    scores: scoresMock,
    selectedBeatmapId: 75,
};

export const Default = {
    args: {
        ...mockProps,
    },
};