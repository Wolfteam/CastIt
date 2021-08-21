import { useSnackbar } from 'notistack';
import { Fragment, useCallback, useContext, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { IFileItemResponseDto, IGetAllPlayListResponseDto, IPlayListItemResponseDto } from '../models';
import {
    onPlayListsChanged,
    onPlayListChanged,
    onPlayListBusy,
    onFileAdded,
    onFilesChanged,
    onFileDeleted,
} from '../services/castithub.service';
import FileItem from '../components/file/file_item';
import { CircularProgress, Container, createStyles, Grid, List, makeStyles, Typography } from '@material-ui/core';
import PlayListAppBar from '../components/playlist/playlist_appbar';
import { Info } from '@material-ui/icons';
import translations from '../services/translations';
import PageContent from './page_content';
import { DragDropContext, Droppable, DropResult } from 'react-beautiful-dnd';
import { CastItHubContext } from '../context/castit_hub.context';

const useStyles = makeStyles((theme) =>
    createStyles({
        nothingFound: {
            textAlign: 'center',
            height: '100%',
        },
        loadingPlayList: {
            height: '100%',
            textAlign: 'center',
        },
    })
);

interface Params {
    id: string;
}

interface State {
    isBusy: boolean;
    playList?: IPlayListItemResponseDto;
    filteredFiles: IFileItemResponseDto[];
    searchText?: string;
}

const initialState: State = {
    isBusy: true,
    filteredFiles: [],
};

function PlayList() {
    const [state, setState] = useState(initialState);
    const [castItHub] = useContext(CastItHubContext);
    const { enqueueSnackbar } = useSnackbar();
    const params = useParams<Params>();

    const classes = useStyles();

    const loadPlayList = useCallback(async () => {
        const playList = await castItHub.connection.getPlayList(+params.id);
        setState((s) => ({
            ...s,
            isBusy: false,
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

        const onPlayListBusySubscription = onPlayListBusy.subscribe((busy) => {
            if (!isThisPlayList(busy.playListId)) {
                return;
            }

            setState((s) => ({ ...s, isBusy: busy.isBusy }));
        });

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
            onPlayListBusySubscription.unsubscribe();
            onPlayListsChangedSubscription.unsubscribe();
            onPlayListChangedSubscription.unsubscribe();

            onFilesChangedSubscription.unsubscribe();
            onFileAddedSubscription.unsubscribe();
            onFileDeletedSubscription.unsubscribe();
        };
    }, [state.playList]);

    if (+params.id <= 0) {
        enqueueSnackbar(translations.invalidPlayList, { variant: 'warning' });
    }

    const handleSearch = (value: string | null) => {
        if (!value || value === '') {
            setState((s) => ({ ...s, filteredFiles: s.playList?.files ?? [], searchText: value ?? '' }));
        } else {
            const filteredFiles = state.playList?.files.filter((f) => f.name.toLowerCase().includes(value.toLowerCase()!)) ?? [];
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

    const files = state.filteredFiles.map((file, index) => <FileItem key={file.id} file={file} index={index} />) ?? [];
    const content =
        files.length > 0 ? (
            <Container style={{ flex: 'auto' }}>
                <Grid container justifyContent="center" alignItems="center">
                    <Grid item xs={12}>
                        <DragDropContext onDragEnd={onDragEnd}>
                            <Droppable droppableId="playlist-droppable" direction="vertical">
                                {(provided) => (
                                    <List {...provided.droppableProps} innerRef={provided.innerRef}>
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
            <Container style={{ flex: 'auto', height: '100%' }}>
                <Grid container className={classes.nothingFound} justifyContent="center" alignItems="center">
                    <Grid item xs={12}>
                        <Info fontSize="large" />
                        <Typography>{translations.nothingFound}</Typography>
                    </Grid>
                </Grid>
            </Container>
        );

    return (
        <PageContent>
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
        </PageContent>
    );
}

export default PlayList;
