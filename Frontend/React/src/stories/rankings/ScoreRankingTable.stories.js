import ScoreRankingTable from '../../components/Rankings/ScoreRankingTable';

import '../styles/ScoreRankingTable.css';

export default {
    title: 'ScoreRankingTable',
    component: ScoreRankingTable,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    rankings: [
        {
            "rank": 1,
            "user": {
                "id": 5256529,
                "username": "Nitroz",
                "countryCode": "SE",
                "country": null
            },
            "scoresCount": 7102
        },
        {
            "rank": 2,
            "user": {
                "id": 39828,
                "username": "WubWoofWolf",
                "countryCode": "PL",
                "country": null
            },
            "scoresCount": 7076
        },
        {
            "rank": 3,
            "user": {
                "id": 8236477,
                "username": "Cyrzai",
                "countryCode": "CA",
                "country": null
            },
            "scoresCount": 6483
        },
        {
            "rank": 4,
            "user": {
                "id": 7806753,
                "username": "chicken_10",
                "countryCode": "RU",
                "country": null
            },
            "scoresCount": 6380
        },
        {
            "rank": 5,
            "user": {
                "id": 3521482,
                "username": "Willy",
                "countryCode": "US",
                "country": null
            },
            "scoresCount": 6078
        },
        {
            "rank": 6,
            "user": {
                "id": 32390879,
                "username": "turbo_igor_777",
                "countryCode": "RU",
                "country": null
            },
            "scoresCount": 5930
        },
        {
            "rank": 7,
            "user": {
                "id": 13752814,
                "username": "See",
                "countryCode": "US",
                "country": null
            },
            "scoresCount": 5338
        },
        {
            "rank": 8,
            "user": {
                "id": 6245906,
                "username": "Furina-",
                "countryCode": "CA",
                "country": null
            },
            "scoresCount": 5166
        },
        {
            "rank": 9,
            "user": {
                "id": 1719471,
                "username": "EZChamp",
                "countryCode": "GB",
                "country": null
            },
            "scoresCount": 4812
        },
        {
            "rank": 10,
            "user": {
                "id": 11323460,
                "username": "wiuuuh",
                "countryCode": "FI",
                "country": null
            },
            "scoresCount": 4808
        }
    ]
};

export const Default = {
    args: {
        ...mockProps,
    },
};