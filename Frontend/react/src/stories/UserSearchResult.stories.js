import UserSearchResult from '../components/UserSearchResult';

import './UserSearchResult.css';

export default {
    title: 'UserSearchResult',
    component: UserSearchResult,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    user: {
        "id": 2558286,
        "username": "Rafis",
        "countryCode": "PL",
        "country": {
            "id": "PL",
            "name": "Poland"
        }
    },
};

export const Default = {
    args: {
        ...mockProps,
    },
};