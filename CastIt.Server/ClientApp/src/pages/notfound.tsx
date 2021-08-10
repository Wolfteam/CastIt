import { Container, Grid, Typography } from '@material-ui/core';
import { Info } from '@material-ui/icons';
import translations from '../services/translations';

function NotFound() {
    return (
        <Container style={{ flex: 'auto' }}>
            <Grid container justifyContent="center" alignItems="center" style={{ textAlign: 'center', height: '100%' }}>
                <Grid item xs={12}>
                    <Info fontSize="large" />
                    <Typography>{translations.errorCodes.notFound}</Typography>
                </Grid>
            </Grid>
        </Container>
    );
}

export default NotFound;
