import CountrySelectorRow from '../../components/Inputs/CountrySelectorRow';

import './styles/CountrySelector.css';

export default {
    title: 'CountrySelectorRow',
    component: CountrySelectorRow,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    country: {
        id: 'UA',
        name: 'Ukraine'
    },
    isHeaderRow: true,
    onClickAction: () => console.log('clicked')
};

export const Default = {
    args: {
        ...mockProps,
    },
};