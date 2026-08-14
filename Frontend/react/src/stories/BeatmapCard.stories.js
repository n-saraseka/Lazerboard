import BeatmapCard from '../components/BeatmapCard';

import './BeatmapCard.css';

export default {
    title: 'BeatmapCard',
    component: BeatmapCard,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
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
};

export const Default = {
    args: {
        ...mockProps,
    },
};