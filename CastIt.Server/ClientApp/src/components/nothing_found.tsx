import { makeStyles, createStyles, Container, Grid, Typography } from '@material-ui/core';
import { Info } from '@material-ui/icons';
import translations from '../services/translations';

const useStyles = makeStyles((theme) =>
    createStyles({
        root: {
            flex: 'auto',
            height: '100%',
        },
        nothingFound: {
            textAlign: 'center',
            height: '100%',
        },
    })
);

interface Props {
    icon?: JSX.Element;
    text?: string;
    children?: JSX.Element | JSX.Element[];
}

function NothingFound(props: Props) {
    const classes = useStyles();
    return (
        <Container className={classes.root}>
            <Grid container className={classes.nothingFound} justifyContent="center" alignItems="center">
                <Grid item xs={12}>
                    {props.icon ?? <Info fontSize="large" />}
                    <Typography>{props.text ?? translations.nothingFound}</Typography>
                    {props.children}
                </Grid>
            </Grid>
        </Container>
    );
}

export default NothingFound;
