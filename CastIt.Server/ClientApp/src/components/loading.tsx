import { Container, Grid, CircularProgress, Typography } from '@mui/material';

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
                <Grid item xs={12}>
                    <CircularProgress />
                    {props.message ? <Typography>{props.message}</Typography> : null}
                </Grid>
            </Grid>
        </Container>
    );
}

export default Loading;
