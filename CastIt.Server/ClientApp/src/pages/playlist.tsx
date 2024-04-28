import { useSnackbar } from 'notistack';
import { styled } from '@mui/material/styles';
import { Fragment, useCallback, useEffect, useState } from 'react';
import { Params, useParams } from 'react-router-dom';
import { IFileItemResponseDto, IGetAllPlayListResponseDto, IPlayListItemResponseDto } from '../models';
import { onPlayListsChanged, onPlayListChanged, onFileAdded, onFilesChanged, onFileDeleted } from '../services/castithub.service';
import FileItem from '../components/file/file_item';
import { Button, CircularProgress, Container, Grid, List } from '@mui/material';
import PlayListAppBar from '../components/playlist/playlist_appbar';
import translations from '../services/translations';
import PageContent from './page_content';
import { DragDropContext, Droppable, DropResult } from '@hello-pangea/dnd';
import { useCastItHub } from '../context/castit_hub.context';
import NothingFound from '../components/nothing_found';
import { Add } from '@mui/icons-material';
import AddFilesDialog from '../components/dialogs/add_files_dialog';

const PREFIX = 'PlayList';

const classes = {
    nothingFound: `${PREFIX}-nothingFound`,
    loadingPlayList: `${PREFIX}-loadingPlayList`,
};

const StyledPageContent = styled(PageContent)(() => ({
    [`&.${classes.nothingFound}`]: {
        textAlign: 'center',
        height: '100%',
    },

    [`&.${classes.loadingPlayList}`]: {
        height: '100%',
        textAlign: 'center',
    },
}));

interface ComponentParams extends Params {
    id: string;
}

interface State {
    playList?: IPlayListItemResponseDto;
    filteredFiles: IFileItemResponseDto[];
    searchText?: string;
}

const initialState: State = {
    filteredFiles: [],
};

function PlayList() {
    const [state, setState] = useState(initialState);
    const castItHub = useCastItHub();
    const { enqueueSnackbar } = useSnackbar();
    const params = useParams<ComponentParams>();

    const [showAddFilesDialog, setShowAddFilesDialog] = useState(false);

    const loadPlayList = useCallback(async () => {
        const playList = await castItHub.connection.getPlayList(+params.id!);
        setState((s) => ({
            ...s,
            playList: playList,
            filteredFiles: playList.files,
        }));
    }, [params.id, castItHub.connection]);

    useEffect(() => {
        loadPlayList();
    }, [loadPlayList]);

    useEffect(() => {
        const isThisPlayList = (id: number): boolean => {
            if (!state.playList) {
                return false;
            }

            if (id !== state.playList.id) {
                return false;
            }

            return true;
        };

        const updatePlayList = (playList: IGetAllPlayListResponseDto): void => {
            if (!isThisPlayList(playList.id)) {
                return;
            }

            const updatedPlayList = { ...state.playList! };
            updatedPlayList.name = playList.name;
            updatedPlayList.position = playList.position;
            updatedPlayList.loop = playList.loop;
            updatedPlayList.shuffle = playList.shuffle;
            updatedPlayList.numberOfFiles = playList.numberOfFiles;
            updatedPlayList.playedTime = playList.playedTime;
            updatedPlayList.totalDuration = playList.totalDuration;
            updatedPlayList.imageUrl = playList.imageUrl;

            setState((s) => ({ ...s, playList: updatedPlayList }));
        };

        const onPlayListsChangedSubscription = onPlayListsChanged.subscribe((playLists) => {
            for (let index = 0; index < playLists.length; index++) {
                updatePlayList(playLists[index]);
            }
        });

        const onPlayListChangedSubscription = onPlayListChanged.subscribe(updatePlayList);

        const onFilesChangedSubscription = onFilesChanged.subscribe((files) => {
            if (!state.playList) {
                return;
            }
            const thisPlayList = files.filter((f) => f.playListId === state.playList!.id).length > 0;

            if (!thisPlayList) {
                return;
            }

            const updatedPlayList = { ...state.playList! };
            updatedPlayList.files = files;
            setState((s) => ({ ...s, playList: updatedPlayList, filteredFiles: files }));
        });

        const onFileAddedSubscription = onFileAdded.subscribe((file) => {
            if (!isThisPlayList(file.playListId)) {
                return;
            }

            const updatedFiles = [...state.playList!.files];
            updatedFiles.splice(file.position, 0, file);

            const updatedPlayList = { ...state.playList! };
            updatedPlayList.files = updatedFiles;
            setState((s) => ({ ...s, playList: updatedPlayList, filteredFiles: updatedPlayList.files }));
        });

        const onFileDeletedSubscription = onFileDeleted.subscribe((file) => {
            if (!isThisPlayList(file.playListId)) {
                return;
            }

            const deletedIndex = state.playList!.files.findIndex((f) => f.id === file.fileId);
            if (deletedIndex < 0) {
                return;
            }
            const updatedFiles = [...state.playList!.files];
            updatedFiles.splice(deletedIndex, 1);

            const updatedPlayList = { ...state.playList! };
            updatedPlayList.files = updatedFiles;
            setState((s) => ({ ...s, playList: updatedPlayList, filteredFiles: updatedPlayList.files }));
        });

        return () => {
            onPlayListsChangedSubscription.unsubscribe();
            onPlayListChangedSubscription.unsubscribe();

            onFilesChangedSubscription.unsubscribe();
            onFileAddedSubscription.unsubscribe();
            onFileDeletedSubscription.unsubscribe();
        };
    }, [state.playList]);

    if (+params.id! <= 0) {
        enqueueSnackbar(translations.invalidPlayList, { variant: 'warning' });
    }

    const handleSearch = (value: string | null) => {
        if (!value || value === '') {
            setState((s) => ({ ...s, filteredFiles: s.playList?.files ?? [], searchText: value ?? '' }));
        } else {
            const filteredFiles =
                state.playList?.files?.filter((f) => {
                    const includes = value.toLowerCase();
                    if (f.name) {
                        return f.name.toLowerCase().includes(includes);
                    }

                    return f.filename.toLowerCase().includes(includes);
                }) ?? [];
            setState((s) => ({ ...s, filteredFiles: filteredFiles, searchText: value }));
        }
    };

    if (!state.playList) {
        return (
            <Grid container justifyContent="center" alignItems="center" className={classes.loadingPlayList}>
                <Grid item xs={12}>
                    <CircularProgress />
                </Grid>
            </Grid>
        );
    }

    const onDragEnd = async (result: DropResult): Promise<void> => {
        if (!result.destination) {
            return;
        }

        if (result.destination!.droppableId === result.source.droppableId && result.destination.index === result.source.index) {
            return;
        }

        const source = state.playList!.files[result.source.index];
        if (!source) {
            return;
        }

        await castItHub.connection.updateFilePosition(state.playList!.id, source.id, result.destination!.index);
    };

    const handleAddFiles = async (path: string | null, includeSubFolder: boolean, onlyVideo: boolean): Promise<void> => {
        setShowAddFilesDialog(false);
        if (path) {
            await castItHub.connection.addFolderOrFileOrUrl(state.playList!.id, path, includeSubFolder, onlyVideo);
        }
    };

    const files = state.filteredFiles.map((file, index) => <FileItem key={file.id} file={file} index={index} />) ?? [];
    const content =
        files.length > 0 ? (
            <Container style={{ flex: 'auto' }}>
                <Grid container justifyContent="center" alignItems="center">
                    <Grid item xs={12}>
                        <DragDropContext onDragEnd={onDragEnd}>
                            <Droppable droppableId="playlist-droppable" direction="vertical">
                                {(provided) => (
                                    <List ref={provided.innerRef} {...provided.droppableProps}>
                                        {files}
                                        {provided.placeholder}
                                    </List>
                                )}
                            </Droppable>
                        </DragDropContext>
                    </Grid>
                </Grid>
            </Container>
        ) : (
            <NothingFound>
                <Grid container justifyContent="center" alignItems="center">
                    <Grid item>
                        <Button variant="contained" color="primary" startIcon={<Add />} onClick={() => setShowAddFilesDialog(true)}>
                            {translations.addFolder + '/' + translations.addFiles}
                        </Button>
                        <AddFilesDialog isOpen={showAddFilesDialog} onClose={handleAddFiles} />
                    </Grid>
                </Grid>
            </NothingFound>
        );

    return (
        <StyledPageContent>
            <Fragment>
                <PlayListAppBar
                    id={state.playList!.id}
                    loop={state.playList!.loop}
                    shuffle={state.playList!.shuffle}
                    name={state.playList!.name}
                    onSearch={handleSearch}
                    searchText={state.searchText}
                />
                {content}
            </Fragment>
        </StyledPageContent>
    );
}

export default PlayList;
