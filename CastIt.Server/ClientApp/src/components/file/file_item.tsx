import React, { useEffect, useState } from 'react';
import {
    Typography,
    Divider,
    ListItem,
    ListItemText,
    Avatar,
    ListItemAvatar,
    makeStyles,
    Tooltip,
    createStyles,
    Menu,
    MenuItem,
    useTheme,
} from '@material-ui/core';
import { IFileItemResponseDto } from '../../models';
import { onFileChanged, onFileEndReached, onPlayerStatusChanged } from '../../services/castithub.service';
import { Add, ClearAll, Delete, FileCopy, Loop, PlayArrow, Refresh } from '@material-ui/icons';
import translations from '../../services/translations';
import AddFilesDialog from '../dialogs/add_files_dialog';
import { Draggable } from 'react-beautiful-dnd';
import FileItemSubtitle from './file_item_subtitle';
import FileItemDuration from './file_item_duration';
import { useCastItHub } from '../../context/castit_hub.context';

const useStyles = makeStyles((theme) =>
    createStyles({
        position: {
            color: theme.palette.getContrastText(theme.palette.primary.main),
            backgroundColor: theme.palette.primary.main,
        },
        beingPlayed: {
            backgroundColor: theme.palette.primary.light,
        },
        menuItemText: {
            marginLeft: 10,
        },
    })
);

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
    const classes = useStyles();
    const theme = useTheme();
    const [state, setState] = useState<State>(initialState);
    const [contextMenu, setContextMenu] = useState(initialContextMenuState);
    const [showAddFilesDialog, setShowAddFilesDialog] = useState(false);
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

    const handleCopy = (): void => {
        handleCloseContextMenu();
        navigator.clipboard.writeText(props.file.path);
    };

    const title = (
        <Tooltip title={state.filename}>
            <Typography className={'text-overflow-elipsis'}>{state.filename}</Typography>
        </Tooltip>
    );

    const toggleLoopMenuItem = state.isBeingPlayed ? (
        <MenuItem onClick={handleToggleLoop}>
            <Loop fontSize="small" />
            <ListItemText className={classes.menuItemText} primary={!state.loop ? translations.loop : translations.disableLoop} />
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
                    <ListItem
                        button
                        onDoubleClick={() => handlePlay()}
                        className={state.isBeingPlayed ? classes.beingPlayed : ''}
                        style={{
                            backgroundColor: snapshot.isDragging ? theme.palette.primary.dark : '',
                        }}
                    >
                        <ListItemAvatar>
                            <Avatar className={classes.position}>{state.position}</Avatar>
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
                    </ListItem>
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
                                <ListItemText className={classes.menuItemText} primary={translations.play} />
                            </MenuItem>
                            <MenuItem onClick={() => handlePlay(true)}>
                                <Refresh fontSize="small" />
                                <ListItemText className={classes.menuItemText} primary={translations.playFromTheStart} />
                            </MenuItem>
                            {toggleLoopMenuItem}
                            <MenuItem onClick={handleShowAddFilesDialog}>
                                <Add fontSize="small" />
                                <ListItemText className={classes.menuItemText} primary={translations.addFiles} />
                            </MenuItem>
                            <MenuItem onClick={handleCopy}>
                                <FileCopy fontSize="small" />
                                <ListItemText className={classes.menuItemText} primary={translations.copyPath} />
                            </MenuItem>
                            {/* TODO: REMOVE ALL SELECTED AND SELECT ALL */}
                            <MenuItem onClick={handleDelete}>
                                <Delete fontSize="small" />
                                <ListItemText className={classes.menuItemText} primary={translations.remove} />
                            </MenuItem>
                            <MenuItem onClick={handleRemoveAllMissing}>
                                <ClearAll fontSize="small" />
                                <ListItemText className={classes.menuItemText} primary={translations.removeAllMissing} />
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
