import {
    Button,
    CardActions,
    CardActionArea,
    CardContent,
    CardMedia,
    Fab,
    IconButton,
    Popover,
    Tooltip,
    Typography,
    Card,
    Grid,
} from '@mui/material';
import { styled } from '@mui/material/styles';
import React, { JSX, useEffect, useState } from 'react';
import { Add, Delete, MoreVert, Edit, Sort } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { playListPath } from '../../routes';
import RenamePlayListDialog from '../dialogs/rename_playlist_dialog';
import { onPlayerStatusChanged } from '../../services/castithub.service';
import translations from '../../services/translations';
import PlayListLoopShuffleButton from './playlist_loop_shuffle_button';
import AddFilesDialog from '../dialogs/add_files_dialog';
import { IGetAllPlayListResponseDto } from '../../models';
import { useCastItHub } from '../../context/castit_hub.context';
import { defaultImg } from '../../utils/app_constants';

const StyledRootCard = styled(Card)({
    minWidth: 175
});

const StyledTypography = styled(Typography)({
    textOverflow: 'ellipsis',
    overflow: 'hidden',
    whiteSpace: 'nowrap',
});

const StyledMoreButtons = styled(Button)({
    width: '100%',
    color: 'white',
});

interface Props {
    index: number;
    toAddNewItem?: boolean;
    raised?: boolean;
    playList: IGetAllPlayListResponseDto;
    onReOrderClick?(): void;
}

interface State {
    id: number;
    name: string;
    totalDuration: string;
    shuffle?: boolean;
    loop?: boolean;
    numberOfFiles: number;
    position: number;
    imageUrl: string;
    loaded: boolean;
}

const initialState: State = {
    id: 0,
    name: '',
    totalDuration: '',
    numberOfFiles: 0,
    position: 0,
    imageUrl: '',
    loaded: false,
};

function PlayListCardItem(props: Props): JSX.Element {
    const navigate = useNavigate();
    const [anchorEl, setAnchorEl] = useState<HTMLElement>();

    const [state, setState] = useState(initialState);
    const [showRenameDialog, setShowRenameDialog] = useState(false);
    const [showAddFilesDialog, setShowAddFilesDialog] = useState(false);

    const castItHub = useCastItHub();

    useEffect(() => {
        setState({ ...props.playList, loaded: true });
    }, [props.playList]);

    useEffect(() => {
        const onPlayerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (!status) {
                return;
            }

            if (!status.playList) {
                return;
            }

            if (props.playList.id !== status.playList.id) {
                return;
            }

            setState((s) => ({ ...s, ...status.playList! }));
        });
        return () => {
            onPlayerStatusChangedSubscription.unsubscribe();
        };
    }, [props.playList]);

    const handleShowMoreClick = (event: React.MouseEvent<HTMLElement>): void => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = (): void => {
        setAnchorEl(undefined);
    };

    const handleClick = (): void => {
        const route = playListPath.replace(':id', `${state.id}`);
        navigate(route);
    };

    const handleDelete = async (): Promise<void> => {
        await castItHub.connection.deletePlayList(state.id);
        handleClose();
    };

    const handleAddNew = async (): Promise<void> => {
        await castItHub.connection.addNewPlayList();
    };

    const handleRename = async (newName: string | null): Promise<void> => {
        handleClose();
        setShowRenameDialog(false);
        if (newName) {
            await castItHub.connection.updatePlayList(state.id, newName);
        }
    };

    const handleAddFiles = async (path: string | null, includeSubFolder: boolean, onlyVideo: boolean): Promise<void> => {
        setShowAddFilesDialog(false);
        if (path) {
            await castItHub.connection.addFolderOrFileOrUrl(state.id, path, includeSubFolder, onlyVideo);
        }
    };

    const handleSort = () => {
        handleClose();
        props.onReOrderClick!();
    };

    const showMorePopup = Boolean(anchorEl);
    const moreId = anchorEl ? 'open-more-popover' : undefined;

    if (props.toAddNewItem) {
        return (
            <StyledRootCard onClick={handleAddNew}>
                <CardActionArea style={{ height: '100%', textAlign: 'center' }}>
                    <CardContent>
                        <Grid container justifyContent="center" alignItems="center">
                            <Add fontSize="large" color="primary" />
                            <Typography color="textSecondary">{translations.addNewPlayList}</Typography>
                        </Grid>
                    </CardContent>
                </CardActionArea>
            </StyledRootCard>
        );
    }

    if (!state.loaded) {
        return <StyledRootCard />;
    }

    const image = state.imageUrl ?? defaultImg;
    return (
        <StyledRootCard raised={props.raised}>
            <CardActionArea onClick={handleClick}>
                <CardMedia component="img" image={image} title={state.name} sx={{ width: '100%', height: 250, objectFit: 'fill' }} />
                <CardContent sx={{ paddingBottom: 0 }}>
                    <Fab color="primary" component="div" style={{ float: 'right', marginTop: -50 }}>
                        {state.numberOfFiles}
                    </Fab>
                    <Typography color="textSecondary" gutterBottom sx={{ fontSize: 14 }}>
                        {translations.playList}
                    </Typography>
                    <Tooltip title={state.name}>
                        <StyledTypography variant="h5">
                            {state.name}
                        </StyledTypography>
                    </Tooltip>
                    <Tooltip title={state.totalDuration}>
                        <StyledTypography color="textSecondary">{state.totalDuration}</StyledTypography>
                    </Tooltip>
                </CardContent>
            </CardActionArea>
            <CardActions
                disableSpacing
                sx={{
                    justifyContent: 'flex-end',
                    '& button': {
                        padding: 1
                    },
                }}
            >
                <IconButton onClick={() => setShowAddFilesDialog(true)} size="large">
                    <Add />
                </IconButton>
                <PlayListLoopShuffleButton id={state.id} loop={state.loop} shuffle={state.shuffle} renderLoop />
                <PlayListLoopShuffleButton id={state.id} loop={state.loop} shuffle={state.shuffle} />
                <IconButton onClick={handleShowMoreClick} size="large">
                    <MoreVert />
                </IconButton>
                {!showMorePopup ? null : (
                    <Popover
                        id={moreId}
                        open={showMorePopup}
                        anchorEl={anchorEl}
                        onClose={handleClose}
                        anchorOrigin={{
                            vertical: 'bottom',
                            horizontal: 'center',
                        }}
                        transformOrigin={{
                            vertical: 'top',
                            horizontal: 'center',
                        }}
                    >
                        <StyledMoreButtons size="small" startIcon={<Edit />} onClick={() => setShowRenameDialog(true)}>
                            {translations.rename}
                        </StyledMoreButtons>
                        <StyledMoreButtons size="small" startIcon={<Sort />} onClick={handleSort}>
                            {translations.sort}
                        </StyledMoreButtons>
                        <StyledMoreButtons size="small" startIcon={<Delete />} onClick={handleDelete}>
                            {translations.delete}
                        </StyledMoreButtons>
                    </Popover>
                )}
                <AddFilesDialog isOpen={showAddFilesDialog} onClose={handleAddFiles} />
                <RenamePlayListDialog isOpen={showRenameDialog} name={state.name} onClose={handleRename} />
            </CardActions>
        </StyledRootCard>
    );
}

export default PlayListCardItem;
