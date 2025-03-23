import { Close } from '@mui/icons-material';
import { DialogTitle, Grid, IconButton, Typography } from '@mui/material';
import { JSX } from 'react';

interface Props {
    icon: JSX.Element;
    title: string;

    close(): void;
}

function AppDialogTitle(props: Props) {
    return (
        <DialogTitle
            sx={(theme) => ({
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                backgroundColor: `${theme.palette.primary.main}`,
            })}
        >
            <Grid container alignItems="center">
                {props.icon}
                <Typography variant="h6" sx={{ paddingLeft: 2 }}>
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
