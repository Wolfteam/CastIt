import { Container, Typography } from '@mui/material';
import Grid from '@mui/material/Grid2';
import { Info } from '@mui/icons-material';
import translations from '../services/translations';
import { JSX } from 'react';

interface Props {
    icon?: JSX.Element;
    text?: string;
    children?: JSX.Element | JSX.Element[];
}

function NothingFound(props: Props) {
    return (
        <Container sx={{ flex: 'auto', height: '100%' }}>
            <Grid container justifyContent="center" alignItems="center" sx={{ textAlign: 'center', height: '100%' }}>
                <Grid size={12}>
                    {props.icon ?? <Info fontSize="large" />}
                    <Typography>{props.text ?? translations.nothingFound}</Typography>
                    {props.children}
                </Grid>
            </Grid>
        </Container>
    );
}

export default NothingFound;
