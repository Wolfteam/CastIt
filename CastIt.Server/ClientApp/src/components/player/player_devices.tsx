import { useEffect, useState } from 'react';
import { Button, Dialog, DialogActions, DialogContent, IconButton, LinearProgress, List } from '@mui/material';
import { Tv, Add } from '@mui/icons-material';
import DeviceItem from '../device/device_item';
import { onCastDevicesChanged, onCastDeviceSet } from '../../services/castithub.service';
import translations from '../../services/translations';
import { IReceiver } from '../../models';
import AppDialogTitle from '../dialogs/app_dialog_title';
import AddEditEndpointDialog from '../dialogs/add_edit_endpoint_dialog';
import { useCastItHub } from '../../context/castit_hub.context';

function PlayerDevices() {
    const [devices, setDevices] = useState<IReceiver[]>([]);
    const [open, setOpen] = useState(false);
    const [isRefreshing, setIsRefreshing] = useState(false);
    const [formOpen, setFormOpen] = useState(false);
    const [editingEndpoint, setEditingEndpoint] = useState<IReceiver | null>(null);
    const castItHub = useCastItHub();

    useEffect(() => {
        const onCastDeviceSetSubscription = onCastDeviceSet.subscribe((device) => {
            const existing = devices.find((d) => d.id === device.id);
            if (!existing) {
                return;
            }
            const index = devices.indexOf(existing!);
            const updatedDevices = [...devices];
            updatedDevices.splice(index, 1);
            updatedDevices.splice(index, 0, device);
            setDevices(updatedDevices);
        });

        const onCastDevicesChangedSubscription = onCastDevicesChanged.subscribe((devices) => {
            setIsRefreshing(false);
            setDevices(devices);
        });

        return () => {
            onCastDeviceSetSubscription.unsubscribe();
            onCastDevicesChangedSubscription.unsubscribe();
        };
    }, []);

    const handleClickOpen = () => {
        setOpen(true);
    };

    const handleClose = () => {
        setOpen(false);
    };

    const handleRefreshDevices = async (): Promise<void> => {
        setIsRefreshing(true);
        await castItHub.connection.refreshCastDevices();
    };

    const handleAdd = (): void => {
        setEditingEndpoint(null);
        setFormOpen(true);
    };

    const handleEdit = (device: IReceiver): void => {
        setEditingEndpoint(device);
        setFormOpen(true);
    };

    const handleDeleteEndpoint = async (device: IReceiver): Promise<void> => {
        await castItHub.connection.deleteCustomEndpoint(Number(device.id));
    };

    const handleFormClose = (): void => {
        setFormOpen(false);
        setEditingEndpoint(null);
    };

    const deviceItems = devices.map((d) => (
        <DeviceItem
            key={d.id}
            id={d.id}
            name={d.friendlyName}
            ipAddress={`${d.host}:${d.port}`}
            isConnected={d.isConnected}
            isCustom={d.isCustom}
            onEdit={() => handleEdit(d)}
            onDelete={() => handleDeleteEndpoint(d)}
        />
    ));

    return (
        <>
            <IconButton onClick={handleClickOpen} size="large">
                <Tv fontSize="large" />
            </IconButton>
            <Dialog fullWidth={true} open={open} maxWidth="xs" onClose={handleClose}>
                <AppDialogTitle title={translations.devices} icon={<Tv />} close={handleClose} />
                {isRefreshing && <LinearProgress />}
                <DialogContent style={{ paddingBottom: 0, paddingTop: 0 }}>
                    <List>{deviceItems}</List>
                </DialogContent>
                <DialogActions>
                    <Button
                        color="primary"
                        variant="outlined"
                        startIcon={<Add />}
                        sx={{ flex: 1 }}
                        onClick={handleAdd}
                    >
                        {translations.addDevice}
                    </Button>
                    <Button
                        color="primary"
                        variant="outlined"
                        disabled={isRefreshing}
                        sx={{ flex: 1 }}
                        onClick={handleRefreshDevices}
                    >
                        {isRefreshing ? translations.refreshing : translations.refresh}
                    </Button>
                </DialogActions>
            </Dialog>
            <AddEditEndpointDialog isOpen={formOpen} endpoint={editingEndpoint} onClose={handleFormClose} />
        </>
    );
}

export default PlayerDevices;
