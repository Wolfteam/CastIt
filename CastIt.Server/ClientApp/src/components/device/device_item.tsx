import { IconButton, Typography } from '@mui/material';
import Grid from '@mui/material/Grid';
import { Tv, CastConnected, Cast, Edit, Delete } from '@mui/icons-material';
import { useCastItHub } from '../../context/castit_hub.context';

interface Props {
    id: string;
    name: string;
    ipAddress: string;
    isConnected?: boolean;
    isCustom?: boolean;
    onEdit?: () => void;
    onDelete?: () => void;
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
            <Grid size={props.isCustom ? 8 : 10}>
                <Grid container direction="column">
                    <Typography>{props.name}</Typography>
                    <Typography variant="subtitle1">{props.ipAddress}</Typography>
                </Grid>
            </Grid>
            <Grid size={props.isCustom ? 3 : 1}>
                <Grid container justifyContent="flex-end" alignItems="center" wrap="nowrap">
                    <IconButton onClick={handleToggleConnect} size="large">
                        {connectedIcon}
                    </IconButton>
                    {props.isCustom && (
                        <>
                            <IconButton onClick={props.onEdit} size="large">
                                <Edit />
                            </IconButton>
                            <IconButton onClick={props.onDelete} size="large">
                                <Delete />
                            </IconButton>
                        </>
                    )}
                </Grid>
            </Grid>
        </Grid>
    );
}

export default DeviceItem;
