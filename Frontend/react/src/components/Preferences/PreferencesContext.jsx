import { createContext } from 'react'

export const PreferencesContext = createContext({
    theme: 'dark',
    preferStandardizedScoring: true,
    globalPreferences: {
        view: 'cards',
        modes: Array(4).fill(0).map((m, i) => {
            return { value: i, enabled: true };
        }),
        excludeConverts: true,
        beatmapStatus: 'all',
        amount: 25
    },
    rankingPreferences: {
        amount: 10
    }
});