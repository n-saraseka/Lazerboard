import CountrySelector from '../components/CountrySelector';

import './CountrySelector.css';

export default {
    title: 'CountrySelector',
    component: CountrySelector,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    countries: [
        {
            id: 'UA', 
            name: 'Ukraine'
        },
        {
            id: 'Ru',
            name: 'Russian Federation'
        },
        {
            id: 'US',
            name: 'United States'
        },
        {
            id: 'GB',
            name: 'United Kingdom'
        },
        {
            id: 'ID',
            name: 'Indonesia'
        },
        {
            id: 'SE',
            name: 'Sweden'
        },
        {
            id: 'NO',
            name: 'Norway'
        }],
    filters: {country: {id: 'All', name: 'All countries'}},
    setFilters: () => {},
};

export const Default = {
    args: {
        ...mockProps,
    },
};