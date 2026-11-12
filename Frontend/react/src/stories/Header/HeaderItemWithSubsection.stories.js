import HeaderItemWithSubsection from '../../components/Header/HeaderItemWithSubsection.jsx';

import './styles/HeaderItemWithSubsection.css';

export default {
    title: 'HeaderItemWithSubsection',
    component: HeaderItemWithSubsection,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    currentPage: "scores",
    sectionTitle: "Scores",
    subItems: [
        {
            techTitle: "scores",
            url: "/",
            title: "Global"
        },
        {
            techTitle: "millionscores",
            url: "/millions",
            title: "Mania millions"
        },
    ]
};

export const Default = {
    args: {
        ...mockProps,
    },
};