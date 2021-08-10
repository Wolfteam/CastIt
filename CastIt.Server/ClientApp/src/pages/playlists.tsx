import { Container } from '@material-ui/core';
import { Fragment, useEffect, useState } from 'react';
import { IGetAllPlayListResponseDto } from '../models';
import {
    initializeHubConnection,
    onPlayListAdded,
    onPlayListsLoaded,
    onPlayListDeleted,
    onPlayListChanged,
    onFileLoading,
    onFileLoaded,
    onFileEndReached,
    onPlayerStatusChanged,
} from '../services/castithub.service';
import PlayListCardItem from '../components/playlist/playlist_card_item';
import PageContent from './page_content';
import { Grid } from '@material-ui/core';

interface State {
    isBusy: boolean;
    playLists: IGetAllPlayListResponseDto[];
}

const initialState: State = {
    isBusy: true,
    playLists: [],
};

function PlayLists() {
    const [state, setState] = useState(initialState);

    useEffect(() => {
        const onPlayerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (!status.playList) {
                return;
            }

            const playList = state.playLists.find((pl) => pl.id === status.playList!.id);
            if (!playList) {
                return;
            }
            
            const index = state.playLists.indexOf(playList);
            const copy = [...state.playLists];
            copy.splice(index, 1);
            copy.splice(index, 0, status.playList!);
            setState((s) => ({ ...s, playLists: copy }));
        });

        const onPlayListAddedSubscription = onPlayListAdded.subscribe((playList) => {
            const copy = [...state.playLists];
            copy.splice(playList.position, 0, playList);
            setState((s) => ({ ...s, playLists: copy }));
        });

        const onPlayListChangedSubscription = onPlayListChanged.subscribe((playList) => {
            const copy = [...state.playLists];
            const index = copy.findIndex((pl) => pl.id === playList.id);
            if (index < 0) {
                return;
            }

            copy.splice(index, 1);
            copy.splice(index, 0, playList);

            setState((s) => ({ ...s, playLists: copy }));
        });

        const onPlayListDeletedSubscription = onPlayListDeleted.subscribe((id) => {
            const copy = [...state.playLists];
            const index = copy.findIndex((pl) => pl.id === id);
            if (index < 0) {
                return;
            }
            copy.splice(index, 1);
            setState((s) => ({ ...s, playLists: copy }));
        });

        return () => {
            onPlayListAddedSubscription.unsubscribe();
            onPlayerStatusChangedSubscription.unsubscribe();
            onPlayListChangedSubscription.unsubscribe();
            onPlayListDeletedSubscription.unsubscribe();
        };
    }, [state.playLists]);

    useEffect(() => {
        const onPlayListsLoadedSubscription = onPlayListsLoaded.subscribe((playLists) => {
            setState({ isBusy: false, playLists: playLists });
        });

        const onFileLoadingSubscription = onFileLoading.subscribe((_) => {
            setState((s) => ({ ...s, isBusy: true }));
        });

        const onFileLoadedSubscription = onFileLoaded.subscribe((_) => {
            setState((s) => ({ ...s, isBusy: false }));
        });

        const onFileEndReachedSubscription = onFileEndReached.subscribe((_) => {
            setState((s) => ({ ...s, isBusy: false }));
        });

        initializeHubConnection();

        return () => {
            onPlayListsLoadedSubscription.unsubscribe();
            onFileLoadingSubscription.unsubscribe();
            onFileLoadedSubscription.unsubscribe();
            onFileEndReachedSubscription.unsubscribe();
        };
    }, []);

    const items = state.playLists.map((pl) => (
        <Grid key={pl.id} item xs={6} sm={6} md={4} lg={3} xl={2}>
            <PlayListCardItem {...pl} />
        </Grid>
    ));

    const addNew = (
        <Grid key="AddNewItem" item xs={6} sm={6} md={4} lg={3} xl={2} style={{ alignSelf: 'center' }}>
            <PlayListCardItem id={0} position={-1} name="" imageUrl="" numberOfFiles={-1} totalDuration="" toAddNewItem />
        </Grid>
    );

    items.push(addNew);

    return (
        <PageContent useContainer>
            <Grid container spacing={3} style={{ marginTop: 20, marginBottom: 20 }}>
                {items}
            </Grid>
        </PageContent>
    );
}

export default PlayLists;
