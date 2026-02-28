import { Container, CircularProgress, Typography } from '@mui/material';
import Grid from '@mui/material/Grid';

interface Props {
    message?: string;
}

function Loading(props: Props) {
    return (
        <Container>
            <Grid
                container
                justifyContent="center"
                alignItems="center"
                direction="column"
                style={{ minHeight: '100vh', textAlign: 'center' }}
            >
                <Grid size={12}>
                    <CircularProgress />
                    {props.message ? <Typography>{props.message}</Typography> : null}
                </Grid>
            </Grid>
        </Container>
    );
}

export default Loading;
