import { useSnackbar } from 'notistack';
import { Fragment, useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { IFileItemResponseDto, IGetAllPlayListResponseDto, IPlayListItemResponseDto } from '../models';
import {
    getPlayList,
    onPlayListsChanged,
    onPlayListChanged,
    onFileAdded,
    onFileLoaded,
    onFileLoading,
    onFileDeleted,
} from '../services/castithub.service';
import FileItem from '../components/file/file_item';
import PlayListLoadingIndicator from '../components/playlist/playlist_loading_indicator';
import { CircularProgress, Container, createStyles, Grid, List, makeStyles, Typography } from '@material-ui/core';
import PlayListAppBar from '../components/playlist/playlist_appbar';
import { Info } from '@material-ui/icons';
import translations from '../services/translations';
import PageContent from './page_content';

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
    const { enqueueSnackbar } = useSnackbar();
    const params = useParams<Params>();

    const classes = useStyles();

    const loadPlayList = useCallback(async () => {
        const playList = await getPlayList(+params.id);
        setState((s) => ({
            ...s,
            isBusy: false,
            playList: playList,
            filteredFiles: playList.files,
        }));
    }, [params.id]);

    useEffect(() => {
        loadPlayList();
    }, [loadPlayList]);

    useEffect(() => {
        const onFileLoadingSubscription = onFileLoading.subscribe((file) => {
            if (!state.playList) {
                return;
            }

            if (file.playListId !== state.playList.id) {
                return;
            }

            setState((s) => ({ ...s, isBusy: true }));
        });

        const updatePlayList = (playList: IGetAllPlayListResponseDto): void => {
            if (!state.playList) {
                return;
            }

            if (playList.id !== state.playList.id) {
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

        const onFileAddedSubscription = onFileAdded.subscribe((file) => {
            if (!state.playList) {
                return;
            }

            if (file.playListId !== state.playList.id) {
                return;
            }

            const updatedFiles = [...state.playList.files];
            updatedFiles.splice(file.position, 0, file);

            const updatedPlayList = { ...state.playList! };
            updatedPlayList.files = updatedFiles;
            setState((s) => ({ ...s, playList: updatedPlayList }));
        });

        const onFileDeletedSubscription = onFileDeleted.subscribe((file) => {
            if (!state.playList) {
                return;
            }

            if (file.playListId !== state.playList.id) {
                return;
            }

            const deletedIndex = state.playList.files.findIndex((f) => f.id == file.fileId);
            if (deletedIndex < 0) {
                return;
            }
            const updatedFiles = [...state.playList.files];
            updatedFiles.splice(deletedIndex, 1);

            const updatedPlayList = { ...state.playList! };
            updatedPlayList.files = updatedFiles;
            setState((s) => ({ ...s, playList: updatedPlayList }));
        });

        return () => {
            onFileLoadingSubscription.unsubscribe();
            onPlayListsChangedSubscription.unsubscribe();
            onPlayListChangedSubscription.unsubscribe();

            onFileAddedSubscription.unsubscribe();
            onFileDeletedSubscription.unsubscribe();
        };
    }, [state.playList]);

    useEffect(() => {
        const onFileLoadedSubscription = onFileLoaded.subscribe((file) => {
            if (!state.playList) {
                return;
            }

            if (!state.isBusy || file.playListId !== state.playList.id) {
                return;
            }

            setState((s) => ({ ...s, isBusy: false }));
        });
        return () => {
            onFileLoadedSubscription.unsubscribe();
        };
    }, [state.playList, state.isBusy]);

    if (+params.id <= 0) {
        enqueueSnackbar(translations.invalidPlayList, { variant: 'warning' });
    }

    const files = state.filteredFiles.map((file) => <FileItem key={file.id} file={file} />) ?? [];
    const loading = state.playList ? <PlayListLoadingIndicator playListId={state.playList!.id} /> : null;

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

    const content =
        files.length > 0 ? (
            <Container style={{ flex: 'auto' }}>
                <Grid container justifyContent="center" alignItems="center">
                    <Grid item xs={12}>
                        <List>{files}</List>
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
                {loading}
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
