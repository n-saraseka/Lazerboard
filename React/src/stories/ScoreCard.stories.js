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
        "id": 2319215043,
        "date": "2024-02-10T02:31:20Z",
        "mode": 0,
        "beatmap": {
            "id": 75,
            "beatmapsetId": 1,
            "beatmapset": {
                "id": 1,
                "artist": "Kenji Ninuma",
                "title": "DISCO PRINCE",
                "creator": "peppy",
                "userId": 2,
                "user": null
            },
            "mode": 0,
            "difficultyName": "Normal",
            "difficulty": 2.41431,
            "bpm": 120,
            "approachRate": 6,
            "circleSize": 4,
            "overallDifficulty": 6,
            "health": 6,
            "drainLength": 109,
            "status": 1
        },
        "beatmapId": 75,
        "user": {
            "id": 5795337,
            "username": "TheShadowOfDark",
            "countryCode": "CL",
            "country": {
                "id": "CL",
                "name": "Chile"
            }
        },
        "userId": 5795337,
        "grade": 6,
        "modAcronyms": [
            "DT",
            "HD",
            "FL"
        ],
        "speedChange": 2,
        "accuracy": 0.978593,
        "combo": 314,
        "misses": null,
        "totalScore": 1699546,
        "classicTotalScore": 2253266,
        "legacyTotalScore": 0,
        "pp": 154.84521,
        "rank": 1
    },
    usingStandardized: true,
};

export const Default = {
    args: {
        ...mockProps,
    },
};