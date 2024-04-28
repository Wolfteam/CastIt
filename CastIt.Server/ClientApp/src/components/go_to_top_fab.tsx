import { useEffect, useState } from 'react';
import { Fab } from '@mui/material';
import { ArrowUpward } from '@mui/icons-material';
import React from 'react';

function GoToTopFab() {
    const [scrolling, setScrolling] = useState<boolean>(false);

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
        <Fab
            onClick={handleFabClick}
            sx={(theme) => ({
                position: 'fixed',
                bottom: theme.spacing(2),
                right: theme.spacing(2),
            })}
        >
            <ArrowUpward />
        </Fab>
    );
}

export default React.memo(GoToTopFab);
