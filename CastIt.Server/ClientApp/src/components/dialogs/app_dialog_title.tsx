import { Close } from '@mui/icons-material';
import { DialogTitle, Grid, IconButton, Typography } from '@mui/material';
import { createStyles, makeStyles } from '@mui/styles';

const useStyles = makeStyles((theme) =>
    createStyles({
        dialogTitle: {
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            backgroundColor: theme.palette.primary.main,
        },
        title: {
            paddingLeft: 10,
        },
    })
);

interface Props {
    icon: JSX.Element;
    title: string;

    close(): void;
}

function AppDialogTitle(props: Props) {
    const classes = useStyles();
    return (
        <DialogTitle className={classes.dialogTitle}>
            <Grid container alignItems="center">
                {props.icon}
                <Typography className={classes.title} variant="h6">
                    {props.title}
                </Typography>
            </Grid>
            <IconButton onClick={props.close} size="large">
                <Close />
            </IconButton>
        </DialogTitle>
    );
}

export default AppDialogTitle;
