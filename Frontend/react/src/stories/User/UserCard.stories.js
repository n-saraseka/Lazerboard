import UserCard from '../../components/User/UserCard';

import './styles/UserCard.css';

export default {
    title: 'UserCard',
    component: UserCard,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    user: {
        id: 34446270,
        username: "Drop",
        countryCode: "US", 
        country: {
            id: "US",
            name: "United States"
        },
        userGroups: [],
        isRestricted: false
    },
    scoreCount: 125
};

export const Default = {
    args: {
        ...mockProps,
    },
};