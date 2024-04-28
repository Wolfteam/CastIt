import React, { useEffect, useState } from 'react';
import { styled } from '@mui/material/styles';
import {
    Typography,
    Divider,
    ListItemText,
    Avatar,
    ListItemAvatar,
    Tooltip,
    Menu,
    MenuItem,
    useTheme,
    ListItemButton,
} from '@mui/material';
import { IFileItemResponseDto } from '../../models';
import { onFileChanged, onFileEndReached, onPlayerStatusChanged } from '../../services/castithub.service';
import { Add, ClearAll, Delete, FileCopy, Loop, PlayArrow, Refresh } from '@mui/icons-material';
import translations from '../../services/translations';
import AddFilesDialog from '../dialogs/add_files_dialog';
import { Draggable } from '@hello-pangea/dnd';
import FileItemSubtitle from './file_item_subtitle';
import FileItemDuration from './file_item_duration';
import { useCastItHub } from '../../context/castit_hub.context';
import { useSnackbar } from 'notistack';

const StyledListItemText = styled(ListItemText)({
    marginLeft: 10,
});

interface Props {
    index: number;
    file: IFileItemResponseDto;
}

interface State {
    playListId: number;
    id: number;
    filename: string;
    subTitle: string;
    path: string;
    playedPercentage: number;
    position: number;
    playedTime: string;
    duration: string;
    fullTotalDuration: string;
    isBeingPlayed: boolean;
    loop: boolean;
}

interface ContextMenuState {
    mouseX: number | null;
    mouseY: number | null;
}

const initialState: State = {
    playListId: 0,
    id: 0,
    filename: '',
    subTitle: '',
    path: '',
    playedPercentage: 0,
    position: 1,
    playedTime: '',
    duration: '',
    fullTotalDuration: '',
    isBeingPlayed: false,
    loop: false,
};

const initialContextMenuState: ContextMenuState = {
    mouseX: null,
    mouseY: null,
};

function FileItem(props: Props) {
    const theme = useTheme();
    const [state, setState] = useState<State>(initialState);
    const [contextMenu, setContextMenu] = useState(initialContextMenuState);
    const [showAddFilesDialog, setShowAddFilesDialog] = useState(false);
    const { enqueueSnackbar } = useSnackbar();
    const castItHub = useCastItHub();

    useEffect(() => {
        setState({
            playListId: props.file.playListId,
            id: props.file.id,
            filename: props.file.filename,
            subTitle: props.file.subTitle,
            path: props.file.path,
            playedPercentage: props.file.playedPercentage,
            position: props.file.position,
            fullTotalDuration: props.file.fullTotalDuration,
            isBeingPlayed: props.file.isBeingPlayed,
            loop: props.file.loop,
            playedTime: props.file.playedTime,
            duration: props.file.duration,
        });
    }, [props.file]);

    useEffect(() => {
        const onFileChangedSubscription = onFileChanged.subscribe((file) => {
            if (file.id !== props.file.id) {
                if (state.isBeingPlayed) {
                    setState((s) => ({
                        ...s,
                        isBeingPlayed: false,
                    }));
                }
                return;
            }
            setState((s) => ({
                ...s,
                name: file.name,
                subTitle: file.subTitle,
                path: file.path,
                playedPercentage: file.playedPercentage,
                position: file.position,
                fullTotalDuration: file.fullTotalDuration,
                isBeingPlayed: file.isBeingPlayed,
                loop: file.loop,
            }));
        });

        const onPlayerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (!status) {
                return;
            }
            if (status.playedFile?.id !== props.file.id) {
                if (state.isBeingPlayed) {
                    setState((s) => ({
                        ...s,
                        isBeingPlayed: false,
                    }));
                }
                return;
            }

            setState((s) => ({
                ...s,
                name: status.playedFile!.name,
                subTitle: status.playedFile!.subTitle,
                path: status.playedFile!.path,
                playedPercentage: status.playedFile!.playedPercentage,
                position: status.playedFile!.position,
                fullTotalDuration: status.playedFile!.fullTotalDuration,
                isBeingPlayed: status.playedFile!.isBeingPlayed,
                loop: status.playedFile!.loop,
                playedTime: status.playedFile!.playedTime,
                duration: status.playedFile!.duration,
            }));
        });

        const onFileEndReachedSubscription = onFileEndReached.subscribe((file) => {
            if (file.id !== props.file.id) {
                return;
            }

            setState((s) => ({ ...s, playedPercentage: 100 }));
        });

        return () => {
            onFileChangedSubscription.unsubscribe();
            onPlayerStatusChangedSubscription.unsubscribe();
            onFileEndReachedSubscription.unsubscribe();
        };
    }, [props.file.id, state.isBeingPlayed]);

    const handleOpenContextMenu = (event: React.MouseEvent<HTMLDivElement>) => {
        event.preventDefault();
        setContextMenu({
            mouseX: event.clientX - 2,
            mouseY: event.clientY - 4,
        });
    };

    const handlePlay = async (force: boolean = false): Promise<void> => {
        handleCloseContextMenu();
        await castItHub.connection.play(props.file.playListId, state.id, force, false);
    };

    const handleCloseContextMenu = (): void => {
        setContextMenu(initialContextMenuState);
    };

    const handleToggleLoop = async (): Promise<void> => {
        handleCloseContextMenu();
        await castItHub.connection.loopFile(state.playListId, state.id, !state.loop);
    };

    const handleAddFiles = async (path: string | null, includeSubFolder: boolean, onlyVideo: boolean): Promise<void> => {
        handleCloseContextMenu();
        setShowAddFilesDialog(false);
        if (path) {
            await castItHub.connection.addFolderOrFileOrUrl(state.playListId, path, includeSubFolder, onlyVideo);
        }
    };

    const handleDelete = async (): Promise<void> => {
        handleCloseContextMenu();
        await castItHub.connection.deleteFile(props.file.playListId, props.file.id);
    };

    const handleRemoveAllMissing = async (): Promise<void> => {
        handleCloseContextMenu();
        await castItHub.connection.removeAllMissingFiles(props.file.playListId);
    };

    const handleShowAddFilesDialog = (): void => {
        handleCloseContextMenu();
        setShowAddFilesDialog(true);
    };

    //if the app runs on http you wouldn't be able to use the copy to clipboard method
    const secure = navigator.clipboard && window.isSecureContext;
    const handleCopy = async (): Promise<void> => {
        handleCloseContextMenu();
        const text = props.file.path;
        if (!text) {
            return;
        }

        if (!secure) {
            return;
        }

        await navigator.clipboard.writeText(text);
        enqueueSnackbar(translations.copiedToClipboard, { variant: 'success' });
    };

    const title = (
        <Tooltip title={state.filename}>
            <Typography className={'text-overflow-elipsis'}>{state.filename}</Typography>
        </Tooltip>
    );

    const toggleLoopMenuItem = state.isBeingPlayed ? (
        <MenuItem onClick={handleToggleLoop}>
            <Loop fontSize="small" />
            <StyledListItemText primary={!state.loop ? translations.loop : translations.disableLoop} />
        </MenuItem>
    ) : null;

    return (
        <Draggable draggableId={`${props.file.id}_${props.file.filename}`} index={props.index}>
            {(provided, snapshot) => (
                <div
                    data-played-file={state.isBeingPlayed}
                    onContextMenu={handleOpenContextMenu}
                    style={{ cursor: 'context-menu' }}
                    {...provided.dragHandleProps}
                    {...provided.draggableProps}
                    ref={provided.innerRef}
                >
                    <ListItemButton
                        onDoubleClick={() => handlePlay()}
                        sx={state.isBeingPlayed ? { backgroundColor: 'primary.light' } : null}
                        style={{
                            backgroundColor: snapshot.isDragging ? theme.palette.primary.dark : '',
                        }}
                    >
                        <ListItemAvatar>
                            <Avatar
                                sx={(theme) => ({
                                    color: theme.palette.getContrastText(theme.palette.primary.main),
                                    backgroundColor: theme.palette.primary.main
                                })}
                            >
                                {state.position}
                            </Avatar>
                        </ListItemAvatar>
                        <ListItemText
                            primary={title}
                            secondaryTypographyProps={{ component: 'span' }}
                            secondary={
                                <FileItemSubtitle
                                    path={state.path}
                                    playedPercentage={state.playedPercentage}
                                    subTitle={state.subTitle}
                                    duration={state.duration}
                                    playedTime={state.playedTime}
                                />
                            }
                        />
                        <FileItemDuration fullTotalDuration={state.fullTotalDuration} loop={state.loop} />
                    </ListItemButton>
                    {contextMenu.mouseY === null ? null : (
                        <Menu
                            keepMounted
                            open={contextMenu.mouseY !== null}
                            onClose={handleCloseContextMenu}
                            anchorReference="anchorPosition"
                            anchorPosition={
                                contextMenu.mouseY !== null && contextMenu.mouseX !== null
                                    ? { top: contextMenu.mouseY, left: contextMenu.mouseX }
                                    : undefined
                            }
                        >
                            <MenuItem onClick={() => handlePlay()}>
                                <PlayArrow fontSize="small" />
                                <StyledListItemText primary={translations.play} />
                            </MenuItem>
                            <MenuItem onClick={() => handlePlay(true)}>
                                <Refresh fontSize="small" />
                                <StyledListItemText primary={translations.playFromTheStart} />
                            </MenuItem>
                            {toggleLoopMenuItem}
                            <MenuItem onClick={handleShowAddFilesDialog}>
                                <Add fontSize="small" />
                                <StyledListItemText primary={translations.addFiles} />
                            </MenuItem>
                            {secure ? (
                                <MenuItem onClick={handleCopy}>
                                    <FileCopy fontSize="small" />
                                    <StyledListItemText primary={translations.copyPath} />
                                </MenuItem>
                            ) : null}
                            {/* TODO: REMOVE ALL SELECTED AND SELECT ALL */}
                            <MenuItem onClick={handleDelete}>
                                <Delete fontSize="small" />
                                <StyledListItemText primary={translations.remove} />
                            </MenuItem>
                            <MenuItem onClick={handleRemoveAllMissing}>
                                <ClearAll fontSize="small" />
                                <StyledListItemText primary={translations.removeAllMissing} />
                            </MenuItem>
                        </Menu>
                    )}
                    <Divider variant="middle" />
                    <AddFilesDialog isOpen={showAddFilesDialog} onClose={handleAddFiles} />
                </div>
            )}
        </Draggable>
    );
}

export default FileItem;
