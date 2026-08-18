import ModSelectorRow from '../../components/Inputs/ModSelectorRow';

import './styles/ModSelector.css';

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