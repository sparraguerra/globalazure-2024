import { createUseStyles } from "react-jss";

export const FighterStyles = createUseStyles({
    container: {
        display: 'flex',
        flexFlow: 'column',
        justifyContent: 'center',
        alignItems: 'center',
        padding: '20px',
        backgroundColor: 'rgba(0,0,0,0.7)',
        color: 'white',
        borderRadius: '12px',
    },
    title: {
        fontSize: '2rem',
        marginBottom: '1rem',
    },
    attackButton: {
        padding: '0.5rem 2rem',
        fontSize: '1rem',
        backgroundColor: 'white',
        color: 'black',
        border: 'none',
        borderRadius: '5px',
        cursor: 'pointer',
    },
    attackInput: {
        padding: '0.5rem',
        fontSize: '1rem',
        border: 'none',
        borderRadius: '5px',
        marginRight: '1rem',
    },
    image: {
        width: '200px',
        height: '200px',
        marginRight: '1rem',
        borderRadius: '25%',
    },
    '@media (max-width: 768px)': {
        container: {
            flexDirection: 'column',
        },
    },
});