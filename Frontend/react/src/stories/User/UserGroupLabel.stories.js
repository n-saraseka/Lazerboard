import UserGroupLabel from '../../components/User/UserGroupLabel';

import './styles/UserGroupLabel.css';

export default {
    title: 'UserGroupLabel',
    component: UserGroupLabel,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    group: 1,
};

export const Default = {
    args: {
        ...mockProps,
    },
};