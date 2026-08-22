import ScoreTable from '../../components/Scores/ScoresTable';

import './styles/ScoreCard.css';

export default {
    title: 'ScoresTable',
    component: ScoreTable,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    scores: [
        {
            "id": 7320556432,
            "date": "2026-08-22T01:40:10Z",
            "mode": 0,
            "beatmap": {
                "id": 28216,
                "beatmapsetId": 5257,
                "beatmapset": {
                    "id": 5257,
                    "artist": "Imogen Heap",
                    "title": "Headlock",
                    "creator": "Ruduen",
                    "userId": 22966,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Larto's Free",
                "difficulty": 2.00859,
                "bpm": 120,
                "approachRate": 5,
                "circleSize": 4,
                "overallDifficulty": 5,
                "health": 4,
                "drainLength": 162,
                "status": 1
            },
            "beatmapId": 28216,
            "user": {
                "id": 939966,
                "username": "Metaves",
                "countryCode": "JP",
                "country": {
                    "id": "JP",
                    "name": "Japan"
                }
            },
            "userId": 939966,
            "grade": 7,
            "modAcronyms": [
                "DT",
                "PF",
                "CL"
            ],
            "speedChange": 1.5,
            "accuracy": 1,
            "combo": 431,
            "misses": null,
            "totalScore": 1213561,
            "classicTotalScore": 1639775,
            "legacyTotalScore": 1869490,
            "pp": 59.2006,
            "rank": 88
        },
        {
            "id": 7321488713,
            "date": "2026-08-22T05:36:44Z",
            "mode": 0,
            "beatmap": {
                "id": 10979,
                "beatmapsetId": 1269,
                "beatmapset": {
                    "id": 1269,
                    "artist": "Joby Talbot",
                    "title": "Journey of the Sorcerer",
                    "creator": "MOOMANiBE",
                    "userId": 8950,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Normal",
                "difficulty": 2.38906,
                "bpm": 159.87,
                "approachRate": 5,
                "circleSize": 4,
                "overallDifficulty": 5,
                "health": 4,
                "drainLength": 73,
                "status": 1
            },
            "beatmapId": 10979,
            "user": {
                "id": 21978073,
                "username": "nsmky",
                "countryCode": "PH",
                "country": {
                    "id": "PH",
                    "name": "Philippines"
                }
            },
            "userId": 21978073,
            "grade": 5,
            "modAcronyms": [
                "NC",
                "HR",
                "CL"
            ],
            "speedChange": 1.5,
            "accuracy": 0.954693,
            "combo": 179,
            "misses": null,
            "totalScore": 1154046,
            "classicTotalScore": 514168,
            "legacyTotalScore": 362875,
            "pp": 53.219,
            "rank": 101
        },
        {
            "id": 7320772795,
            "date": "2026-08-22T02:33:00Z",
            "mode": 3,
            "beatmap": {
                "id": 29704,
                "beatmapsetId": 6587,
                "beatmapset": {
                    "id": 6587,
                    "artist": "M.I.A",
                    "title": "Paper Planes",
                    "creator": "EEeee",
                    "userId": 19819,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Pineapple Express!!!",
                "difficulty": 4.09006,
                "bpm": 172,
                "approachRate": 6,
                "circleSize": 5,
                "overallDifficulty": 6,
                "health": 7,
                "drainLength": 168,
                "status": 1
            },
            "beatmapId": 29704,
            "user": {
                "id": 17628441,
                "username": "iblameagus",
                "countryCode": "AR",
                "country": {
                    "id": "AR",
                    "name": "Argentina"
                }
            },
            "userId": 17628441,
            "grade": 5,
            "modAcronyms": [
                "4K"
            ],
            "speedChange": 1,
            "accuracy": 0.984585,
            "combo": 728,
            "misses": 1,
            "totalScore": 839452,
            "classicTotalScore": 839452,
            "legacyTotalScore": 0,
            "pp": 49.6397,
            "rank": 64
        },
        {
            "id": 7321910003,
            "date": "2026-08-22T07:32:20Z",
            "mode": 0,
            "beatmap": {
                "id": 22236,
                "beatmapsetId": 3633,
                "beatmapset": {
                    "id": 3633,
                    "artist": "Yoriko",
                    "title": "Daia no Hana",
                    "creator": "Card N'FoRcE",
                    "userId": 3936,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Hard",
                "difficulty": 3.2206,
                "bpm": 150,
                "approachRate": 7,
                "circleSize": 6,
                "overallDifficulty": 7,
                "health": 7,
                "drainLength": 76,
                "status": 1
            },
            "beatmapId": 22236,
            "user": {
                "id": 32619071,
                "username": "technobuild",
                "countryCode": "US",
                "country": {
                    "id": "US",
                    "name": "United States"
                }
            },
            "userId": 32619071,
            "grade": 6,
            "modAcronyms": [
                "FL",
                "CL"
            ],
            "speedChange": 1,
            "accuracy": 0.985915,
            "combo": 206,
            "misses": null,
            "totalScore": 1133212,
            "classicTotalScore": 857549,
            "legacyTotalScore": 1057244,
            "pp": 49.3596,
            "rank": 81
        },
        {
            "id": 7321562995,
            "date": "2026-08-22T05:56:30Z",
            "mode": 0,
            "beatmap": {
                "id": 10758,
                "beatmapsetId": 1280,
                "beatmapset": {
                    "id": 1280,
                    "artist": "Satoshi Hosoi",
                    "title": "Morino Ichigo A",
                    "creator": "Card N'FoRcE",
                    "userId": 3936,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Hard",
                "difficulty": 3.07057,
                "bpm": 102,
                "approachRate": 6,
                "circleSize": 6,
                "overallDifficulty": 6,
                "health": 6,
                "drainLength": 101,
                "status": 1
            },
            "beatmapId": 10758,
            "user": {
                "id": 32619071,
                "username": "technobuild",
                "countryCode": "US",
                "country": {
                    "id": "US",
                    "name": "United States"
                }
            },
            "userId": 32619071,
            "grade": 6,
            "modAcronyms": [
                "FL",
                "CL"
            ],
            "speedChange": 1,
            "accuracy": 0.997549,
            "combo": 369,
            "misses": null,
            "totalScore": 1173375,
            "classicTotalScore": 2944771,
            "legacyTotalScore": 2791208,
            "pp": 46.657,
            "rank": 29
        },
        {
            "id": 7320532411,
            "date": "2026-08-22T01:34:26Z",
            "mode": 0,
            "beatmap": {
                "id": 33943,
                "beatmapsetId": 8190,
                "beatmapset": {
                    "id": 8190,
                    "artist": "Yutaka Minobe & Takayuki Maeda",
                    "title": "Kingdom of Ixataka",
                    "creator": "Real1",
                    "userId": 40998,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Easy",
                "difficulty": 2.0852,
                "bpm": 94,
                "approachRate": 2,
                "circleSize": 3,
                "overallDifficulty": 2,
                "health": 2,
                "drainLength": 82,
                "status": 1
            },
            "beatmapId": 33943,
            "user": {
                "id": 939966,
                "username": "Metaves",
                "countryCode": "JP",
                "country": {
                    "id": "JP",
                    "name": "Japan"
                }
            },
            "userId": 939966,
            "grade": 7,
            "modAcronyms": [
                "DT",
                "PF",
                "HR",
                "CL"
            ],
            "speedChange": 1.5,
            "accuracy": 1,
            "combo": 195,
            "misses": null,
            "totalScore": 1322832,
            "classicTotalScore": 784111,
            "legacyTotalScore": 589868,
            "pp": 44.0436,
            "rank": 89
        },
        {
            "id": 7321340691,
            "date": "2026-08-22T04:57:44Z",
            "mode": 3,
            "beatmap": {
                "id": 30480,
                "beatmapsetId": 6892,
                "beatmapset": {
                    "id": 6892,
                    "artist": "Toyosaki Aki",
                    "title": "Happy!? Sorry!!",
                    "creator": "DJPop",
                    "userId": 2363,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Hard",
                "difficulty": 3.0749,
                "bpm": 179.98,
                "approachRate": 5,
                "circleSize": 5,
                "overallDifficulty": 5,
                "health": 4,
                "drainLength": 76,
                "status": 1
            },
            "beatmapId": 30480,
            "user": {
                "id": 17538226,
                "username": "Melonichia",
                "countryCode": "ID",
                "country": {
                    "id": "ID",
                    "name": "Indonesia"
                }
            },
            "userId": 17538226,
            "grade": 5,
            "modAcronyms": [
                "CL"
            ],
            "speedChange": 1,
            "accuracy": 0.976477,
            "combo": 356,
            "misses": 8,
            "totalScore": 904674,
            "classicTotalScore": 904674,
            "legacyTotalScore": 873382,
            "pp": 43.7324,
            "rank": 97
        },
        {
            "id": 7321174056,
            "date": "2026-08-22T04:14:05Z",
            "mode": 2,
            "beatmap": {
                "id": 17670,
                "beatmapsetId": 1357,
                "beatmapset": {
                    "id": 1357,
                    "artist": "Asian Kung-Fu Generation",
                    "title": "World World World",
                    "creator": "DawnII",
                    "userId": 8399,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Normal",
                "difficulty": 2.62672,
                "bpm": 114.75,
                "approachRate": 4,
                "circleSize": 4,
                "overallDifficulty": 4,
                "health": 0,
                "drainLength": 76,
                "status": 1
            },
            "beatmapId": 17670,
            "user": {
                "id": 38561180,
                "username": "Fubuki Feet",
                "countryCode": "US",
                "country": {
                    "id": "US",
                    "name": "United States"
                }
            },
            "userId": 38561180,
            "grade": 7,
            "modAcronyms": [
                "DT",
                "PF",
                "CL"
            ],
            "speedChange": 1.5,
            "accuracy": 1,
            "combo": 186,
            "misses": null,
            "totalScore": 1103282,
            "classicTotalScore": 1020775,
            "legacyTotalScore": 724955,
            "pp": 30.9176,
            "rank": 77
        },
        {
            "id": 7321969613,
            "date": "2026-08-22T07:48:30Z",
            "mode": 0,
            "beatmap": {
                "id": 10189,
                "beatmapsetId": 1201,
                "beatmapset": {
                    "id": 1201,
                    "artist": "Genbu",
                    "title": "Ganymede",
                    "creator": "Reikin",
                    "userId": 7186,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Another",
                "difficulty": 2.36954,
                "bpm": 82,
                "approachRate": 8,
                "circleSize": 6,
                "overallDifficulty": 8,
                "health": 5,
                "drainLength": 114,
                "status": 1
            },
            "beatmapId": 10189,
            "user": {
                "id": 32619071,
                "username": "technobuild",
                "countryCode": "US",
                "country": {
                    "id": "US",
                    "name": "United States"
                }
            },
            "userId": 32619071,
            "grade": 6,
            "modAcronyms": [
                "FL",
                "CL"
            ],
            "speedChange": 1,
            "accuracy": 0.960784,
            "combo": 354,
            "misses": null,
            "totalScore": 1051681,
            "classicTotalScore": 2639361,
            "legacyTotalScore": 3432137,
            "pp": 29.5694,
            "rank": 56
        },
        {
            "id": 7320170710,
            "date": "2026-08-22T00:08:57Z",
            "mode": 3,
            "beatmap": {
                "id": 27857,
                "beatmapsetId": 5257,
                "beatmapset": {
                    "id": 5257,
                    "artist": "Imogen Heap",
                    "title": "Headlock",
                    "creator": "Ruduen",
                    "userId": 22966,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Larto's Locked",
                "difficulty": 3.43509,
                "bpm": 120,
                "approachRate": 7,
                "circleSize": 4,
                "overallDifficulty": 7,
                "health": 6,
                "drainLength": 162,
                "status": 1
            },
            "beatmapId": 27857,
            "user": {
                "id": 27963579,
                "username": "Pro58324",
                "countryCode": "ES",
                "country": {
                    "id": "ES",
                    "name": "Spain"
                }
            },
            "userId": 27963579,
            "grade": 5,
            "modAcronyms": [
                "4K"
            ],
            "speedChange": 1,
            "accuracy": 0.992077,
            "combo": 689,
            "misses": null,
            "totalScore": 875398,
            "classicTotalScore": 875398,
            "legacyTotalScore": 0,
            "pp": 22.875,
            "rank": 40
        },
        {
            "id": 7321463385,
            "date": "2026-08-22T05:30:06Z",
            "mode": 0,
            "beatmap": {
                "id": 21614,
                "beatmapsetId": 3232,
                "beatmapset": {
                    "id": 3232,
                    "artist": "Capcom Sound Team",
                    "title": "Jewel Temptation",
                    "creator": "Zerostarry",
                    "userId": 3480,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Normal",
                "difficulty": 2.05044,
                "bpm": 182,
                "approachRate": 3,
                "circleSize": 4,
                "overallDifficulty": 3,
                "health": 3,
                "drainLength": 82,
                "status": 1
            },
            "beatmapId": 21614,
            "user": {
                "id": 32619071,
                "username": "technobuild",
                "countryCode": "US",
                "country": {
                    "id": "US",
                    "name": "United States"
                }
            },
            "userId": 32619071,
            "grade": 8,
            "modAcronyms": [
                "FL",
                "HR",
                "CL"
            ],
            "speedChange": 1,
            "accuracy": 1,
            "combo": 212,
            "misses": null,
            "totalScore": 1290200,
            "classicTotalScore": 952648,
            "legacyTotalScore": 696464,
            "pp": 21.7494,
            "rank": 88
        },
        {
            "id": 7320476539,
            "date": "2026-08-22T01:20:55Z",
            "mode": 3,
            "beatmap": {
                "id": 22466,
                "beatmapsetId": 3730,
                "beatmapset": {
                    "id": 3730,
                    "artist": "Orange Lounge",
                    "title": "Comment te dire adieu",
                    "creator": "Takuma",
                    "userId": 43677,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Hard",
                "difficulty": 2.45604,
                "bpm": 160,
                "approachRate": 6,
                "circleSize": 6,
                "overallDifficulty": 6,
                "health": 6,
                "drainLength": 84,
                "status": 1
            },
            "beatmapId": 22466,
            "user": {
                "id": 17043574,
                "username": "Rajan31",
                "countryCode": "AL",
                "country": {
                    "id": "AL",
                    "name": "Albania"
                }
            },
            "userId": 17043574,
            "grade": 5,
            "modAcronyms": [
                "4K"
            ],
            "speedChange": 1,
            "accuracy": 0.957886,
            "combo": 221,
            "misses": 4,
            "totalScore": 750532,
            "classicTotalScore": 750532,
            "legacyTotalScore": 0,
            "pp": 21.2244,
            "rank": 96
        },
        {
            "id": 7320213555,
            "date": "2026-08-22T00:18:48Z",
            "mode": 3,
            "beatmap": {
                "id": 25370,
                "beatmapsetId": 4931,
                "beatmapset": {
                    "id": 4931,
                    "artist": "Weird Al Yankovic",
                    "title": "White And Nerdy",
                    "creator": "James2250",
                    "userId": 16978,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Normal",
                "difficulty": 2.49631,
                "bpm": 143,
                "approachRate": 5,
                "circleSize": 5,
                "overallDifficulty": 5,
                "health": 3,
                "drainLength": 142,
                "status": 1
            },
            "beatmapId": 25370,
            "user": {
                "id": 38553620,
                "username": "2000rats",
                "countryCode": "US",
                "country": {
                    "id": "US",
                    "name": "United States"
                }
            },
            "userId": 38553620,
            "grade": 6,
            "modAcronyms": [
                "FI",
                "4K"
            ],
            "speedChange": 1,
            "accuracy": 0.957256,
            "combo": 282,
            "misses": 3,
            "totalScore": 749300,
            "classicTotalScore": 749300,
            "legacyTotalScore": 0,
            "pp": 18.2637,
            "rank": 56
        },
        {
            "id": 7320629912,
            "date": "2026-08-22T01:57:47Z",
            "mode": 3,
            "beatmap": {
                "id": 20103,
                "beatmapsetId": 2680,
                "beatmapset": {
                    "id": 2680,
                    "artist": "Laura Jane",
                    "title": "La La",
                    "creator": "Larto",
                    "userId": 12328,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Sweatin'",
                "difficulty": 3.08301,
                "bpm": 130,
                "approachRate": 4,
                "circleSize": 3,
                "overallDifficulty": 4,
                "health": 3,
                "drainLength": 124,
                "status": 1
            },
            "beatmapId": 20103,
            "user": {
                "id": 39727979,
                "username": "vzho7s",
                "countryCode": "CL",
                "country": {
                    "id": "CL",
                    "name": "Chile"
                }
            },
            "userId": 39727979,
            "grade": 5,
            "modAcronyms": [
                "4K"
            ],
            "speedChange": 1,
            "accuracy": 0.970044,
            "combo": 381,
            "misses": 1,
            "totalScore": 809186,
            "classicTotalScore": 809186,
            "legacyTotalScore": 0,
            "pp": 15.922,
            "rank": 55
        },
        {
            "id": 7321026810,
            "date": "2026-08-22T03:36:34Z",
            "mode": 2,
            "beatmap": {
                "id": 22132,
                "beatmapsetId": 3593,
                "beatmapset": {
                    "id": 3593,
                    "artist": "Yasunori Mitsuda",
                    "title": "Robo's Theme",
                    "creator": "kingcobra52",
                    "userId": 9934,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Normal",
                "difficulty": 1.82769,
                "bpm": 115.7,
                "approachRate": 4,
                "circleSize": 4,
                "overallDifficulty": 4,
                "health": 4,
                "drainLength": 62,
                "status": 1
            },
            "beatmapId": 22132,
            "user": {
                "id": 38561180,
                "username": "Fubuki Feet",
                "countryCode": "US",
                "country": {
                    "id": "US",
                    "name": "United States"
                }
            },
            "userId": 38561180,
            "grade": 7,
            "modAcronyms": [
                "DT",
                "PF",
                "CL"
            ],
            "speedChange": 1.5,
            "accuracy": 1,
            "combo": 145,
            "misses": null,
            "totalScore": 1109667,
            "classicTotalScore": 625331,
            "legacyTotalScore": 469026,
            "pp": 15.481,
            "rank": 97
        },
        {
            "id": 7321392211,
            "date": "2026-08-22T05:11:21Z",
            "mode": 1,
            "beatmap": {
                "id": 12061,
                "beatmapsetId": 510,
                "beatmapset": {
                    "id": 510,
                    "artist": "FLOW",
                    "title": "Okuru Kotoba",
                    "creator": "Kai",
                    "userId": 4537,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Easy",
                "difficulty": 1.79947,
                "bpm": 89.98,
                "approachRate": 3,
                "circleSize": 5,
                "overallDifficulty": 3,
                "health": 2,
                "drainLength": 76,
                "status": 1
            },
            "beatmapId": 12061,
            "user": {
                "id": 40259736,
                "username": "TeriDoki",
                "countryCode": "CL",
                "country": {
                    "id": "CL",
                    "name": "Chile"
                }
            },
            "userId": 40259736,
            "grade": 5,
            "modAcronyms": [],
            "speedChange": 1,
            "accuracy": 0.970588,
            "combo": 51,
            "misses": null,
            "totalScore": 916599,
            "classicTotalScore": 143502,
            "legacyTotalScore": 0,
            "pp": 15.166,
            "rank": 46
        },
        {
            "id": 7321008219,
            "date": "2026-08-22T03:31:52Z",
            "mode": 3,
            "beatmap": {
                "id": 36354,
                "beatmapsetId": 8965,
                "beatmapset": {
                    "id": 8965,
                    "artist": "Amuro vs Killer",
                    "title": "Mei",
                    "creator": "v2b",
                    "userId": 47060,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Normal",
                "difficulty": 2.20244,
                "bpm": 200,
                "approachRate": 5,
                "circleSize": 4,
                "overallDifficulty": 5,
                "health": 1,
                "drainLength": 101,
                "status": 1
            },
            "beatmapId": 36354,
            "user": {
                "id": 16268395,
                "username": "Granite Ocean",
                "countryCode": "CA",
                "country": {
                    "id": "CA",
                    "name": "Canada"
                }
            },
            "userId": 16268395,
            "grade": 5,
            "modAcronyms": [
                "CL"
            ],
            "speedChange": 1,
            "accuracy": 0.979957,
            "combo": 545,
            "misses": 1,
            "totalScore": 928243,
            "classicTotalScore": 928243,
            "legacyTotalScore": 958212,
            "pp": 11.0611,
            "rank": 35
        },
        {
            "id": 7320960125,
            "date": "2026-08-22T03:19:43Z",
            "mode": 3,
            "beatmap": {
                "id": 27423,
                "beatmapsetId": 5715,
                "beatmapset": {
                    "id": 5715,
                    "artist": "dBu",
                    "title": "Shanghai Alice of Meiji 17",
                    "creator": "Starrodkirby86",
                    "userId": 410,
                    "user": null
                },
                "mode": 0,
                "difficultyName": "Easy",
                "difficulty": 2.17362,
                "bpm": 140,
                "approachRate": 3,
                "circleSize": 4,
                "overallDifficulty": 3,
                "health": 3,
                "drainLength": 128,
                "status": 1
            },
            "beatmapId": 27423,
            "user": {
                "id": 40006685,
                "username": "so5estsx5",
                "countryCode": "CA",
                "country": {
                    "id": "CA",
                    "name": "Canada"
                }
            },
            "userId": 40006685,
            "grade": 7,
            "modAcronyms": [],
            "speedChange": 1,
            "accuracy": 0.997439,
            "combo": 256,
            "misses": null,
            "totalScore": 991335,
            "classicTotalScore": 991335,
            "legacyTotalScore": 0,
            "pp": 6.88427,
            "rank": 35
        }
    ],
    usingStandardized: true,
};

export const Default = {
    args: {
        ...mockProps,
    },
};