import ModSelectorRow from '../components/ModSelectorRow';

import './ModSelector.css';

export default {
    title: 'ModSelectorRow',
    component: ModSelectorRow,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    acronym: 'DT',
    mods: [{
        acronym: 'DT',
        active: false,
    }],
    setMods: (mods) => {}
};

export const Default = {
    args: {
        ...mockProps,
    },
};