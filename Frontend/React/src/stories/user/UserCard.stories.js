import UserCard from '../../components/User/UserCard';

import '../styles/UserCard.css';

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
        id: 8706541,
        username: 'SomeMelGuy',
        countryCode: 'UA',
        country: {
            id: 'UA',
            name: 'Ukraine'
        }
    },
    scoreCount: 125
};

export const Default = {
    args: {
        ...mockProps,
    },
};