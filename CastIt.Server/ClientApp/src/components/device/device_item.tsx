import { IconButton, Typography } from '@mui/material';
import Grid from '@mui/material/Grid2';
import { Tv, CastConnected, Cast } from '@mui/icons-material';
import { useCastItHub } from '../../context/castit_hub.context';

interface Props {
    id: string;
    name: string;
    ipAddress: string;
    isConnected?: boolean;
}

function DeviceItem(props: Props) {
    const castItHub = useCastItHub();
    const connectedIcon = props.isConnected ? <CastConnected /> : <Cast />;

    const handleToggleConnect = async (): Promise<void> => {
        const id = props.isConnected ? null : props.id;
        await castItHub.connection.connectToCastDevice(id);
    };

    return (
        <Grid container justifyContent="space-evenly" alignItems="center">
            <Grid size={1}>
                <Tv />
            </Grid>
            <Grid size={10}>
                <Grid container direction="column">
                    <Typography>{props.name}</Typography>
                    <Typography variant="subtitle1">{props.ipAddress}</Typography>
                </Grid>
            </Grid>
            <Grid size={1}>
                <IconButton onClick={handleToggleConnect} size="large">
                    {connectedIcon}
                </IconButton>
            </Grid>
        </Grid>
    );
}

export default DeviceItem;
