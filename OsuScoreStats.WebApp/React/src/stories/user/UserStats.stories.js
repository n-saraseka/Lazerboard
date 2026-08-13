import UserStats from '../../components/User/UserStats';

import '../styles/UserStats.css';

export default {
    title: 'UserStats',
    component: UserStats,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    data: {
        "count": 231,
        "history": [
            {
                "month": "2023-10-31T21:00:00Z",
                "monthlyCount": 1
            },
            {
                "month": "2023-11-30T21:00:00Z",
                "monthlyCount": 2
            },
            {
                "month": "2024-01-31T21:00:00Z",
                "monthlyCount": 26
            },
            {
                "month": "2024-02-29T21:00:00Z",
                "monthlyCount": 27
            },
            {
                "month": "2024-03-31T21:00:00Z",
                "monthlyCount": 38
            },
            {
                "month": "2024-04-30T21:00:00Z",
                "monthlyCount": 45
            },
            {
                "month": "2024-05-31T21:00:00Z",
                "monthlyCount": 49
            },
            {
                "month": "2024-06-30T21:00:00Z",
                "monthlyCount": 51
            },
            {
                "month": "2024-07-31T21:00:00Z",
                "monthlyCount": 52
            },
            {
                "month": "2024-08-31T21:00:00Z",
                "monthlyCount": 73
            },
            {
                "month": "2024-09-30T21:00:00Z",
                "monthlyCount": 74
            },
            {
                "month": "2024-11-30T21:00:00Z",
                "monthlyCount": 165
            },
            {
                "month": "2024-12-31T21:00:00Z",
                "monthlyCount": 167
            },
            {
                "month": "2025-01-31T21:00:00Z",
                "monthlyCount": 169
            },
            {
                "month": "2025-02-28T21:00:00Z",
                "monthlyCount": 192
            },
            {
                "month": "2025-03-31T21:00:00Z",
                "monthlyCount": 194
            },
            {
                "month": "2025-04-30T21:00:00Z",
                "monthlyCount": 195
            },
            {
                "month": "2025-05-31T21:00:00Z",
                "monthlyCount": 203
            },
            {
                "month": "2025-06-30T21:00:00Z",
                "monthlyCount": 215
            },
            {
                "month": "2025-07-31T21:00:00Z",
                "monthlyCount": 216
            },
            {
                "month": "2025-08-31T21:00:00Z",
                "monthlyCount": 217
            },
            {
                "month": "2025-11-30T21:00:00Z",
                "monthlyCount": 218
            },
            {
                "month": "2025-12-31T21:00:00Z",
                "monthlyCount": 219
            },
            {
                "month": "2026-01-31T21:00:00Z",
                "monthlyCount": 222
            },
            {
                "month": "2026-02-28T21:00:00Z",
                "monthlyCount": 225
            },
            {
                "month": "2026-04-30T21:00:00Z",
                "monthlyCount": 226
            },
            {
                "month": "2026-05-31T21:00:00Z",
                "monthlyCount": 230
            },
            {
                "month": "2026-06-30T21:00:00Z",
                "monthlyCount": 231
            }
        ],
        "starStats": [
            {
                "srBracket": 1,
                "count": 28
            },
            {
                "srBracket": 2,
                "count": 122
            },
            {
                "srBracket": 3,
                "count": 60
            },
            {
                "srBracket": 4,
                "count": 21
            }
        ],
        "rankStats": [
            {
                "rankBound": 1,
                "count": 3
            },
            {
                "rankBound": 5,
                "count": 15
            },
            {
                "rankBound": 10,
                "count": 24
            },
            {
                "rankBound": 25,
                "count": 52
            },
            {
                "rankBound": 50,
                "count": 112
            },
            {
                "rankBound": 100,
                "count": 231
            }
        ],
        "speedStats": [
            {
                "speedBracket": 1,
                "count": 3
            },
            {
                "speedBracket": 1.5,
                "count": 113
            },
            {
                "speedBracket": 1.6,
                "count": 4
            },
            {
                "speedBracket": 1.7,
                "count": 6
            },
            {
                "speedBracket": 1.8,
                "count": 7
            },
            {
                "speedBracket": 1.9,
                "count": 1
            },
            {
                "speedBracket": 2,
                "count": 97
            }
        ]
    }
};

export const Default = {
    args: {
        ...mockProps,
    },
};