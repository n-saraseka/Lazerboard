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
    isPartOfList: false,
    onClickAction: () => console.log('clicked'),
    hasChevron: true
};

export const Default = {
    args: {
        ...mockProps,
    },
};