import { createContext } from 'react'

export const PreferencesContext = createContext({
    theme: 'dark',
    preferStandardizedScoring: true
});