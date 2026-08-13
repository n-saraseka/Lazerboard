import ModSelector from '../../components/Selectors/ModSelector';

import '../styles/ModSelector.css';
import {allMods} from "../../utils/score-things.js";

export default {
    title: 'ModSelector',
    component: ModSelector,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

let mockMods = [];
for (const key of Object.keys(allMods)) {
    const matchingMods = allMods[key].filter(m => m.modes.includes(0));
    matchingMods.forEach(m => {
        mockMods.push({
            acronym: m.acronym,
            active: false,
        })
    })
}

const mockProps = {
    availableMods: mockMods,
    excludeMode: false,
    filters: [],
    setFilters: () => {},
    refetchScores: () => {}
};

export const Default = {
    args: {
        ...mockProps,
    },
};