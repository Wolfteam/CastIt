import { useEffect, useState } from 'react';
import { Fab } from '@mui/material';
import { createStyles, makeStyles } from '@mui/styles';
import { ArrowUpward } from '@mui/icons-material';
import React from 'react';

const useStyles = makeStyles((theme) =>
    createStyles({
        backButton: {
            marginTop: 10,
        },
        fab: {
            position: 'fixed',
            bottom: theme.spacing(2),
            right: theme.spacing(2),
        },
    })
);

function GoToTopFab() {
    const [scrolling, setScrolling] = useState<boolean>(false);
    const classes = useStyles();

    useEffect(() => {
        window.addEventListener('scroll', handleScroll);
        return () => {
            window.removeEventListener('scroll', handleScroll);
        };
    }, []);

    const handleFabClick = () => {
        window.scrollTo({
            top: 0,
            left: 0,
            behavior: 'smooth',
        });
    };

    const handleScroll = () => {
        let currentScrollPos = window.pageYOffset;
        if (currentScrollPos > 0) {
            setScrolling(true);
        } else {
            setScrolling(false);
        }
    };

    return !scrolling ? null : (
        <Fab className={classes.fab} onClick={handleFabClick}>
            <ArrowUpward />
        </Fab>
    );
}

export default React.memo(GoToTopFab);
