import { useScrollTrigger } from '@mui/material';
import React from 'react';

interface Props {
    children: React.ReactElement<any>;
}

function ElevationScroll(props: Props) {
    const { children } = props;
    const trigger = useScrollTrigger({
        disableHysteresis: true,
        threshold: 0,
    });

    return React.cloneElement(children, {
        elevation: trigger ? 4 : 0,
    });
}

export default ElevationScroll;
